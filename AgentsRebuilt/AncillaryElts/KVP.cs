using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AgentsRebuilt
{
    public class KVP 
    {
        public String Key;
        private String _value="\0";
        public List<KVP> ListOfItems = null;
        public ItemType Type = ItemType.None;
        
        
        public KVP(String key, List<KVP> list)
        {
            Key = key;
            ListOfItems = list;
          
        }

        public KVP(String key, String value)
        {
            Key = key;
            Value = value;
            Type = ItemType.Attribute;
        }

        public String Value
        {
            get { return _value; }
            set
            {
                _value = value;  
               
            }
        }

    }
}
