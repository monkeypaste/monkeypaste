using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCommandErrorHandler : MpIErrorHandler {
        private static MpCommandErrorHandler _instance;
        public static MpCommandErrorHandler Instance => _instance ??= (_instance = new MpCommandErrorHandler());
        public void HandleError(Exception ex) {
            Debugger.Break();
            MpConsole.WriteTraceLine("Command error: ", ex);
        }
    }
    public class MpCommand<T> : ICommand where T: class {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;
        public MpCommand(Action<T> execute) : this(execute,null,null) { }

        public MpCommand(Action execute) : this(o => execute()) { }

        public MpCommand(
            Action execute,
            Func<bool> canExecute,
            params object[] npcArgs) : this(o => execute(), o => canExecute(), npcArgs) { }

        public MpCommand(
            Action<T> execute,
            Func<T, bool> canExecute,
            params object[] npcArgs)  {
            _execute = execute;
            _canExecute = canExecute;

            if (npcArgs != null) {
                foreach (var npc in npcArgs) {
                    if (npc == null) {
                        continue;
                    }
                    var npci = npc as INotifyPropertyChanged;
                    if (npci == null) {
                        throw new Exception("Command references must implement INotifyPropertyChanged");
                    }
                    npci.PropertyChanged += delegate { RaiseCanExecuteChanged(); };
                }
            }
        }

        public bool CanExecute(T parameter) {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(T parameter) {
            if (CanExecute(parameter)) {
                _execute(parameter);
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Explicit implementations
        bool ICommand.CanExecute(object parameter) {
            if (parameter == null) {
                return CanExecute(default(T));
            }
            return CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter) {
            Execute((T)parameter);
        }
        #endregion

    }
    public class MpCommand : ICommand {
        //from https://www.wpfsharp.com/2015/04/28/binding-to-icommand-with-xamarin-forms/

        //public MpCommand(Action<object> execute) : base(execute) { }
        //public MpCommand(Action execute) : this(o => execute()) { }

        //from https://johnthiriet.com/mvvm-going-async-with-async-command/
        public event EventHandler CanExecuteChanged;

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        private readonly MpIErrorHandler _errorHandler;

        public MpCommand(
            Action execute,
            Func<bool> canExecute,
            MpIErrorHandler errorHandler = null,
            params object[] npcArgs)  {
            _execute = execute;
            _canExecute = canExecute;
            _errorHandler = errorHandler;

            if (npcArgs != null) {
                foreach(var npc in npcArgs) {
                    if(npc == null) {
                        continue;
                    }
                    var npci = npc as INotifyPropertyChanged;
                    if(npci == null) {
                        throw new Exception("Command references must implement INotifyPropertyChanged");
                    }
                    npci.PropertyChanged += (s,e)=> { CanExecuteChanged.Invoke(s,e); };
                }                
            }
        }
        
        public MpCommand(
            Action execute,
            Func<bool> canExecute) : this(execute, canExecute, MpCommandErrorHandler.Instance, null) { }

        public MpCommand(
            Action execute) : this(execute, () => true, MpCommandErrorHandler.Instance, null) { }

        public MpCommand(
            Action execute,
            Func<bool> canExecute,
            params object[] npcArgs) : this(() => execute(), () => canExecute(), MpCommandErrorHandler.Instance, npcArgs) { }


        public bool CanExecute(object param) {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object param) {
            if(CanExecute(param)) {
                _execute();
            }
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
