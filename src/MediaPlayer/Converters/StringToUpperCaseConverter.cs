using System;

namespace MediaPlayer.Converters
{
    public class StringToUpperCaseConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        /// <summary>
        /// Takes a file extension (.<extension>) and returns it without the dot and in upper cases.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentNullException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(String).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type String.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            return value.ToString().ToUpper().Replace(".", string.Empty);
        }

        /// <summary>
        /// Takes a file extension (<EXTENSION>) and returns it with the dot and in lower cases.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(String).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type String.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            return string.Format(".{0}", value.ToString().ToLower());
        }
    }
}
