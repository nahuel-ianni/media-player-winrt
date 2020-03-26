using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.ViewModels
{
    class MediaGroupsViewModel : BaseViewModel
    {
        #region Declarations
        private System.Collections.Generic.List<Media.Group> groups = new List<Media.Group>();
        #endregion

        #region Properties
        /// <summary>
        /// Collection with all the media groups created by the user.
        /// 
        /// http://msdn.microsoft.com/library/windows/apps/hh780627.aspx
        /// </summary>
        public System.Collections.Generic.List<Media.Group> Groups
        {
            get { return groups; }
            private set 
            { 
                groups = value;
                OnPropertyChanged("Groups");
                OnPropertyChanged("GroupsAvailable");
            }
        }

        public bool GroupsAvailable { get { return Groups.Count != 0; } }

        public enum SortOption
        {
            Name,
            Files
        }
        #endregion

        #region Initializer
        public MediaGroupsViewModel() { }
        #endregion

        public async Task<System.Collections.Generic.List<Media.Group>> LoadSettings()
        {
            if (mediaManager.Groups.Count == 0)
                mediaManager.Groups = await Helpers.StateSettingsManager.LoadSettings();

            Groups = mediaManager.Groups;

            return Groups;
        }

        #region Public Methods
        public string AddGroup(string groupName)
        {
            string iteratedGroupName = string.Empty;

            if (!string.IsNullOrEmpty(groupName))
            {
                iteratedGroupName = mediaManager.CreateGroup(groupName);
                Groups = mediaManager.Groups;
            }

            return iteratedGroupName;
        }

        /// <summary>
        /// Creates a set of groups with the files it contains.
        /// </summary>
        /// <param name="groupFiles"> Dictionary containing the group name (key) and the files it contains (value). </param>
        public async void AddFilledGroups(List<MediaFileDictionary> groupFiles)
        {
            List<string> createdGroups = new List<string>();

            for (int index = 0; index < groupFiles.Count; index++)
            {
                string groupName = groupFiles.ElementAt(index).Key;

                if (!createdGroups.Contains(groupName))
                {
                    createdGroups.Add(groupName);
                    groupName = mediaManager.CreateGroup(groupName);                    
                }

                mediaManager.AddFile(groupName, await Helpers.StorageFileHelper.GetKnownFolderPath(groupFiles.ElementAt(index).Value));
            }

            Groups = mediaManager.Groups;
        }
        #endregion

        #region Commands
        private System.Windows.Input.ICommand removeAllGroupsCommand;
        public System.Windows.Input.ICommand RemoveAllGroupsCommand
        {
            get
            {
                if (removeAllGroupsCommand == null)
                    removeAllGroupsCommand = new Helpers.DelegateCommand<System.Collections.Generic.List<Media.Group>>(RemoveAllGroups);

                return removeAllGroupsCommand;
            }
        }

        private System.Windows.Input.ICommand sortGroupsCommand;
        public System.Windows.Input.ICommand SortGroupsCommand
        {
            get
            {
                if (sortGroupsCommand == null)
                    sortGroupsCommand = new Helpers.DelegateCommand<string>(SortGroups);

                return sortGroupsCommand;
            }
        }
        #endregion

        #region Private Methods
        private void RemoveAllGroups(System.Collections.Generic.List<Media.Group> groups)
        {
            for(int index = groups.Count - 1; index >= 0; index--)
                mediaManager.RemoveGroup(groups.ElementAt(index).Name);

            Groups = mediaManager.Groups;
        }

        private void SortGroups(string option)
        {
            if (option == SortOption.Name.ToString())
                Groups = Groups.OrderBy(group => group.Name).ToList();

            else if (option == SortOption.Files.ToString())
                Groups = Groups.OrderBy(group => group.Files.Count).ToList();
        }
        #endregion

        public struct MediaFileDictionary
        {
            public string Key;
            public string Value;

            public MediaFileDictionary(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}