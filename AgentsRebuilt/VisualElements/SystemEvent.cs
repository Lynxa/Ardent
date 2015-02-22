using System;
using System.Linq;
using System.Text;

namespace AgentsRebuilt
{
    class SystemEvent
    {
        private readonly String _timestamp = "";
        private readonly String _message = "";
        private readonly int _stateNum = 0;

        public SystemEvent GetFullCopy()
        {
            return new SystemEvent(_timestamp, _message);
        }

        public String Message
        {
            get { return Clock.ToTime(_timestamp) + ": " + _message; }
        }

        public SystemEvent(String time, String msg)
        {
            this._timestamp = time;
            this._message = msg;
        }

        public SystemEvent(String time, int statenum, String msg)
        {
            this._timestamp = time;
            this._message = msg;
            this._stateNum = statenum;
        }

        public SystemEvent(KVP kvp, AgentDataDictionary ag) 
        {
            foreach (var k in kvp.ListOfItems)
            {
                if (k.Key == "description")
                {
                    String ts = k.Value;
                    
                    ts = ts.Remove(0, k.Value.IndexOf("(") + 1);
                    ts = ts.Remove(ts.Length - 1, 1);

                    String subject = ts.Substring(0, ts.IndexOf(","));
                    ts = ts.Remove(0, ts.IndexOf(",") + 1);

                    String action = ts.Substring(0, ts.LastIndexOf(","));
                    ts = ts.Remove(0, ts.LastIndexOf(",") + 1);

                    String successfully = ts;

                    String obj = "";

                    if (action.Contains("("))
                    {
                        obj = action.Substring(action.IndexOf("(")+1, action.LastIndexOf(")")-action.IndexOf("(")-1);
                        action = action.Remove(action.IndexOf("("), action.LastIndexOf(")") - action.IndexOf("(")+1);
                    }

                    if (action.StartsWith("initializ"))
                    {
                        _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " +
                                   "has successfully initialized the simulation.";
                    }
                    else if (action.StartsWith("admit"))
                    {
                        _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " +
                                   "has admitted " + (!obj.Equals("") ? ag.GetAgentNameByID(obj)+ "(" + obj + ")": "" + ".");
                    }
                    else if (action.StartsWith("dismiss"))
                    {
                        _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " +
                                  "has dismissed " + (!obj.Equals("") ? ag.GetAgentNameByID(obj) + "(" + obj + ")" : "" + ".");
                    }
                    else if (action.StartsWith("dismiss"))
                    {
                        _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " +
                                  "has enforced the following punishments: " + obj + ".";
                    }
                    else if (successfully.Equals("successfully"))
                    {
                        if (action.StartsWith("open_auction"))
                        {
                            String _objAuction = obj.Substring(0, obj.IndexOf(','));
                            String obj1 = (obj.Remove(0, obj.IndexOf(',') + 1)).Replace(" ", "");
                            String _objItem = obj1.Substring(0, obj1.IndexOf(","));
                            String _objPrice = (obj1.Remove(0, obj1.IndexOf(',') + 1));

                            _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " + "has created the auction \"" + _objAuction + "\" for " +
                                       _objItem + " with opening price of " + _objPrice;
                        }
                        else if (action.StartsWith("close_auction"))
                        {
                            _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " + "has closed the auction \"" +
                                       obj +"\"";
                        }
                        else if (action.StartsWith("place_bid"))
                        {
                            String _objAuction = obj.Substring(0, obj.IndexOf(','));
                            String _objPrice = (obj.Remove(0, obj.IndexOf(',') + 1)).Replace(" ", "");
                            _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " + "has placed the bid of " + _objPrice +
                                       " in auction \"" + _objAuction +"\".";
                        }
                        else if (action.StartsWith("sell"))
                        {
                            String _objItem = obj.Substring(0, obj.IndexOf(','));
                            String obj1 = (obj.Remove(0, obj.IndexOf(',') + 1)).Replace(" ", "");
                            String _objPrice = obj1.Substring(0, obj1.IndexOf(","));
                            String _objAgent = (obj1.Remove(0, obj1.IndexOf(',') + 1));
                            _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " + "has sold the item " + _objItem +
                                       " for " +_objPrice +" to " + ag.GetAgentNameByID(_objAgent) + "(" + _objAgent + ")";
                        }
                        else if (action.StartsWith("flip_coin"))
                        {
                            _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " + "has flipped the coin.";
                        }
                        else
                        {
                            _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " + successfully +
                                       " performed the following action: " +
                                       action + (!obj.Equals("") ? "(" + obj + ")" : "") + ".";
                        }
                    }
                    else
                    {
                        _message = "Agent " + ag.GetAgentNameByID(subject) + "(" + subject + ") " + successfully +
                                   " tried to perform the following action: " +
                                   action + (!obj.Equals("") ? "(" + obj + ")" : "") + ".";
                    }


                }
                if (k.Key == "happened_at")
                {
                    _timestamp = (k.Value.Contains('.')) ? k.Value.Substring(0, k.Value.LastIndexOf('.') + 2) : k.Value;
                }
            }
        }
    }
}
