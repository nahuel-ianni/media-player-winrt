using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Helpers
{
    public class MediaFileStream
    {
        #region Properties
        public Windows.Storage.Streams.IRandomAccessStream Stream { get; private set; }
        public string MimeType { get; private set; }
        #endregion

        #region Initializers
        public MediaFileStream(Windows.Storage.Streams.IRandomAccessStream stream, string mimeType)
        {
            Stream = stream;
            MimeType = mimeType;
        }
        #endregion
    }
}
