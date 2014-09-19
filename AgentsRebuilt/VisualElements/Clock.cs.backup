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
        private int _stepNo = 0;

        public Clock(KVP kvp)
        {
            foreach (var n in kvp.ListOfItems)
            {
                if (n.Key.Equals("expired_at"))
                {
                    ExpiredAt = (n.Value.Contains('.')) ?  n.Value.Substring(0, n.Value.LastIndexOf('.') + 2):n.Value ;
                }
                if (n.Key.Equals("happened_at"))
                {
                    HappenedAt = (n.Value.Contains('.')) ? n.Value.Substring(0, n.Value.LastIndexOf('.') + 2): n.Value ;
                }
            }
            TextList = new List<CfgStr>() { new CfgStr("Happened at: \t" + HappenedAt), new CfgStr("Expired at: \t" + ExpiredAt), new CfgStr("Step number: \t" + "0") };
        }

        public int StepNo
        {
            get { return _stepNo; }
            set { _stepNo = value; }
        }

        internal void SetTextList()
        {
            TextList[0].Content = "Happened at: \t"+ HappenedAt;
            TextList[1].Content = "Expired at: \t" + ExpiredAt;
            TextList[2].Content = "Step number: \t" +(_stepNo).ToString();
        }
    }
}
