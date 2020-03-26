using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MediaPlayer.UserControls
{
    public sealed partial class MediaFileTemplateZoomedOutUserControl : UserControl
    {
        public MediaFileTemplateZoomedOutUserControl()
        {
            this.InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Image thumbnailImage = ((Border)((((Windows.UI.Xaml.Controls.Panel)(sender)).Children).ToArray()[0])).Child as Image;
            Media.File mediaFile = thumbnailImage.DataContext as Media.File;
            Helpers.StorageFileHelper.ThumbnailRetrievalMode thumbnailRetrievalMode = Helpers.StorageFileHelper.ThumbnailRetrievalMode.Static;

            string extension = mediaFile.Extension.ToLower();
            if (extension.StartsWith("."))
                extension = extension.Remove(0, 1);

            if (!Helpers.MediaFormats.Music.Contains(extension))
                thumbnailRetrievalMode = Helpers.StorageFileHelper.ThumbnailRetrievalMode.Dynamic;

            thumbnailImage.Source = await Helpers.StorageFileHelper.GetFileThumbnail(mediaFile, thumbnailRetrievalMode);
        }
    }
}
