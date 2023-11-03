using System;

namespace MonkeyPaste.Common {
    public interface MpIUiStringToEnumConverter {
        object UiStringToEnum(string uiStr, Type enumType = null);
    }
}
