using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AgentsRebuilt
{
    class AgentState
    {
        public Clock Clock;
        public ObservableCollection <Agent> Agents = new ObservableCollection<Agent>();
        public SystemEvent Event = null;

    }
}
