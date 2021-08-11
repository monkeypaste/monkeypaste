using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIScreenshot {
        byte[] Capture();
    }
}
