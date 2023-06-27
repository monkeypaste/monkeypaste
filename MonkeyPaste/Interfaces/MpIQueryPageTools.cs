using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIQueryPageTools {
        void SetTotalCount(int count);
        int TotalCount { get; }
    }
}