namespace Covers.Models.DTOs
{
    public class PlaybackStateDTO
    {
        public string TotalTime { get; set; }
        public string CurrentTime { get; set; }
        public double CurrentProgressInPercent { get; set; }
    }
}
