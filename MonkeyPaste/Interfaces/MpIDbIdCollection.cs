namespace MonkeyPaste {
    public interface MpIDbIdCollection {
        int GetItemId(int queryIdx);
        int GetItemOffsetIdx(int itemId);
        void InsertId(int idx, int id);
        bool RemoveIdx(int queryIdx);
        bool RemoveItemId(int itemId);
    }
}