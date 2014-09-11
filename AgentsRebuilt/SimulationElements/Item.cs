using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public class Item  : KVP
    {
        public ItemType Type;

        public Item(String key, List<KVP> list): base(key, list)
        {

        }

        public Item(String key, String value):base(key, value)
        {
        }

        public static Item KvpToItem(KVP src)
        {
            Item result;
            if (src.Value != "\0")
            {
                result = new Item(src.Key, src.Value);
                result.Type = ItemType.Asset;
            }
            else if (src.ListOfItems!=null)
            {
                result = new Item(src.Key, src.ListOfItems);
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


        public static List<Item> KvpToItems(List<KVP> src)
        {
            List<Item> result = new List<Item>();

            foreach (var tm in src)
            {
                result.Add(KvpToItem(tm));
            }
            return result;
        }
    }
}
