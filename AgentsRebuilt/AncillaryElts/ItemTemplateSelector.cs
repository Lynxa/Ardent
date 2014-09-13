using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AgentsRebuilt
{
    public class ItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is CfgStr)
            {
                CfgStr person = item as CfgStr;
                Window window = System.Windows.Application.Current.MainWindow;
                ListBox list = window.FindName("ClockList") as ListBox;
                CheckBox chk = window.FindName("ChkBox") as CheckBox;
                CfgStr selectedPerson = list.SelectedItem as CfgStr;
               // if (selectedPerson != null)
                if (chk.IsChecked==true)
                {
                    return window.FindResource("CfgItemTemplate2") as DataTemplate;
                }
                else
                {
                    return window.FindResource("CfgItemTemplate1") as DataTemplate;
                }
            }

            return null;
        }
    }
}
