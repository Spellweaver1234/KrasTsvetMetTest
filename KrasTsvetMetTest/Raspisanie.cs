using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KrasTsvetMetTest
{
    // модель
    public class Raspisanie : INotifyPropertyChanged
    {
        private string party;
        private string equipment;
        private string tStart;
        private string tStop;

        public string TStop
        {
            get { return tStop; }
            set
            {
                tStop = value;
                OnPropertyChanged("TStop");
            }
        }

        public string TStart
        {
            get { return tStart; }
            set
            {
                tStart = value;
                OnPropertyChanged("TStart");
            }
        }

        public string Equipment
        {
            get { return equipment; }
            set
            {
                equipment = value;
                OnPropertyChanged("Equipment");
            }
        }

        public string Party
        {
            get { return party; }
            set
            {
                party = value;
                OnPropertyChanged("Party");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
