using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgentsRebuilt
{
    class SystemEvent
    {
        private String Timestamp = "";
        private String _message = "";

        public String Message
        {
            get { return Timestamp + ": " + _message; }
        }

        public SystemEvent(KVP kvp, AgentDataDictionary ag) 
        {
            foreach (var k in kvp.ListOfItems)
            {
                if (k.Key == "description")
                {
                    String ts = k.Value;
                    StringBuilder res = new StringBuilder();

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
                        _message = ag.GetAgentNameByID(subject) + "(" + subject + ") " +
                                   "has successfully initialized the simulation.";
                    }
                    else if (action.StartsWith("admit"))
                    {
                         _message = ag.GetAgentNameByID(subject) + "(" + subject + ") " +
                                   "has admitted " + (!obj.Equals("") ? ag.GetAgentNameByID(obj)+ "(" + obj + ")": "" + ".");
                    }
                    else if (action.StartsWith("dismiss"))
                    {
                        _message = ag.GetAgentNameByID(subject) + "(" + subject + ") " +
                                  "has dismissed " + (!obj.Equals("") ? ag.GetAgentNameByID(obj) + "(" + obj + ")" : "" + ".");
                    }
                    else if (action.StartsWith("dismiss"))
                    {
                        _message = ag.GetAgentNameByID(subject) + "(" + subject + ") " +
                                  "has enforced the following punishments: " + obj + ".";
                    }
                    else
                    {
                        _message = ag.GetAgentNameByID(subject) + "(" + subject + ") " + successfully + " tried to perform the following action: " +
                                   action + (!obj.Equals("") ? "(" + obj + ")" : "") + ".";
                    }

                }
                if (k.Key == "happened_at")
                {
                    Timestamp = (k.Value.Contains('.')) ? k.Value.Substring(0, k.Value.LastIndexOf('.') + 2) : k.Value;
                }
            }
        }
    }
}
