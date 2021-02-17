using Covers.Contracts;
using Covers.Contracts.Interfaces;
using ImageMagick;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Covers.Services
{
    public class CoverDownloadService : ICoverDownloadService
    {
        private readonly ILogger<CoverDownloadService> _logger;
        private readonly ISpotifyService _spotifyService;
        private readonly CoverDownloadConfiguration _coverDownloaderConfiguration;

        public CoverDownloadService(ILogger<CoverDownloadService> logger, IConfiguration configuration, ISpotifyService spotifyService)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _spotifyService = spotifyService ?? throw new ArgumentNullException(nameof(spotifyService));
            _coverDownloaderConfiguration = configuration.GetSection(CoverDownloadConfiguration.CoverDownloader).Get<CoverDownloadConfiguration>();
        }
        public async Task<Tuple<byte[], byte[]>> DownloadCoverAsync(string albumName, string artist)
        {
            if (string.IsNullOrWhiteSpace(albumName) || string.IsNullOrWhiteSpace(artist))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(_coverDownloaderConfiguration.Executable))
            {
                return null;
            }

            if (!File.Exists(_coverDownloaderConfiguration.Executable))
            {
                return null;
            }

            string arguments = string.Empty;
            switch (_coverDownloaderConfiguration.Type)
            {
                case CoverDownloaderType.AAD:
                    arguments = $"/ar \"{artist}\" /al \"{albumName}\" /path \"%type%.jpg\" /coverType front,back /s \"Qobuz (fr-fr),Amazon (.com),iTunes\"";
                    break;
                case CoverDownloaderType.SACAD:
                    arguments = $"--disable-low-quality-sources \"{artist}\" \"{albumName}\" 800 Front.jpg";
                    break;
            }

            byte[] frontCover = null;
            byte[] backCover = null;

            if (_coverDownloaderConfiguration.Type == CoverDownloaderType.Spotify)
            {
                frontCover = await _spotifyService.GetAlbumCover(albumName, artist);
                backCover = null;
            }
            else
            {
                var process = new Process();
                process.StartInfo.FileName = _coverDownloaderConfiguration.Executable;
                process.StartInfo.Arguments = arguments;
                process.Start();

                await process.WaitForExitAsync();
                var errorCode = process.ExitCode;

                if (errorCode != 0)
                {
                    return null;
                }
            }

            switch (_coverDownloaderConfiguration.Type)
            {
                case CoverDownloaderType.AAD:
                    frontCover = ScaleAndConvert("Front.jpg");
                    backCover = ScaleAndConvert("Back.jpg");
                    break;
                case CoverDownloaderType.SACAD:
                    frontCover = ScaleAndConvert("Front.jpg");
                    backCover = null; // not supported by SACAD
                    break;
            }

            if (File.Exists("Front.jpg"))
            {
                File.Delete("Front.jpg");
            }

            if (File.Exists("Back.jpg"))
            {
                File.Delete("Back.jpg");
            }

            return new Tuple<byte[], byte[]>(frontCover, backCover);
        }

        private static byte[] ScaleAndConvert(string fileName)
        {
            if (File.Exists(fileName))
            {
                using var cover = new MagickImage(File.ReadAllBytes(fileName));
                if (cover.Width > 800)
                {
                    cover.Scale(new MagickGeometry { IgnoreAspectRatio = false, Width = 800 });
                }

                return cover.ToByteArray(MagickFormat.Png);
            }

            return null;
        }
    }
}
