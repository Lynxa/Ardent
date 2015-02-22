using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AgentsRebuilt
{
    public class MinusOneConverter : IValueConverter
    {
        public static int FieldCount = 0;
        public static int FieldDuration = 0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            int tp = (int) value ;

            return tp-1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
