using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIDbStringToDbObjectTypeConverter {
        Type Convert(string typeString);
    }
}
