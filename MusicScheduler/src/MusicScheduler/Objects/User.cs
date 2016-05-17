using System.Collections.ObjectModel;

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

        public double TimePlayed { get; set; }

        public ObservableCollection<YoutubeFile> YoutubeLinks { get; } = new ObservableCollection<YoutubeFile>();
    }
}