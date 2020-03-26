using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace MediaPlayer.Pages
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class MediaGroupsPage : MediaPlayer.Common.LayoutAwarePage
    {
        #region Declarations
        private Popup menuPopup;
        private UserControls.AddEditGroupPopupContent addEditGroupPopupContent;
        #endregion

        #region Initializer
        public MediaGroupsPage()
        {
            this.InitializeComponent();
        }
        #endregion

        #region Private Methods
        private async void AskForGroupImport()
        {
            Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader(@"Resources");

            string messageText = resourceLoader.GetString("MediaLibraryEmptyText");
            string messageTitle = resourceLoader.GetString("MediaLibraryEmptyTitle");
            string yesButtonText = resourceLoader.GetString("Yes");
            string noButtonText = resourceLoader.GetString("No");

            Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog(messageText, messageTitle);
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand(yesButtonText, new Windows.UI.Popups.UICommandInvokedHandler(ImportFiles)));
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand(noButtonText));

            await messageDialog.ShowAsync();
        }

        private async Task<List<ViewModels.MediaGroupsViewModel.MediaFileDictionary>> GetFiles(List<ViewModels.MediaGroupsViewModel.MediaFileDictionary> groupsAndFiles, Windows.Storage.Pickers.PickerLocationId folderId)
        {
            List<Windows.Storage.StorageFile> files = await Helpers.StorageFileHelper.GetFolderFiles(folderId.ToString());

            foreach (Windows.Storage.StorageFile storageFile in files)
            {
                string groupName = storageFile.Path.Remove(storageFile.Path.IndexOf("\\" + storageFile.Name));

                int index = groupName.LastIndexOf("\\");
                groupName = groupName.Substring(index, groupName.Length - index);

                if (groupName.StartsWith("\\"))
                    groupName = groupName.Remove(0, "\\".Length);

                groupsAndFiles.Add(new ViewModels.MediaGroupsViewModel.MediaFileDictionary(groupName, storageFile.Path));
            }

            return groupsAndFiles;
        }

        private async void ImportFiles(Windows.UI.Popups.IUICommand command)
        {
            SetProgressRingVisibility(Visibility.Visible);

            List<ViewModels.MediaGroupsViewModel.MediaFileDictionary> groupsAndFiles = new List<ViewModels.MediaGroupsViewModel.MediaFileDictionary>();

            groupsAndFiles = await GetFiles(groupsAndFiles, Windows.Storage.Pickers.PickerLocationId.VideosLibrary);
            groupsAndFiles = await GetFiles(groupsAndFiles, Windows.Storage.Pickers.PickerLocationId.MusicLibrary);

            ((ViewModels.MediaGroupsViewModel)this.DataContext).AddFilledGroups(groupsAndFiles);
        }

        private void NavigateToGroup(string groupName)
        {
            if (this.Frame != null)
                this.Frame.Navigate(typeof(Pages.GroupFilesPage), groupName);
        }

        private void SetProgressRingVisibility(Visibility visibility)
        {
            this.progressRingBackground.Visibility = visibility;
            this.progressRing.Visibility = visibility;

            this.progressRing.IsActive = visibility == Windows.UI.Xaml.Visibility.Visible;
        }
        #endregion

        #region Events
        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
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
            {
                string iteratedGroupName = ((MediaPlayer.ViewModels.MediaGroupsViewModel)this.DataContext).AddGroup(sender.ToString());

                if (!string.IsNullOrEmpty(iteratedGroupName))
                    NavigateToGroup(iteratedGroupName);
            }
        }

        private void GroupName_Click(object sender, RoutedEventArgs e)
        {
            NavigateToGroup(((MediaPlayer.Helpers.CustomMediaGroup)((FrameworkElement)(sender)).DataContext).Group.Name);
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
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            this.DataContext = new ViewModels.MediaGroupsViewModel();
            ((ViewModels.MediaGroupsViewModel)this.DataContext).PropertyChanged += MediaGroupsPage_PropertyChanged;
            await ((ViewModels.MediaGroupsViewModel)this.DataContext).LoadSettings();

            if (((ViewModels.MediaGroupsViewModel)this.DataContext).Groups.Count == 0) AskForGroupImport();

            menuPopup = new Popup();
            menuPopup.IsLightDismissEnabled = true;
            addEditGroupPopupContent = new UserControls.AddEditGroupPopupContent(bottomAppBar.Background);
            addEditGroupPopupContent.NameAccepted += addEditGroupPopupContent_NameAccepted;
        }

        private void RemoveMediaButton_Click(object sender, RoutedEventArgs e)
        {
            menuPopup.Child = new UserControls.RemoveGroupPopupContent(bottomAppBar.Background, this.DataContext, true);

            menuPopup.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - bottomAppBar.ActualHeight - ((Button)sender).ActualHeight + 30;
            menuPopup.HorizontalOffset = Window.Current.CoreWindow.Bounds.Left + ((Button)sender).ActualWidth + 4;

            menuPopup.IsOpen = true;
        }

        private void MediaGroup_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.Frame != null)
                this.Frame.Navigate(typeof(Pages.MediaPlayerPage), new List<Media.File>() { (Media.File)e.ClickedItem });
        }

        private async void MediaGroupsPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Groups"))
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

                if (this.progressRing.IsActive)
                    SetProgressRingVisibility(Visibility.Collapsed);

                menuPopup.IsOpen = false;
            }
        }

        private void SortGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            menuPopup.Child = new UserControls.SortGroupsPopupContent(bottomAppBar.Background, this.DataContext);

            menuPopup.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - bottomAppBar.ActualHeight - ((Button)sender).ActualHeight - 20;
            menuPopup.HorizontalOffset = Window.Current.CoreWindow.Bounds.Right - ((Button)sender).ActualWidth - 25;

            menuPopup.IsOpen = true;
        }
        #endregion
    }
}
