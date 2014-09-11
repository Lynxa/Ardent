using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class Clock
    {
        public String ExpiredAt;
        public String HappenedAt;

        public Clock(KVP kvp)
        {
            foreach (var n in kvp.ListOfItems)
            {
                if (n.Key.Equals("expired_at"))
                {
                    ExpiredAt = n.Value;
                }
                if (n.Key.Equals("expired_at"))
                {
                    HappenedAt = n.Value;
                }
            }
        }
    }
}
