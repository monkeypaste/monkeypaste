﻿using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpIShortcutCommandViewModel : MpIViewModel {
        MpShortcutType ShortcutType { get; }
        string KeyString { get; }
        ICommand ShortcutCommand { get; }
        object ShortcutCommandParameter { get; }
    }
}
