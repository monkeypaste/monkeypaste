using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIUserColorViewModel : MpIViewModel {
        //bool IsReadOnly { get; }
        //string GetColor();
        //ICommand SetColorCommand { get; } 
        string UserHexColor { get; set; }
    }
}
