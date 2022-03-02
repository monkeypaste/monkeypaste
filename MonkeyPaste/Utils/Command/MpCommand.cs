using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCommand<T> : Command where T: new() {
        public MpCommand(Action<object> execute) : base(execute) { }

        public MpCommand(Action execute) : this(o => execute()) { }

        public MpCommand(
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

        public MpCommand(
            Action execute,
            Func<bool> canExecute,
            params object[] npcArgs) : this(o => execute(), o => canExecute(), npcArgs) {
        }
    }
    public class MpCommand : Command {
        //from https://www.wpfsharp.com/2015/04/28/binding-to-icommand-with-xamarin-forms/

        //public MpCommand(Action<object> execute) : base(execute) { }

        //public MpCommand(Action execute) : this(o => execute()) { }

        public MpCommand(
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
        
        public MpCommand(
            Action<object> execute,
            Func<object, bool> canExecute) : this(execute, canExecute, null) { }

        public MpCommand(
            Action<object> execute) : this(execute, o => true, null) { }

        public MpCommand(
            Action execute,
            Func<bool> canExecute,
            params object[] npcArgs) : this(o => execute(), o => canExecute(), npcArgs) { }

        public MpCommand(
            Action execute,
            Func<bool> canExecute) : this(o => execute(), o => canExecute(), null) { }


        public MpCommand(
            Action execute) : this(o => execute(), o => true, null) { }
    }
}
