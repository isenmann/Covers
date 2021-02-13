using Covers.Contracts;
using Covers.Contracts.Interfaces;
using Covers.Hubs;
using Covers.Persistency.Entities;
using ImageMagick;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Covers.BackgroundServices
{
    public class AlbumScanner : BackgroundService
    {
        private ILogger<AlbumScanner> _logger;
        private readonly IServiceProvider _services;
        private DirectoryInfo _musicDirectory;

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
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                using var scope = _services.CreateScope();

                var albumService = scope.ServiceProvider.GetRequiredService<IAlbumService>();
                var artistService = scope.ServiceProvider.GetRequiredService<IArtistService>();
                var trackService = scope.ServiceProvider.GetRequiredService<ITrackService>();
                var coverDownloaderService = scope.ServiceProvider.GetRequiredService<ICoverDownloadService>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<CoversHub>>();
                var spotify = scope.ServiceProvider.GetRequiredService<ISpotifyService>();
                
                var existingAlbums = await albumService.GetAsync();

                // Delete all local albums from the database if they are no longer exist on disk
                existingAlbums.Where(a => !Directory.Exists(a.Path) && string.IsNullOrWhiteSpace(a.SpotifyId)).ToList().ForEach(async album => await albumService.DeleteAsync(album));
                var count = existingAlbums.RemoveAll(a => !Directory.Exists(a.Path) && string.IsNullOrWhiteSpace(a.SpotifyId));
                if (count > 0)
                {
                    await hubContext.Clients.All.SendAsync("AlbumUpdates");
                }

                var existingArtists = await artistService.GetAsync();
                var existingTracks = await trackService.GetAsync();
                var albumsToAdd = new List<Album>();
                
                var localAlbums = await SyncLocalAlbums(hubContext, existingAlbums, existingTracks, existingArtists);
                albumsToAdd.AddRange(localAlbums);

                var spotifyAlbums = await SyncSpotifyAlbums(spotify, albumService, hubContext, existingAlbums, existingTracks, existingArtists);
                albumsToAdd.AddRange(spotifyAlbums);

                foreach (var album in albumsToAdd)
                {
                    var artistUnique = album.Tracks.Select(t => t.Artist.Name).Distinct().Count() == 1;
                    if (artistUnique)
                    {
                        album.Artist = album.Tracks.First().Artist;
                    }
                    else
                    {
                        album.Artist = null;
                    }
                }

                if (albumsToAdd.Count > 0)
                {
                    foreach (var album in albumsToAdd.Where(a => a.Covers == null || a.Covers.Count == 0))
                    {
                        await hubContext.Clients.All.SendAsync("Processing", $"Fetching album cover: {album.Name}");
                        var artistUnique = album.Tracks.Select(t => t.ArtistId).Distinct().Count() == 1;
                        var artist = artistUnique ? album.Tracks.First().Artist.Name : " ";

                        var covers = await coverDownloaderService.DownloadCoverAsync(album.Name, artist);

                        if (covers == null)
                        {
                            continue;
                        }

                        album.Covers = new List<Cover>();

                        if (covers.Item1 != null)
                        {
                            var cover = new Cover
                            {
                                AlbumId = album.AlbumId,
                                Type = CoverType.Front,
                                CoverImage = covers.Item1
                            };
                            album.Covers.Add(cover);
                        }

                        if (covers.Item2 != null)
                        {
                            var cover = new Cover
                            {
                                AlbumId = album.AlbumId,
                                Type = CoverType.Back,
                                CoverImage = covers.Item2
                            };
                            album.Covers.Add(cover);
                        }
                    }

                    await hubContext.Clients.All.SendAsync("Processing", $"Done...", cancellationToken: cancellationToken);
                    await albumService.AddAsync(albumsToAdd);
                    await hubContext.Clients.All.SendAsync("AlbumUpdates", cancellationToken: cancellationToken);
                }
            }
        }

        private async Task<List<Album>> SyncLocalAlbums(IHubContext<CoversHub> hubContext, List<Album> existingAlbums, List<Track> existingTracks, List<Artist> existingArtists)
        {
            var albumsToAdd = new List<Album>();
            var newArtists = new List<Artist>();
            var newTracks = new List<Track>();

            // Scanning all mp3 files in the music directory
            foreach (var file in Directory.EnumerateFiles(_musicDirectory.FullName, "*.mp3", SearchOption.AllDirectories))
            {
                await hubContext.Clients.All.SendAsync("Processing", $"Processing file: {file}");
                using var taglibFile = TagLib.File.Create(file);

                // because of scanning all mp3 files, look if another track already created the album, artist or track
                var existingAlbum = existingAlbums.FirstOrDefault(a => a.Name == taglibFile.Tag.Album);
                var existingArtist = existingArtists.FirstOrDefault(a => a.Name == string.Join(",", taglibFile.Tag.Performers));
                var existingTrack = existingTracks.FirstOrDefault(a => a.Name == taglibFile.Tag.Title && a.Number == (int)taglibFile.Tag.Track);

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
                    var numberOfTrack = (int)taglibFile.Tag.Track;
                    if (taglibFile.Tag.DiscCount > 1)
                    {
                        // multi disc albums will have 101 as track number for first track on first CD and so on
                        numberOfTrack = ((int)taglibFile.Tag.Disc * 100) + numberOfTrack;
                    }

                    existingTrack = new Track
                    {
                        Name = taglibFile.Tag.Title,
                        Path = file,
                        Number = numberOfTrack,
                        Artist = existingArtist
                    };

                    newTracks.Add(existingTrack);
                }

                // if the mp3 tags have a picture, check if it's a front or back cover and add it to the album
                if (taglibFile.Tag.Pictures.Any(p => p.Type == TagLib.PictureType.FrontCover || p.Type == TagLib.PictureType.BackCover))
                {
                    if (existingAlbum.Covers == null)
                    {
                        existingAlbum.Covers = new List<Cover>();
                    }

                    foreach (var picture in taglibFile.Tag.Pictures)
                    {
                        using var cover = new MagickImage(picture.Data.Data);
                        if (picture.Type == TagLib.PictureType.FrontCover)
                        {
                            var frontCover = existingAlbum.Covers.FirstOrDefault(c => c.Type == CoverType.Front);
                            if (frontCover == null)
                            {
                                frontCover = new Cover
                                {
                                    AlbumId = existingAlbum.AlbumId,
                                    Type = CoverType.Front
                                };
                                existingAlbum.Covers.Add(frontCover);
                            }

                            frontCover.CoverImage = cover.ToByteArray(MagickFormat.Png);
                        }

                        if (picture.Type == TagLib.PictureType.BackCover)
                        {
                            var backCover = existingAlbum.Covers.FirstOrDefault(c => c.Type == CoverType.Back);
                            if (backCover == null)
                            {
                                backCover = new Cover
                                {
                                    AlbumId = existingAlbum.AlbumId,
                                    Type = CoverType.Back
                                };
                                existingAlbum.Covers.Add(backCover);
                            }

                            backCover.CoverImage = cover.ToByteArray(MagickFormat.Png);
                        }
                    }
                }

                existingAlbum.Tracks.Add(existingTrack);
            }

            return albumsToAdd;
        }


        private async Task<List<Album>> SyncSpotifyAlbums(ISpotifyService spotifyService, IAlbumService albumService, IHubContext<CoversHub> hubContext, List<Album> existingAlbums, List<Track> existingTracks, List<Artist> existingArtists)
        {
            var spotifyAlbums = await spotifyService.GetAlbumsFromUserLibrary();
            if (spotifyAlbums == null)
            {
                return new List<Album>();
            }

            existingAlbums.Where(a => !string.IsNullOrWhiteSpace(a.SpotifyId) && !spotifyAlbums.Any(sa => sa.Album.Id == a.SpotifyId)).ToList().ForEach(async album => await albumService.DeleteAsync(album));
            var count = existingAlbums.RemoveAll(a => !string.IsNullOrWhiteSpace(a.SpotifyId) && !spotifyAlbums.Any(sa => sa.Album.Id == a.SpotifyId));
            if (count > 0)
            {
                await hubContext.Clients.All.SendAsync("AlbumUpdates");
            }

            var albumsToAdd = new List<Album>();
            var newArtists = new List<Artist>();
            var newTracks = new List<Track>();

            foreach (var album in spotifyAlbums)
            {
                await hubContext.Clients.All.SendAsync("Processing", $"Processing Spotify album: {album.Album.Name}");

                // because of scanning all mp3 files, look if another track already created the album, artist or track
                var existingAlbum = existingAlbums.FirstOrDefault(a => a.Name == album.Album.Name);
                var existingArtist = existingArtists.FirstOrDefault(a => a.Name == string.Join(",", album.Album.Artists.Select(a => a.Name)));
                
                if (existingAlbum == null)
                {
                    existingAlbum = albumsToAdd.FirstOrDefault(a => a.Name == album.Album.Name);
                }

                if (existingArtist == null)
                {
                    existingArtist = newArtists.FirstOrDefault(a => a.Name == string.Join(",", album.Album.Artists.Select(a => a.Name)));
                }

                if (existingAlbum == null)
                {
                    existingAlbum = new Album
                    {
                        Name = album.Album.Name,
                        Tracks = new List<Track>(),
                        Covers = new List<Cover>(),
                        SpotifyId = album.Album.Id,
                        SpotifyUri = album.Album.Uri
                    };

                    albumsToAdd.Add(existingAlbum);
                }

                if (existingArtist == null)
                {
                    existingArtist = new Artist
                    {
                        Name = string.Join(",", album.Album.Artists.Select(a => a.Name))
                    };

                    newArtists.Add(existingArtist);
                }

                foreach (var track in album.Album.Tracks.Items)
                {
                    var existingTrack = existingTracks.FirstOrDefault(a => a.Name == track.Name && a.Number == track.TrackNumber);
                    if (existingTrack == null)
                    {
                        existingTrack = newTracks.FirstOrDefault(a => a.Name == track.Name && a.Number == track.TrackNumber);
                    }

                    if (existingTrack == null)
                    {
                        var numberOfTrack = track.TrackNumber;
                        if (track.DiscNumber > 1)
                        {
                            // multi disc albums will have 101 as track number for first track on first CD and so on
                            numberOfTrack = (track.DiscNumber * 100) + numberOfTrack;
                        }

                        existingTrack = new Track
                        {
                            Name = track.Name,
                            Path = null,
                            Number = numberOfTrack,
                            Artist = existingArtist,
                            SpotifyId = track.Id,
                            SpotifyUri = track.Uri
                        };

                        newTracks.Add(existingTrack);
                    }

                    existingAlbum.Tracks.Add(existingTrack);
                }

                // if the mp3 tags have a picture, check if it's a front or back cover and add it to the album
                if (album.Album.Images.Count > 0)
                {
                    if (existingAlbum.Covers == null)
                    {
                        existingAlbum.Covers = new List<Cover>();
                    }

                    using var webclient = new WebClient();
                    var coverImage = webclient.DownloadData(album.Album.Images[0].Url);
                    
                    using var image = new MagickImage(coverImage);
                    if (image.Width > 800)
                    {
                        image.Scale(new MagickGeometry { IgnoreAspectRatio = false, Width = 800 });
                    }

                    var frontCover = existingAlbum.Covers.FirstOrDefault(c => c.Type == CoverType.Front);
                    if (frontCover == null)
                    {
                        frontCover = new Cover
                        {
                            AlbumId = existingAlbum.AlbumId,
                            Type = CoverType.Front
                        };
                        existingAlbum.Covers.Add(frontCover);
                    }

                    frontCover.CoverImage = image.ToByteArray(MagickFormat.Png);
                }
            }

            return albumsToAdd;
        }
    }
}
