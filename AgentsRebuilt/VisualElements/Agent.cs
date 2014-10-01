using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AgentsRebuilt
{
    
    public class Agent : INotifyPropertyChanged
    {
        public String ID;
        public ElementStatus st;
        public ObservableCollection<Item> Items;
        public Boolean Minimized = false;
        private int _account = 0;
        private Brush bg;
        private Brush brd;
        private String name;
        private AgentDataDictionary ImageDictionary;
        private Dispatcher uiDispatcher;
        private bool _isExpanded=true;

        public ImageSource _imageSource;


        public Agent(String id, List<KVP> items, AgentDataDictionary iAgentDataDictionary,Dispatcher uiThread)
        {
            ID = id;
            ImageDictionary = iAgentDataDictionary;
            
            name = iAgentDataDictionary.GetAgentNameByID(id);

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

            uiDispatcher = uiThread;
            uiDispatcher.Invoke(() =>
            {
                bg = new SolidColorBrush(Colors.LightYellow);
                brd = new SolidColorBrush(ColorAndIconAssigner.GetOrAssignColorById(id));
                if (tIt!=null) Items.Remove(tIt);
            });

            st = ElementStatus.New;
        }

        public int Account
        {
            get { return _account; }

            set
            {

                uiDispatcher.Invoke(() =>
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
                
                uiDispatcher.Invoke(() =>
                {
                    _isExpanded = value;
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
            get { return name.Equals("") ? "#" + ID : name + " (" + ID + ")"; }
        }

        public ObservableCollection<Item> itms 
        {
            get { return new ObservableCollection<Item>(Items); }
            set
            {
                uiDispatcher.Invoke(() =>
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
                uiDispatcher.Invoke(() =>
                {
                    Background = getBgColor();
                }); 
            
                NotifyPropertyChanged();
            }
        }

        public Brush Background
        {
            get { return bg; }
            set 
            {
                uiDispatcher.Invoke(() =>
                {
                    bg = value;                    
                });
                NotifyPropertyChanged();
            }
        }

        public Brush BorderBrush
        {
            get { return brd; }
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
           return ImageDictionary.GetAgentSourceByID(ID);
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