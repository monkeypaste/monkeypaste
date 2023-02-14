using System;

namespace MonkeyPaste {
    public interface MpIStringToSyncObjectTypeConverter {
        Type Convert(string typeString);
    }
}
