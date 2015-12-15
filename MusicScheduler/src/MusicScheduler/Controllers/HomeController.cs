using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                string youtubeLink = Request.Form.ElementAt(0).Value.ElementAt(0);
                string userName = Request.Form.ElementAt(1).Value.ElementAt(0).ToLower();

                string videoUrl = youtubeLink;

                bool isYoutubeUrl = DownloadUrlResolver.TryNormalizeYoutubeUrl(videoUrl, out videoUrl);

                if (string.IsNullOrWhiteSpace(youtubeLink) || string.IsNullOrWhiteSpace(userName) || !isYoutubeUrl)
                {
                    ViewData["ErrorMessage"] = "Dulli!";
                    return View();
                }

                User user = this.userManager.Users.Where(x => x.Name == userName).FirstOrDefault();

                if (user != null)
                {
                    user.YoutubeLinks.Add(new YoutubeFile { Url = youtubeLink });
                }
                else
                {
                    this.userManager.Users.Add(new User(userName, new YoutubeFile { Url = youtubeLink }));
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

        public IActionResult Info()
        {
            ViewData["Title"] = "Info";
            ViewData["Paused"] = ServiceLocator.Musicplayer.IsPaused;
            ViewData["CurrentPlayingSong"] = ServiceLocator.Musicplayer.CurrentPlayingSong != null ? ServiceLocator.Musicplayer.CurrentPlayingSong.Name : "";

            ViewData["Users"] = this.userManager.Users.Count;

            for (int i = 0; i < this.userManager.Users.Count; i++)
            {
                ViewData["User" + i] = this.userManager.Users[i].Name;
                ViewData["User" + i + "TimePlayed"] = this.userManager.Users[i].TimePlayed;
                ViewData["User" + i + "YoutubeLinks"] = this.userManager.Users[i].YoutubeLinks.Count;
                for (int j = 0; j < this.userManager.Users[i].YoutubeLinks.Count; j++)
                {
                    YoutubeFile youtubefile = this.userManager.Users[i].YoutubeLinks.ElementAt(j);

                    string additionalInfo = " downloading...";
                    if (youtubefile.Downloaded == true)
                    {
                        additionalInfo = " Title: " + youtubefile.Name + " Duration: " + youtubefile.Duration + " seconds";
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
    }
}
