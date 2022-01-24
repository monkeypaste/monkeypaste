using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpICustomColorChooserMenu {
        ICommand SelectCustomColorCommand { get; }

        string ShowCustomColorMenu(string selectedColor, MpIUserColorViewModel ucvm);
    }
}
