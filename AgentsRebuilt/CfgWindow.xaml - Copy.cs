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
using System.Xml;
using Microsoft.Win32;
using System.Windows.Forms;
using AgentsConfig;

namespace AgentsRebuilt
{
    /// <summary>
    /// Interaction logic for CfgWindow.xaml
    /// </summary>
    public partial class CfgWindow : Window
    {
        private CfgSettings config;

        public CfgWindow(CfgSettings cfg) : base()
        {
            InitializeComponent();
            config = cfg;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                config = CfgSettings.LoadFromFile("config.xml");

            }
            catch (Exception e1)
            {
                System.Windows.MessageBox.Show("Error parsing config.xml file: " + e1.Message);
                this.Close();
            }

            switch (config.DataSource)
            {
                case DataSource.Socket:
                    this.RadioButton1.IsChecked = true;
                    this.TextBox2.IsEnabled = false;
                    this.TextBox5.IsEnabled = true;
                    break;
                case DataSource.Logfile:
                    this.RadioButton2.IsChecked = true;
                    this.TextBox1.IsEnabled = false;
                    this.TextBox5.IsEnabled = false;
                    break;
            }

            TextBox1.Text = config.Socket;
            TextBox2.Text = config.HistPath;
            TextBox3.Text = config.IconPath;
            TextBox4.Text = config.IconExt;
            TextBox5.Text = config.HostName;
        }

        private void logOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
            bool? userClickedOK = openFileDialog1.ShowDialog();

            if (userClickedOK == true)
            {
                TextBox2.Text = openFileDialog1.FileName;
            }
        }

        private void iconOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            openFileDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            openFileDialog1.SelectedPath = TextBox3.Text;
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TextBox3.Text = openFileDialog1.SelectedPath;
            }
        }

        private void SaveCfgBtn_Click(object sender, RoutedEventArgs e)
        {
            OnSave();
            System.Windows.MessageBox.Show("The configuration is saved!");
        }

        private void SaveExitCfgBtn_Click(object sender, RoutedEventArgs e)
        {
            OnSave();
            this.Close();
        }

        private void OnSave()
        {
            try
            {
                config.DataSource = (RadioButton2.IsChecked == true) ? DataSource.Logfile : DataSource.Socket;
                config.Socket = TextBox1.Text;
                config.HistPath = TextBox2.Text;
                config.IconPath = TextBox3.Text;
                config.IconExt = TextBox4.Text;
                config.SetTextList();
                CfgSettings.WriteToFile("config.xml", config);
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Cannot save configuration:" + e2.Message);
            }
        }

        private void CgfCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            this.TextBox1.IsEnabled = true;
            this.TextBox2.IsEnabled = false;
            this.TextBox5.IsEnabled = true;
        }

        private void RadioButton1_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TextBox2.IsEnabled = true;
            this.TextBox1.IsEnabled = false;
            this.TextBox5.IsEnabled = false;
        }


    }
}
