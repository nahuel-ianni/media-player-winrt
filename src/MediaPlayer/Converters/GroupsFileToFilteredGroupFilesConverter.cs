using MediaPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPlayer.Converters
{
    public class GroupsFileToFilteredGroupFilesConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        /// <summary>
        /// Gets a collection of Media.Group and returns a CustomMediaGroup collection with a smaller amount of Media.Files per Media.Group.
        /// </summary>
        /// <param name="value"> Media.Group collection. </param>
        /// <returns> CustomMediaGroup collection. </returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is ICollection<Media.Group>))
                //throw new ArgumentException("Value needs to be a type implementing ICollection<Media.Group>");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            ICollection<Media.Group> mediaGroups = value as ICollection<Media.Group>;
            ICollection<CustomMediaGroup> customMediaGroups = new List<CustomMediaGroup>();

            foreach (Media.Group group in mediaGroups)
                customMediaGroups.Add(new CustomMediaGroup(group, 6));

            return customMediaGroups;
        }

        /// <summary>
        /// Gets the Media.Manager from the application resources and returns it's Media.Group collection.
        /// </summary>
        /// <returns> Media.Group collection from the application Media.Manager. </returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Media.Manager manager = (Media.Manager)App.Current.Resources["mediaManager"];

            if (manager != null)
                return manager.Groups;
            else
                //throw new Exception("Applications Media.Manager could not be retrieved.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }
}
