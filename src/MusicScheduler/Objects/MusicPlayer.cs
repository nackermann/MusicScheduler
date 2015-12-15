using System;
using System.Linq;
using CSCore.SoundOut;
using CSCore.Codecs;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MusicScheduler.Objects
{
    public class MusicPlayer
    {
        private readonly ISoundOut soundDevice;
        private readonly UserManager userManager;
        private readonly DownloadManager downloadManager;
        private bool isPlaying;
        public bool IsPaused { get; set; }
        public YoutubeFile CurrentPlayingSong { get; set; }

        public MusicPlayer()
        {
            this.userManager = ServiceLocator.Usermanager;
            this.downloadManager = ServiceLocator.Downloadmanager;

            this.soundDevice = this.GetSoundOut();

            this.downloadManager.OnYoutubeFileDownloadFinish += () =>
            {
                if (!this.isPlaying && this.IsPaused == false)
                {
                    this.PlayNextSong();
                }
            };

            this.soundDevice.Stopped += HandleWaveOutEventPlaybackStopped;
        }


        private void HandleWaveOutEventPlaybackStopped(object sender, PlaybackStoppedEventArgs e)
        {
            this.isPlaying = false;
            this.CurrentPlayingSong = null;
            this.PlayNextSong();
        }

        private void PlayNextSong()
        {
            if (this.IsPaused)
            {
                return;
            }

            User user = null;
            YoutubeFile youtubeFile = null;
            for (int i = 0; i < this.userManager.Users.Count; i++)
            {
                user = this.ChooseNextUser(i);
                youtubeFile = user.YoutubeLinks.FirstOrDefault();

                if (youtubeFile != null)
                {
                    break;
                }
            }

            if (youtubeFile == null || string.IsNullOrEmpty(youtubeFile.Path))
            {
                // there is nothing to play
                this.isPlaying = false;
                return;
            }
            else
            {
                this.isPlaying = true;

                user.TimePlayed += youtubeFile.Duration;
                this.CurrentPlayingSong = youtubeFile;


                Task.Run(() =>
                {
                    //this.soundDevice.Stop(); // not sure about this
                    this.soundDevice.Initialize(CodecFactory.Instance.GetCodec(youtubeFile.Path));
                    this.soundDevice.Play();
                });

                var time = DateTime.Now;
                Debug.WriteLine(time.Hour + ":" + time.Minute + ":" + time.Second + " " + youtubeFile.Name);
                user.YoutubeLinks.Remove(youtubeFile);
            }
        }

        private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            else
                return new DirectSoundOut();
        }

        private User ChooseNextUser(int userToChoose = 0)
        {
            return this.userManager.Users.OrderBy(x => x.TimePlayed).ElementAt(userToChoose);
        }

        public void SkipCurrentSong()
        {
            if (this.IsPaused || !this.isPlaying)
            {
                return;
            }

            Task.Run( () => 
            {
                // Event gets invoked automatically!
                this.soundDevice.Stop();
            });
        }

        public void PauseResumeCurrentSong()
        {
            if (this.IsPaused && !this.isPlaying)
            {
                // TODO: Resume?
                Task.Run( () => 
                {
                    this.soundDevice.Play();
                });
                this.IsPaused = false;
                this.isPlaying = true;
            }
            else if (!this.IsPaused && this.isPlaying)
            {
                Task.Run(() =>
                {
                    this.soundDevice.Pause();
                });
                this.IsPaused = true;
                this.isPlaying = false;
            }
        }

    }
}
