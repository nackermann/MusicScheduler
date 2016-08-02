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

                if (!Directory.Exists(ServiceLocator.MusicSchedulerDownloadDirectory))
                    Directory.CreateDirectory(ServiceLocator.MusicSchedulerDownloadDirectory);

                var fileName = NormalizeFileName(video.Title);

                var outputFile = new MediaFile
                {
                    Filename =
                        Path.Combine(ServiceLocator.MusicSchedulerDownloadDirectory,
                            fileName + ".mp3")
                };

                if (!File.Exists(Path.Combine(ServiceLocator.MusicSchedulerDownloadDirectory, fileName + ".mp3")))
                {
                    var videoDownloader = new VideoDownloader(video, Path.Combine(ServiceLocator.MusicSchedulerDownloadDirectory, fileName + video.AudioExtension));

                    try
                    {
                        videoDownloader.DownloadProgressChanged += (sender, args) =>
                        {
                            youtubeFile.DownloadStatus = (double)(int)(args.ProgressPercentage * 100) / 100;
                        };
                        videoDownloader.Execute();
                    }
                    catch (Exception e)
                    {
                        // if one/two person(s) download the same song this will happen -> removing the link should be fine I guess
                        Console.WriteLine(e.ToString());
                        user.YoutubeLinks.Remove(youtubeFile);
                        return;
                    }

                    // Convert mp4 to mp3
                    // -------------------
                    var inputFile = new MediaFile
                    {
                        Filename =
                            Path.Combine(ServiceLocator.MusicSchedulerDownloadDirectory,
                                fileName + video.AudioExtension)
                    };

                    using (var engine = new Engine())
                    {
                        engine.Convert(inputFile, outputFile);
                    }
                    // -------------------

                    File.Delete(Path.Combine(ServiceLocator.MusicSchedulerDownloadDirectory, fileName + video.AudioExtension));
                }

                using (var engine = new Engine())
                {
                    try
                    {
                        engine.GetMetadata(outputFile);
                    }
                    catch (Exception e)
                    {
                        // ffmpeg fails here in a very rare case
                        Console.WriteLine(e.ToString());
                        user.YoutubeLinks.Remove(youtubeFile);
                        return;
                    }
                }

                youtubeFile.Name = video.Title;

                if (string.IsNullOrEmpty(youtubeFile.Name))
                {
                    youtubeFile.Name = video.DownloadUrl;
                }

                youtubeFile.Duration = Math.Round(outputFile.Metadata.Duration.TotalSeconds, MidpointRounding.ToEven);
                youtubeFile.Downloaded = true;
                youtubeFile.Path =
                    Path.Combine(ServiceLocator.MusicSchedulerDownloadDirectory,
                        fileName + ".mp3");
                youtubeFile.Author = user.Name;

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