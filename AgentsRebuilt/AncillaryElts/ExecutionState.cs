using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentsRebuilt
{
    public enum ExecutionState
    {
        Running,
        Following,
        Moving,
        Paused,
        Stopped,
        Switching,
        Void
    }
}
