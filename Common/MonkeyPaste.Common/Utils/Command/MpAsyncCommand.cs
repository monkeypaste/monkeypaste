using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Common {
    public class MpAsyncCommand<T> : MpIAsyncCommand<T> {
        //from https://johnthiriet.com/mvvm-going-async-with-async-command/
        public event EventHandler CanExecuteChanged;

        private bool _isExecuting;
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private readonly MpIErrorHandler _errorHandler;

        public MpAsyncCommand(
            Func<T, Task> execute,
            Func<T, bool> canExecute = null,
            MpIErrorHandler errorHandler = null) : this(execute, canExecute, errorHandler, null) { }

        public MpAsyncCommand(
             Func<T, Task> execute,
             Func<T, bool> canExecute,
             MpIErrorHandler errorHandler,
             params object[] npcArgs) {
            _execute = execute;
            _canExecute = canExecute;
            _errorHandler = errorHandler;

            if (npcArgs != null) {
                foreach (var npc in npcArgs) {
                    if (npc == null) {
                        continue;
                    }
                    var npci = npc as INotifyPropertyChanged;
                    if (npci == null) {
                        throw new Exception("Command references must implement INotifyPropertyChanged");
                    }
                    npci.PropertyChanged += (s, e) => { CanExecuteChanged?.Invoke(s, e); };
                }
            }
        }

        public bool CanExecute(T parameter) {
            return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
        }

        public async Task ExecuteAsync(T parameter) {
            if (CanExecute(parameter)) {
                try {
                    _isExecuting = true;
                    await _execute(parameter);
                }
                finally {
                    _isExecuting = false;
                }
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
            ExecuteAsync((T)parameter).FireAndForgetSafeAsync(_errorHandler);
        }
        #endregion
    }

    public class MpAsyncCommand : MpIAsyncCommand {
        public event EventHandler CanExecuteChanged;

        private bool _isExecuting;
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private readonly MpIErrorHandler _errorHandler;
        public MpAsyncCommand(
            Func<Task> execute,
            Func<bool> canExecute = null,
            MpIErrorHandler errorHandler = null) : this(execute, canExecute, errorHandler, null) { }

        public MpAsyncCommand(
            Func<Task> execute,
            Func<bool> canExecute = null,
            MpIErrorHandler errorHandler = null,
            params object[] npcArgs) {
            _execute = execute;
            _canExecute = canExecute;
            _errorHandler = errorHandler;


            if (npcArgs != null) {
                foreach (var npc in npcArgs) {
                    if (npc == null) {
                        continue;
                    }
                    var npci = npc as INotifyPropertyChanged;
                    if (npci == null) {
                        throw new Exception("Command references must implement INotifyPropertyChanged");
                    }
                    npci.PropertyChanged += (s, e) => { CanExecuteChanged?.Invoke(s, e); };
                }
            }
        }

        public bool CanExecute() {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async Task ExecuteAsync() {
            if (CanExecute()) {
                try {
                    _isExecuting = true;
                    await _execute();
                }
                finally {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged() {
            //CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            if (MpCommonTools.Services == null ||
                MpCommonTools.Services.MainThreadMarshal == null) {
                return;
            }
            MpCommonTools.Services.MainThreadMarshal.RunOnMainThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
        }

        #region Explicit implementations
        bool ICommand.CanExecute(object parameter) {
            return CanExecute();
        }

        void ICommand.Execute(object parameter) {
            ExecuteAsync().FireAndForgetSafeAsync(_errorHandler);
        }
        #endregion
    }
}
