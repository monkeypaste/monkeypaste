using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIUserColorViewModel : MpIViewModel {
        string UserHexColor { get; set; }
    }


}
