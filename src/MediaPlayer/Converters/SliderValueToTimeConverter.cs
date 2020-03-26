using System;

namespace MediaPlayer.Converters
{
    public class SliderValueToTimeConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        /// <summary>
        /// Takes a double value and returns it as a string representing time (mm:ss).
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentNullException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if (!typeof(Double).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type Double.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            double castedValue = 0;
            double.TryParse(value.ToString(), out castedValue);

            //return String.Format("{0}:{1}", (int)(castedValue / 60), (int)(castedValue % 60));

            return (TimeSpan.FromSeconds(castedValue)).ToString(@"hh\:mm\:ss");
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
