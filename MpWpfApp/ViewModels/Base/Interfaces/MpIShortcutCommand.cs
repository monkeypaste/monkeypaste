using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp{
    public interface MpIShortcutCommand {
        ICommand AssignCommand { get; }
        MpShortcutType ShortcutType { get; }
        MpShortcutViewModel ShortcutViewModel { get; }
        string ShortcutKeyString { get; }
    }
}
