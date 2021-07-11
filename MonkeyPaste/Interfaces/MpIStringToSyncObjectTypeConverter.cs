using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIStringToSyncObjectTypeConverter {
        Type Convert(string typeString);
    }
}
