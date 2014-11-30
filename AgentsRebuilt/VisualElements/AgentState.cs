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
        public SystemEvent Event = null;
        public ObservableCollection <Agent> Agents = new ObservableCollection<Agent>();
        public ObservableCollection<Item> Auctions = new ObservableCollection<Item>();
        public ObservableCollection<Item> CommonRights = new ObservableCollection<Item>();
        public ObservableCollection<Agent> AllAgents = new ObservableCollection<Agent>();
        public ObservableCollection<Item> AllItems = new ObservableCollection<Item>();

        public AgentState GetFullCopy()
        {
            var result = new AgentState();
            result.Clock = Clock.GetFullCopy();

            result.Event = Event == null ? null:Event.GetFullCopy();

            ObservableCollection<Agent> ti = new ObservableCollection<Agent>();
            foreach (var agent in Agents)
            {
                ti.Add(agent.GetFullCopy());
            }
            result.Agents = ti;

            ti = new ObservableCollection<Agent>();
            foreach (var agent in AllAgents)
            {
                ti.Add(agent.GetFullCopy());
            }
            result.AllAgents = ti;

            ObservableCollection<Item> tii = new ObservableCollection<Item>();
            foreach (var allItem in AllItems)
            {
                tii.Add(allItem.GetFullCopy());
            }
            result.AllItems = tii;

            tii = new ObservableCollection<Item>();
            foreach (var allItem in Auctions)
            {
                tii.Add(allItem.GetFullCopy());
            }
            result.Auctions = tii;

            tii = new ObservableCollection<Item>();
            foreach (var allItem in CommonRights)
            {
                tii.Add(allItem.GetFullCopy());
            }
            result.CommonRights = tii;

            return result;
        }



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
