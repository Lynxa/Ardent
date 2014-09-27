﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
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

        private ExecutionState execState = ExecutionState.Void;
        private ExecutionState _previousState = ExecutionState.Void;
        private Boolean isFirstLine = true;
        private bool stopwork;
        private AgentState ast = null;
        private String _lockerLogProcessorIndex = "meta";
        private bool _is_actual = false;

        private AgentDataDictionary _agentDataDictionary;
        private double _statePeriod;
        private bool _stopSleeping=false;
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

            Config = CfgSettings.LoadFromFile(rawpath, out _readCfg);

            if (!_readCfg) System.Windows.MessageBox.Show("Invalid configuration file");
            else
            {
                ConfigList.DataContext = Config.TextList;
                _agentDataDictionary = new AgentDataDictionary(Config);
                _statePeriod = SpeedSlider.Value;
            }
            //AgentDataDictionary = new AgentDataDictionary(@"D:\#Recent_desktop\UNI\PES602\DMG\dammage 1.0\domains\english.pl");
        }


        private void RunFromLog(CfgSettings Config)
        {
            ObservableCollection<Agent> visualAgents;
            ObservableCollection<Item> visualAuctions;
            ObservableCollection<Item> visualCommons;
            ObservableCollection<String> tradeLog = null;
            
            if (File.Exists(Config.HistPath))
                try
                {
                    
                    Dispatcher dsc = this.Dispatcher;

                    LogProcessor.InitOrReread(Config.HistPath, _agentDataDictionary, dsc);
                    Item.cfgSettings = Config;
                    AgentState newState;

                    
                    /*TextBlock tb = new TextBlock();
                    tb.Text = "Hello World";
                    tb.Background = new SolidColorBrush(Colors.Aqua);
                    Border bb = new Border();
                    */

                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            lock (_lockerLogProcessorIndex)
                            {
                                if (execState == ExecutionState.Running || execState == ExecutionState.Following ||execState==ExecutionState.Moving)
                                {
                                    Boolean isLine = LogProcessor.GetNextLine(out newState, execState);
                                    _stopSleeping = false;
                                    if (isLine)
                                    {
                                        if (isFirstLine || ast == null || execState == ExecutionState.Moving)
                                        {
                                            if (execState == ExecutionState.Moving)
                                            {
                                                AgentState secondState;
                                                if (LogProcessor.GetNextLine(out secondState, ExecutionState.Running))
                                                {
                                                    StateObjectMapper.UpdateState(secondState, newState, _agentDataDictionary, dsc);
                                                }
                                            }
                                            ast = 
                                                newState;
                                            dsc.Invoke(() =>
                                            {
                                                visualAgents = ast.Agents;
                                                visualAuctions = ast.Auctions;
                                                visualCommons = ast.CommonRights;
                                                tradeLog = new ObservableCollection<string>() {};

                                                AgentsList.DataContext = visualAgents;
                                                AuctionsList.DataContext = visualAuctions;
                                                CommonsList.DataContext = visualCommons;
                                               
                                                ast.Clock.StepNo = LogProcessor.GetIndex;
                                                ClockList.DataContext = ast.Clock.TextList;
                                                ClockListNames.DataContext = ast.Clock.TextListNames;
                                                MainSlider.Maximum = LogProcessor.GetNumber;
                                                MainSlider.Value = LogProcessor.GetIndex;
                                                TradeChannel.DataContext = tradeLog;
                                                if (ast.Event != null && ast.Event.Message != "")
                                                    tradeLog.Add(ast.Event.Message);

                                                AuctionsLabel.Text =  ast.Auctions != null && ast.Auctions.Count!=0 ? "Auctions" : "";
                                                CommonsLabel.Text = ast.CommonRights != null && ast.CommonRights.Count != 0 ? "Common rights" : "";
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
                                            if (execState == ExecutionState.Moving) execState = _previousState;
                                        }
                                        else
                                        {
                                            StateObjectMapper.UpdateState(newState, ast, _agentDataDictionary, dsc);
                                            dsc.Invoke(() =>
                                            {
                                                ast.Clock.StepNo = LogProcessor.GetIndex;
                                                MainSlider.Value = LogProcessor.GetIndex;
                                                MainSlider.Maximum = LogProcessor.GetNumber;
                                                if (tradeLog == null) tradeLog = new ObservableCollection<string>();
                                                if (ast.Event != null && ast.Event.Message != "")
                                                    tradeLog.Add(ast.Event.Message);
                                                if (TradeChannel.Items.Count > 0)
                                                {
                                                    TradeChannel.ScrollIntoView(
                                                        TradeChannel.Items[TradeChannel.Items.Count - 1]);
                                                }

                                                AuctionsLabel.Text = ast.Auctions != null && ast.Auctions.Count != 0 ? "Auctions" : "";
                                                CommonsLabel.Text = ast.CommonRights != null && ast.CommonRights.Count != 0 ? "Common rights" : "";
                                                //znaju, chto kostyl'. No sil moih net vozit'sja dal'she s data bindingom.
                                            });
                                        }
                                    }

                                    else
                                    {
                                        LogProcessor.InitOrReread(Config.HistPath, _agentDataDictionary, Dispatcher);

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
                                delay = (int) (_statePeriod*1000);
                            }
                            else
                            {

                                double i1, i2;
                                if (ast != null && ast.Clock != null && Double.TryParse(ast.Clock.HappenedAt.Replace(".", ","), out i1)
                                    && Double.TryParse(ast.Clock.ExpiredAt.Replace(".", ","), out i2))
                                {
                                    delay = (int) ((i2 - i1)*1000);
                                }
                                else 
                                {
                                    delay = 100; // very short for test purposes -- would be seen easily -- and it shouldn't ever fire
                                }
                            }

                            while (delay > 0 && !_stopSleeping)
                            {
                                if (delay >= 1000)
                                {
                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    Thread.Sleep(delay);    
                                }
                                delay -= 1000;
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
                    System.Windows.MessageBox.Show("Configuration file error: " + Config.HistPath + " cannot be found");
                }
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
                if (!result) System.Windows.MessageBox.Show("Something wrong with config file content");
                else
                {
                    execState = ExecutionState.Running;
                    RunButton.IsEnabled = false;
                    FollowButton.IsEnabled = true;
                    RestartButton.IsEnabled = true;
                    StopButton.IsEnabled = true;
                    PauseButton.IsEnabled = true;
                    ConfigButton.IsEnabled = false;
                    LogProcessor.SetIndex(0);
                    isFirstLine = true;
                    MainSlider.Value = 0;

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
            }
            if (execState == ExecutionState.Following)
            {
                execState = ExecutionState.Running;
                RunButton.IsEnabled = false;
                FollowButton.IsEnabled = true;
                _stopSleeping = true;
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
                    LogProcessor.SetIndex(0);
                    isFirstLine = true;
                    MainSlider.Value = 0;

                    RunFromLog(Config);
                    _stopSleeping = true;
                }
            }
            if (execState == ExecutionState.Paused)
            {
                execState = ExecutionState.Following;
                _stopSleeping = true;
            }
            if (execState == ExecutionState.Running)
            {
                LogProcessor.SetCurrentLatency();
                execState = ExecutionState.Following;
                RunButton.IsEnabled = true;
                FollowButton.IsEnabled = false;
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
            Window frm2 = new CfgWindow(Config);
            frm2.Show();
        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            isFirstLine = true;
            LogProcessor.SetIndex(0);
            MainSlider.Value = 0;
            _stopSleeping = true;
        }

        private void Go(object sender, RoutedEventArgs e)
        {
            string text = LineNumber.Text;
            Task.Factory.StartNew(() =>
            {
                int tp;
                lock (_lockerLogProcessorIndex)
                {
                    bool t = Int32.TryParse(text, out tp);
                    if (!t || LogProcessor.GetNumber < tp || tp < 0)
                    {
                        System.Windows.MessageBox.Show("Illegal step number");
                        return;
                    }
                    //ast = null;
                    if (tp == 0)
                    {
                        isFirstLine = true;
                        LogProcessor.SetIndex(0);
                        _previousState = execState;
                        execState = ExecutionState.Moving;
                    }
                    else
                    {
                        isFirstLine = true;
                        LogProcessor.SetIndex(tp - 1);
                        _previousState = execState;
                        execState = ExecutionState.Moving;
                    }
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
                _stopSleeping = true;
            }
        }


        private void MainSlider_OnValueChangedSlider_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (execState!=ExecutionState.Stopped && (int) MainSlider.Value != LogProcessor.GetIndex)
            {
                double d = MainSlider.Value;
                Task.Factory.StartNew(() =>
                {
                    int tp = (int) d;
                    lock (_lockerLogProcessorIndex)
                    {
                        if (LogProcessor.GetNumber < tp || tp < 0)
                        {
                            System.Windows.MessageBox.Show("Illegal step number");
                            return;
                        }
                        //ast = null;
                        if (tp == 0)
                        {
                            isFirstLine = true;
                            LogProcessor.SetIndex(0);
                            _previousState = execState;
                            execState = ExecutionState.Moving;
                        }
                        else
                        {
                            isFirstLine = true;
                            LogProcessor.SetIndex(tp - 1);
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
            tp.IsExpanded = !tp.IsExpanded;
        }


        private void KVPItem_Clicked(object sender, RoutedEventArgs e)
        {
            var item = sender as ListBoxItem;
            Item tp = (Item)item.DataContext;
            Window frm3 = new ItemWindow(tp);
            frm3.Show();
            //MessageBox.Show("suspicious "+ tp.InstanceOf);
        }

        private void CommonRightsItem_Clicked(object sender, MouseButtonEventArgs e)
        {
            var item = sender as System.Windows.Controls.Button;
            Item tp = (Item)item.DataContext;
            Window frm3 = new ItemWindow(tp);
            frm3.Show();
        }
    }
}