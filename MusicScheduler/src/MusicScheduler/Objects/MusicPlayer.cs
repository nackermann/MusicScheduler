﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CSCore.Codecs;
using CSCore.SoundOut;

namespace MusicScheduler.Objects
{
    public class MusicPlayer
    {
        private readonly DownloadManager downloadManager;
        private readonly ISoundOut soundDevice;
        private readonly UserManager userManager;
        private bool isPlaying;

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

            this.soundDevice.Stopped += this.HandleWaveOutEventPlaybackStopped;
        }

        public bool IsPaused { get; set; }
        public YoutubeFile CurrentPlayingSong { get; set; }


        private float soundVolume = 0.3f;
        public float SoundVolume
        {
            get { return this.soundVolume; }
            set
            {
                if (value >= 1f)
                {
                    value = 1f;
                }
                else if (value <= 0f)
                {
                    value = 0f;
                }

                this.soundVolume = value;
                if (this.soundDevice.WaveSource != null)
                {
                    this.soundDevice.Volume = value;
                }
            }
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
            for (var i = 0; i < this.userManager.Users.Count; i++)
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
                    this.soundDevice.Volume = this.soundVolume;
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

            Task.Run(() =>
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
                Task.Run(() => { this.soundDevice.Play(); });
                this.IsPaused = false;
                this.isPlaying = true;
            }
            else if (!this.IsPaused && this.isPlaying)
            {
                Task.Run(() => { this.soundDevice.Pause(); });
                this.IsPaused = true;
                this.isPlaying = false;
            }
        }
    }
}