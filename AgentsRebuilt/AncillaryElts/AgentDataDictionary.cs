using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AgentsRebuilt.Properties;


namespace AgentsRebuilt
{
    public class AgentDataDictionary
    {

        private CfgSettings conf;
        private static List<KVP> _agentDataList = new List<KVP>();
        private static List<KVP> _itemDataList = new List<KVP>();
        private static List<String> _specialList = new List<string>();

        public AgentDataDictionary(CfgSettings cfg)
        {
            //String tpath = Path.Combine(cfg.DammagePath, @"domains\visual.pl");
            String tpath = cfg.DammagePath;
            _agentDataList = LogProcessor.GetAgentData(tpath);
            _itemDataList = LogProcessor.GetItemData(tpath);
            _specialList = LogProcessor.GetSpecialData(tpath);
            conf = cfg;
        }

        public String GetAgentNameByID(String id)
        {
            foreach (var kvp in _agentDataList)
            {
                if (kvp.Key == id)
                {
                    foreach (var l in kvp.ListOfItems)
                    {
                        if (l.Key == "name") return l.Value.Replace("\"", "");
                    }
                }
            }
            return "";
        }

        public ImageSource GetAgentSourceByID(String id)
        {
            String def = "", result = "";
            foreach (var kvp in _agentDataList)
            {
                if (kvp.Key == id)
                {
                    foreach (var l in kvp.ListOfItems)
                    {
                        if (l.Key == "image")
                        {
                            result = l.Value.Remove(0, 1);
                            result = result.Remove(result.Length - 1, 1);
                            break;
                        }
                    }
                }
                if (kvp.Key == "default")
                {
                    foreach (var l in kvp.ListOfItems)
                    {
                        if (l.Key == "image")
                        {
                            def = l.Value.Remove(0, 1);
                            def = def.Remove(def.Length - 1, 1);
                            break;
                        }
                    }
                }

            }
            if (!result.Equals("")) result = Path.Combine(Path.GetDirectoryName(conf.DammagePath), result);
            if (File.Exists(result)) return new BitmapImage(new Uri(result));

            if (!def.Equals("")) def = Path.Combine(Path.GetDirectoryName(conf.DammagePath), def);
            if (File.Exists(def)) return new BitmapImage(new Uri(def));

            Stream s = this.GetType().Assembly.GetManifestResourceStream("AgentsRebuilt.default_agent.png");
            Bitmap bmp = new Bitmap(s);
            s.Close();

            return ToBitmapSource(bmp);

        }

        public ImageSource GetItemSourceByID(String id)
        {
            String def = "", result = "";
            foreach (var kvp in _itemDataList)
            {
                if (kvp.Key == id)
                {
                    result = kvp.Value.Remove(0, 1);
                    result = result.Remove(result.Length - 1, 1);
                }

                if (kvp.Key == "default")
                {
                    def = kvp.Value.Remove(0, 1);
                    def = def.Remove(def.Length - 1, 1);
                }
            }
            if (!result.Equals("")) result = Path.Combine(Path.GetDirectoryName(conf.DammagePath), result);
            if (File.Exists(result)) return new BitmapImage(new Uri(result));

            if (!def.Equals("")) def = Path.Combine(Path.GetDirectoryName(conf.DammagePath), def);
            if (File.Exists(def)) return new BitmapImage(new Uri(def));


            Stream s = this.GetType().Assembly.GetManifestResourceStream("AgentsRebuilt.default_item.jpg");
            Bitmap bmp = new Bitmap(s);
            s.Close();

            return ToBitmapSource(bmp);

        }

        public static bool IsSpecialItem(String s)
        {
            foreach (var itm in _specialList)
            {
                if (s.StartsWith(itm)) return true;
            }
            return false;
        }

        public static BitmapSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;


        }
        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }
    }


}
