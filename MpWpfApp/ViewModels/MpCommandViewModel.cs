using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpCommandViewModel : MpViewModelBase {
        public MpCommandViewModel(string displayName, ICommand command) {
            if(command == null)
                throw new ArgumentNullException("command");
            base.DisplayName = displayName;
            this.Command = command;
        }
        public ICommand Command { get; private set; }
    }
}
