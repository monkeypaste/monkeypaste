using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpICopyItemBuilder {
        MpCopyItem Create(int remainingTryCount = 5);
    }
}
