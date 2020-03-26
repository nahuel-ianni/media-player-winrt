using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaPlayer.Converters
{
    public class ExtensionToBitmapImageConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(String).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type String.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            Windows.Storage.FileProperties.ThumbnailMode thumbnailMode = GetFileThumbnailMode(value.ToString());

            switch (thumbnailMode)
            {
                case Windows.Storage.FileProperties.ThumbnailMode.VideosView:
                    return new BitmapImage(new Uri("ms-appx:///Assets/StaticThumbnails/Video.png", UriKind.Absolute));

                case Windows.Storage.FileProperties.ThumbnailMode.MusicView:
                    return new BitmapImage(new Uri("ms-appx:///Assets/StaticThumbnails/Music.png", UriKind.Absolute));

                default:
                    return new BitmapImage(new Uri("ms-appx:///Assets/StaticThumbnails/Unrecognized.png", UriKind.Absolute));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Windows.UI.Xaml.DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Checks what type of Thumbnail mode a file has according to it's extension.
        /// </summary>
        /// <param name="fileExtension"> Extension of the file. </param>
        /// <returns> ThumbnailMode stating the type of file. </returns>
        private Windows.Storage.FileProperties.ThumbnailMode GetFileThumbnailMode(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
                //throw new ArgumentException();
                return Windows.Storage.FileProperties.ThumbnailMode.DocumentsView;

            fileExtension = new StringToUpperCaseConverter().Convert(fileExtension, null, null, null).ToString();
            fileExtension = fileExtension.ToLower();

            if (MediaPlayer.Helpers.MediaFormats.Video.Contains(fileExtension))
            {
                return Windows.Storage.FileProperties.ThumbnailMode.VideosView;
            }
            else if (MediaPlayer.Helpers.MediaFormats.Music.Contains(fileExtension))
            {
                return Windows.Storage.FileProperties.ThumbnailMode.MusicView;
            }
            else
                return Windows.Storage.FileProperties.ThumbnailMode.DocumentsView;
        }
    }
}
