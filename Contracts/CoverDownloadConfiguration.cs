
namespace Covers.Contracts
{
    public class CoverDownloadConfiguration
    {
        public const string CoverDownloader = "CoverDownloader";
        public CoverDownloaderType Type { get; set; }
        public string Executable { get; set; }
        public bool CheckRegularForMissingCovers { get; set; }
    }
}
