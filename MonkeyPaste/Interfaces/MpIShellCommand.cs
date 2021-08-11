using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIShellCommand {
        object Run(string cmd, params object[] args);
    }
}
