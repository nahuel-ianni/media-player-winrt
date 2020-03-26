using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace MediaPlayer.Pages
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class GroupFilesPage : MediaPlayer.Common.LayoutAwarePage
    {
        #region Declarations
        private Popup menuPopup;
        private UserControls.AddEditGroupPopupContent addEditGroupPopupContent;

        private FileOpenPicker filePicker;
        #endregion

        public GroupFilesPage()
        {
            this.InitializeComponent();
        }

        #region Events
        private async void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModels.GroupFilesViewModel groupFilesViewModel = ((ViewModels.GroupFilesViewModel)this.DataContext);
            List<string> unsupportedFiles = new List<string>();

            foreach (var file in await filePicker.PickMultipleFilesAsync())
            {
                try
                {
                    groupFilesViewModel.AddFile(await Helpers.StorageFileHelper.GetKnownFolderPath(file.Path));
                }
                catch (NotSupportedException)
                {
                    unsupportedFiles.Add(file.Path);
                }
                catch (Exception)
                {
                    unsupportedFiles.Add(file.Path);
                }
            }

            if (unsupportedFiles.Count > 0)
                ShowUnsupportedFiles(unsupportedFiles);
        }

        private void GroupFilesPage_OnGroupRemoved(object sender, EventArgs e)
        {
            menuPopup.IsOpen = false;

            GoBack(this, null);
        }

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
            this.DataContext = new ViewModels.GroupFilesViewModel(navigationParameter.ToString());
            ((ViewModels.GroupFilesViewModel)this.DataContext).OnGroupRemoved += GroupFilesPage_OnGroupRemoved;

            menuPopup = new Windows.UI.Xaml.Controls.Primitives.Popup();
            menuPopup.IsLightDismissEnabled = true;

            addEditGroupPopupContent = new UserControls.AddEditGroupPopupContent(bottomAppBar.Background, ((ViewModels.GroupFilesViewModel)this.DataContext).CurrentGroup.Name);
            addEditGroupPopupContent.NameAccepted += addEditGroupPopupContent_NameAccepted;

            filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            foreach (string format in Helpers.MediaFormats.Video)
                filePicker.FileTypeFilter.Add("." + format);

            foreach (string format in Helpers.MediaFormats.Music)
                filePicker.FileTypeFilter.Add("." + format);
        }

        protected override async void SaveState(Dictionary<string, object> pageState)
        {
            string exceptionMessage = string.Empty;

            try
            {
                await Helpers.StateSettingsManager.SaveSettings();
            }
            catch (InvalidOperationException exception)
            {
                exceptionMessage = exception.Message;
            }
            catch (UnauthorizedAccessException) { /* Thrown when the file where things are getting saved is being used. */ }

            if (!string.IsNullOrEmpty(exceptionMessage))
                await new Windows.UI.Popups.MessageDialog(exceptionMessage).ShowAsync();
        }

        private void RemoveMediaButton_Click(object sender, RoutedEventArgs e)
        {
            menuPopup.Child = new UserControls.RemoveGroupPopupContent(bottomAppBar.Background, this.DataContext);

            menuPopup.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - bottomAppBar.ActualHeight - ((Button)sender).ActualHeight - 20;
            menuPopup.HorizontalOffset = Window.Current.CoreWindow.Bounds.Left + ((Button)sender).ActualWidth * 2 + 4;

            menuPopup.IsOpen = true;
        }

        private void SelectAllFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.itemGridView.Visibility == Windows.UI.Xaml.Visibility.Visible)
                this.itemGridView.SelectAll();

            if (this.itemListView.Visibility == Windows.UI.Xaml.Visibility.Visible)
                this.itemListView.SelectAll();
        }

        private void mediaFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ICollection<Media.File> selectedFiles = ((ViewModels.GroupFilesViewModel)this.DataContext).SelectedFiles;
            selectedFiles.Clear();

            foreach (Media.File mediaFile in ((Windows.UI.Xaml.Controls.ListViewBase)(sender)).SelectedItems)
                selectedFiles.Add(mediaFile);
        }

        private void PlayFileButton_Click(object sender, RoutedEventArgs e)
        {
            ICollection<Media.File> selectedFiles = ((ViewModels.GroupFilesViewModel)this.DataContext).SelectedFiles;

            if (this.Frame != null)
            {
                if (selectedFiles.Count > 0)
                    this.Frame.Navigate(typeof(Pages.MediaPlayerPage), selectedFiles);
                else
                    this.Frame.Navigate(typeof(Pages.MediaPlayerPage), ((ViewModels.GroupFilesViewModel)this.DataContext).CurrentGroup.Files);
            }
        }

        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            menuPopup.Child = addEditGroupPopupContent;

            menuPopup.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - bottomAppBar.ActualHeight - ((Button)sender).ActualHeight + 30;
            menuPopup.HorizontalOffset = Window.Current.CoreWindow.Bounds.Left + 4;

            menuPopup.IsOpen = true;
        }

        public void addEditGroupPopupContent_NameAccepted(object sender, EventArgs e)
        {
            menuPopup.IsOpen = false;

            if (sender is string)
                ((MediaPlayer.ViewModels.GroupFilesViewModel)this.DataContext).RenameGroup(sender.ToString());
        }
        #endregion

        #region Private Methods
        private async void ShowUnsupportedFiles(List<string> unsupportedFiles)
        {
            Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader(@"Resources");

            int index = 0;
            string messageFormat = resourceLoader.GetString("UnsupportedFilesSelectedMessage");
            string filesPathFormat = "{0}\n\n{1}";
            string filesPath = string.Empty;

            foreach (string path in unsupportedFiles)
            {
                if (index >= 3)
                {
                    filesPath = string.Format(filesPathFormat, filesPath, "...");
                    break;
                }

                filesPath = string.Format(filesPathFormat, filesPath, path);
                index++;
            }

            await new Windows.UI.Popups.MessageDialog(string.Format(messageFormat, filesPath), resourceLoader.GetString("UnsupportedFilesSelectedTitle")).ShowAsync();
        }
        #endregion
    }
}
