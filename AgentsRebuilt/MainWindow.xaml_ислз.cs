using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace AgentsRebuilt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CfgSettings Config = new CfgSettings();
        //private List<CfgStr> cfg;

        Boolean stopWork;
        private Boolean isFirstLine = true;
        AgentState ast = null;
                

        public class NameMultiValueConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return String.Format("{0} {1}", values[0], values[1]);
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                string[] splitValues = ((string)value).Split(' ');
                return splitValues;
            }
        }
        public MainWindow()
        {
            
            InitializeComponent();
            Config = CfgSettings.LoadFromFile("config.xml");
           // cfg = new List<CfgStr>() { new CfgStr(Config.HistPath), new CfgStr(Config.IconExt), new CfgStr(Config.IconExt) };
            ConfigList.DataContext = Config.TextList;
            
           // ConfigBlock1.DataContext = Config.IconPath;

            //listBox.DataContext = ast.Agents[0].Items;
        }


        void OnRun(object sender, RoutedEventArgs e)
        {
            Config = CfgSettings.LoadFromFile("config.xml");
            stopWork = false;
            switch (Config.DataSource)
            {
                case DataSource.Logfile:
                    RunFromLog(Config);break;
                case DataSource.Socket:
                    RunFromSocket(Config);
                    break;
            }

        }

        private void RunFromSocket(CfgSettings Config)
        {
            try
            {
                NetworkReader nr = new NetworkReader();
                int soc;
                if (Int32.TryParse(Config.Socket, out soc))
                {
                    nr.Connect(Config.HostName, soc);
                    nr.OnDataRevieved += OnMessage;
                }
                else
                {
                    MessageBox.Show("Configuration file error: Socket connection failure");
                }
            }
            catch
            {
                MessageBox.Show("Configuration file error: Socket connection failure");
            }
        }

        private void OnMessage(String msg)
        {
            try
            {
                Item.cfgSettings = Config;
                KVP newKVP = LogProcessor.DecipherLine(msg);
                if (newKVP != null)
                {
                    if (isFirstLine)
                    {
                        ast = StateObjectMapper.MapState(newKVP, Dispatcher);
                        ObservableCollection<Agent> visualAgents = ast.Agents;
                        AgentsList.DataContext = visualAgents;
                        isFirstLine = false;
                    }
                    else
                    {
                        StateObjectMapper.UpdateState(newKVP, ast, Dispatcher);
                    }
                }
            }
            catch
            {
            }
        }

        private void RunFromLog(CfgSettings Config)
        {
            ObservableCollection<Agent> visualAgents;
            //LogProcessor.InitOrReread(@"D:\C# Projects\WindowsFormsApplication1\WindowsFormsApplication1\Log\history_ftm.db");
            if (File.Exists(Config.HistPath))
                try
                {
                    LogProcessor.InitOrReread(Config.HistPath);
                    Item.cfgSettings = Config;
                    KVP newKVP;
                  /*  if (!LogProcessor.GetNextLine(out newKVP)) return;
                    ast = StateObjectMapper.MapState(newKVP, Dispatcher);

                    ObservableCollection<Agent> visualAgents = ast.Agents;

                    AgentsList.DataContext = visualAgents;
                    ClockList.DataContext = ast.Clock.TextList;
                        */
                    Dispatcher dsc = this.Dispatcher;

                    Task.Factory.StartNew(() =>
                    {
                        
                        while (!stopWork) 
                        {
                            
                            Boolean isLine = LogProcessor.GetNextLine(out newKVP);
                            if (isLine)
                            {
                                if (isFirstLine || ast==null)
                                {
                                    ast = StateObjectMapper.MapState(newKVP, Dispatcher);
                                    dsc.Invoke(() =>
                                    {
                                        visualAgents = ast.Agents;
                                        AgentsList.DataContext = visualAgents;
                                        ClockList.DataContext = ast.Clock.TextList;
                                    });
                                    isFirstLine = false;
                                }
                                else
                                {
                                    StateObjectMapper.UpdateState(newKVP, ast, Dispatcher);
                                }
                        }
                            
                            else
                            {
                                LogProcessor.InitOrReread(Config.HistPath);
                            }
                            if (stopWork)
                            return;

                            Thread.Sleep(2500);
                        }

                        
                        /*
                        Thread.Sleep(2500);

                        newKVP = LogProcessor.DecipherLogLine(2);
                        StateObjectMapper.UpdateState(newKVP, ast, Dispatcher);*/
                    });
                }
                catch 
                {
                    MessageBox.Show("Configuration file error: " + Config.HistPath + " cannot be found");
                }
        }

        void OnStop(object sender, RoutedEventArgs e)
        {
            stopWork = true;
        }

        private void KVPItem_Clicked(object sender, RoutedEventArgs e)
        {
            var item = sender as ListBoxItem;
            Item tp = (Item)item.DataContext;
            Window frm3 = new ItemWindow(tp);
            frm3.Show();
            //MessageBox.Show("suspicious "+ tp.InstanceOf);
        }

        private void OnConfig(object sender, RoutedEventArgs e)
        {
            Window frm2 = new CfgWindow(Config);

            frm2.Show();

        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            ast = null;
            LogProcessor.SetIndex(0);
        }

        private void Go(object sender, RoutedEventArgs e)
        {
            int tp;
            bool t = Int32.TryParse(LineNumber.Text, out tp);
            if (!t || (!LogProcessor.SetIndex(tp)))
            {
                MessageBox.Show("Illegal step number");
                return;
            }
            ast = null;
        }  

        void OnExit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
    }
}
