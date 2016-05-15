using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicScheduler.Objects
{
    public class ServiceLocator
    {
        public static UserManager Usermanager { get; private set; }
        public static DownloadManager Downloadmanager { get; private set; }
        public static MusicPlayer Musicplayer { get; private set; }

        public static void Initalize()
        {
            Usermanager = new UserManager();
            Downloadmanager = new DownloadManager();
            Musicplayer = new MusicPlayer();
        }
    }
}
