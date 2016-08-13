using System;

namespace MusicScheduler.Objects
{
    public class ServiceLocator
    {
        public static UserManager Usermanager { get; private set; }
        public static DownloadManager Downloadmanager { get; private set; }
        public static MusicPlayer Musicplayer { get; private set; }
        public static string MusicSchedulerDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/MusicSchedulerData";
        public static string MusicSchedulerConfigFile => MusicSchedulerDirectory + "/config.ini";
        public static string MusicSchedulerDownloadDirectory { get; set; } = MusicSchedulerDirectory + "/Downloaded";

        public static void Initalize()
        {
            Usermanager = new UserManager();
            Downloadmanager = new DownloadManager();
            Musicplayer = new MusicPlayer();
        }
    }
}