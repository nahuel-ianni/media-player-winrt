namespace MediaPlayer.Helpers
{
    /// <summary>
    /// Supported video and sound formats.
    /// 
    /// http://msdn.microsoft.com/en-us/library/windows/apps/hh986969.aspx
    /// </summary>
    public static class MediaFormats
    {
        public static string[] Video
        {
            get 
            { 
                return new string[] 
                { 
                    "3g2",
                    "3gp2",
                    "3gp",
                    "3gpp",
                    "m4v",
                    "mp4v",
                    "mp4",
                    "mov",
                    "m2ts",
                    "asf",
                    "wm",
                    "wmv",
                    "adt",
                    "adts",
                    "avi",
                    "ec3",
                }; 
            }
        }

        public static string[] Music
        {
            get
            {
                return new string[] 
                { 
                    "m4a",
                    "wma",
                    "aac",
                    "mp3",
                    "wav",
                    "ac3",
                };
            }
        }

        public static string[] Subtitles
        {
            get
            {
                return new string[]
                {
                    "srt",
                };
            }
        }
    }    
}
