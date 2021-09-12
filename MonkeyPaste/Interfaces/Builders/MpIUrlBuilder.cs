using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIUrlBuilder {
        MpUrl Create(string url);
    }
}
