using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIDbInfo {
        string GetDbFilePath();
        string GetDbPassword();
        string GetDbName();
    }

}
