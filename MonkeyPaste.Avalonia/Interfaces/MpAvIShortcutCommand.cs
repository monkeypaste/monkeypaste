﻿using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIShortcutCommand {
        ICommand AssignCommand { get; }
        MpShortcutType ShortcutType { get; }
        MpShortcutViewModel ShortcutViewModel { get; }
        string ShortcutKeyString { get; }
    }

    public interface MpIShortcutCommandViewModel : MpIViewModel {
        string ShortcutLabel { get; }
        MpShortcutType ShortcutType { get; }

        int ModelId { get; }

        ICommand ShortcutCommand { get; }
        object ShortcutCommandParameter { get; }
    }

    public interface MpIShortcutCommandCollectionViewModel : MpIViewModel {
        IEnumerable<MpAvIShortcutCommand> ShortcutCommands { get; }
    }
}