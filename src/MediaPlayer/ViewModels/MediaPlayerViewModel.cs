using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPlayer.ViewModels
{
    public class MediaPlayerViewModel : BaseViewModel
    {
        #region Declarations
        private Media.File currentMediaFile;

        private Random random = new Random();

        private bool repeat;

        private bool showSubtitles;

        private bool shuffle;

        private List<Media.File> filesRemoved = new List<Media.File>();

        /// <summary>
        /// Collection containing all the Media.Files to be reproduced.
        /// </summary>
        private List<Media.File> playbackList;

        /// <summary>
        /// Collection containing all the already reproduced files.
        /// 
        /// Used for calculating the next file to be reproduced according to the Shuffle and Repeat values.
        /// </summary>
        private ICollection<Media.File> playedFiles;

        /// <summary>
        /// Collection containing all the files waiting to be played.
        /// 
        /// Used for calculating the next file to be reproduced according to the Shuffle and Repeat values.
        /// </summary>
        private List<Media.File> unplayedFiles;

        private List<SubtitleLine> subtitleList = new List<SubtitleLine>();

        private List<SubtitleLine> remainingSubtitleList = new List<SubtitleLine>();

        private string actualSubtitle = string.Empty;
        #endregion

        #region Properties
        public string ActualSubtitle
        {
            get { return actualSubtitle; }
            set
            {
                actualSubtitle = value;
                OnPropertyChanged("ActualSubtitle");
                OnPropertyChanged("ContainSubtitles");
            }
        }

        /// <summary>
        /// Boolean value that indicates if a file has been reproduced.
        /// </summary>
        public bool CanSkipBack { get { return playedFiles.Count > 1; } }

        /// <summary>
        /// Boolean value that indicates if a new file can been reproduced.
        /// </summary>
        public bool CanSkipAhead { get { return unplayedFiles.Count > 0; } }

        /// <summary>
        /// Current file being reproduced.
        /// </summary>
        public Media.File CurrentMediaFile
        {
            get { return currentMediaFile; }
            set
            {
                currentMediaFile = value;
                OnPropertyChanged("CurrentMediaFile");
            }
        }

        /// <summary>
        /// Boolean indicating if the same file can be reproduced two or more times.
        /// </summary>
        public bool Repeat
        {
            get { return repeat; }
            set
            {
                repeat = value;
                OnPropertyChanged("Repeat");
            }
        }

        /// <summary>
        /// Boolean indicating if the media files should be played consecutively.
        /// </summary>
        public bool Shuffle
        {
            get { return shuffle; }
            set
            {
                shuffle = value;
                OnPropertyChanged("Shuffle");
            }
        }

        public bool ShowSubtitles
        {
            get { return showSubtitles; }
            set
            {
                showSubtitles = value;
                OnPropertyChanged("ShowSubtitles");
            }
        }

        public bool ContainSubtitles { get { return remainingSubtitleList.Count > 0; } }
        #endregion

        #region Initializers
        public MediaPlayerViewModel(Media.File mediafile) : this(new List<Media.File> { mediafile }) { }

        public MediaPlayerViewModel(List<Media.File> mediaFiles)
        {
            playbackList = mediaFiles;
            playedFiles = new List<Media.File>();
            unplayedFiles = new List<Media.File>();

            FillUnplayedFiles(mediaFiles);
        }
        #endregion

        #region Public Methods
        public void AddSubtitles(string subtitleFileContent)
        {
            string[] subtitleLines = subtitleFileContent.Split(new[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                subtitleList = subtitleLines.Select(line => line.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                                                     .Select(splittedSubtitleLine => new SubtitleLine()
                                                     {
                                                         Number = Int32.Parse(splittedSubtitleLine[0]),
                                                         Time = splittedSubtitleLine[1].Split(new[] { " --> " }, StringSplitOptions.RemoveEmptyEntries).Select(t => DateTime.ParseExact(t, "hh:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture).TimeOfDay).ToArray(),
                                                         Caption = GetFullSubtitleLineContent(splittedSubtitleLine),
                                                         //Caption = s[2]
                                                     })
                                                     .ToList();

                remainingSubtitleList = new List<SubtitleLine>(subtitleList);
            }
            catch
            {
                OnPropertyChanged("SubtitleLoadException");
            }
        }

        private string GetFullSubtitleLineContent(string[] splittedSubtitleLine)
        {
            if (splittedSubtitleLine.Length < 3)
                return string.Empty;

            string returnValue = string.Empty;

            for (int index = 2; index < splittedSubtitleLine.Length; index++)
            {
                returnValue = returnValue + splittedSubtitleLine[index];
                
                if(index + 1 < splittedSubtitleLine.Length)
                    returnValue = returnValue + "\r\n";
            }

            return returnValue;
        }

        public void LoadSubtitles(string loadSubtitles)
        {
            OnPropertyChanged(loadSubtitles);

            if (!ShowSubtitles)
                SwitchShowSubtitles(ShowSubtitles);
        }

        public void UpdateSubtitles(double totalSecondsPassed)
        {
            if (ContainSubtitles)
            {
                TimeSpan elapsedTime = TimeSpan.FromSeconds(totalSecondsPassed);
                string elapsedTimeString = elapsedTime.ToString(@"hh\:mm\:ss");

                UpdateSubtitleList(elapsedTime);

                if (ContainSubtitles &&
                    (elapsedTime >= remainingSubtitleList[0].Time[0] &&
                     elapsedTime <= remainingSubtitleList[0].Time[1]))
                {
                    ActualSubtitle = remainingSubtitleList[0].Caption;
                }
                else
                    ActualSubtitle = string.Empty;
            }
        }

        public void ResetSubtitles()
        {
            remainingSubtitleList.Clear();
            ActualSubtitle = string.Empty;

            remainingSubtitleList = new List<SubtitleLine>(subtitleList);
        }

        /// <summary>
        /// Removes a file from the unplayed and played lists.
        /// </summary>
        /// <param name="mediaFile"> File to be removed. </param>
        public void RemoveFileFromList(Media.File mediaFile)
        {
            filesRemoved.Add(mediaFile);

            playedFiles.Remove(mediaFile);
            unplayedFiles.Remove(mediaFile);
        }

        /// <summary>
        /// Gets the next Media.File item.
        /// </summary>
        /// <param name="mediaFile"> Current Media.File item. </param>
        public void SkipAhead(Media.File mediaFile)
        {
            if (!CanSkipAhead && Repeat && unplayedFiles.Count == 0)
                FillUnplayedFiles(playbackList);

            if (CanSkipAhead)
            {
                CurrentMediaFile = GetNextMediaFile();
                CleanSubtitles();
            }
        }

        /// <summary>
        /// Gets the last Media.File item.
        /// </summary>
        /// <param name="mediaFile"> Current Media.File item. </param>
        public void SkipBack(Media.File mediaFile)
        {
            if (CanSkipBack)
            {
                CurrentMediaFile = GetLastMediaFile(mediaFile);
                CleanSubtitles();
            }
        }

        /// <summary>
        /// Switches the value of Repeat.
        /// </summary>
        /// <param name="actualValue"> Current Repeat value. </param>
        public void SwitchRepeat(bool actualValue)
        {
            Repeat = !actualValue;
        }

        /// <summary>
        /// Switches the value of ShowSubtitles.
        /// </summary>
        /// <param name="actualValue"> Current ShowSubtitles value. </param>
        public void SwitchShowSubtitles(bool actualValue)
        {
            ShowSubtitles = !actualValue;
        }

        /// <summary>
        /// Switches the value of Shuffle.
        /// </summary>
        /// <param name="actualValue"> Current Shuffle value. </param>
        public void SwitchShuffle(bool actualValue)
        {
            Shuffle = !actualValue;
        }
        #endregion

        #region Private Methods
        private void CleanSubtitles()
        {
            remainingSubtitleList.Clear();
            subtitleList.Clear();
            ActualSubtitle = string.Empty;
        }

        /// <summary>
        /// Fills the "unplayed files" list with a new collection of Media.Files.
        /// 
        /// Filled this way because if not, when removing a file it also gets removed from the original collection.
        /// </summary>
        /// <param name="mediaFiles"> ICollection containing Media.File items. </param>
        private void FillUnplayedFiles(ICollection<Media.File> mediaFiles)
        {
            foreach (Media.File mediaFile in mediaFiles)
                if (!filesRemoved.Contains(mediaFile))
                    unplayedFiles.Add(mediaFile);
        }

        /// <summary>
        /// Gets the last Media.File reproduced.
        /// </summary>
        /// <returns> Media.File to be reproduced. </returns>
        private Media.File GetLastMediaFile(Media.File currentMediaFile)
        {
            unplayedFiles.Insert(0, currentMediaFile);
            playedFiles.Remove(currentMediaFile);

            Media.File returnMediaFile = playedFiles.LastOrDefault();
            playedFiles.Remove(returnMediaFile);

            UpdatePlaylist(returnMediaFile);

            return returnMediaFile;
        }

        /// <summary>
        /// Gets the next Media.File to be reproduced.
        /// </summary>
        /// <returns> Media.File to be reproduced. </returns>
        private Media.File GetNextMediaFile()
        {
            Media.File returnMediaFile;

            if (Shuffle)
                returnMediaFile = GetRandomMediaFile(unplayedFiles);
            else
                returnMediaFile = GetNextMediaFile(unplayedFiles);

            UpdatePlaylist(returnMediaFile);

            return returnMediaFile;
        }

        /// <summary>
        /// Gets the first Media.File from the collection.
        /// </summary>
        /// <param name="mediaFiles"> Collection to get the file from. </param>
        /// <returns> First Media.File. </returns>
        private Media.File GetNextMediaFile(ICollection<Media.File> mediaFiles)
        {
            return mediaFiles.FirstOrDefault();
        }

        /// <summary>
        /// Gets a random Media.File from the collection.
        /// </summary>
        /// <param name="mediaFiles"> Collection to get the file from. </param>
        /// <returns> Random Media.File. </returns>
        private Media.File GetRandomMediaFile(ICollection<Media.File> mediaFiles)
        {
            return mediaFiles.ElementAt(random.Next(0, mediaFiles.Count));
        }

        /// <summary>
        /// Moves the Media.File from the unplayed files list to the played files list and resets the collection if needed.
        /// </summary>
        /// <param name="mediaFile"> Media.File to move from one collection to the other. </param>
        private void UpdatePlaylist(Media.File mediaFile)
        {
            playedFiles.Add(mediaFile);
            unplayedFiles.Remove(mediaFile);

            if (Repeat && Shuffle)
            {
                unplayedFiles.Clear();
                FillUnplayedFiles(playbackList);
            }

            //if (Repeat &&
            //    unplayedFiles.Count == 0)
            //    FillUnplayedFiles(playbackList);

            OnPropertyChanged("CanSkipAhead");
            OnPropertyChanged("CanSkipBack");
        }

        /// <summary>
        /// Removes all the subtitles that should have been shown 
        /// according to the time span passed.
        /// </summary>
        /// <param name="elapsedTime"> Actual timespan of the media file. </param>
        private void UpdateSubtitleList(TimeSpan elapsedTime)
        {
            if (remainingSubtitleList.Count > 1 &&
                elapsedTime >= remainingSubtitleList[0].Time[1])
            {
                remainingSubtitleList.RemoveAt(0);

                UpdateSubtitleList(elapsedTime);
            }
        }
        #endregion

        #region Commands
        private System.Windows.Input.ICommand loadSubtitlesCommand;
        public System.Windows.Input.ICommand LoadSubtitlesCommand
        {
            get
            {
                if (loadSubtitlesCommand == null)
                    loadSubtitlesCommand = new Helpers.DelegateCommand<string>(LoadSubtitles);

                return loadSubtitlesCommand;
            }
        }

        private System.Windows.Input.ICommand skipBackCommand;
        public System.Windows.Input.ICommand SkipBackCommand
        {
            get
            {
                if (skipBackCommand == null)
                    skipBackCommand = new Helpers.DelegateCommand<Media.File>(SkipBack);

                return skipBackCommand;
            }
        }

        private System.Windows.Input.ICommand skipAheadCommand;
        public System.Windows.Input.ICommand SkipAheadCommand
        {
            get
            {
                if (skipAheadCommand == null)
                    skipAheadCommand = new Helpers.DelegateCommand<Media.File>(SkipAhead);

                return skipAheadCommand;
            }
        }

        private System.Windows.Input.ICommand switchRepeatCommand;
        public System.Windows.Input.ICommand SwitchRepeatCommand
        {
            get
            {
                if (switchRepeatCommand == null)
                    switchRepeatCommand = new Helpers.DelegateCommand<bool>(SwitchRepeat);

                return switchRepeatCommand;
            }
        }

        private System.Windows.Input.ICommand switchShowSubtitlesCommand;
        public System.Windows.Input.ICommand SwitchShowSubtitlesCommand
        {
            get
            {
                if (switchShowSubtitlesCommand == null)
                    switchShowSubtitlesCommand = new Helpers.DelegateCommand<bool>(SwitchShowSubtitles);

                return switchShowSubtitlesCommand;
            }
        }

        private System.Windows.Input.ICommand switchShuffleCommand;
        public System.Windows.Input.ICommand SwitchShuffleCommand
        {
            get
            {
                if (switchShuffleCommand == null)
                    switchShuffleCommand = new Helpers.DelegateCommand<bool>(SwitchShuffle);

                return switchShuffleCommand;
            }
        }
        #endregion
    }

    internal class SubtitleLine
    {
        public int Number { get; set; }
        public string Caption { get; set; }
        public TimeSpan[] Time { get; set; }
    }
}
