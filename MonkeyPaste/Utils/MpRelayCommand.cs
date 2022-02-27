using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpRelayCommand<T> : Command where T: new() {
        public MpRelayCommand(Action<object> execute) : base(execute) { }

        public MpRelayCommand(Action execute) : this(o => execute()) { }

        public MpRelayCommand(
            Action<object> execute,
            Func<object, bool> canExecute,
            params object[] npcArgs) : base(execute, canExecute) {
            if (npcArgs != null) {
                foreach (var npc in npcArgs) {
                    if (npc == null) {
                        continue;
                    }
                    var npci = npc as INotifyPropertyChanged;
                    if (npci == null) {
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
    public class MpRelayCommand : Command {
        //from https://www.wpfsharp.com/2015/04/28/binding-to-icommand-with-xamarin-forms/

        public MpRelayCommand(Action<object> execute) : base(execute) { }

        public MpRelayCommand(Action execute) : this(o => execute()) { }

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
