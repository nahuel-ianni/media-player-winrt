using System;

namespace MediaPlayer.Converters
{
    public class NameToShorterNameConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                //throw new ArgumentNullException("value", "Value cannot be null.");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            if(!typeof(String).Equals(value.GetType()))
                //throw new ArgumentException("Value must be of type String.", "value");
                return Windows.UI.Xaml.DependencyProperty.UnsetValue;

            string name = value.ToString();

            if (parameter.Equals("Small"))
            {
                if (name.Length > 15)
                    name = name.Substring(0, 11) + " ...";

                return name;
            }
            else if (parameter.Equals("Medium"))
            {
                if (name.Length > 21)
                    name = name.Substring(0, 17) + " ...";

                return name;
            }
            else if (parameter.Equals("Large"))
            {
                if (name.Length > 45)
                    name = name.Substring(0, 41) + " ...";

                return name;
            }
            else if (parameter.Equals("ExtraLarge"))
            {
                if (name.Length > 100)
                    name = name.Substring(0, 46) + " ...";

                return name + " group";
            }
            else
            {
                // No special treatment specified or detected.
                return name;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Windows.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }
}
