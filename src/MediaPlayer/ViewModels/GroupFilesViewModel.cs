using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPlayer.ViewModels
{
    public class GroupFilesViewModel : BaseViewModel
    {
        private System.Collections.ObjectModel.ObservableCollection<Media.File> files;

        #region Properties
        public Media.Group CurrentGroup { get { return mediaManager.CurrentGroup; } }

        public System.Collections.ObjectModel.ObservableCollection<Media.File> Files
        {
            get { return files; }
            set
            {
                files = value;
                OnPropertyChanged("Files");
                OnPropertyChanged("FilesAvailable");
            }
        }

        public bool FilesAvailable { get { return Files.Count != 0; } }

        public ICollection<Media.File> SelectedFiles { get; set; }

        public bool SelectedFilesAvailable { get { return SelectedFiles.Count != 0; } }

        public event EventHandler OnGroupRemoved;
        #endregion

        #region Initializer
        public GroupFilesViewModel(string groupName)
        {
            Media.Group mediaGroup = mediaManager.Groups.FirstOrDefault(group => group.Name == groupName);

            if (mediaGroup != null)
            {
                mediaManager.CurrentGroup = mediaGroup;
                SelectedFiles = new List<Media.File>();
                UpdateFiles();
                Files.CollectionChanged += Files_CollectionChanged;
            }
        }
        #endregion

        #region Commands
        private System.Windows.Input.ICommand removeGroupCommand;
        public System.Windows.Input.ICommand RemoveGroupCommand
        {
            get
            {
                if (removeGroupCommand == null)
                    removeGroupCommand = new Helpers.DelegateCommand<Media.Group>(RemoveGroup);

                return removeGroupCommand;
            }
        }

        private System.Windows.Input.ICommand removeSelectedFilesCommand;
        public System.Windows.Input.ICommand RemoveSelectedFilesCommand
        {
            get
            {
                if (removeSelectedFilesCommand == null)
                    removeSelectedFilesCommand = new Helpers.DelegateCommand<ICollection<Media.File>>(RemoveSelectedFiles);

                return removeSelectedFilesCommand;
            }
        }
        #endregion

        #region Public Methods
        public void AddFile(string filePath)
        {
            this.mediaManager.AddFile(CurrentGroup.Name, filePath);

            //OnPropertyChanged("Files");
            UpdateFiles();
        }

        public void RenameGroup(string newName)
        {
            this.mediaManager.ChangeGroupName(CurrentGroup.Name, newName);

            OnPropertyChanged("CurrentGroup");
        }
        #endregion

        #region Private Methods
        private void RemoveGroup(Media.Group mediaGroup)
        {
            this.mediaManager.RemoveGroup(mediaGroup.Name);

            OnPropertyChanged("Groups");

            if (OnGroupRemoved != null)
                OnGroupRemoved(mediaGroup.Name, null);
        }

        private void RemoveSelectedFiles(ICollection<Media.File> selectedFiles)
        {
            foreach (Media.File mediaFile in selectedFiles)
                this.mediaManager.RemoveFile(this.CurrentGroup.Name, mediaFile.Path);

            SelectedFiles.Clear();

            //OnPropertyChanged("Files");
            UpdateFiles();
        }

        private void UpdateFiles()
        {
            Files = new System.Collections.ObjectModel.ObservableCollection<Media.File>(mediaManager.CurrentGroup.Files);
            OnPropertyChanged("Files");
        }
        #endregion

        #region Events
        private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Collections.ObjectModel.ObservableCollection<Media.File> files = (System.Collections.ObjectModel.ObservableCollection<Media.File>)sender;

            foreach (Media.File mediaFile in files)
                this.mediaManager.RemoveFile(CurrentGroup.Name, mediaFile.Path);

            foreach (Media.File mediaFile in files)
                this.mediaManager.AddFile(CurrentGroup.Name, mediaFile.Path);

            UpdateFiles();
            OnPropertyChanged("FilesAvailable");
        }
        #endregion
    }
}
