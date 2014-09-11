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

namespace AgentsRebuilt
{
    /// <summary>
    /// Interaction logic for CfgWindow.xaml
    /// </summary>
    public partial class CfgWindow : Window
    {
        private CfgSettings config;
        private String path;

        public CfgWindow(CfgSettings cfg) : base()
        {
            InitializeComponent();
            config = cfg;
            if (cfg != null)
            {
                path = config.ConfigPath;
            }
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
            //System.Windows.MessageBox.Show("The configuration is saved!");
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
                config.HistPath = TextBox2.Text;
                config.DammagePath = TextBox3.Text;
                config.SetTextList();
                CfgSettings.WriteToFile(config);
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

        private void Window_Initialized(object sender, RoutedEventArgs e)
        {
            try
            {
                bool result;
                config = CfgSettings.LoadFromFile(path, out result);
                if (result)
                {
                    TextBox2.Text = config.HistPath;
                    TextBox3.Text = config.DammagePath;
                    ConfigTextBox.Text = config.ConfigPath;
                }
            }
            catch (Exception e1)
            {
                System.Windows.MessageBox.Show("Error parsing config.xml file: " + e1.Message);
                this.Close();
            }
        }

        private void ConfigFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
            bool? result = openFileDialog1.ShowDialog();

            if (result==true)
            {
                ConfigTextBox.Text = openFileDialog1.FileName;
            }
        }

        private void ConfigFileLoadButton_Click(object sender, RoutedEventArgs e)
        {
            String tpath = ConfigTextBox.Text;
            bool result;
            config = CfgSettings.LoadFromFile(tpath, out result);
            if (result)
            {
                TextBox2.Text = config.HistPath;
                TextBox3.Text = config.DammagePath;
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid configuration file");
            }
        }

    }
}
