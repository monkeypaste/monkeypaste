using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {

    public interface MpICustomColorChooserMenuAsync {
        ICommand SelectCustomColorCommand { get; }
        Task<string> ShowCustomColorMenuAsync(
            string selectedColor,
            string title = null,
            object ucvm = null,
            object owner = null,
            string[] fixedPalette = null,
            bool allowAlpha = false);
    }
}
