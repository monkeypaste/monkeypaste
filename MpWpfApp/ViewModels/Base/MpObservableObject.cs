using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpObservableObject : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsChanged { get; set; }
    }
}
