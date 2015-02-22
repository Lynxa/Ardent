using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentsRebuilt
{
    internal class Duo
    {
        public int StartStep;
        public int EndStep;

        public Duo(int alpha, int beta)
        {
            StartStep = alpha;
            EndStep = beta;
        }
    }
}
