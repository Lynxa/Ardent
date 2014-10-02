using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Annotations;
using System.Windows.Forms;
using System.Windows.Threading;

namespace AgentsRebuilt
{


    internal class LogProcessor : INotifyPropertyChanged
    {
        //private static List<String> _log;
        private ComplexLog _states;
        private List<String> _agts = new List<string>() {"god"};
        private List<String> _agentsWithLogAvailable = new List<string>() {"god"};
        private Dictionary<String, int> _stateTimeStampDictionary = new Dictionary<string, int>();
        private Dictionary<String, Duo> _agentSteps = new Dictionary<string, Duo>();
        

        private String _currentAgent ;

        private AgentDataDictionary _dataDictionary;
        private Dispatcher _mainDispatcher;

        private int _index = 0;
        private int _latency = 0;

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                NotifyPropertyChanged();
            }
        }

        
        public int GetNumber
        {
            get { return _states.Count; }
        }

        public string CurrentAgent
        {
            get { return _currentAgent; }
            set
            {
                _currentAgent = value;
                NotifyPropertyChanged();
            }
        }

        public void SetCurrentLatency()
        {
            _latency = _states.Count - _index;
        }

        public bool SetIndex(int num)
        {
            if (_states == null) return false;
            if (num > -1 && num < _states.Count)
            {
                Index = num;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void InitOrReread(String filename, AgentDataDictionary _agentData, Dispatcher _dispatcher, String curAgent)
        {
            //VIK -- add check for whether file exists
            _dataDictionary = _agentData;
            _mainDispatcher = _dispatcher;
            _states = new ComplexLog();
            _stateTimeStampDictionary = new Dictionary<string, int>();

            _currentAgent = curAgent;
            int tCounter = 0;
            var _log = System.IO.File.ReadAllLines(filename).ToList<String>();
            _log = RemoveEmpty(_log);

            foreach (var line in _log)
            {
                KVP tResKvp = null;
                tResKvp = DecipherLine(line);
                if (tResKvp != null)
                {
                    AgentState tst = null;
                    try
                    {
                        tst = StateObjectMapper.MapState(tResKvp, _dataDictionary, _mainDispatcher);
                        
                    }
                    catch(Exception)
                    {
                        //can write something somewhere in running log, but I seriously doubt this program will ever be that sophisticated.
                    }
                    if (tst != null)
                    {
                        tst.Clock.StepNo = tCounter;
                        _stateTimeStampDictionary.Add(tst.Clock.HappenedAt, tCounter);
                        
                        _states.Add(tst, "god");
                        
                        foreach (var ag in tst.Agents)
                        {
                            if (!_agts.Contains(ag.ID))
                            {
                                _agts.Add(ag.ID);
                            }
                            if (!_agentSteps.ContainsKey(ag.ID))
                            {
                                _agentSteps.Add(ag.ID, new Duo(tCounter, 0));
                            }
                            else
                            {
                                Duo tb;
                                _agentSteps.TryGetValue(ag.ID, out tb);
                                tb.EndStep = tCounter;
                            }
                        }

                        tCounter++;

                    }

                }
            }

            foreach (var ag in _agts)
            {
                String agFile = filename.Remove(filename.LastIndexOf("."), 3);
                agFile += "_" + ag + ".db";
                if (File.Exists(agFile))
                {
                    List<String> agLog = File.ReadAllLines(agFile).ToList<String>();
                    agLog = RemoveEmpty(agLog);
                    _agentsWithLogAvailable.Add(ag);
                    foreach (var l2 in agLog)
                    {
                        KVP tKvp = null;
                        tKvp = DecipherLine(l2);
                        if (tKvp != null)
                        {
                            AgentState tst2 = null;
                            try
                            {
                                tst2 = StateObjectMapper.MapState(tKvp, _dataDictionary, _mainDispatcher);
                            }
                            catch (Exception)
                            {
                            }
                            if (tst2 != null)
                            {
                                int tStep;
                                _stateTimeStampDictionary.TryGetValue(tst2.Clock.HappenedAt, out tStep);
                                tst2.Clock.StepNo = tStep;
                                _states.Add(tst2, ag);
                            }
                        }
                    }

                }   
            }

            _latency = GetNumber;
        }

        public bool IsAgentLogAvailable(String id)
        {
            return _agentsWithLogAvailable.Contains(id);
        }

        public bool GetNextLine(out AgentState result, ExecutionState execState)
        {
            AgentState sTate = null;
            if (execState == ExecutionState.Running )
            {
                if (_states.Count > Index)
                {
                    sTate = _states[_currentAgent,Index];
                    Index++;
                    _latency--;
                }
            }
            else if (execState == ExecutionState.Moving)
            {
                if (Index==0 && _states.Count > 0)
                {
                    sTate = _states[_currentAgent, Index];
                    Index++;
                    _latency--;
                }
                else if (_states.Count > Index && Index>0)
                {
                    sTate = new AgentState();
                    var sTate1 = _states[_currentAgent, Index-1];
                    var state2 = _states[_currentAgent, Index];
                    StateObjectMapper.UpdateState(sTate1, sTate, _dataDictionary, _mainDispatcher);
                    //StateObjectMapper.UpdateState(state2, sTate, _dataDictionary, _mainDispatcher);
                    Index++;
                    _latency--;
                }

            }
            else if (execState == ExecutionState.Following)
            {
                if (_states.Count > Index && _latency <= (_states.Count - Index))
                {
                    sTate = _states[_currentAgent, Index];
                    Index++;
                }
            }
            result = sTate;
            return (sTate!=null);
        }

        public bool GetThisLine(out AgentState result, ExecutionState execState)
        {
            AgentState sTate = null;
            if (_states.Count > Index && Index > 0)
            {
                sTate = _states[_currentAgent, Index-1];
            }

            result = sTate;
            return (sTate != null);
        }

        public bool GetFirstLine(out AgentState result, ExecutionState execState)
        {
            AgentState sTate = null;
            if (_states.Count > 0)
            {
                sTate = _states[_currentAgent, 0]; //TODO REFACTORING FOR AGENT VIEW
            }
            Index = 0;
            result = sTate;
            return (sTate != null);
        }

        public bool GetLastLine(out AgentState result)
        {
            Index = _states.Count - 1;
            var sTate = _states[_currentAgent, Index];
            result = sTate;
            return (sTate != null);
        }


        public static KVP DecipherLine(String msg)
        {
            try
            {
                String line = msg.Replace(" ", "");
                line = line.Replace("\r", "");
                line = line.Replace("\n", "");
                StringBuilder ss = new StringBuilder(line); //proverit', ne odli li tam chasy
                ss.Remove(0, 6); // length of "state(["
                ss.Remove(ss.Length - 2, 2); //length of ");"
                KVP root = new KVP("state", IsolateList(ss.ToString()));
                return root;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected static KVP IsolateValuepair(String piece)
        {
            if (piece[0].Equals('(') && piece[piece.Length - 1].Equals(')') && piece.IndexOf(',') > 0)
            {
                String s1 = piece.Substring(1, piece.IndexOf(',') - 1);
                String s2 = piece.Substring(piece.IndexOf(',') + 1, piece.Length - piece.IndexOf(',') - 2);
                if (s2[0].Equals('[') && s2[s2.Length - 1].Equals(']'))
                {
                    return new KVP(s1, IsolateList(s2));
                }
                return new KVP(s1, s2);
            }
            else
                throw new Exception("Format!");

        }

        private static List<KVP> IsolateList(string src)
        {
            List<KVP> lst = new List<KVP>();
            StringBuilder sd = new StringBuilder(src.Substring(1, src.Length - 2));
            StringBuilder tStr = new StringBuilder();
            int ind = 0;
            Stack<Char> st = new Stack<Char>();
            while (!sd.ToString().Equals(""))
            {

                while ((st.Count() != 0 && ind < sd.Length) || (ind == 0))
                {
                    if (sd[ind] == '(') st.Push('(');
                    if (sd[ind] == ')') st.Pop();
                    tStr.Append(sd[ind]);
                    ind++;
                }
                if (!tStr.ToString().Equals(""))
                {
                    lst.Add(IsolateValuepair(tStr.ToString()));
                }
                sd.Remove(0, tStr.Length);
                if (sd.Length > 0 && sd[0] == ',')
                {
                    sd.Remove(0, 1);
                }
                ind = 0;
                tStr.Remove(0, tStr.Length);
            }
            return lst;
        }

        protected static List<String> RemoveEmpty(List<String> lst)
        {
            List<String> result = new List<string>();
            foreach (var st in lst)
            {
                if (!st.Equals("")) result.Add(st);
            }
            return result;
        }

        public static List<KVP> GetAgentData(String filename)
        {
            List<String> tsList;

            if (File.Exists(filename))
            {
                tsList = File.ReadAllLines(filename).ToList<String>();
                int i = 0;
                while (!tsList[i].StartsWith("visualize") && i < tsList.Count)
                {
                    i++;
                }
                String t = tsList[i].Replace(" ", "");
                StringBuilder ss = new StringBuilder(t);
                ss.Remove(0, 10);               // vsiualize([
                int tail = ss.Length - ss.ToString().IndexOf("],[")-1;
                ss.Remove(ss.ToString().IndexOf("],[")+1, tail);
                //ss.Remove(ss.ToString().IndexOf("],["), ss.Length -1- (ss.ToString().IndexOf("].[")));
                
                List<KVP>  mekeke = IsolateList(ss.ToString());
                return mekeke;

            }

            return null;
        }

        public int GetAgentStartStep(String id)
        {
            Duo tb;
            if (_agentSteps.TryGetValue(id, out tb))
            {
                return tb.StartStep;
            }
            return 0;
        }

        public int GetAgentEndStep(String id)
        {
            Duo tb;
            if (_agentSteps.TryGetValue(id, out tb))
            {
                return tb.EndStep;
            }
            return 0;
        }

        public static void CleanupState()
        {
        }

        public static List<KVP> GetItemData(String filename)
        {
            List<String> tsList;

            if (File.Exists(filename))
            {
                tsList = File.ReadAllLines(filename).ToList<String>();
                int i = 0;
                while (!tsList[i].StartsWith("visualize") && i < tsList.Count)
                {
                    i++;
                }
                String t = tsList[i].Replace(" ", "");
                StringBuilder ss = new StringBuilder(t);
                ss.Remove(0, 10);               // vsiualize([
                int head = ss.ToString().IndexOf("],[") +2;
                ss.Remove(0, head);
                ss.Remove(ss.Length - 3, 2);
                //ss.Remove(ss.ToString().IndexOf("],["), ss.Length -1- (ss.ToString().IndexOf("].[")));

                List<KVP> mekeke = IsolateList(ss.ToString());
                return mekeke;

            }

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    private class Duo
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

}

