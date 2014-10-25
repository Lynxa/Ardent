using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AgentsRebuilt
{
    
    public class Agent : INotifyPropertyChanged
    { 
        private int _account = 0;
        private Brush _bg;
        private Brush _brd;
        private bool _isExpanded = true;
        private bool _isPresent = true;
        private bool _isPaneExpanded;
        private int _lastStep;
        private int _firstStep;

        private readonly String _name;
        private readonly AgentDataDictionary _imageDictionary;
        private readonly Dispatcher _uiDispatcher;
        
        public String ID;
        public ElementStatus st;
        public Boolean Minimized = false;

        //public ImageSource ImageSource;
        public ObservableCollection<Item> Items;
        


        public Agent(String id, List<KVP> items, AgentDataDictionary iAgentDataDictionary,Dispatcher uiThread)
        {
            ID = id;
            _imageDictionary = iAgentDataDictionary;
            
            _name = iAgentDataDictionary.GetAgentNameByID(id);

            Items = Item.KvpToItems(items, iAgentDataDictionary, uiThread);

            int tsc = 0;
            Item tIt = null;
            foreach (var item in Items)
            {
                if (item.Key.StartsWith("account"))
                {
                    tsc = Int32.Parse(item.Amount);
                    tIt = item;
                    break;
                }
            }

            _account = tsc;

            _uiDispatcher = uiThread;
            _uiDispatcher.Invoke(() =>
            {
                _bg = new SolidColorBrush(Colors.LightYellow);
                _brd = new SolidColorBrush(ColorAndIconAssigner.GetOrAssignColorById(id));
                if (tIt!=null) Items.Remove(tIt);
            });

            st = ElementStatus.New;
        }

        public Agent GetNeutralCopy()
        {
            var result = new Agent(ID, new List<KVP>(){}, _imageDictionary, _uiDispatcher );
            result.Background = new SolidColorBrush(Colors.White);
            result.IsPresent = true;
            result.Status = ElementStatus.Unchanged;
            result.Source = Source;
            
            
            return result;
        }

        public int LastStep
        {
            get { return _lastStep; }

            set
            {

                _uiDispatcher.Invoke(() =>
                {
                    _lastStep = value;
                });

                NotifyPropertyChanged();
            }
        }
        public int FirstStep
        {
            get { return _firstStep; }

            set
            {

                _uiDispatcher.Invoke(() =>
                {
                    _firstStep = value;
                });

                NotifyPropertyChanged();
            }
        }

        public int Account
        {
            get { return _account; }

            set
            {

                _uiDispatcher.Invoke(() =>
                {
                    _account = value;
                });

                NotifyPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }

            set
            {
                
                _uiDispatcher.Invoke(() =>
                {
                    _isExpanded = value;
                });

                NotifyPropertyChanged();
            }
        }

        public bool IsPaneExpanded
        {
            get { return _isPaneExpanded; }

            set
            {

                _uiDispatcher.Invoke(() =>
                {
                    _isPaneExpanded = value;
                });

                NotifyPropertyChanged();
            }
        }

        public bool IsPresent
        {
            get { return _isPresent; }
            set
            {
                _uiDispatcher.Invoke(() =>
                {
                    _isPresent = value;
                });

                NotifyPropertyChanged();
            }
        }

        public String Id 
        {
            get { return ID; }
            set { }
        }

        public String Title
        {
            get { return _name.Equals("") ? "#" + ID : _name + " (" + ID + ")"; }
        }

        public ObservableCollection<Item> itms 
        {
            get { return new ObservableCollection<Item>(Items); }
            set
            {
                _uiDispatcher.Invoke(() =>
                {
                    Items = value;
                });
                NotifyPropertyChanged();
            }
        }

        public ElementStatus Status
        {
            get { return st; }

            set
            {
                this.st = value;
                _uiDispatcher.Invoke(() =>
                {
                    Background = getBgColor();
                }); 
            
                NotifyPropertyChanged();
            }
        }

        public Brush Background
        {
            get { return _bg; }
            set 
            {
                _uiDispatcher.Invoke(() =>
                {
                    _bg = value;                    
                });
                NotifyPropertyChanged();
            }
        }

        public Brush BorderBrush
        {
            get { return _brd; }
        }

        private Brush getBgColor()
        {
            switch (Status)
            {
                case ElementStatus.New: return new SolidColorBrush(Colors.LightGoldenrodYellow);
                case ElementStatus.Changed: return new SolidColorBrush(Colors.LightCyan);
                case ElementStatus.Unchanged: return new SolidColorBrush(Colors.White);
                case ElementStatus.Deleted: return new SolidColorBrush(Colors.Gray);
            }
            return new SolidColorBrush(Colors.Gold);
        }

        public ImageSource Source
        {
            get { return getSource(); }
            set { }
        }

        private ImageSource getSource()
        {
           return _imageDictionary.GetAgentSourceByID(ID);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        
        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void ToggleMin()
        {
            //Minimized = !Minimized;
        }

     
    }
}