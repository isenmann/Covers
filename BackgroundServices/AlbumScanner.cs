﻿using Covers.Contracts.Interfaces;
using Covers.Persistency.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
                using var scope = _services.CreateScope();

                var albumService = scope.ServiceProvider.GetRequiredService<IAlbumService>();
                var artistService = scope.ServiceProvider.GetRequiredService<IArtistService>();
                var trackService = scope.ServiceProvider.GetRequiredService<ITrackService>();
                var existingAlbums = await albumService.GetAsync();
                var exisitingArtists = await artistService.GetAsync();
                var exisitingTracks = await trackService.GetAsync();
                var albumsToAdd = new List<Album>();
                var newArtists = new List<Artist>();
                var newTracks = new List<Track>();

                foreach (var file in Directory.EnumerateFiles(_musicDirectory.FullName, "*.mp3", SearchOption.AllDirectories))
                {
                    using var taglibFile = TagLib.File.Create(file);
                    var existingAlbum = existingAlbums.FirstOrDefault(a => a.Name == taglibFile.Tag.Album);
                    var existingArtist = exisitingArtists.FirstOrDefault(a => a.Name == string.Join(",", taglibFile.Tag.AlbumArtists));
                    var existingTrack = exisitingTracks.FirstOrDefault(a => a.Name == taglibFile.Tag.Title && a.Number == (int)taglibFile.Tag.Track);
                    
                    if (existingAlbum == null)
                    {
                        existingAlbum = albumsToAdd.FirstOrDefault(a => a.Name == taglibFile.Tag.Album);
                    }

                    if (existingArtist == null)
                    {
                        existingArtist = newArtists.FirstOrDefault(a => a.Name == string.Join(",", taglibFile.Tag.AlbumArtists));
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
                            Name = string.Join(",", taglibFile.Tag.AlbumArtists)
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

                    existingAlbum.Tracks.Add(existingTrack);
                }

                if (albumsToAdd.Count > 0)
                {
                    await albumService.AddAsync(albumsToAdd);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }
}