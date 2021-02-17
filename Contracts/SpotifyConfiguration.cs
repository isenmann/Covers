
namespace Covers.Contracts
{
    public class SpotifyConfiguration
    {
        public const string Spotify = "Spotify";
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackUri { get; set; }
    }
}
