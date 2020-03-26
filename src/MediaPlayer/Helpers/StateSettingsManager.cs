using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Helpers
{
    public static class StateSettingsManager
    {
        #region Declarations
        private static Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader(@"ErrorResources");

        private static Windows.Storage.StorageFile settingsFile;
        #endregion

        #region Properties
        public const string LocalFileName = "Settings.xml";

        public static Windows.Storage.StorageFolder LocalFolder
        {
            get { return Windows.Storage.ApplicationData.Current.LocalFolder; }
        }

        public static async Task<Windows.Storage.StorageFile> GetSettingsFile()
        {
            if (settingsFile == null)
                settingsFile = await StorageFileHelper.GetStorageFile(LocalFolder, LocalFileName);

            return settingsFile;
        }
        #endregion

        public static T DeserializeFromString<T>(string xml)
        {
            System.Xml.Serialization.XmlSerializer deserializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (System.IO.StringReader reader = new System.IO.StringReader(xml))
            {
                return (T)deserializer.Deserialize(reader);
            }
        }

        public async static Task<List<Media.Group>> LoadSettings()
        {
            List<Media.Group> mediaGroups = null;
            Windows.Storage.StorageFile storageFile = await GetSettingsFile();

            string serializedContent = await Windows.Storage.FileIO.ReadTextAsync(storageFile);

            if (!String.IsNullOrEmpty(serializedContent))
                mediaGroups = DeserializeFromString<List<Media.Group>>(serializedContent);

            return mediaGroups == null ? new List<Media.Group>() : mediaGroups;
        }

        public async static Task<Windows.Storage.StorageFile> SaveSettings()
        {
            Media.Manager mediaManager = (Media.Manager)App.Current.Resources["mediaManager"];

            if (mediaManager == null)
                throw new InvalidOperationException(resourceLoader.GetString("NoMediaManagerFound"));

            string content = mediaManager.SerializeData();

            Windows.Storage.StorageFile storageFile = await GetSettingsFile();

            await Windows.Storage.FileIO.WriteTextAsync(storageFile, content);

            return storageFile;
        }
    }
}
