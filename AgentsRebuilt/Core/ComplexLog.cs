using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AgentsRebuilt
{
    internal class ComplexLog
    {
        private List<Dictionary<string, AgentState>> _dict = new List<Dictionary<string, AgentState>>();


        public void UpdateStatesAgentsItems (ObservableCollection<Agent> allAgents, ObservableCollection<Item> allItems, String agentID)
        {
            foreach (var dcc in _dict)
            {
                AgentState tpState;
                dcc.TryGetValue(agentID, out tpState);
                if (tpState != null)
                {
                    tpState.AllAgents = allAgents;
                    tpState.AllItems = allItems;
                }
            }
        }

        public AgentState this[String agent, int i]
        {
            get
            {
                AgentState hState;
                if (_dict[i].TryGetValue(agent, out hState)) return hState;
                return null;
            }
        }

        private Dictionary<String, AgentState> GetDictByTimeStamp(AgentState ag)
        {
            String hAt = ag.Clock.TimeStampH;
            foreach (var dict in _dict)
            {
                foreach (var agentState in dict.Values)
                {
                    if (agentState.Clock.TimeStampH.Equals(hAt))
                    {
                        return dict;
                    }
                }
            }
            return null;
        }

        internal void Add(AgentState state, String ag)
        {
            var dc = GetDictByTimeStamp(state);
            if (dc != null && !dc.ContainsKey(ag))
            {
                dc.Add(ag, state);
            }
            else
            {
                var ddc = new Dictionary<String, AgentState>();
                ddc.Add(ag, state);
                _dict.Add(ddc);
            }

        }

        public int Count 
        {
            get { return _dict.Count; } 
        }
    }
}