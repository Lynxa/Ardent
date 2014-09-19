using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentsConfig;

namespace AgentsRebuilt
{
    internal class Clock
    {
        public String ExpiredAt;
        public String HappenedAt;
        public List<CfgStr> TextList;
        public List<CfgStr> TextListNames;
        private int _stepNo = 0;

        public Clock(KVP kvp)
        {
            foreach (var n in kvp.ListOfItems)
            {
                if (n.Key.Equals("expired_at"))
                {
                    ExpiredAt = (n.Value.Contains('.')) ?  n.Value.Substring(0, n.Value.LastIndexOf('.') + 2):n.Value ;
                    ExpiredAt = ToTime(ExpiredAt);
                }
                if (n.Key.Equals("happened_at"))
                {
                    HappenedAt = (n.Value.Contains('.')) ? n.Value.Substring(0, n.Value.LastIndexOf('.') + 2): n.Value ;
                    HappenedAt = ToTime(HappenedAt);
                }
            }
            //TextList = new List<CfgStr>() { new CfgStr("Happened at: \t" + HappenedAt), new CfgStr("Expired at: \t" + ExpiredAt), new CfgStr("Step number: \t" + "0") };

            TextList = new List<CfgStr>() { new CfgStr(HappenedAt), new CfgStr(ExpiredAt), new CfgStr("0") };
            TextListNames = new List<CfgStr>() { new CfgStr("Happened at:"), new CfgStr("Expired at:"), new CfgStr("Step number:") };
        }

        public int StepNo
        {
            get { return _stepNo; }
            set { _stepNo = value; }
        }

        internal void SetTextList()
        {
            //TextList[0].Content = "Happened at: \t"+ HappenedAt;
            //TextList[1].Content = "Expired at: \t" + ExpiredAt;
            //TextList[2].Content = "Step number: \t" +(_stepNo).ToString();

            TextList[0].Content = HappenedAt;
            TextList[1].Content = ExpiredAt;
            TextList[2].Content = (_stepNo).ToString();
        }


        // Convert given string with seconds to foramted string in foramt hh:mm:ss.ms
        private string ToTime(string TimeString)
        {
            string[] split = TimeString.Split(new Char[] { '.' }); // separate seconds from miliseconds
            string milisecs = (split.Length == 2) ? split[1] : ""; // get miliseconds if any, if not - empty string
            int secs;
            if (Int32.TryParse(split[0], out secs))
            {
                int mins = 0;   // default
                int hours = 0;  // default
                // get minutes
                if (secs > 0)
                {
                    mins = secs / 60;
                    secs = secs % 60;
                }
                // get hours
                if (mins > 0)
                {
                    hours = mins / 60;
                    mins = mins % 60;
                }
                string TimeFormat = "{1:D2}:{2:D2}"; // normal time format
                if (hours > 0) TimeFormat = "{0:D2}:" + TimeFormat; // add hours to time, if at least one has passed
                if (milisecs.Length != 0) TimeFormat = TimeFormat + ".{3}"; // add miliseconds to time, if any
                TimeString = String.Format(TimeFormat, hours, mins, secs, milisecs); // return formated string
            }
            return TimeString; // return formated string or given string, if can't parse to int
        }
    }
}
