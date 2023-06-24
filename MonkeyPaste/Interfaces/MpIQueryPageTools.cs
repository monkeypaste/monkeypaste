using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIQueryPageTools {
        int GetItemId(int queryIdx);
        int GetItemOffsetIdx(int itemId);
        bool AddIdToOmit(int itemId);
        bool RemoveIdToOmit(int itemId);
        void Reset(bool isRequery);
        void SetTotalCount(int count);
        int TotalCount { get; }
    }
}