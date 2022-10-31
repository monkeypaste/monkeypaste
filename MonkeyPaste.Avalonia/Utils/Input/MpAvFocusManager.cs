using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIFocusElement {
        int Level { get; }
        MpIFocusElement Parent { get; }
        MpIFocusElement Next { get; }
        MpIFocusElement Previous { get; }
        MpIFocusElement FirstChild { get; }
    }

    public static class MpAvFocusManager {

    }
}
