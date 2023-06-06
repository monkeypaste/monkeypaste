using System.Collections;
using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIDownKeyHelper {
        //int DownCount { get; }
        //bool IsDown(object key);
        List<object> Downs { get; }
    }
}
