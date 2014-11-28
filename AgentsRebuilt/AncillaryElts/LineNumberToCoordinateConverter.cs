using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AgentsRebuilt
{
    public class LineNumberToCoordinateConverter : IValueConverter
    {
        public static int FieldCount = 0;
        public static int FieldDuration = 0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Window window = System.Windows.Application.Current.MainWindow;
            
            RadioButton radio = new RadioButton(); //this is workaround for xaml to show the actual UI element in Designer. More robust this way also, I guess
            radio.IsChecked = true;

            if (window != null && window.FindName("StateRadio") != null)
            {
                var tradio = window.FindName("StateRadio") as RadioButton;
                if (tradio != null) radio = tradio;
            }


            int tp;
            Int32.TryParse((value as String), out tp);

            //if (radio == null) return 0;

            switch (radio.IsChecked)
            {
                case true:
                    if (FieldCount !=-1)
                    {
                        tp = 1074*tp/(FieldCount+1);
                    }
                    else tp = 0;
                    break;

                case false:
                    if (FieldDuration != -1)
                    {
                        tp = 1074 * tp / (FieldDuration + 1);
                    }
                    else tp = 0;
                    break;
                default:
                    tp = 0;
                    break;
            }

            return tp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
