using System;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Windows.Data;

namespace AgentsRebuilt
{
    public class LineNumberToCoordinateConverter : IValueConverter
    {
        public static int FieldCount = 0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int tp;
            Int32.TryParse((value as String), out tp);
            if (FieldCount !=-1)
            {
                tp = 1074*tp/(FieldCount+1);
            }
            else tp = 0;
            return tp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
