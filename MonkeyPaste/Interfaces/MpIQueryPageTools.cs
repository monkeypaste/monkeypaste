using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIQueryPageTools {
        int GetItemId(int queryIdx);
        int GetItemOffsetIdx(int itemId);
        //void InsertId(int idx, int id);
        //bool RemoveIdx(int queryIdx);
        bool RemoveItemId(int itemId);
        void Reset();
        void SetTotalCount(int count);
        int TotalCount { get; }
    }
}