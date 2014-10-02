using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AgentsRebuilt
{
    internal class AgentState
    {
        public Clock Clock;
        public ObservableCollection <Agent> Agents = new ObservableCollection<Agent>();
        public SystemEvent Event = null;
        public ObservableCollection<Item> Auctions = new ObservableCollection<Item>();
        public ObservableCollection<Item> CommonRights = new ObservableCollection<Item>();


        //public AgentState ShallowCopy()
        //{
        //    var res = (AgentState) this.MemberwiseClone();
        //    res.Clock = Clock.ShallowCopy();
        //    res.Agents = new ObservableCollection<Agent>();
        //    Agent[] tAgents= new Agent[Agents.Count];
        //    Agents.CopyTo(tAgents, 0);
        //    foreach (var tAgent in tAgents)
        //    {
        //        res.Agents.Add(tAgent);
        //    }
        //    return res;
        //}
    }

    
}
