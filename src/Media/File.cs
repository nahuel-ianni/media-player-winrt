namespace Media
{
    public class File
    {
        #region Properties
        /// <summary>
        /// Returns the directory for the file.
        /// </summary>
        public string DirectoryName { get { return System.IO.Path.GetDirectoryName(Path); } }

        /// <summary>
        /// Extension of the file.
        /// </summary>
        public string Extension { get { return System.IO.Path.GetExtension(Path); } }

        /// <summary>
        /// Date and time the file was last reproduced.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public System.Nullable<System.DateTime> LastTimeReproduced { get; internal set; }

        /// <summary>
        /// Name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to the file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Number of times the file has been reproduced.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public int TimesReproduced { get; internal set; }
        #endregion

        #region Initializers
        private File() { }

        /// <summary>
        /// Creates a new instance of a media file.
        /// </summary>
        /// <param name="path"> Path containing the file. </param>
        internal File(string path)
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Path = path;
        }
        #endregion
    }
}
