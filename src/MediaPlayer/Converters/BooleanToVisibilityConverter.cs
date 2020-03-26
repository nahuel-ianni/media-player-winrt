using System;
using Windows.UI.Xaml;

namespace MediaPlayer.Converters
{
    public class BooleanToVisibilityConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        /// <summary>
        /// Takes a Boolean value and returns it as a Windows.UI.Xaml.Visibility option.
        /// 
        /// Parameter value: VisibleOnTrue / NotVisibleOnTrue
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentNullException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (parameter == null)
                //throw new ArgumentNullException("parameter", "Parameter cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(Boolean).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type Boolean.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            bool castedValue = (bool)value;

            if (parameter.ToString() == "VisibleOnTrue")
                return castedValue ? Visibility.Visible : Visibility.Collapsed;
            else
                return castedValue ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Takes a Windows.UI.Xaml.Visibility value and returns it as a Boolean option.
        ///
        /// Parameter value: VisibleOnTrue / NotVisibleOnTrue
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentNullException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (parameter == null)
                //throw new ArgumentNullException("parameter", "Parameter cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(Visibility).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type Windows.UI.Xaml.Visibility.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            Windows.UI.Xaml.Visibility castedValue = (Windows.UI.Xaml.Visibility)value;

            if (parameter.ToString() == "VisibleOnTrue")
                return castedValue == Visibility.Visible;
            else
                return castedValue == Visibility.Collapsed;
        }
    }
}
