using MediaPlayer.Helpers;
using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MediaPlayer.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MediaPlayerPage : MediaPlayer.Common.LayoutAwarePage
    {
        #region Declarations
        private Windows.System.Display.DisplayRequest displayRequest;

        private DispatcherTimer sliderTimer;

        private DispatcherTimer transportControlsTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 2) };

        private DispatcherTimer subtitlesTimer;

        private bool sliderPressed = false;

        private Windows.UI.Xaml.Controls.Primitives.Popup menuPopup;

        private UserControls.RepeatShufflePopupContent repeatShufflePopupContent;
        #endregion

        #region Initializers
        public MediaPlayerPage()
        {
            this.InitializeComponent();
        }
        #endregion

        #region State managers
        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            this.DataContext = new MediaPlayer.ViewModels.MediaPlayerViewModel((List<Media.File>)navigationParameter);
            ((ViewModels.MediaPlayerViewModel)this.DataContext).PropertyChanged += MediaPlayerPage_PropertyChanged;

            displayRequest = new Windows.System.Display.DisplayRequest();

            repeatShufflePopupContent = new MediaPlayer.UserControls.RepeatShufflePopupContent(bottomAppBar.Background, this.DataContext);

            menuPopup = new Windows.UI.Xaml.Controls.Primitives.Popup();
            menuPopup.IsLightDismissEnabled = true;
            menuPopup.Child = repeatShufflePopupContent;

            transportControlsTimer.Tick += transportControlsTimer_Tick;
            transportControlsTimer.Start();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }
        #endregion

        #region Private Methods
        private async System.Threading.Tasks.Task<bool> ErrorSubtitleFile()
        {
            Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader(@"ErrorResources");
            return await ShowErrorMessage(resourceLoader.GetString("SubtitleFileCouldNotBeOpenedContent"), resourceLoader.GetString("SubtitleFileCouldNotBeOpened"));
        }

        private async void FileNotFound(Media.File mediaFile)
        {
            Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader(@"Resources");

            ((ViewModels.MediaPlayerViewModel)this.DataContext).SkipAhead(mediaFile);
            ((ViewModels.MediaPlayerViewModel)this.DataContext).RemoveFileFromList(mediaFile);
            await ShowErrorMessage(mediaFile.Path, resourceLoader.GetString("FileNotFound"));
        }

        private void GetNextFile()
        {
            ((ViewModels.MediaPlayerViewModel)this.DataContext).SkipAhead(((ViewModels.MediaPlayerViewModel)this.DataContext).CurrentMediaFile);
        }

        private async System.Threading.Tasks.Task<bool> LoadSubtitles()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            foreach (string format in Helpers.MediaFormats.Subtitles)
                filePicker.FileTypeFilter.Add("." + format);

            Windows.Storage.StorageFile subtitlesFile = await filePicker.PickSingleFileAsync();

            if (subtitlesFile != null)
                await LoadSubtitle(subtitlesFile);

            return true;
        }

        private async System.Threading.Tasks.Task<bool> LoadSubtitle(Windows.Storage.StorageFile subtitlesFile)
        {
            Windows.Storage.StorageFile temporalSubtitleFile = null;
            string temporalFileName = "Auxiliar Subtitle File.txt";
            bool failedToOpenFile = false;

            try
            {
                if (subtitlesFile == null)
                    return false;

                await subtitlesFile.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder, temporalFileName, Windows.Storage.NameCollisionOption.ReplaceExisting);

                temporalSubtitleFile = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(temporalFileName);

                string temporalSubtitleFileContent;
                Windows.Storage.Streams.IBuffer buffer = await Windows.Storage.FileIO.ReadBufferAsync(temporalSubtitleFile);
                using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
                {
                    var bytes1251 = new Byte[buffer.Length];
                    dataReader.ReadBytes(bytes1251);

                    temporalSubtitleFileContent = System.Text.Encoding.GetEncoding("Utf-8").GetString(bytes1251, 0, bytes1251.Length);  //Windows-1251
                }

                ((ViewModels.MediaPlayerViewModel)this.DataContext).AddSubtitles(temporalSubtitleFileContent);
            }
            catch
            {
                failedToOpenFile = true;
            }

            if (failedToOpenFile)
                await ErrorSubtitleFile();

            return !failedToOpenFile;
        }

        private async System.Threading.Tasks.Task<bool> ShowErrorMessage(string content, string title)
        {
            await new Windows.UI.Popups.MessageDialog(content, title).ShowAsync();

            return true;
        }
        #endregion

        #region Events
        private async void MediaPlayerPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ViewModels.MediaPlayerViewModel mediaPlayerViewModel = (ViewModels.MediaPlayerViewModel)this.DataContext;

            switch (e.PropertyName)
            {
                case "ActualSubtitle":
                    if (mediaPlayerViewModel.ShowSubtitles &&
                        mediaPlayerViewModel.ContainSubtitles &&
                        !string.IsNullOrEmpty(mediaPlayerViewModel.ActualSubtitle))
                    {
                        this.SubtitleBorder.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                        this.SubtitleBorder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    break;

                case "CurrentMediaFile":
                    Media.File mediaFile = ((ViewModels.MediaPlayerViewModel)this.DataContext).CurrentMediaFile;

                    if (mediaFile != null)
                    {
                        try
                        {
                            MediaFileStream mediaFileStream = await StorageFileHelper.GetMediaFileStream(mediaFile);
                            this.MediaFilePlayer.SetSource(mediaFileStream.Stream, mediaFileStream.MimeType);
                        }
                        catch
                        {
                            FileNotFound(mediaFile);
                        }
                    }
                    break;

                case "LoadSubtitles":
                    await LoadSubtitles();
                    break;

                case "SubtitleLoadException":
                    await ErrorSubtitleFile();
                    break;
            }
        }

        private void MediaFilePlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (this.MediaFilePlayer.CurrentState)
            {
                case MediaElementState.Paused:
                    this.PauseButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.PlayButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

                    sliderTimer.Stop();
                    break;

                case MediaElementState.Playing:
                    this.PauseButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.PlayButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    if (sliderPressed)
                        sliderTimer.Stop();
                    else
                        sliderTimer.Start();
                    break;

                case MediaElementState.Stopped:
                    this.PauseButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.PlayButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

                    this.MediaFileTimelineSlider.Value = 0;
                    sliderTimer.Stop();
                    break;

                //case MediaElementState.Buffering:
                //case MediaElementState.Closed:
                //case MediaElementState.Opening:
            }
        }

        private void MediaFilePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.MediaFilePlayer.Stop();
            GetNextFile();

            try
            {
                displayRequest.RequestRelease();
            }
            catch //(System.ArithmeticException exception)
            {
                // This exception is thrown if the request release was executed with no request active.
            }
        }

        private void MediaFilePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            displayRequest.RequestActive();

            double absoluteValue = (int)Math.Round(this.MediaFilePlayer.NaturalDuration.TimeSpan.TotalSeconds, MidpointRounding.AwayFromZero);
            this.MediaFileTimelineSlider.Maximum = absoluteValue;
            this.MediaFileTimelineSlider.StepFrequency = SliderFrequency(this.MediaFilePlayer.NaturalDuration.TimeSpan);
            SetupTimer();
        }

        private void MediaFilePlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            string HResult = GetHresultFromErrorMessage(e);
        }

        private void MediaFilePlayer_Loaded(object sender, RoutedEventArgs e)
        {
            GetNextFile();
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (Windows.UI.ViewManagement.ApplicationView.Value == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
                menuPopup.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - bottomAppBar.ActualHeight - ((Button)sender).ActualHeight - 170;   // 85
            else
                menuPopup.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - bottomAppBar.ActualHeight - ((Button)sender).ActualHeight - 265;   // 180

            menuPopup.HorizontalOffset = Window.Current.CoreWindow.Bounds.Right - ((Button)sender).ActualWidth - 10;

            menuPopup.IsOpen = true;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.MediaFilePlayer.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.MediaFilePlayer.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.MediaFilePlayer.Stop();
            ((ViewModels.MediaPlayerViewModel)this.DataContext).ResetSubtitles();
        }
        #endregion

        #region Fade transitions
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FadeInTransitionControls();
        }

        private void transportControlsTimer_Tick(object sender, object e)
        {
            // Stops the Fade in transition (allowing it to be ready for next time).
            FadeInStoryboard.Stop();

            // Hides all the Transport controls.
            if (FadeOutStoryboard.GetCurrentState() == Windows.UI.Xaml.Media.Animation.ClockState.Stopped)
                FadeOutStoryboard.Begin();

            // Stops the 5 second timer.
            transportControlsTimer.Stop();

            this.PlayButton.IsEnabled = false;
            this.PauseButton.IsEnabled = false;
            this.MediaFileTimelineSlider.IsEnabled = false;
            this.backButton.IsEnabled = false;
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FadeInTransitionControls();
        }

        private void FadeInTransitionControls()
        {
            // Stops the Fade out transition (allowing it to be ready for next time).
            FadeOutStoryboard.Stop();

            // Shows all the Transport controls.
            if (FadeInStoryboard.GetCurrentState() == Windows.UI.Xaml.Media.Animation.ClockState.Stopped)
                FadeInStoryboard.Begin();

            // Start 5 second timer.
            transportControlsTimer.Start();

            this.PlayButton.IsEnabled = true;
            this.PauseButton.IsEnabled = true;
            this.MediaFileTimelineSlider.IsEnabled = true;
            this.backButton.IsEnabled = true;
        }
        #endregion

        #region Slider
        private double SliderFrequency(TimeSpan timevalue)
        {
            double stepFrequency = -1;

            double absoluteValue = (int)Math.Round(timevalue.TotalSeconds, MidpointRounding.AwayFromZero);

            stepFrequency = (int)(Math.Round(absoluteValue / 100));

            if (timevalue.TotalMinutes >= 10 && timevalue.TotalMinutes < 30)
            {
                stepFrequency = 10;
            }
            else if (timevalue.TotalMinutes >= 30 && timevalue.TotalMinutes < 60)
            {
                stepFrequency = 30;
            }
            else if (timevalue.TotalHours >= 1)
            {
                stepFrequency = 60;
            }

            if (stepFrequency == 0) stepFrequency += 1;

            if (stepFrequency == 1)
            {
                stepFrequency = absoluteValue / 100;
            }

            return stepFrequency;
        }

        private void SetupTimer()
        {
            sliderTimer = new DispatcherTimer();
            sliderTimer.Interval = TimeSpan.FromSeconds(this.MediaFileTimelineSlider.StepFrequency);

            subtitlesTimer = new DispatcherTimer();
            subtitlesTimer.Interval = TimeSpan.FromMilliseconds(500);
            StartTimer();
        }

        private void StartTimer()
        {
            sliderTimer.Tick += dispatcherTimer_Tick;
            sliderTimer.Start();

            subtitlesTimer.Tick += subtitlesTimer_Tick;
            subtitlesTimer.Start();
        }

        void subtitlesTimer_Tick(object sender, object e)
        {
            ((ViewModels.MediaPlayerViewModel)this.DataContext).UpdateSubtitles(this.MediaFilePlayer.Position.TotalSeconds);
        }

        //private void StopTimer()
        //{
        //    dispatcherTimer.Stop();
        //    dispatcherTimer.Tick -= dispatcherTimer_Tick;
        //}

        private void dispatcherTimer_Tick(object sender, object e)
        {
            if (!sliderPressed)
                this.MediaFileTimelineSlider.Value = this.MediaFilePlayer.Position.TotalSeconds;
        }

        private void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            this.MediaFileTimelineSlider.ValueChanged += timelineSlider_ValueChanged;

            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            this.MediaFileTimelineSlider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            this.MediaFileTimelineSlider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        private void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            sliderPressed = true;
        }

        private void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            this.MediaFilePlayer.Position = TimeSpan.FromSeconds(this.MediaFileTimelineSlider.Value);
            sliderPressed = false;
            ((ViewModels.MediaPlayerViewModel)this.DataContext).ResetSubtitles();
        }

        private void timelineSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!sliderPressed)
                this.MediaFilePlayer.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private string GetHresultFromErrorMessage(ExceptionRoutedEventArgs e)
        {
            String HResult = String.Empty;
            String token = "HRESULT - ";
            const int hrLength = 10;     // eg "0xFFFFFFFF"

            int tokenPos = e.ErrorMessage.IndexOf(token, StringComparison.Ordinal);
            if (tokenPos != -1)
            {
                HResult = e.ErrorMessage.Substring(tokenPos + token.Length, hrLength);
            }

            return HResult;
        }
        #endregion
    }
}
