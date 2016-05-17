using System;
using System.Linq;
using Microsoft.AspNet.Mvc;
using MusicScheduler.Objects;
using YoutubeExtractor;

namespace MusicScheduler.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager userManager;

        public HomeController()
        {
            this.userManager = ServiceLocator.Usermanager;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Book";
            ViewData["Paused"] = ServiceLocator.Musicplayer.IsPaused;

            try
            {
                var youtubeLink = Request.Form.ElementAt(0).Value.ElementAt(0);
                var userName = Request.Form.ElementAt(1).Value.ElementAt(0).ToLower();

                var videoUrl = youtubeLink;

                var isYoutubeUrl = DownloadUrlResolver.TryNormalizeYoutubeUrl(videoUrl, out videoUrl);

                if (string.IsNullOrWhiteSpace(youtubeLink) || string.IsNullOrWhiteSpace(userName) || !isYoutubeUrl)
                {
                    ViewData["ErrorMessage"] = "Dulli!";
                    return View();
                }

                var user = this.userManager.Users.FirstOrDefault(x => x.Name == userName);

                if (user != null)
                {
                    user.YoutubeLinks.Add(new YoutubeFile {Url = youtubeLink});
                }
                else
                {
                    this.userManager.Users.Add(new User(userName, new YoutubeFile {Url = youtubeLink}));
                }
            }
            catch (Exception)
            {
                // No body was sent
            }
            return View();
        }

        public void Skip()
        {
            ServiceLocator.Musicplayer.SkipCurrentSong();
        }

        public void PauseResume()
        {
            ServiceLocator.Musicplayer.PauseResumeCurrentSong();
        }

        public IActionResult Info(int actionType)
        {
            if ((ActionType) actionType == ActionType.SkipSong)
            {
                this.Skip();
            }
            else if ((ActionType) actionType == ActionType.ToggleMusic)
            {
                this.PauseResume();
            }

            ViewData["Title"] = "Info";
            ViewData["Paused"] = ServiceLocator.Musicplayer.IsPaused;
            ViewData["CurrentPlayingSong"] = ServiceLocator.Musicplayer.CurrentPlayingSong != null
                ? ServiceLocator.Musicplayer.CurrentPlayingSong.Name
                : "";

            ViewData["Users"] = this.userManager.Users.Count;

            for (var i = 0; i < this.userManager.Users.Count; i++)
            {
                ViewData["User" + i] = this.userManager.Users[i].Name;
                ViewData["User" + i + "TimePlayed"] = this.userManager.Users[i].TimePlayed;
                ViewData["User" + i + "YoutubeLinks"] = this.userManager.Users[i].YoutubeLinks.Count;
                for (var j = 0; j < this.userManager.Users[i].YoutubeLinks.Count; j++)
                {
                    var youtubefile = this.userManager.Users[i].YoutubeLinks.ElementAt(j);

                    var additionalInfo = "- Downloading";
                    if (youtubefile.Downloaded)
                    {
                        additionalInfo = " Title: " + youtubefile.Name + " Duration: " + youtubefile.Duration +
                                         " seconds";
                    }

                    ViewData["User" + i + "YoutubeLink" + j] = youtubefile.Url + additionalInfo;
                }
            }

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        private enum ActionType
        {
            SkipSong = 1,
            ToggleMusic
        }
    }
}