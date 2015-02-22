using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AgentsRebuilt
{
    /// <summary>
    /// Interaction logic for ItemWindow.xaml
    /// </summary>
    public partial class ItemWindow : Window
    {
        private Item itm;

        public Item Item
        {
            get { return itm; }
            set { itm = value; }
        }

        private ImageSource src;

        public ImageSource Src
        {
            get { return src; }
            set { src = value; }
        }

        public ItemWindow(Item item, int state, String time)
        {
            InitializeComponent();
            Item = item;
            itmImage.DataContext = item.Source;
            itmTitle.DataContext = item.Title;
            listBox1.DataContext = item.StringAttributeList;
            //itmInst.DataContext = item.InstanceOf;
            brd1.DataContext = item.BorderBrush2;
            StateText.DataContext = state;
            TimeText.DataContext = time;
        }
    }
}
