using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {

    public interface MpICustomColorChooserMenuAsync {
        ICommand SelectCustomColorCommand { get; }
        Task<string> ShowCustomColorMenuAsync(string selectedColor, string title = null, MpIUserColorViewModel ucvm = null);
    }
}
