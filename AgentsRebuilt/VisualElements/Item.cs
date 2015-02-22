using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AgentsConfig;


namespace AgentsRebuilt
{
    public class Item : KVP, INotifyPropertyChanged
    {
        public ElementStatus st;

        private Dictionary<String, String> _stringAttributeList = new Dictionary<string, string>();
        public Dictionary<String, String> StringAttributeList
        {
            get { return _stringAttributeList; }
            set
            {
                UIDispatcher.Invoke(() =>
                {
                    _stringAttributeList = value;
                });
                NotifyPropertyChanged();
            }
        }
        public String InstanceOf;
       
        public static CfgSettings cfgSettings;
        private AgentDataDictionary ImageDictionary;

        private bool _isExpanded;
        private Brush bg;
        private Brush brd, brd2;
        private static Dispatcher UIDispatcher;
        private bool _isPaneExpanded;
        private String _owned_by="";
        private String _held_by="";

        public bool IsExpanded
        {
            get { return _isExpanded; }

            set
            {
                UIDispatcher.Invoke(() =>
                {
                    _isExpanded = value;
                });
                NotifyPropertyChanged();
            }
        }

        public String OwnedBy
        {
            get { return _owned_by; }

            set
            {
                UIDispatcher.Invoke(() =>
                {
                    _owned_by = value;
                });
                NotifyPropertyChanged();
            }
        }

        public String HeldBy
        {
            get { return _held_by; }

            set
            {
                UIDispatcher.Invoke(() =>
                {
                    _held_by = value;
                });
                NotifyPropertyChanged();
            }
        }

        public bool IsPaneExpanded
        {
            get { return _isPaneExpanded; }

            set
            {
                UIDispatcher.Invoke(() =>
                {
                    _isPaneExpanded = value;
                });
                NotifyPropertyChanged();
            }
        }


        public string Title
        {
            get { return Key; }
            set { Key = value; }
        }

        public string ElementType
        {
            get { return this.Type.ToString() + "s"; }
        }

        public Item GetNeutralCopy()
        {
            var result = new Item(Key, ListOfItems, cfgSettings, ImageDictionary, UIDispatcher);
            result.StringAttributeList = new Dictionary<string, string>();
            foreach (var keyvaluepair in StringAttributeList)
            {
                result.StringAttributeList.Add(String.Copy(keyvaluepair.Key), String.Copy(keyvaluepair.Value));
            }
            result.Status = ElementStatus.Unchanged;
            result.ImageDictionary = ImageDictionary;
            result.InstanceOf = InstanceOf;
            result.Type = this.Type;
            result.OwnedBy = String.Copy(OwnedBy);
            result.HeldBy = String.Copy(HeldBy);


            return result;
        }

        public Item GetFullCopy()
        {
            var result = new Item(Key, ListOfItems, cfgSettings, ImageDictionary, UIDispatcher);
            result.StringAttributeList = new Dictionary<string, string>();
            foreach (var keyvaluepair in StringAttributeList)
            {
                result.StringAttributeList.Add(String.Copy(keyvaluepair.Key), String.Copy(keyvaluepair.Value));
            }
            result.Status = Status;
            result.ImageDictionary = ImageDictionary;
            result.InstanceOf = InstanceOf;
            result.Type = this.Type;
            result.IsPaneExpanded = IsPaneExpanded;
            result.IsExpanded = IsExpanded;
            result.Background = Background;
            result.BorderBrush = BorderBrush;
            result.OwnedBy = String.Copy(OwnedBy);
            result.HeldBy = String.Copy(HeldBy);
            return result;
        }

        public ElementStatus Status
        {
            get { return st; }

            set
            {
                this.st = value;
                UIDispatcher.Invoke(() =>
                {
                    Background = getBgColor();
                });

                NotifyPropertyChanged();
            }
        }

        public ImageSource Source 
        {
            get { return getSource();}
            set { }
        }

        private ImageSource getSource()
        {

            String tst;
            //if (Key.StartsWith("right") || Key.StartsWith("oblig"))
            //{
            //    tst = Key.Contains("(") ? Key.Remove(Key.IndexOf("("), Key.Length - Key.IndexOf("(")) : Key;
            //}
            //else
            {
                tst = InstanceOf;
            }
            return ImageDictionary.GetItemSourceByID(tst);
                
        }


        public string Amount 
        {
            get { return getAmount();  }
            set { }
        }

        public Brush Background
        {
            get { return bg; }
            set
            {
                UIDispatcher.Invoke(() =>
                {
                    bg = value;
                });
                NotifyPropertyChanged();
            }
        }

        public Brush BorderBrush
        {
            get
            {
                return brd;
            }
            set
            {
                UIDispatcher.Invoke(() =>
                {
                    brd = value;
                });
                NotifyPropertyChanged();
            }
        }
        public Brush BorderBrush2
        {
            get { return brd2; }
            set
            {
                UIDispatcher.Invoke(() =>
                {
                    brd2 = value;
                });
                NotifyPropertyChanged();
            }
        }

