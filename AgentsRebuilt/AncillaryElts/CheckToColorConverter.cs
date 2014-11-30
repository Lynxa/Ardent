using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AgentsRebuilt
{
    public class CheckToColorConverter : IValueConverter
    {
        public static int FieldCount = 0;
        public static int FieldDuration = 0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool tp = (bool?) value != null && (bool)value;
            return new SolidColorBrush((tp?Colors.BlueViolet:Colors.Red));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
