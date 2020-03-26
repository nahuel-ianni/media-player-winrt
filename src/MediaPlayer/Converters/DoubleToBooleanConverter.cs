using System;
using Windows.UI.Xaml;

namespace MediaPlayer.Converters
{
    public class DoubleToBooleanConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        /// <summary>
        /// Takes a Double value and returns it as a Boolean option if the value is 0.0.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentNullException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(Double).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type Boolean.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            double castedValue = (double)value;

            return castedValue == 1.0;
        }

        /// <summary>
        /// Takes a Boolean value and returns it as a Double.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentNullException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(Boolean).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type Boolean.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            bool castedValue = (bool)value;

            return castedValue ? 1.0 : 0.0;
        }
    }
}
