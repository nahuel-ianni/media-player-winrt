using System.Linq;

namespace MediaPlayer.Helpers
{
    /// <summary>
    /// Helper used to take only the desired number of Media.Files on a Media.Group.
    /// </summary>
    public class CustomMediaGroup
    {
        #region Declarations
        private int amountOfFilesDesired;
        #endregion

        #region Properties
        public System.Collections.Generic.IEnumerable<Media.File> Files { get { return Group.Files.Take(amountOfFilesDesired); } }

        public Media.Group Group { get; internal set; }

        public int TotalFiles { get { return Group.Files.Count; } }
        #endregion

        #region Initializer
        public CustomMediaGroup(Media.Group group, int amountOfFilesDesired)
        {
            this.Group = group;
            this.amountOfFilesDesired = amountOfFilesDesired;
        }
        #endregion
    }
}
