using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Annotations;

namespace AgentsRebuilt
{


    public class LogProcessor
    {
        private static List<String> _log;
        private static int Index = 0;
        private static int _latency = 0;

        public static int GetIndex
        {
            get { return Index; }

        }

        public static int GetNumber
        {
            get { return _log.Count; }
        }


        public static void SetCurrentLatency()
        {
            _latency = _log.Count - Index;
        }

        public static bool SetIndex(int num)
        {
            if (_log == null) return false;
            if (num > -1 && num < _log.Count)
            {
                Index = num;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void InitOrReread(String filename)
        {
            //VIK -- add check for whether file exists
            _log = System.IO.File.ReadAllLines(filename).ToList<String>();
            _log = RemoveEmpty(_log);
            _latency = GetNumber;
        }

        /// <summary>
        /// WARNING!!! getting first line sets the index to 0!!!
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetFirstLineAndReset(out KVP result)
        {
            Index = 0;
            KVP tResKvp = null;
            while (tResKvp == null && _log.Count > Index)
            {
                Index++;
                tResKvp = DecipherLogLine(Index - 1);
            }
            result = tResKvp;
            return (tResKvp != null);
        }

        public static bool GetNextLine(out KVP result, ExecutionState execState)
        {
            KVP tResKvp=null;
            if (execState == ExecutionState.Running)
            {
                while (tResKvp == null && _log.Count > Index)
                {
                    Index++;
                    _latency--;
                    tResKvp = DecipherLogLine(Index - 1);
                }
            }
            else if (execState == ExecutionState.Following)
            {
                while (tResKvp == null && _log.Count > Index && _latency <= (_log.Count - Index))
                {
                    Index++;
                    tResKvp = DecipherLogLine(Index - 1);
                }
            }
            result = tResKvp;
            return (tResKvp!=null);
        }

        public static bool GetLastLine(out KVP result)
        {
            KVP tResKvp = null;
            Index = _log.Count - 1;
            while (tResKvp == null &&  Index>=0)
            {
                Index--;
                tResKvp = DecipherLogLine(Index + 1);
            }
            result = tResKvp;
            return (tResKvp != null);
        }

        public static KVP DecipherLogLine(int number)
        {
                if (_log.Count <= number) return null;
                return DecipherLine(_log.ElementAt(number));
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
    }
}