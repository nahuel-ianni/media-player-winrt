using System;

namespace MediaPlayer.Converters
{
    public class MediaPositionToStringConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        /// <summary>
        /// Takes a double value and returns it as a string representing time (hh:mm:ss).
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentNullException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(TimeSpan).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type TimeSpan.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            return ((TimeSpan)value).ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Takes a string representing time (mm:ss) and returns a double value.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Windows.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }
}
