using System.Collections.ObjectModel;

namespace MusicScheduler.Objects
{
    public class UserManager
    {
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
    }
}