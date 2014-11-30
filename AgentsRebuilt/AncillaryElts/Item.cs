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
        public Dictionary<String, String> StringAttributeList;
        public String InstanceOf;
       
        public static CfgSettings cfgSettings;
        private AgentDataDictionary ImageDictionary;
        
        private Brush bg;
        private Brush brd, brd2;
        private static Dispatcher UIDispatcher;

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

            return result;
        }

        public Item GetFullCopy()
        {
            var result = new Item(Key, ListOfItems, cfgSettings, ImageDictionary, UIDispatcher);
            result.StringAttributeList = new Dictionary<string, string>();

            result.Status = Status;
            result.ImageDictionary = ImageDictionary;
            result.InstanceOf = InstanceOf;
            result.Type = this.Type;
            result.Background = Background;
            result.BorderBrush = BorderBrush;
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
            if (!StringAttributeList.TryGetValue("Owned by", out ownedID)) return null;
            if (!StringAttributeList.TryGetValue("Held by", out heldID)) return null;
            if (ownedID.Equals(heldID))
            {
                return new SolidColorBrush(Colors.LightSlateGray);
            }
            else
            {
                return new SolidColorBrush(ColorAndIconAssigner.GetOrAssignColorById(ownedID));
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
            ImageDictionary = ag;
            UIDispatcher = uiDispatcher;
            Status = ElementStatus.Unchanged;
            
            StringAttributeList = new Dictionary<string, string>();
            foreach (var l in list)
            {
                if (l.Key.Equals("held_by"))
                {
                    StringAttributeList.Add("Held by", l.Value);
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("owned_by"))
                {
                    StringAttributeList.Add("Owned by", l.Value);
                    ListOfItems.Remove(l);
                }
                else if (l.Key.Equals("instance_of"))
                {
                    InstanceOf = l.Value;
                }
                else if (l.Type == ItemType.Attribute)
                {
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
                    if (!ts.Equals(att.Value))
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
