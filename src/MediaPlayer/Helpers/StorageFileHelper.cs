using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaPlayer.Helpers
{
    public static class StorageFileHelper
    {
        private static Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader(@"ErrorResources");

        #region Properties
        public enum ThumbnailRetrievalMode
        {
            /// <summary>
            /// Used when the wanted thumbnail image is a static image and not the real thumbnail.
            /// </summary>
            Static,

            /// <summary>
            /// Used when the wanted thumbnail image is the real file thumbnail.
            /// </summary>
            Dynamic
        }
        #endregion

        #region Public Methods
        public static async Task<bool> FileExists(Windows.Storage.StorageFolder folder, string filePath)
        {
            if (filePath.StartsWith(folder.Name + folder.DisplayType))
                filePath = filePath.Remove(0, folder.DisplayName.Length + folder.DisplayType.Length + "\\".Length);

            try
            {
                await folder.GetFileAsync(filePath);
                return true;
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }
        }

        public static async Task<bool> FileExists(string filePath)
        {
            return await FileExists(GetStorageFolder(filePath), filePath);
        }

        /// <summary>
        /// Gets the thumbnail image to be used as the file previews.
        /// </summary>
        /// <param name="mediaFile"> File for which to look for the thumbnail. </param>
        /// <param name="thumbnailRetrievalMode"> ThumbnailRetrievalMode indicating if the bitmap is a static image or a thumbnail. </param>
        /// <returns> Bitmap of the file thumbnail preview. </returns>
        public static async Task<BitmapImage> GetFileThumbnail(Media.File mediaFile, ThumbnailRetrievalMode thumbnailRetrievalMode)
        {
            switch (thumbnailRetrievalMode)
            {
                default:
                case ThumbnailRetrievalMode.Static:
                    return GetFileStaticThumbnail(GetFileThumbnailMode(mediaFile.Extension));

                case ThumbnailRetrievalMode.Dynamic:
                    return await GetFileDynamicThumbnail(mediaFile);
            }
        }

        public static async Task<System.Collections.Generic.List<Windows.Storage.StorageFile>> GetFolderFiles(string folderName)
        {
            Windows.Storage.StorageFolder folder = GetStorageFolder(folderName);

            System.Collections.Generic.IReadOnlyList<Windows.Storage.StorageFile> files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
            System.Collections.Generic.List<Windows.Storage.StorageFile> returnFiles = new System.Collections.Generic.List<Windows.Storage.StorageFile>();

            foreach (Windows.Storage.StorageFile file in files)
            {
                string fileExtension = System.IO.Path.GetExtension(file.Path);
                if (fileExtension.StartsWith("."))
                    fileExtension = fileExtension.Remove(0, 1);

                if (MediaFormats.Video.Contains(fileExtension) ||
                    MediaFormats.Music.Contains(fileExtension))
                {
                    returnFiles.Add(file);
                }
            }

            return returnFiles;
        }

        public static async Task<MediaFileStream> GetMediaFileStream(Media.File mediaFile)
        {
            Windows.Storage.StorageFile storageFile = await GetStorageFile(mediaFile);

            var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);

            return new MediaFileStream(stream, storageFile.ContentType);
        }

        public static async Task<Windows.Storage.StorageFile> GetStorageFile(Windows.Storage.StorageFolder folder, string fileName)
        {
            if (await FileExists(folder, fileName))
                return await folder.GetFileAsync(fileName);
            else
                return await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
        }

        public static async Task<Windows.Storage.StorageFile> GetStorageFile(Media.File mediaFile)
        {
            string directoryRoot = mediaFile.DirectoryName.Contains("\\") ? mediaFile.DirectoryName.Substring(0, mediaFile.DirectoryName.IndexOf("\\")) : mediaFile.DirectoryName;

            Windows.Storage.StorageFolder storageFolder = GetStorageFolder(directoryRoot);

            string filePath = mediaFile.Path.Replace(directoryRoot, string.Empty);
            if (filePath.StartsWith("\\"))
                filePath = filePath.Remove(0, 1);

            return await storageFolder.GetFileAsync(filePath);
        }

        public static async Task<string> GetKnownFolderPath(string filePath)
        {
            string folderName = string.Empty;
            string folderDisplayName = string.Empty;
            Windows.Storage.StorageFolder folder = null;

            if (filePath.Contains(Windows.Storage.KnownFolders.VideosLibrary.DisplayName))
            {
                folderName = Windows.Storage.KnownFolders.VideosLibrary.Name + Windows.Storage.KnownFolders.VideosLibrary.DisplayType;
                folderDisplayName = Windows.Storage.KnownFolders.VideosLibrary.DisplayName;
                folder = Windows.Storage.KnownFolders.VideosLibrary;
            }

            else if (filePath.Contains(Windows.Storage.KnownFolders.MusicLibrary.DisplayName))
            {
                folderName = Windows.Storage.KnownFolders.MusicLibrary.Name + Windows.Storage.KnownFolders.MusicLibrary.DisplayType;
                folderDisplayName = Windows.Storage.KnownFolders.MusicLibrary.DisplayName;
                folder = Windows.Storage.KnownFolders.MusicLibrary;
            }

            else
                throw new NotSupportedException(resourceLoader.GetString("DirectoryNotSupported"));

            int index = filePath.IndexOf(folderDisplayName);
            string knownFolderPath = filePath.Substring(index, filePath.Length - index).Replace(folderDisplayName, folderName);

            if (!await FileExists(folder, knownFolderPath))
                throw new NotSupportedException(resourceLoader.GetString("DirectoryNotSupported"));

            return knownFolderPath;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the thumbnail image to be used as the file previews.
        /// </summary>
        /// <param name="thumbnailMode"> Type of file. </param>
        /// <returns> Media.File thumbnail preview. </returns>
        private static BitmapImage GetFileStaticThumbnail(Windows.Storage.FileProperties.ThumbnailMode thumbnailMode)
        {
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

        private static async Task<BitmapImage> GetFileDynamicThumbnail(Media.File mediaFile)
        {
            try
            {
                Windows.Storage.StorageFile storageFile = await GetStorageFile(mediaFile);
                Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                bitmapImage.SetSource(await storageFile.GetThumbnailAsync(GetFileThumbnailMode(mediaFile.Extension)));

                return bitmapImage;
            }
            catch
            {
                return GetFileStaticThumbnail(Windows.Storage.FileProperties.ThumbnailMode.SingleItem);
            }
        }

        /// <summary>
        /// Checks what type of Thumbnail mode a file has according to it's extension.
        /// </summary>
        /// <param name="fileExtension"> Extension of the file. </param>
        /// <returns> ThumbnailMode stating the type of file. </returns>
        private static Windows.Storage.FileProperties.ThumbnailMode GetFileThumbnailMode(string fileExtension)
        {
            fileExtension = new Converters.StringToUpperCaseConverter().Convert(fileExtension, null, null, null).ToString();
            fileExtension = fileExtension.ToLower();

            if (MediaPlayer.Helpers.MediaFormats.Video.Contains(fileExtension))
                return Windows.Storage.FileProperties.ThumbnailMode.VideosView;

            else if (MediaPlayer.Helpers.MediaFormats.Music.Contains(fileExtension))
                return Windows.Storage.FileProperties.ThumbnailMode.MusicView;

            else
                throw new NotSupportedException(resourceLoader.GetString("FileExtensionNotRecognized"));
        }

        private static Windows.Storage.StorageFolder GetStorageFolder(string directoryRoot)
        {
            if (directoryRoot == Windows.Storage.Pickers.PickerLocationId.VideosLibrary.ToString())
                return Windows.Storage.KnownFolders.VideosLibrary;
            else if (directoryRoot == Windows.Storage.Pickers.PickerLocationId.MusicLibrary.ToString())
                return Windows.Storage.KnownFolders.MusicLibrary;
            else
                throw new NotSupportedException(resourceLoader.GetString("DirectoryNotSupported"));
        }
        #endregion
    }
}
