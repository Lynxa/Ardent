using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Media;
using System.Windows.Controls;
using System.Windows.Media;

namespace AgentsRebuilt
{
    public static class ColorAndIconAssigner
    {
        //private static String defaultIconPath = @"C:\Users\VLEONOVA\Desktop\PES602\AgentsRebuilt\AgentsRebuilt\Images\";
        private static String defaultIconPath = @"D:\C# Projects\AgentsRebuilt\AgentsRebuilt\Images\";
        public static String defaultImage = @"D:\C# Projects\AgentsRebuilt\AgentsRebuilt\Images\default_agent.jpg";
       // public static String defaultImage = @"C:\Users\VLEONOVA\Desktop\PES602\AgentsRebuilt\AgentsRebuilt\Images\default_agent.jpg";
        private static String defaultIconExt = ".jpg";
        private static Dictionary<int, String> availableIconNames = new Dictionary<int, String>()
        {
            {0, "agent00"},
            {1, "agent01"}, 
            {2, "agent02"}, 
            {3, "agent03"}, 
            {4, "agent04"} 
        }
    
        ;
        private static Dictionary<int, Color> distinctiveColors = new Dictionary<int, Color>() 
        {
            {0, Colors.Aqua}, 
            {1, Colors.Orchid}, 
            {3, Colors.BlueViolet},
            {2,Colors.Blue}, 
            {4, Colors.Red}, 
            {5, Colors.Green},
            {6, Colors.Orange},
            {7, Colors.GreenYellow}, 
        } 
        ;
        private static List<Color> usedColors = new List<Color>();
        private static List<String> usedIcons = new List<String>();

        private static Color defaultColor = Colors.Gray;
        private static Dictionary<String, Color> colorDict = new Dictionary<string, Color>();
        private static Dictionary<String, String> iconDict = new Dictionary<string, string>();

        public static Color GetOrAssignColorById(String id) 
        { 
            Color c;
            if (colorDict.TryGetValue(id, out c))
            {
                return c;
            }
            else 
            {
                if (colorDict.Count < distinctiveColors.Count && distinctiveColors.TryGetValue(colorDict.Count, out c))
                {
                    colorDict.Add(id, c);
                    usedColors.Add(c);
                    return c;
                }
                else 
                {
                  //  do
                    {
                        Random rnd = new Random();
                        int num = rnd.Next(0, 140); //count of system.media.colors
                        c = defaultColor; //THIS IS SUBJECT TO CHANGE BUT IT SEEMS WITH LISTING ALL SYSTEM.MEDIA.COLORS
                    }
                  //  while (usedColors.Contains(c));
                    return c;
                }
            }            
        }

        public static String GetOrAssignIconById(String id)
        {
            String c;
            if (iconDict.TryGetValue(id, out c))
            {
                return Path.Combine(defaultIconPath, c + defaultIconExt);
            }
            else
            {
                if (iconDict.Count < availableIconNames.Count && availableIconNames.TryGetValue(colorDict.Count, out c))
                {
                    iconDict.Add(id, c);
                    usedIcons.Add(c);
                    c = Path.Combine(defaultIconPath, c + defaultIconExt);
                    if (!File.Exists(c)) c = defaultImage;
                    return c;
                }
                else
                {
                    Random rnd = new Random();
                    int num = rnd.Next(0, availableIconNames.Count);
                    iconDict.Add(id, availableIconNames[num]);

                    c = Path.Combine(defaultIconPath, c + defaultIconExt);
                    if (!File.Exists(c)) c = defaultImage;
                    return c;
                }
            }
        }

        public static Color GetOrAssignBackgroundColorById(string id)
        {
            if (id.Equals("god")) return Colors.White;
            Color agentColor, c;
            if (colorDict.TryGetValue(id, out c))
            {
                agentColor = c;
            }
            else
            {
                if (colorDict.Count < distinctiveColors.Count && distinctiveColors.TryGetValue(colorDict.Count, out c))
                {
                    colorDict.Add(id, c);
                    usedColors.Add(c);
                    agentColor = c;
                }
                else
                {
                    //  do
                    {
                        Random rnd = new Random();
                        int num = rnd.Next(0, 140); //count of system.media.colors
                        c = defaultColor; //THIS IS SUBJECT TO CHANGE BUT IT SEEMS WITH LISTING ALL SYSTEM.MEDIA.COLORS
                        colorDict.Add(id, c);
                    }
                    //  while (usedColors.Contains(c));
                    agentColor = c;
                }
            }
            if (c == Colors.Aqua) { return Colors.Azure; }
            if (c == Colors.Orchid) { return Colors.SeaShell; }
            if (c == Colors.BlueViolet) { return Colors.GhostWhite; }
            if (c == Colors.Blue) { return Colors.AliceBlue; }
            if (c == Colors.Red) { return Colors.Linen; }
            if (c == Colors.Green) { return Colors.MintCream; }
            if (c == Colors.Orange) { return Colors.FloralWhite; }
            if (c == Colors.GreenYellow) { return Colors.Honeydew; }
            return agentColor;
        }
    }
}
