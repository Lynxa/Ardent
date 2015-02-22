using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
using AgentsConfig;
using AgentsRebuilt.Annotations;
using Microsoft.Win32;
using System.Windows.Forms;

namespace AgentsRebuilt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CfgSettings Config = new CfgSettings();
        private LogProcessor _logProcessor = new LogProcessor();

        private ExecutionState execState = ExecutionState.Void;
        private ExecutionState _previousState = ExecutionState.Void;
        private int _move_to = 0;
        private Boolean isFirstLine = true;
        private bool stopwork;
        private EnvironmentState ast = null;
        private String _lockerLogProcessorIndex = "meta";
        private bool _is_actual = false;
        private String _currentAgent = "god";
        private DateTime _statusTime;
        private String _currentMessage;

        private AgentDataDictionary _agentDataDictionary;
        private double _statePeriod;
        private bool _stopSleeping=false;

        private bool _hasAdministratorPrivilleges = false;

        private Dispatcher dsc;

        ObservableCollection<Agent> visualAgents;
        ObservableCollection<Agent> visualAgentsPane;
        ObservableCollection<Item> visualAuctions;
        ObservableCollection<Item> visualItemsPane;
        ObservableCollection<Item> visualCommons;
        ObservableCollection<String> tradeLog = null;



        private static StreamWriter _strWriter;

        private static bool Connect(String host, int port, out StreamWriter outStream)
        {
            try
            {
                TcpClient _client = new TcpClient();
                _client.Connect(host, port);
               NetworkStream stream = _client.GetStream();
                outStream = new StreamWriter(stream);

                 return true;
            }
            catch (Exception)
            {
                outStream = null;
                return false;
            }
        }

        public class NameMultiValueConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
            {
                return String.Format("{0} {1}", values[0], values[1]);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                string[] splitValues = ((string) value).Split(' ');
                return splitValues;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            String rawpath;
            rawpath = ReadPathFromFile("path_to_config.txt");
            bool _readCfg = false;
            while (!_readCfg)
            {
                Config = CfgSettings.LoadFromFile(rawpath, out _readCfg);
                if (!_readCfg)
                {
                    System.Windows.MessageBox.Show("Invalid configuration file. Please, select the correct one.");
                    Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
                    bool? userClickedOK = openFileDialog1.ShowDialog();

                    if (userClickedOK == true)
                    {
                        rawpath= openFileDialog1.FileName;
                    }
                }
            }
            CfgSettings.WritePathToFile("path_to_config.txt", rawpath);
            //if (!_readCfg) System.Windows.MessageBox.Show("Invalid configuration file");
            //else
            {
                ConfigList.DataContext = Config.TextList;
                _agentDataDictionary = new AgentDataDictionary(Config);
                _statePeriod = SpeedSlider.Value;
            }
            AuctionsLabel.Header = "{Special items}";
            LoadAndSetup(Config);
            //AgentDataDictionary = new AgentDataDictionary(@"D:\#Recent_desktop\UNI\PES602\DMG\dammage 1.0\domains\english.pl");
            this.Title = "MultiAgentz Visualization - " + Config.HistPath;
        }

        public static class Dialog
    {
        public static string MyPrompt(string InputHeader, string FormCaption)
        {
            Form dlg = new Form();
            dlg.Width = 300;
            dlg.Height = 150;
            dlg.Text = FormCaption;
            System.Windows.Forms.Label textHeading = new System.Windows.Forms.Label();
            textHeading.Left = 50;
            textHeading.Top = 20;
            textHeading.Width = 230;
            textHeading.Text = InputHeader;

            System.Windows.Forms.Button grabInput = new System.Windows.Forms.Button() ;
            grabInput.Text = "Enter";
            grabInput.Left = 100;
            grabInput.Top = 70;
            grabInput.Click += (sender, evt) => {
                dlg.Close();
            };
            System.Windows.Forms.TextBox textC = new System.Windows.Forms.TextBox();
            textC.Left = 47;
            textC.Top = 40;
            textC.Width = 200;
            textC.UseSystemPasswordChar = true;
            textC.TabIndex = 0;
            
            dlg.Controls.Add(grabInput);
            dlg.Controls.Add(textHeading);
            dlg.Controls.Add(textC);
            dlg.ActiveControl = textC;
            dlg.ShowDialog();
            return textC.Text;
        }
    }


        private void LoadAndSetup(CfgSettings Config)
        {
            if (File.Exists(Config.HistPath))
            {
                try
                {

                    dsc = this.Dispatcher;

                    _logProcessor.InitOrReread(Config.HistPath, _agentDataDictionary, dsc, _currentAgent);
                    Item.cfgSettings = Config;
                    this.Title = "MultiAgentz Visualization - " + Config.HistPath;
                    
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                    StatusLabel.Content = _currentMessage;
                    if (_logProcessor.GetNumber - 1 > 20)
                    {
                        MainSlider.Maximum = (double) _logProcessor.GetNumber - 1;
                    }
                    else
                    {
                        MainSlider.Maximum = Math.Ceiling((double)(_logProcessor.GetNumber - 1) / 10) * 10;
                    }

                    LineNumberToCoordinateConverter.FieldCount = (int)MainSlider.Maximum;
                    LineNumberToCoordinateConverter.FieldDuration =
                    _logProcessor.GetTimeByState(_logProcessor.GetNumber - 1);

                    MainSlider.Value = _logProcessor.Index - 1;
                    UpdateSliderMarks(dsc, (int)MainSlider.Maximum);
                    AuctionsLabel.Header = AgentDataDictionary.GetSpecialItemGroupName();

                    this.AgentsList.DataContext = new ObservableCollection<Agent>();
                    this.AuctionsList.DataContext= new ObservableCollection<Item>();
                    this.CommonsList.DataContext= new ObservableCollection<Item>();
                    this.AgentsPane.DataContext= new ObservableCollection<Agent>();
                    this.ItemsPane.DataContext= new ObservableCollection<Item>();
                    this.TradeChannel.DataContext = new List<String>();

                    //UpdateSliderMarks(dsc, _logProcessor.GetNumber);

                }
                catch
                {
                    System.Windows.MessageBox.Show("Something wrong with configuration file content");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Configuration file error: " + Config.HistPath + " cannot be found");
            }
        }
        

        private void RunFromLog(CfgSettings Config)
        {

            if (File.Exists(Config.HistPath))
            {
                try
                {

                    //Dispatcher dsc = this.Dispatcher;

                    //_logProcessor.InitOrReread(Config.HistPath, _agentDataDictionary, dsc, _currentAgent);
                    Item.cfgSettings = Config;
                    EnvironmentState newState;


                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            lock (_lockerLogProcessorIndex)
                            {
                                if (execState == ExecutionState.Running || execState == ExecutionState.Following || execState == ExecutionState.Moving
                                    || execState == ExecutionState.Switching)
                                {
                                    if (_statusTime.AddSeconds(5) < DateTime.Now)
                                    {
                                        //StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                                            StatusLabel.Content = _currentMessage;
                                            _statusTime = DateTime.Now;
                                        });
                                    }

                                    Boolean isLine;

                                    if (execState == ExecutionState.Moving)
                                    {
                                        _logProcessor.SetIndex(_move_to);
                                    }

                                    //else
                                    if (execState != ExecutionState.Switching)
                                    {
                                        isLine = _logProcessor.GetNextLine(out newState, execState);
                                    }
                                    else
                                    {
                                        isLine = _logProcessor.GetThisLine(out newState, execState);
                                    }
                                    _stopSleeping = false;
                                    if (isLine)
                                    {
                                        if (isFirstLine || ast == null || execState == ExecutionState.Moving || execState == ExecutionState.Switching)
                                        {
                                            if (execState == ExecutionState.Moving && _move_to != _logProcessor.GetAgentStartStep(_currentAgent))
                                            {
                                                //EnvironmentState secondState;
                                                //if (_move_to != 0 && _logProcessor.GetNextLine(out secondState, ExecutionState.Running))
                                                //{
                                                //    StateObjectMapper.UpdateState(secondState, newState, _agentDataDictionary, dsc);
                                                //}
                                                ast = newState;
                                            }
                                            else
                                            {
                                                ast = new EnvironmentState();
                                                StateObjectMapper.UpdateState(newState, ast, _agentDataDictionary, dsc);
                                            }


                                            dsc.Invoke(() =>
                                            {
                                                visualAgents = ast.Agents;
                                                visualAuctions = ast.Auctions;
                                                visualCommons = ast.CommonRights;
                                                visualAgentsPane = ast.AllAgents;
                                                visualItemsPane = ast.AllItems;

                                                if (execState == ExecutionState.Switching)
                                                {
                                                    SolidColorBrush b = new SolidColorBrush(ColorAndIconAssigner.GetOrAssignBackgroundColorById(_currentAgent));

                                                    AgentsList.Background = b;
                                                    Grid1.Background = b;
                                                    TradeChannel.Background = b;
                                                    ClockList.Background = b;
                                                    ClockListNames.Background = b;
                                                }

                                                tradeLog = new ObservableCollection<string>() { };

                                                AgentsList.DataContext = visualAgents;
                                                AuctionsList.DataContext = visualAuctions;
                                                CommonsList.DataContext = visualCommons;
                                                AgentsPane.DataContext = visualAgentsPane;
                                                ItemsPane.DataContext = visualItemsPane;

                                                //ast.Clock.StepNo = _logProcessor.Index;
                                                ClockList.DataContext = ast.Clock.TextList;
                                                ClockListNames.DataContext = ast.Clock.TextListNames;
                                                //AuctionsLabel.Header = AgentDataDictionary.GetSpecialItemGroupName();

                                                if (_logProcessor.GetNumber - 1 > 20)
                                                {
                                                    MainSlider.Maximum = (double) _logProcessor.GetNumber - 1;
                                                }
                                                else
                                                {
                                                    MainSlider.Maximum = Math.Ceiling((double) (_logProcessor.GetNumber - 1) / 10) * 10;

                                                }
                                                MainSlider.Value = _logProcessor.Index - 1;
                                                UpdateSliderMarks(dsc, (int) MainSlider.Maximum);
                                                //LineNumberToCoordinateConverter.FieldCount = _logProcessor.GetNumber;
                                                //LineNumberToCoordinateConverter.FieldDuration =
                                                //    _logProcessor.GetTimeByState(_logProcessor.GetNumber - 1);

                                                if (_currentAgent != "god")
                                                {
                                                    MainSlider.IsSelectionRangeEnabled = true;
                                                    MainSlider.SelectionStart =
                                                        _logProcessor.GetAgentStartStep(_currentAgent);
                                                    MainSlider.SelectionEnd =
                                                        LogProcessor.GetAgentEndStep(_currentAgent);
                                                }
                                                else
                                                {
                                                    MainSlider.IsSelectionRangeEnabled = false;
                                                }

                                                TradeChannel.DataContext = tradeLog;
                                                if (ast.Event != null && ast.Event.Message != "")
                                                    tradeLog.Add(ast.Event.Message);

                                                //AuctionsLabel.Text =  ast.Auctions != null && ast.Auctions.Count!=0 ? "Auctions" : "";
                                                //CommonsLabel.Text = ast.CommonRights != null && ast.CommonRights.Count != 0 ? "Common rights" : "";
                                                IndexBlock.DataContext = _logProcessor;
                                                AgentBlock.DataContext = _logProcessor;

                                                /*
                                        bb.BorderThickness = new Thickness(1);
                                        bb.BorderBrush = new SolidColorBrush(Colors.Black);
                                        bb.Width = 150;
                                        bb.Height = 20;
                                        bb.Child = tb;
                                        Grid1.Children.Add(bb);
                                        */
                                            });
                                            isFirstLine = false;
                                            if (execState == ExecutionState.Moving || execState == ExecutionState.Switching)
                                                execState = _previousState;
                                        }
                                        else
                                        {
                                            StateObjectMapper.UpdateState(newState, ast, _agentDataDictionary, dsc);
                                            dsc.Invoke(() =>
                                            {
                                                //ast.Clock.StepNo = _logProcessor.Index;
                                                if (_logProcessor.GetNumber - 1 > 20)
                                                {
                                                    MainSlider.Maximum = (double)_logProcessor.GetNumber - 1;
                                                }
                                                else
                                                {
                                                    MainSlider.Maximum = Math.Ceiling((double)(_logProcessor.GetNumber - 1) / 10) * 10;

                                                }
                                                MainSlider.Value = _logProcessor.Index - 1;

                                                if (tradeLog == null) tradeLog = new ObservableCollection<string>();
                                                if (ast.Event != null && ast.Event.Message != "")
                                                    tradeLog.Add(ast.Event.Message);
                                                if (TradeChannel.Items.Count > 0)
                                                {
                                                    TradeChannel.ScrollIntoView(
                                                        TradeChannel.Items[TradeChannel.Items.Count - 1]);
                                                }

                                                //AuctionsLabel.Text = ast.Auctions != null && ast.Auctions.Count != 0 ? "Auctions" : "";
                                                //CommonsLabel.Text = ast.CommonRights != null && ast.CommonRights.Count != 0 ? "Common rights" : "";
                                                //znaju, chto kostyl'. No sil moih net vozit'sja dal'she s data bindingom.
                                            });
                                        }
                                    }

                                    else
                                    {
                                        _logProcessor.InitOrReread(Config.HistPath, _agentDataDictionary, Dispatcher, _currentAgent);
                                        
                                        
                                        int slidercounter;

                                        dsc.Invoke(() =>
                                        {
                                            if (_logProcessor.GetNumber < 2) slidercounter = 10;
                                            else
                                                //ast.Clock.StepNo = _logProcessor.Index;
                                                if (_logProcessor.GetNumber - 1 > 20)
                                                {
                                                    slidercounter = _logProcessor.GetNumber - 1;
                                                    MainSlider.Maximum = (double) _logProcessor.GetNumber - 1;
                                                }
                                                else
                                                {
                                                    slidercounter =
                                                        (int)
                                                            Math.Ceiling((double) (_logProcessor.GetNumber - 1)/10)*
                                                        10;
                                                    MainSlider.Maximum = slidercounter;

                                                }

                                            LineNumberToCoordinateConverter.FieldCount = slidercounter;
                                            LineNumberToCoordinateConverter.FieldDuration =
                                                _logProcessor.GetTimeByState(_logProcessor.GetNumber - 1);
                                            UpdateSliderMarks(dsc, slidercounter);
                                        });

                                        //if (ast!=null)
                                        //{
                                        //    visualAgentsPane = ast.AllAgents;
                                        //    visualItemsPane = ast.AllItems;
                                        //    dsc.Invoke(() =>
                                        //    {
                                        //        AgentsList.DataContext = visualAgents;
                                        //        AuctionsList.DataContext = visualAuctions;
                                        //    });
                                        //}

                                    }
                                }

                                if (execState == ExecutionState.Stopped)
                                {
                                    _stopSleeping = false;
                                    dsc.Invoke(() =>
                                    {
                                        RunButton.IsEnabled = true;
                                        RestartButton.IsEnabled = false;
                                        StopButton.IsEnabled = false;
                                        PauseButton.IsEnabled = false;
                                        ConfigButton.IsEnabled = true;
                                        MainSlider.Value = MainSlider.Maximum;
                                    });
                                    return;
                                }
                                /*
                                if (execState == ExecutionState.Moving)
                                {
                                    Boolean isLine = LogProcessor.GetNextLine(out newState, ExecutionState.Running);
                                    tradeLog = new ObservableCollection<string>();
                                    _stopSleeping = false;
                                    if (isLine)
                                    {
                                        //ast = StateObjectMapper.MapState(newKVP, AgentDataDictionary, Dispatcher);
                                        StateObjectMapper.UpdateState(newState, ast, _agentDataDictionary, Dispatcher);

                                        Boolean isLine2 = LogProcessor.GetNextLine(out newState, ExecutionState.Running);
                                        StateObjectMapper.UpdateState(newState, ast, _agentDataDictionary, Dispatcher);
                                        dsc.Invoke(() =>
                                        {
                                            RunButton.IsEnabled = true;
                                            visualAgents = ast.Agents;
                                            AgentsList.DataContext = visualAgents;
                                            ClockList.DataContext = ast.Clock.TextList;
                                            ClockListNames.DataContext = ast.Clock.TextListNames;
                                            MainSlider.Value = LogProcessor.GetIndex;
                                            ast.Clock.StepNo = LogProcessor.GetIndex;
                                            if (ast.Event != null && ast.Event.Message != "") tradeLog.Add(ast.Event.Message);
                                            TradeChannel.DataContext = tradeLog;
                                            if (TradeChannel.Items.Count > 0)
                                            {
                                                TradeChannel.ScrollIntoView(
                                                    TradeChannel.Items[TradeChannel.Items.Count - 1]);
                                            }
                                            ClockList.DataContext = ast.Clock.TextList;
                                        });
                                        LogProcessor.SetCurrentLatency();
                                        execState = _previousState;
                                    }
                                } 
                                */

                            }
                            int delay;
                            if (!_is_actual)
                            {
                                delay = (int)(_statePeriod * 1000);
                            }
                            else
                            {

                                double i1, i2;
                                if (ast != null && ast.Clock != null && Double.TryParse(ast.Clock.TimeStampH.Replace(".", ","), out i1)
                                    && Double.TryParse(ast.Clock.TimeStampE.Replace(".", ","), out i2))
                                {
                                    delay = (int)((i2 - i1) * 1000);
                                }
                                else
                                {
                                    delay = 100; // very short for test purposes -- would be seen easily -- and it shouldn't ever fire
                                }
                            }

                            while (delay > 0 && !_stopSleeping)
                            {
                                if (delay >= 100)
                                {
                                    Thread.Sleep(100);
                                }
                                else
                                {
                                    Thread.Sleep(delay);
                                }
                                delay -= 100;
                            }

                        }


                        /*
                        Thread.Sleep(2500);

                        newKVP = LogProcessor.DecipherLogLine(2);
                        StateObjectMapper.UpdateState(newKVP, ast, Dispatcher);*/
                    });
                }
                catch
                {
                    System.Windows.MessageBox.Show("Something wrong with configuration in "+ Config.ConfigPath);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Configuration file error: " + Config.HistPath + " cannot be found");
                this.Dispatcher.Invoke(() =>
                {
                    ConfigButton.IsEnabled = true;
                    RunButton.IsEnabled = true;
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    StatusLabel.Content = "Error in configuration file";
                });
            }
        }

        private void UpdateSliderMarks(Dispatcher dsc, int number)
        {
            dsc.Invoke(() =>
            {
                SliderMark10.Content = number;
                {
                    SliderMark1.Content = (int) (number / 10);
                    SliderMark2.Content = (int)(number *2 / 10);
                    SliderMark3.Content = (int)(number *3 / 10);
                    SliderMark4.Content = (int)(number *4 / 10);
                    SliderMark5.Content = (int)(number *5 / 10);
                    SliderMark6.Content = (int)(number *6 / 10);
                    SliderMark7.Content = (int)(number *7 / 10);
                    SliderMark8.Content = (int)(number *8 / 10);
                    SliderMark9.Content = (int)(number * 9/ 10);
                }

            });
        }


        #region Config

        public string ReadPathFromFile(string filename)
        {
            string result = string.Empty;
            if (File.Exists(filename))
            {
                String[] st = File.ReadAllLines(filename);
                if (st.Length>0) result = st[0];
            }
            return result;
        }



        #endregion

        #region ControlElements

        private void OnRun(object sender, RoutedEventArgs e)
        {
            if (execState == ExecutionState.Void || execState == ExecutionState.Stopped)
            {
                bool result;
                Config = CfgSettings.LoadFromFile(Config.ConfigPath, out result);
                if (!result || !File.Exists(Config.HistPath) || !File.Exists(Config.DammagePath))
                {
                    System.Windows.MessageBox.Show("Something wrong with config file content");
                }
                else
                {
                    
                    execState = ExecutionState.Running;
                    RunButton.IsEnabled = false;
                    FollowButton.IsEnabled = true;
                    RestartButton.IsEnabled = true;
                    StopButton.IsEnabled = true;
                    PauseButton.IsEnabled = true;
                    ConfigButton.IsEnabled = false;
                    _logProcessor.SetIndex(_logProcessor.GetAgentStartStep(_currentAgent));
                    isFirstLine = true;
                    MainSlider.Value = _logProcessor.GetAgentStartStep(_currentAgent);
                    this.Dispatcher.Invoke(() =>
                    {
                        MainSlider.Value = MainSlider.Minimum;
                        visualAgents = new ObservableCollection<Agent>();
                        visualAuctions = new ObservableCollection<Item>();
                        visualCommons = new ObservableCollection<Item>(); 
                        visualAgentsPane = new ObservableCollection<Agent>(); 
                        visualItemsPane = new ObservableCollection<Item>(); 

                        StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                        StatusLabel.Content = "The visualisation is executing in Run mode";
                    });
                    _statusTime = DateTime.Now;
                    _currentMessage = "The visualisation is executing in Run mode";

                    RunFromLog(Config);
                    _stopSleeping = true;                    
                }
            }
            if (execState == ExecutionState.Paused)
            {

                execState = ExecutionState.Running;
                RunButton.IsEnabled = false;
                FollowButton.IsEnabled = true;
                RestartButton.IsEnabled = true;
                StopButton.IsEnabled = true;
                PauseButton.IsEnabled = true;
                ConfigButton.IsEnabled = false;
                _stopSleeping = true;

                this.Dispatcher.Invoke(() =>
                {
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                    StatusLabel.Content = "The visualisation is executing in Run mode";
                });

                _statusTime = DateTime.Now;
                _currentMessage = "The visualisation is executing in Run mode";
            }
            if (execState == ExecutionState.Following)
            {
                execState = ExecutionState.Running;
                RunButton.IsEnabled = false;
                PauseButton.IsEnabled = true;
                FollowButton.IsEnabled = true;
                _stopSleeping = true;

                this.Dispatcher.Invoke(() =>
                {
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                    StatusLabel.Content = "The visualisation is executing in Run mode";
                });
                _statusTime = DateTime.Now;
                _currentMessage = "The visualisation is executing in Run mode";
            }
        }
        private void OnFollow(object sender, RoutedEventArgs e)
        {
            if (execState == ExecutionState.Void || execState == ExecutionState.Stopped)
            {
                bool result;
                Config = CfgSettings.LoadFromFile(Config.ConfigPath, out result);
                if (!result) System.Windows.MessageBox.Show("Something wrong with config file content");
                else
                {
                    execState = ExecutionState.Following;
                    FollowButton.IsEnabled = false;
                    RestartButton.IsEnabled = true;
                    StopButton.IsEnabled = true;
                    PauseButton.IsEnabled = true;
                    ConfigButton.IsEnabled = false;
                    _logProcessor.SetIndex(_logProcessor.GetAgentStartStep(_currentAgent));
                    isFirstLine = true;
                    this.Dispatcher.Invoke(() =>
                    {
                        MainSlider.Value = _logProcessor.GetAgentStartStep(_currentAgent);
                        StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                        StatusLabel.Content = "The visualisation is executing in Follow mode";
                    });
                    _statusTime = DateTime.Now;
                    _currentMessage = "The visualisation is executing in Follow mode";

                    RunFromLog(Config);
                    _stopSleeping = true;
                }
            }
            if (execState == ExecutionState.Paused)
            {
                _logProcessor.SetCurrentLatency();
                execState = ExecutionState.Following;
                this.Dispatcher.Invoke(() =>
                {
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                    StatusLabel.Content = "The visualisation is executing in Follow mode";
                });
                _statusTime = DateTime.Now;
                _currentMessage = "The visualisation is executing in Follow mode";
                _stopSleeping = true;
            }
            if (execState == ExecutionState.Running)
            {
                _logProcessor.SetCurrentLatency();
                execState = ExecutionState.Following;
                RunButton.IsEnabled = true;
                FollowButton.IsEnabled = false;
                this.Dispatcher.Invoke(() =>
                {
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                    StatusLabel.Content = "The visualisation is executing in Follow mode";
                });
                _statusTime = DateTime.Now;
                _currentMessage = "The visualisation is executing in Follow mode";
                _stopSleeping = true;
            }
        }

        private void OnStop(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                lock (_lockerLogProcessorIndex)
                {
                    if (execState == ExecutionState.Running || execState == ExecutionState.Following ||
                        execState == ExecutionState.Paused)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                            StatusLabel.Content = "The visualisation is stopped";
                        });
                        _statusTime = DateTime.Now;
                        _currentMessage = "The visualisation is stopped";
                        execState = ExecutionState.Stopped;
                       // LogProcessor.SetIndex(LogProcessor.GetNumber - 1);

                    }
                    _stopSleeping = true;
                }
            });

        }

        private void OnConfig(object sender, RoutedEventArgs e)
        {
            String rawpath = ReadPathFromFile("path_to_config.txt");
            bool _readCfg = false;

            Config = CfgSettings.LoadFromFile(rawpath, out _readCfg);
            Window frm2 = new CfgWindow(Config, this);
            frm2.Show();
        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                lock (_lockerLogProcessorIndex)
                {
                    //ast = null;
                    isFirstLine = true;
                    _move_to = _logProcessor.GetAgentStartStep(_currentAgent);
                    //_logProcessor.SetIndex(tp);
                    //_logProcessor.CurrentAgent = "god";
                    _previousState = execState;
                    execState = ExecutionState.Moving;
                }

            });
            _stopSleeping = true;
        }

        private void Go(object sender, RoutedEventArgs e)
        {
            string text = LineNumber.Text;
            int tp;
            
            if (!Int32.TryParse(text, out tp))
            {
                this.Dispatcher.Invoke(() =>
                {
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    StatusLabel.Content = "Illegal step number!";
                    _statusTime = DateTime.Now;
                });
                //System.Windows.MessageBox.Show("Illegal step number");
                return;
            }

            bool isChecked = StateRadio.IsChecked != null && (bool)StateRadio.IsChecked;

            if (!isChecked)
            {
                int hh;
                if (_logProcessor.GetStateByTime(tp, out hh))
                {
                    Move(hh);
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                        StatusLabel.Content = "Illegal time!";
                        _statusTime = DateTime.Now;
                    });

                }
            }
            else
            {
                Move(tp);
            }
            _stopSleeping = true;
        }


        private void Move(int Step)
        {
            Task.Factory.StartNew(() =>
            {
                
                lock (_lockerLogProcessorIndex)
                {
                    int start = _logProcessor.GetAgentStartStep(_currentAgent);
                    int stop = LogProcessor.GetAgentEndStep(_currentAgent);

                    if (stop < Step || Step < start)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                            StatusLabel.Content = "Illegal step number!";
                            _statusTime = DateTime.Now;
                        });
                        return;
                    }
                    //ast = null;
                    isFirstLine = true;
                    _move_to = Step;
                    //_logProcessor.SetIndex(tp);
                    //_logProcessor.CurrentAgent = "god";
                    _previousState = execState;
                    execState = ExecutionState.Moving;
                }

            });
            _stopSleeping = true;
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnPause(object sender, RoutedEventArgs e)
        {
            if (execState == ExecutionState.Running || execState == ExecutionState.Following)
            {
                RunButton.IsEnabled = true;
                PauseButton.IsEnabled = false;
                FollowButton.IsEnabled = true;
                _previousState = execState;
                execState = ExecutionState.Paused;

                this.Dispatcher.Invoke(() =>
                {
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                    StatusLabel.Content = "The visualisation is paused at state " + (_logProcessor.Index - 1);
                });
                _statusTime = DateTime.Now;
                _currentMessage = "The visualisation is paused at state " + (_logProcessor.Index-1);

                _stopSleeping = true;
            }
        }


        private void MainSlider_OnValueChangedSlider_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (execState != ExecutionState.Stopped && (int)MainSlider.Value != _logProcessor.Index - 1)
            {
                double d = MainSlider.Value;
                int tp = (int)d;
                if (tp < _logProcessor.GetAgentStartStep(_currentAgent))
                {
                    tp = _logProcessor.GetAgentStartStep(_currentAgent);
                    //MainSlider.Value = tp;
                }
                else if (tp > LogProcessor.GetAgentEndStep(_currentAgent))
                {
                    tp = LogProcessor.GetAgentEndStep(_currentAgent);
                    //MainSlider.Value = tp;
                }

                Task.Factory.StartNew(() =>
                {

                    lock (_lockerLogProcessorIndex)
                    {
                        if (_logProcessor.GetNumber < tp || tp < 0)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                                StatusLabel.Content = "Illegal step number!";
                                _statusTime = DateTime.Now;
                            });
                            return;
                        }
                        //ast = null;
                        isFirstLine = true;
                        _move_to = tp;
                        if (execState != ExecutionState.Moving)
                        {
                            _previousState = execState;
                            execState = ExecutionState.Moving;
                        }
                    }
                });
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _statePeriod = SpeedSlider.Value;
        }

        #endregion

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            _is_actual = true;
            _stopSleeping = true;
            SpeedSlider.IsEnabled = false;
        }

        private void CheckBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            _is_actual = false;
            _stopSleeping = true;
            SpeedSlider.IsEnabled = true;
        }


        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as Grid);
            Agent tp = (Agent)item.DataContext;
            if (Keyboard.Modifiers.ToString().Contains("Control"))
            {
                if (tp.ID == _currentAgent)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                        StatusLabel.Content = "This is actually the current agent.";
                        _statusTime = DateTime.Now;
                    });
                }
                else if (_logProcessor.IsAgentLogAvailable(tp.ID))
                {
                    _currentAgent = tp.ID;
                    _logProcessor.CurrentAgent = _currentAgent;
                    _previousState = execState;
                    execState = ExecutionState.Switching;
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                        StatusLabel.Content = "Agent #" + tp.ID + " doesn't have the log file";
                        _statusTime = DateTime.Now;
                    });
                }
            }
            else
            {
                tp.IsExpanded = !tp.IsExpanded;
            }
        }


        private void KVPItem_Clicked(object sender, RoutedEventArgs e)
        {
            var item = sender as Grid;
            Item tp = (Item)item.DataContext;
            Window frm3 = new ItemWindow(tp, _logProcessor.Index-1, _logProcessor.GetTimeStampByState(_logProcessor.Index-1));
            frm3.Show();
            //MessageBox.Show("suspicious "+ tp.InstanceOf);
        }

        private void CommonRightsItem_Clicked(object sender, RoutedEventArgs e)
        {
            var item = sender as System.Windows.Controls.Button;
            var st = item.DataContext.GetType();
            if (st.Name.Equals("Item"))
            {
                Item tp = (Item) item.DataContext;
                Window frm3 = new ItemWindow(tp, _logProcessor.Index - 1, _logProcessor.GetTimeStampByState(_logProcessor.Index - 1));
                frm3.Show();
            }
            else if (st.Name.Equals("Agent"))
            {
                Agent tp = (Agent) item.DataContext;
                tp.IsPaneExpanded = !tp.IsPaneExpanded;
            }
        
        }

        private void MainSlider_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
           
        }

        private void LineNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceMarker == null) return;
            string text = LineNumber.Text;
            int tp;
            bool t = Int32.TryParse(text, out tp);

            bool isChecked = StateRadio.IsChecked != null && (bool) StateRadio.IsChecked;

            bool notTooBig = isChecked ? tp < _logProcessor.GetNumber : tp < _logProcessor.GetTimeByState(_logProcessor.GetNumber-1);
            if (!LineNumber.Text.Equals("0") && !LineNumber.Text.Equals("") && (tp >= 0 && notTooBig))
            {
                 {
                    this.Dispatcher.Invoke(() =>
                    {
                        PlaceMarker.Visibility = Visibility.Visible;
                    });
                }
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    PlaceMarker.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void AgentFirstStepButton_onClick(object sender, RoutedEventArgs e)
        {
            var item = sender as System.Windows.Controls.Button;
            var st = item.DataContext.GetType();
            if (st.Name.Equals("Agent"))
            {
                Agent tp = (Agent)item.DataContext;
                Move(tp.FirstStep);
            }
        }

        private void AgentLastStepButton_onClick(object sender, RoutedEventArgs e)
        {
            var item = sender as System.Windows.Controls.Button;
            var st = item.DataContext.GetType();
            if (st.Name.Equals("Agent"))
            {
                Agent tp = (Agent)item.DataContext;
                Move(tp.LastStep);
            }
        }

        public void Reload(CfgSettings cfg)
        {
            LoadAndSetup(cfg);
        }

        private void OnAdmin(object sender, RoutedEventArgs e)
        {
            if (!_hasAdministratorPrivilleges)
            {
                string name = Dialog.MyPrompt("Please enter administration password:", "Enter password");
                if (name.Equals("meta"))
                {
                    _hasAdministratorPrivilleges = true;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Incorrect password!");
                }
            }
            if (_hasAdministratorPrivilleges )
            {
                if (_strWriter != null)
                {
                    AdminButton.Content = "Admin";
                    SimulationConsoleWindow console = new SimulationConsoleWindow(_strWriter, this);
                    console.Show();
                }
                else
                {
                    if (Connect("localhost", 5100, out _strWriter))
                    {
                        SimulationConsoleWindow console = new SimulationConsoleWindow(_strWriter, this);
                        console.Show();
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Error connecting to Communication Module!");
                    }

                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Error connecting to Communication Module!");
            }
        }

        public void ShutdownAdmin()
        {
            AdminButton.IsEnabled = false;
            _hasAdministratorPrivilleges = false;
        }

        private void OnAgent(object sender, RoutedEventArgs e)
        {
            StreamWriter sst;
            if (Connect("localhost", 7100, out sst))
            {
                SimulationConsoleWindow console = new SimulationConsoleWindow(sst, this);
                console.Title = "Agent Console";
                console.Show();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Cannot connect to the simulation! Check if simulation and proxy are running.");
            }
        }

        private void OnMainLoad(object sender, RoutedEventArgs e)
        {
            if (Config.AdminRights)
            {
                AdminButton.Visibility = Visibility.Visible;

                string name = Dialog.MyPrompt("Please enter administration password:", "Enter password");
                if (name.Equals("meta") )
                {
                    _hasAdministratorPrivilleges = true;
                    if (!Connect("localhost", 5100, out _strWriter))
                    {
                        System.Windows.Forms.MessageBox.Show("Error connecting to Communication Module!");
                        AdminButton.Content = @"Connect 
as admin";
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Incorrect password!");
                    AdminButton.Content = @"Connect 
as admin";
                }
            }
        }

        private void StateRadio_Checked(object sender, RoutedEventArgs e)
        {
            PlaceMarker.Fill = new SolidColorBrush(Colors.BlueViolet);
            LineNumber.Text = "0";
        }

        private void StateRadio_Unchecked(object sender, RoutedEventArgs e)
        {
            PlaceMarker.Fill = new SolidColorBrush(Colors.Red);
            LineNumber.Text = "0";
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as System.Windows.Controls.Grid);
            Item tp = (Item)item.DataContext;
            tp.IsExpanded = !tp.IsExpanded;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (sender as System.Windows.Controls.Button);
            Item tp = (Item)item.DataContext;
            tp.IsPaneExpanded = !tp.IsPaneExpanded;
        }
    }
}