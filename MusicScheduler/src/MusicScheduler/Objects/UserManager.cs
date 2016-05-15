using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MusicScheduler.Objects
{
    public class UserManager
    {
        private readonly ObservableCollection<User> users = new ObservableCollection<User>();
        public ObservableCollection<User> Users { get { return this.users; } }
    }
}
