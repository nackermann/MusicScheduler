using System;
using System.Collections.Generic;
using System.IO;
using MusicScheduler.Objects.DAO;

namespace MusicScheduler.Objects
{
    public class HistoryProvider
    {
        // -------------------------------------------------
        // file structure:
        // youtubeLink><youtubeTitle><datePlayed@CRLF
        // youtubeLink><youtubeTitle><datePlayed@CRLF
        // ...
        // -------------------------------------------------

        public HistoryProvider()
        {
            if (!File.Exists(this.historyFilePath))
            {
                File.Create(this.historyFilePath);
            }

            foreach (string readLine in File.ReadLines(this.historyFilePath))
            {
                string[] splitResult = readLine.Split(new[] { "><" }, StringSplitOptions.None);

                this.HistoryItems.Add(new HistoryItem
                {
                    YoutubeLink = splitResult[0],
                    YoutubeTitle = splitResult[1],
                    Date = splitResult[2]
                });
            }
        }

        private string historyFilePath => Path.Combine(ServiceLocator.MusicSchedulerDirectory, "playedSongs.txt");

        public List<HistoryItem> HistoryItems { get; set; } = new List<HistoryItem>();
    }
}