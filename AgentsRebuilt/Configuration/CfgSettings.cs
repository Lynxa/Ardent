using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;


namespace AgentsRebuilt
{
    public class CfgSettings : INotifyPropertyChanged
    {
       
        public String DammagePath;
        public String HistPath;
        public String ConfigPath;
        public List<CfgStr> TextList;

    
        public CfgSettings()
        {
        }

        public CfgSettings(String _histPath, String _dmgPath, String filename)
        {
            HistPath = _histPath;
            DammagePath = _dmgPath;
            ConfigPath = filename;
            TextList = new List<CfgStr>() { new CfgStr("Log file path: " + _histPath), new CfgStr("Dammage folder path: " + _dmgPath) };
        }

        public static CfgSettings LoadFromFile(String filename, out bool success)
        {
            String histPath, dmgPath;
            XmlDocument xmlDoc;
            
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);

                histPath = xmlDoc.GetElementById("HistPath").InnerText;
                dmgPath = xmlDoc.GetElementById("DmgPath").InnerText;

                success = true;
                return new CfgSettings(histPath, dmgPath, filename);

            }
            catch(Exception)
            {
                success = false;
                return null;
            }
        }

        public static void WriteToFile(CfgSettings cfg)
        {
            XmlDocument xmlDoc;
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(cfg.ConfigPath);
                xmlDoc.GetElementById("HistPath").InnerText = cfg.HistPath;
                xmlDoc.GetElementById("DmgPath").InnerText = cfg.DammagePath;
                xmlDoc.Save(cfg.ConfigPath);
                WritePathToFile("path_to_config.txt", cfg.ConfigPath);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetTextList()
        {
            TextList[0].Content = "Log file path: " + HistPath;
            TextList[1].Content = "Dammage folder: " + DammagePath;
        }

        public static void WritePathToFile(string filename, String text)
        {
            var f = File.CreateText(filename);
            f.WriteLine(text);
            f.Close();
        }
    }
}