        private String getAmount() 
        {
            String tp;
            if (StringAttributeList.TryGetValue("amount", out tp))
            {
                return tp;
            }
            else
            return "";
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

        private Brush getBrush()
        {
            String ownedID, heldID;
            //if (!StringAttributeList.TryGetValue("Owned by", out ownedID)) return null;
            //if (!StringAttributeList.TryGetValue("Held by", out heldID)) return null;
            if (_owned_by.Equals(_held_by))
            {
                return new SolidColorBrush(Colors.LightSlateGray);
            }
            else
            {
                return new SolidColorBrush(ColorAndIconAssigner.GetOrAssignColorById(_owned_by));
            }
        }

        private Brush getHeldBrush()
        {
            String heldID;

            if (!StringAttributeList.TryGetValue("Held by", out heldID)) return null;
            return new SolidColorBrush(ColorAndIconAssigner.GetOrAssignColorById(heldID));
        }

        public Item(String key, List<KVP> list, CfgSettings cfg, AgentDataDictionary ag, Dispatcher uiDispatcher): base(key, new List<KVP>())
        {
            cfgSettings = cfg;
            if (key.StartsWith("oblig"))
            {
               // InstanceOf = "obligation";
            }
            ImageDictionary = ag;
            UIDispatcher = uiDispatcher;
            Status = ElementStatus.Unchanged;
            
            foreach (var l in list)
            {
                if (l.Key.Equals("held_by"))
                {
                    StringAttributeList.Add("Held by", ag.GetAgentNameByID(l.Value) + " (" + l.Value + ")");
                    _held_by = l.Value;
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("owned_by"))
                {
                    StringAttributeList.Add("Owned by", ag.GetAgentNameByID(l.Value) + " (" + l.Value + ")");
                    _owned_by = l.Value;
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("instance_of"))
                {
                    StringAttributeList.Add("Instance of", l.Value);
                    InstanceOf = l.Value;
                }
                else if (l.Key.Equals("weight"))
                {
                    StringAttributeList.Add("Weight", l.Value);
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("status"))
                {
                    StringAttributeList.Add("Status", l.Value);
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("auctioneer"))
                {
                    StringAttributeList.Add("Auctioneer", ag.GetAgentNameByID(l.Value) + " (" + l.Value + ")");
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("highest_bidder"))
                {
                    StringAttributeList.Add("Highest bidder", ag.GetAgentNameByID(l.Value) + " (" + l.Value + ")");
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("last_bid_time"))
                {
                    StringAttributeList.Add("Last bid time", l.Value.Substring(0, 5));
                    ListOfItems.Remove(l);
                }
                else if (l.Key.IndexOf("_") > 0)
                {
                    String st = l.Key.Replace("_", " ");
                    st = st[0].ToString().ToUpper()[0] + st.Substring(1);
                    StringAttributeList.Add(st, l.Value);
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("item"))
                {
                    StringAttributeList.Add("Item", l.Value);
                    InstanceOf = l.Value;
                }
                
                else if (l.Type == ItemType.Attribute)
                {
                   // String st = l.Key[0].ToString().ToUpper()[0] + l.k.Substring(1);
                    StringAttributeList.Add(l.Key, l.Value);
                    ListOfItems.Remove(l);
                }
                else
                {
                    ListOfItems.Add(l);
                }
            }
            UIDispatcher.Invoke(() =>
            {
                BorderBrush = getBrush();
                BorderBrush2 = getHeldBrush();
            });
            

        }

        public Item(String key, String value, Dispatcher uiDispatcher):base(key, value)
        {
            UIDispatcher = uiDispatcher;
            UIDispatcher.Invoke(() =>
            {
                BorderBrush = getBrush();
                BorderBrush2 = getHeldBrush();
            });
            
        }

        public static Item KvpToItem(KVP src, AgentDataDictionary adata, Dispatcher uiDispatcher)
        {
            Item result;

            if (src.Value != "\0")
            {
                result = new Item(src.Key, src.Value, uiDispatcher);
                result.Type = ItemType.Attribute;
            }
            else if (src.ListOfItems!=null)
            {
                result = new Item(src.Key, src.ListOfItems, cfgSettings, adata, uiDispatcher);
                if (src.Key.StartsWith("right("))
                {
                    result.Type = ItemType.Right;
                }
                else if (src.Key.StartsWith("obligation("))
                {
                    result.Type = ItemType.Obligation;
                }
                else
                {
                    result.Type = ItemType.Asset;
                }
            }
            else throw new Exception("Empty attribute value");

            return result;
        }


        public static ObservableCollection<Item> KvpToItems(List<KVP> src, AgentDataDictionary _agentDataDictionary, Dispatcher uiDispatcher)
        {
            List<Item> result = new List<Item>();
            if (src == null) return null;

            foreach (var tm in src)
            {
                result.Add(KvpToItem(tm, _agentDataDictionary,uiDispatcher));
            }
            return new ObservableCollection<Item>(result);
        }

        public static ElementStatus Compare (Item oldItem, Item newItem)
        {
            foreach (var att in oldItem.StringAttributeList)
            {
                String ts;
                if (!newItem.StringAttributeList.TryGetValue(att.Key, out ts))
                {
                    return ElementStatus.Changed;
                }
                else
                {
                    if (!ts.Equals(att.Value) && !att.Key.Equals("Instance of"))
                    {
                        return ElementStatus.Changed;

                    }
                }
            }
            return ElementStatus.Unchanged;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        internal void RenewBrush()
        {
            UIDispatcher.Invoke(() =>
            {
                BorderBrush = getBrush();
                BorderBrush2 = getHeldBrush();
            });
        }
    }
}
