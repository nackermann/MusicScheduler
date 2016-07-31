namespace MusicScheduler.Objects
{
    public class YoutubeFile
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string Path { get; set; }

        public string Author { get; set; }

        public bool Downloaded { get; set; }

        public double Duration { get; set; }

        public double DownloadStatus { get; set; }
    }
}