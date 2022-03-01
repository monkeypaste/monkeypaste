using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIAsyncCommand : ICommand {
        Task ExecuteAsync();
        bool CanExecute();
    }

    public interface MpIAsyncCommand<T> : ICommand {
        Task ExecuteAsync(T parameter);
        bool CanExecute(T parameter);
    }
}
