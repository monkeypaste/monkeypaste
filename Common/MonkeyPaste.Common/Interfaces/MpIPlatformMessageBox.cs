﻿using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformMessageBox {
        Task ShowOkMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null, object owner = null);
        Task<bool> ShowOkCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null, object owner = null);

        Task<bool?> ShowYesNoCancelMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null, object owner = null);
        Task<bool> ShowYesNoMessageBoxAsync(string title, string message, object anchor = null, object iconResourceObj = null, object owner = null);
        Task<string> ShowTextBoxMessageBoxAsync(string title, string message, string currentText = null, string placeholderText = null, object anchor = null, object iconResourceObj = null, object owner = null);
    }
}