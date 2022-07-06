using System;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpICommand : ICommand {
        event EventHandler CanExecuteChanged;
        bool CanExecute(object parameter);
        void Execute(object parameter);
    }
}
