using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIUserColorViewModel {
        bool IsReadOnly { get; }
        string GetColor();
        ICommand SetColorCommand { get; } // has hex color arg
    }
}
