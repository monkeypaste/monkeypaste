using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIDbIdCollection {
        int GetItemId(int queryIdx);
        int GetItemOffsetIdx(int itemId);
        void InsertId(int idx, int id);
        bool RemoveIdx(int queryIdx);
        bool RemoveItemId(int itemId);
    }
}