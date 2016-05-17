using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MediaToolkit;
using MediaToolkit.Model;
using YoutubeExtractor;

namespace MusicScheduler.Objects
{
    public class DownloadManager
    {
        private readonly UserManager userManager;

        public DownloadManager()
        {
            this.userManager = ServiceLocator.Usermanager;

            // Observe if new users will be added
            this.userManager.Users.CollectionChanged += (o, e) =>
            {
                // and observe his youtube links
                var user = (o as ObservableCollection<User>).ElementAt(e.NewStartingIndex);

                // beim ersten mussen wirs noch selber machen
                this.DownloadYoutubeLink(user.YoutubeLinks[0], user);

                user.YoutubeLinks.CollectionChanged += (ob, arg) =>
                {
                    if (arg.NewStartingIndex == -1)
                    {
                        return;
                    }
                    var youtubeFileToDownload = (ob as ObservableCollection<YoutubeFile>).ElementAt(arg.NewStartingIndex);

                    if (youtubeFileToDownload.Downloaded)
                    {
                        return;
                    }

                    this.DownloadYoutubeLink(youtubeFileToDownload, user);
                };
            };
        }

        public event Action OnYoutubeFileDownloadFinish = delegate { };

        private void DownloadYoutubeLink(YoutubeFile youtubeFile, User user)
        {
            Task.Run(() =>
            {
                VideoInfo video = null;
                try
                {
                    var videoInfos = DownloadUrlResolver.GetDownloadUrls(youtubeFile.Url);
                    video = videoInfos.First(info => info.VideoType == VideoType.Mp4);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    user.YoutubeLinks.Remove(youtubeFile);
                    return;
                }

                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }

                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Downloaded"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) +
                                              "/Downloaded");

                var fileName = NormalizeFileName(video.Title);

                var videoDownloader = new VideoDownloader(video,
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Downloaded",
                        fileName + video.AudioExtension));

                try
                {
                    videoDownloader.Execute();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    user.YoutubeLinks.Remove(youtubeFile);
                    return;
                }

                // Convert to mp4 to mp3
                // -------------------
                var inputFile = new MediaFile
                {
                    Filename =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Downloaded",
                            fileName + video.AudioExtension)
                };
                var outputFile = new MediaFile
                {
                    Filename =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Downloaded",
                            fileName + ".mp3")
                };

                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);
                    engine.Convert(inputFile, outputFile);
                }
                // -------------------

                youtubeFile.Name = video.Title;
                youtubeFile.Duration = inputFile.Metadata.Duration.TotalSeconds;
                youtubeFile.Downloaded = true;
                youtubeFile.Path =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Downloaded",
                        fileName + ".mp3");

                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Downloaded",
                    fileName + video.AudioExtension));

                this.OnYoutubeFileDownloadFinish();
            });
        }

        private static string NormalizeFileName(string filename)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, "");
        }
    }
}