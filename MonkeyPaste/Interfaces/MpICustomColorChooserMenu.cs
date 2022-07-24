using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpICustomColorChooserMenu {
        ICommand SelectCustomColorCommand { get; }

        string ShowCustomColorMenu(string selectedColor, MpIUserColorViewModel ucvm);
    }

    public interface MpICustomColorChooserMenuAsync : MpICustomColorChooserMenu {
        Task<string> ShowCustomColorMenuAsync(string selectedColor, MpIUserColorViewModel ucvm);
    }
}
