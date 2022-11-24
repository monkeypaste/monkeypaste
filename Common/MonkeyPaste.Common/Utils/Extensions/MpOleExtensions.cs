using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    [Flags]
    public enum MpOleOperationFlags {
        None = 0,
        Cut = 1,
        Copy = 2,
        Paste = 4,
    }
    public static class MpOleExtensions {
    }
}
