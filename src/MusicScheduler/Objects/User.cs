using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MusicScheduler.Objects
{
    public class User
    {
        public User(string name)
        {
            this.Name = name;
        }

        public User(string name, YoutubeFile youtubeLink)
        {
            this.Name = name;
            this.YoutubeLinks.Add(youtubeLink);
        }

        public string Name { get; set; }

        private readonly ObservableCollection<YoutubeFile> youtubeLinks = new ObservableCollection<YoutubeFile>();

        public double TimePlayed { get; set; }

        public ObservableCollection<YoutubeFile> YoutubeLinks
        {
            get
            {
                return this.youtubeLinks;
            }
        }
    }
}
