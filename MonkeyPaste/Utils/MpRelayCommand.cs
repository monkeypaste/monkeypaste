using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpRelayCommand : Command {
        public MpRelayCommand(Action<object> execute) : base(execute) { }

        public MpRelayCommand(Action execute) : this(o => execute()) { }

        //public MpRelayCommand(
        //    Action<object> execute, 
        //    Func<object, bool> canExecute, 
        //    INotifyPropertyChanged npc = null) : base(execute, canExecute) {
        //    if (npc != null) {
        //        npc.PropertyChanged += delegate { ChangeCanExecute(); };
        //    }
        //}

        //public MpRelayCommand(
        //    Action execute, 
        //    Func<bool> canExecute, 
        //    INotifyPropertyChanged npc = null) : this(o => execute(), o => canExecute(), npc) {
        //}

        public MpRelayCommand(
            Action<object> execute,
            Func<object, bool> canExecute,
            params object[] npcArgs) : base(execute, canExecute) {
            if (npcArgs != null) {
                foreach(var npc in npcArgs) {
                    if(npc == null) {
                        continue;
                    }
                    var npci = npc as INotifyPropertyChanged;
                    if(npci == null) {
                        throw new Exception("Command references must implement INotifyPropertyChanged");
                    }
                    npci.PropertyChanged += delegate { ChangeCanExecute(); };
                }                
            }
        }

        public MpRelayCommand(
            Action execute,
            Func<bool> canExecute,
            params object[] npcArgs) : this(o => execute(), o => canExecute(), npcArgs) {
        }
    }
}
