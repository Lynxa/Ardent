using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace AgentsRebuilt
{
    static class StateObjectMapper
    {
        public static AgentState MapState(KVP root,AgentDataDictionary _agentDataDictionary, Dispatcher uiThread)
        {
            ObservableCollection<Agent> agList = new ObservableCollection <Agent>();
            ObservableCollection<Item> aucList = new ObservableCollection<Item>();
            Dictionary<String, List<KVP>> agts = new Dictionary<string, List<KVP>>();
            AgentState state = new AgentState();
            foreach (var kvp in root.ListOfItems)
            {
                if (kvp.Key.Equals("clock"))
                {
                    state.Clock = new Clock(kvp);
                }
                if (kvp.Key.StartsWith("mekeke"))
                {
                    aucList.Add(Item.KvpToItem(kvp, _agentDataDictionary, uiThread));
                }
                else if (kvp.Key.StartsWith("event"))
                {
                    //VIK - for future use
                    //Vik -- the future has come. 2014-08-25
                    state.Event = new SystemEvent(kvp, _agentDataDictionary);
                }
                else
                {
                    foreach (var elt in kvp.ListOfItems)
                    {
                        if (elt.Key.Equals("held_by"))
                        {
                            if (!agts.ContainsKey(elt.Value))
                            {
                                agts.Add(elt.Value, new List<KVP> {kvp});
                            }
                            else
                            {
                                List<KVP> tl;
                                if (agts.TryGetValue(elt.Value, out tl))
                                {
                                    tl.Add(kvp);
                                }
                            }
                        }
                        if (elt.Key.Equals("owned_by"))
                        {
                            if (!agts.ContainsKey(elt.Value))
                            {
                                agts.Add(elt.Value, new List<KVP> {});
                            }
                        }
                    }
                }

            }
            foreach (var agt in agts)
            {
                agList.Add(new Agent(agt.Key, agt.Value, _agentDataDictionary,uiThread));
            }
            if (!agts.ContainsKey("god"))
            {
                agList.Add(new Agent("god", new List<KVP>(), _agentDataDictionary, uiThread));
            }
            state.Agents = agList;
            state.Auctions = aucList;
            return state;
        }

        public static void UpdateState (KVP root, AgentState oldState,AgentDataDictionary _agentDataDictionary, Dispatcher uiThread)
        {
            AgentState newState = MapState(root, _agentDataDictionary, uiThread);
            oldState.Clock.HappenedAt = newState.Clock.HappenedAt;
            oldState.Clock.ExpiredAt = newState.Clock.ExpiredAt;
            oldState.Clock.SetTextList();
            oldState.Event = newState.Event;
            UpdateAuctions(oldState.Auctions, newState.Auctions, uiThread);
            UpdateStep(oldState.Agents, newState.Agents, uiThread);
        }

        private static void UpdateAuctions(ObservableCollection<Item> oldAuctions, ObservableCollection<Item> newAuctions, Dispatcher uiThread)
        {
            var toExecute = new List<Item>();
            foreach (var agn in oldAuctions)
            {
                if (agn.Status == ElementStatus.Deleted)
                {
                    toExecute.Add(agn);
                }
                else
                {
                    agn.Status = ElementStatus.Unchanged;

                }

                Item newA = GetItemByKey(newAuctions, agn.Key);
                if (newA == null)
                {
                    agn.Status = ElementStatus.Deleted;
                }
                else
                {
                 //TODO:Update auction   UpdateAgentItems(agn, newA, uiThread);
                }
                //TODO:Update auction  
            }
            foreach (var auction1 in toExecute)
            {
                uiThread.Invoke(() =>
                {
                    oldAuctions.Remove(auction1);
                });
            }

            foreach (var auction in newAuctions)
            {
                if (!ContainsItem(oldAuctions, auction.Key))
                {
                    Item auction1 = auction;
                    uiThread.Invoke(() => oldAuctions.Add(auction1));
                }
            }
        } 

        private static void UpdateStep(ObservableCollection<Agent> oldAgents, ObservableCollection<Agent> newAgents, Dispatcher uiThread)
        {
            var AgentsToExecute = new List<Agent>();
            foreach (var agn in oldAgents)
            {
                if (agn.Status == ElementStatus.Deleted)
                {
                    AgentsToExecute.Add(agn);
                }
                else
                {
                    agn.Status = ElementStatus.Unchanged;

                }

                Agent newA = GetAgentByID(newAgents, agn.ID);
                if (newA == null)
                {
                    agn.Status = ElementStatus.Deleted;
                }
                else
                {
                    UpdateAgentItems(agn, newA, uiThread);
                }
            }
            foreach (var agn1 in AgentsToExecute)
            {
                uiThread.Invoke(() =>
                {
                    oldAgents.Remove(agn1); //GYGYGY
                });
            }

            foreach (var agent in newAgents)
            {
                if (!ContainsAgent(oldAgents, agent.ID))
                {
                    Agent agent1 = agent;
                    uiThread.Invoke(() => oldAgents.Add(agent1));
                }
            }
        }

        private static bool ContainsAgent(ObservableCollection<Agent> lst, String id)
        {
            foreach (var ag in lst)
            {
                if (ag.ID == id)
                {
                    return true;
                }
            }
            return false;
        }

        private static Agent GetAgentByID(ObservableCollection<Agent> lst, String id)
        {
            foreach (var ag in lst)
            {
                if (ag.ID == id)
                {
                    return ag;
                }
            }
            return null;
        }

        private static bool ContainsItem(ObservableCollection<Item> lst, String id)
        {
            foreach (var it in lst)
            {
                if (it.Key == id)
                {
                    return true;
                }
            }
            return false;
        }

        private static Item GetItemByKey(ObservableCollection<Item> lst, String key)
        {
            foreach (var item in lst)
            {
                if (item.Key == key)
                {
                    return item;
                }
            }
            return null;
        }

        private static void UpdateAgentItems(Agent oldAgent, Agent newAgent, Dispatcher uiThread)
        {
            var ItemsToRemove = new List<Item>();
            
            foreach (var item in oldAgent.Items)
            {
                if (item.Status == ElementStatus.Deleted)
                {
                    ItemsToRemove.Add(item);
                }
                else
                {
                    Item newI = GetItemByKey(newAgent.Items, item.Key);
                    if (newI == null)
                    {
                        item.Status = ElementStatus.Deleted;
                    }
                    else
                    {
                        ElementStatus st = Item.Compare(item, newI);
                        if (st != ElementStatus.Unchanged) 
                        {
                            UpdateItem(item, newI);
                        }
                        item.Status = st;                        
                    }

                }
            }

            foreach (var item1 in ItemsToRemove)
            {
                uiThread.Invoke(() =>
                {
                    oldAgent.Items.Remove(item1);

                });
            }

            foreach (var item in newAgent.Items)
            {
                if (!ContainsItem(oldAgent.Items, item.Key))
                {
                    Item item1 = item;
                    item1.Status = ElementStatus.New;
                    ObservableCollection<Item> nIt = oldAgent.Items;
                    nIt.Add(item1);

                    uiThread.Invoke(() => oldAgent.itms = nIt); //Vik -- kostyl'!!!!!!!

                }
            }
        }

        private static void UpdateItem(Item oldItem, Item newItem)
        {
            List<String> for_removal = new List<string>();
            Dictionary<String, String> for_addition = new Dictionary<string,string>();
            foreach (var att in oldItem.StringAttributeList)
            {
                String ts;
                if (!newItem.StringAttributeList.TryGetValue(att.Key, out ts))
                {
                    for_removal.Add(ts);
                }
                else
                {
                    if (!ts.Equals(att.Value))
                    {
                        for_removal.Add(att.Key);
                        for_addition.Add(att.Key, ts);
                    }
                }
            }
            foreach (var t in for_removal) { oldItem.StringAttributeList.Remove(t); }
            foreach (var t in for_addition) { oldItem.StringAttributeList.Add(t.Key, t.Value); }
            oldItem.RenewBrush();
        }
    } 
}
