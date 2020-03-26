using System.Linq;

namespace Media
{
    public class Manager
    {
        #region Declarations
        /// <summary>
        /// Name format of the new object if duplicates were found.
        /// 
        /// {0} = Name of the object. {1} = Iteration number.
        /// </summary>
        private const string IteratedNameFormat = "{0} ({1})";

        /// <summary>
        /// Pattern to look for when checking if there are duplicated names.
        /// 
        /// {0} = Name of the object. {1} = Iteration number.
        /// </summary>
        private const string IteratedNamePattern = @"{0} \(\{1}\)"; //@"{0}|{0} \(\{1}\)";

        /// <summary>
        /// Key to be replaced inside of a string.
        /// 
        /// Ex: ^\d{1,<int.MaxValue>}$
        /// </summary>
        private const string IteratedNameMaxValueKeyPattern = "<int.MaxValue>";

        /// <summary>
        /// Pattern to check for numbers between 1 and int.MaxValue.
        /// 
        /// <int.MaxValue> = Maximum number of iterations. Ex: ^\d{1,10}$
        /// The <int.MaxValue> key is used instead of {0} because string.Format throws an exception for not finding a correct index.
        /// </summary>
        private const string IteratedNameNumberPattern = "d{1,<int.MaxValue>}";
        #endregion

        #region Properties
        /// <summary>
        /// Selected File.
        /// </summary>
        public File CurrentFile { get; set; }

        /// <summary>
        /// Selected group.
        /// </summary>
        public Group CurrentGroup { get; set; }

        /// <summary>
        /// Collection with all the media groups created by the user.
        /// </summary>
        public System.Collections.Generic.List<Group> Groups { get; set; }
        #endregion

        #region Initializers
        /// <summary>
        /// Initializes an empty Media Manager.
        /// </summary>
        public Manager() : this(new System.Collections.Generic.List<Group>()) { }

        /// <summary>
        /// Initializes the Media Manager.
        /// </summary>
        /// <param name="groups"> List with all the groups created by the user. </param>
        public Manager(System.Collections.Generic.List<Group> groups)
        {
            Groups = groups;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a new File to the specified group.
        /// </summary>
        /// <param name="groupName"> Group to be updated. </param>
        /// <param name="filePath"> Path of the file to be added. </param>
        public void AddFile(string groupName, string filePath)
        {
            Group group = Groups.FirstOrDefault(mg => mg.Name == groupName);

            if (group == null)
                throw new System.Collections.Generic.KeyNotFoundException(groupName);
            else
                group.Files.Add(new File(filePath));
        }

        /// <summary>
        /// Changed the name of an existing group.
        /// 
        /// Note: If a group with the same name already exists, an iteration number is added at the end.
        /// </summary>
        /// <param name="oldGroupName"> Name of the group to be changed. </param>
        /// <param name="newGroupName"> Desired new name of the group. </param>
        public void ChangeGroupName(string oldGroupName, string newGroupName)
        {
            Group group = Groups.FirstOrDefault(mg => mg.Name == oldGroupName);

            if (group == null)
                throw new System.Collections.Generic.KeyNotFoundException(oldGroupName);
            else
            {
                System.Collections.Generic.ICollection<string> groupNames = GetGroupsNames();

                if (groupNames.Contains(newGroupName))
                    group.Name = CreateIteratedGroupName(newGroupName, groupNames);
                else
                    group.Name = newGroupName;
            }
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <param name="groupName"> The desired name of the new group. </param>
        /// <returns> The iterated name of the new group. </returns>
        public string CreateGroup(string groupName)
        {
            if (Groups.FirstOrDefault(mg => mg.Name == groupName) != null)
                groupName = CreateIteratedGroupName(groupName, GetGroupsNames());

            Groups.Add(new Group(groupName, new System.Collections.Generic.List<File>()));

            return groupName;
        }

        /// <summary>
        /// Gets a list with the names of all the groups.
        /// </summary>
        /// <returns> Names of all the groups. </returns>
        public System.Collections.Generic.ICollection<string> GetGroupsNames()
        {
            System.Collections.Generic.List<string> groupNames = new System.Collections.Generic.List<string>();

            foreach (Group group in Groups)
                groupNames.Add(group.Name);

            groupNames.Sort();

            return groupNames;
        }

        /// <summary>
        /// Gets a list of all the files that contain in their name the filter value.
        /// </summary>
        /// <param name="name"> A substring that will be looked for in the file name property. </param>
        /// <returns> Files that contain in their name the specified filter value. </returns>
        public System.Collections.Generic.ICollection<File> GetFilteredFiles(string name)
        {
            System.Collections.Generic.List<File> files = new System.Collections.Generic.List<File>();
            name = name.ToLower();

            foreach (Group group in Groups)
                foreach (File file in group.Files)
                    if (file.Name.ToLower().Contains(name))
                        files.Add(file);

            return files;
        }

        /// <summary>
        /// Removes the specified group from the collection.
        /// </summary>
        /// <param name="groupName"> Name of the group to be removed. </param>
        public void RemoveGroup(string groupName)
        {
            Group group = Groups.FirstOrDefault(mg => mg.Name == groupName);

            if (group == null)
                throw new System.Collections.Generic.KeyNotFoundException(groupName);
            else
                Groups.Remove(group);
        }

        /// <summary>
        /// Removes a file from the specified group.
        /// </summary>
        /// <param name="groupName"> Group from which the file needs to be removed. </param>
        /// <param name="filePath"> File to be removed. </param>
        public void RemoveFile(string groupName, string filePath)
        {
            Group group = Groups.FirstOrDefault(mg => mg.Name == groupName);

            if (group == null)
                throw new System.Collections.Generic.KeyNotFoundException(groupName);
            else
            {
                File file = group.Files.FirstOrDefault(mf => mf.Path == filePath);

                if (file == null)
                    throw new System.Collections.Generic.KeyNotFoundException(filePath);
                else
                    group.Files.Remove(file);
            }
        }

        /// <summary>
        /// Serializes the Group collection.
        /// </summary>
        /// <returns> Xml with the Group collection information. </returns>
        public string SerializeData()
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(Groups.GetType());

            using (System.IO.StringWriter writer = new System.IO.StringWriter())
            {
                serializer.Serialize(writer, Groups);
                return writer.ToString();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks for name duplicates and returns the name with an iteration number if it finds any.
        /// </summary>
        /// <param name="desiredName"> Desired name. </param>
        /// <param name="groupNames"> A list with all the existing names. </param>
        /// <returns> Desired name with an iteration number (if it finds duplicates). </returns>
        private string CreateIteratedGroupName(string desiredName, System.Collections.Generic.ICollection<string> groupNames)
        {
            int nameCounter = 1;    // 0;

            string regexPattern = IteratedNameNumberPattern.Replace(IteratedNameMaxValueKeyPattern, int.MaxValue.ToString());
            string iteratedNamePattern = string.Format(IteratedNamePattern, desiredName, regexPattern);

            foreach (string name in groupNames)
                if (System.Text.RegularExpressions.Regex.IsMatch(name, iteratedNamePattern) == true)
                {
                    nameCounter++;

                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(name, @"\(\d+\)");

                    if (match.Success)
                    {
                        int index;
                        int.TryParse(name.Substring(name.IndexOf("(") + 1, name.IndexOf(")") - 1 - name.IndexOf("(")), out index);

                        if (index == nameCounter)
                        {
                            nameCounter--;
                            break;
                        }
                    }
                }

            if (nameCounter > 0)
                return string.Format(IteratedNameFormat, desiredName, nameCounter);
            else
                return desiredName;
        }
        #endregion
    }
}