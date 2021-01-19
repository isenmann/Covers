using Covers.Contracts.Interfaces;
using Covers.Persistency.Entities;
using ImageMagick;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Covers.BackgroundServices
{
    public class AlbumScanner : BackgroundService
    {
        private ILogger<AlbumScanner> _logger;
        private readonly IServiceProvider _services;
        private DirectoryInfo _musicDirectory;
        private string _aadExecutable;

        public AlbumScanner(ILogger<AlbumScanner> logger, IServiceProvider services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            var path = configuration.GetValue<string>("MusicDirectory");
            _musicDirectory = new DirectoryInfo(path);
            _aadExecutable = configuration.GetValue<string>("aadExecutable");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();

                var albumService = scope.ServiceProvider.GetRequiredService<IAlbumService>();
                var artistService = scope.ServiceProvider.GetRequiredService<IArtistService>();
                var trackService = scope.ServiceProvider.GetRequiredService<ITrackService>();
                var existingAlbums = await albumService.GetAsync();

                existingAlbums.Where(a => !Directory.Exists(a.Path)).ToList().ForEach(async album => await albumService.DeleteAsync(album));
                existingAlbums.RemoveAll(a => !Directory.Exists(a.Path));

                var exisitingArtists = await artistService.GetAsync();
                var exisitingTracks = await trackService.GetAsync();
                var albumsToAdd = new List<Album>();
                var newArtists = new List<Artist>();
                var newTracks = new List<Track>();

                foreach (var file in Directory.EnumerateFiles(_musicDirectory.FullName, "*.mp3", SearchOption.AllDirectories))
                {
                    using var taglibFile = TagLib.File.Create(file);
                    var existingAlbum = existingAlbums.FirstOrDefault(a => a.Name == taglibFile.Tag.Album);
                    var existingArtist = exisitingArtists.FirstOrDefault(a => a.Name == string.Join(",", taglibFile.Tag.Performers));
                    var existingTrack = exisitingTracks.FirstOrDefault(a => a.Name == taglibFile.Tag.Title && a.Number == (int)taglibFile.Tag.Track);
                    
                    if (existingAlbum == null)
                    {
                        existingAlbum = albumsToAdd.FirstOrDefault(a => a.Name == taglibFile.Tag.Album);
                    }

                    if (existingArtist == null)
                    {
                        existingArtist = newArtists.FirstOrDefault(a => a.Name == string.Join(",", taglibFile.Tag.Performers));
                    }

                    if (existingTrack == null)
                    {
                        existingTrack = newTracks.FirstOrDefault(a => a.Name == taglibFile.Tag.Title && a.Number == (int)taglibFile.Tag.Track);
                    }

                    if (existingAlbum == null)
                    {
                        existingAlbum = new Album
                        {
                            Name = taglibFile.Tag.Album,
                            Path = Path.GetDirectoryName(file),
                            Tracks = new List<Track>()
                        };

                        albumsToAdd.Add(existingAlbum);
                    }

                    if (existingArtist == null)
                    {
                        existingArtist = new Artist
                        {
                            Name = string.Join(",", taglibFile.Tag.Performers)
                        };

                        newArtists.Add(existingArtist);
                    }

                    if (existingTrack == null)
                    {
                        existingTrack = new Track
                        {
                            Name = taglibFile.Tag.Title,
                            Path = file,
                            Number = (int)taglibFile.Tag.Track,
                            Artist = existingArtist
                        };

                        newTracks.Add(existingTrack);
                    }

                    // if the mp3 tags have a picture, check if it's a front or back cover and add it to the album
                    if (taglibFile.Tag.Pictures.Any(p => p.Type == TagLib.PictureType.FrontCover || p.Type == TagLib.PictureType.BackCover))
                    {
                        if (existingAlbum.Cover == null)
                        {
                            existingAlbum.Cover = new Cover();
                        }

                        foreach (var picture in taglibFile.Tag.Pictures)
                        {
                            using var cover = new MagickImage(picture.Data.Data);
                            if (picture.Type == TagLib.PictureType.FrontCover)
                            {
                                if (existingAlbum.Cover.FrontCover == null)
                                {
                                    existingAlbum.Cover.FrontCover = cover.ToByteArray(MagickFormat.Png);
                                }
                            }

                            if (picture.Type == TagLib.PictureType.BackCover)
                            {
                                if (existingAlbum.Cover.BackCover == null)
                                {
                                    existingAlbum.Cover.BackCover = cover.ToByteArray(MagickFormat.Png);
                                }
                            }
                        }
                    }

                    existingAlbum.Tracks.Add(existingTrack);
                }

                if (albumsToAdd.Count > 0)
                {
                    foreach (var album in albumsToAdd.Where(a => a.Cover == null))
                    {
                        var artistUnique = album.Tracks.Select(t => t.ArtistId).Distinct().Count() == 1;
                        var artist = artistUnique ? album.Tracks.First().Artist.Name : " ";

                        if (await FetchCover(album.Name, artist))
                        {
                            album.Cover = new Cover();

                            if (File.Exists("Front.jpg"))
                            {
                                using var frontCover = new MagickImage(File.ReadAllBytes("Front.jpg"));
                                album.Cover.FrontCover = frontCover.ToByteArray(MagickFormat.Png);
                            }

                            if (File.Exists("Back.jpg"))
                            {
                                using var backCover = new MagickImage(File.ReadAllBytes("Back.jpg"));
                                album.Cover.BackCover = backCover.ToByteArray(MagickFormat.Png);
                            }
                        };
                    }

                    await albumService.AddAsync(albumsToAdd);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }

        protected async Task<bool> FetchCover(string albumName, string artist)
        {
            if (string.IsNullOrWhiteSpace(albumName) || string.IsNullOrWhiteSpace(artist))
            {
                return false;
            }

            if (!File.Exists(_aadExecutable))
            {
                return false;
            }

            if (File.Exists("Front.jpg"))
            {
                File.Delete("Front.jpg");
            }

            if (File.Exists("Back.jpg"))
            {
                File.Delete("Back.jpg");
            }

            var process = new Process();
            process.StartInfo.FileName = _aadExecutable;
            process.StartInfo.Arguments = $"/ar \"{artist}\" /al \"{albumName}\" /path \"%type%.jpg\" /coverType front,back /s \"Qobuz (fr-fr),Amazon (.com),iTunes\"";
            process.Start();

            await process.WaitForExitAsync();
            var errorCode = process.ExitCode;

            return errorCode == 0;
        }
    }
}
