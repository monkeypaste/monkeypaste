//using Avalonia.Win32;

using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpContentCapInfo {
        public const string NEXT_TRASH_IMG_RESOURCE_KEY = "GhostImage";
        public const string NEXT_REMOVE_IMG_RESOURCE_KEY = "SkullImage";
        public const string ADD_BLOCKED_RESOURCE_KEY = "LockImage";

        public int ToBeTrashed_ciid { get; set; }
        public int NextToBeTrashed_ciid { get; set; }

        public int ToBeRemoved_ciid { get; set; }
        public int NextToBeRemoved_ciid { get; set; }

        public override string ToString() {
            return $"ToTrash: {ToBeTrashed_ciid} NextTrash: {NextToBeTrashed_ciid} ToRemove: {ToBeRemoved_ciid} NextRemove: {NextToBeRemoved_ciid}";
        }
    }
    public interface MpIAccountTools {
        Task<MpContentCapInfo> RefreshCapInfoAsync();
        bool IsContentAddPausedByAccount { get; }

        MpContentCapInfo LastCapInfo { get; }
        int GetContentCapacity(MpUserAccountType acctType);
        int GetTrashCapacity(MpUserAccountType acctType);
        MpUserAccountType CurrentAccountType { get; }
        void SetAccountType(MpUserAccountType newType);

        string AccountStateInfo { get; }
    }
}
