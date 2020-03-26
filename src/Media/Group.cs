namespace Media
{
    public class Group
    {
        #region Properties
        /// <summary>
        /// Collection of files the group contains.
        /// </summary>
        public System.Collections.Generic.List<File> Files { get; internal set; }

        /// <summary>
        /// Name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Last date and time the group had a file reproduced.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public System.Nullable<System.DateTime> LastTimeReproduced { get; internal set; }
        #endregion

        #region Initializers
        /// <summary>
        /// Creates a new instance of a media group.
        /// </summary>
        internal Group() : this(string.Empty, new System.Collections.Generic.List<string>()) { }

        /// <summary>
        /// Creates a new instance of a media group.
        /// </summary>
        /// <param name="name"> Name of the group. </param>
        /// <param name="files"> Collection of files the group will contain. </param>
        internal Group(string name, System.Collections.Generic.List<File> files)
        {
            Name = name;
            Files = files;
        }

        /// <summary>
        /// Creates a new instance of a media group.
        /// </summary>
        /// <param name="name"> Name of the group. </param>
        /// <param name="filesPath"> Paths of the files the group will contain. </param>
        internal Group(string name, System.Collections.Generic.ICollection<string> filesPath)
        {
            Name = name;

            System.Collections.Generic.List<File> files = new System.Collections.Generic.List<File>();
            foreach (string path in filesPath)
                files.Add(new File(path));

            Files = files;
        }
        #endregion
    }
}
