using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace AgentsRebuilt
{
    public class CfgStr:INotifyPropertyChanged
    {
        private String content;
        public String Content {
            get { return content; }

            set
            {
                this.content = value;
                NotifyPropertyChanged();
            }
        }

        public CfgStr(String t)
        {
            content = t;
        }

        public override string ToString()
        {
            return content;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
