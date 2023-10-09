//using Avalonia.Win32;

using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpContentCapInfo {
        public const string NEXT_TRASH_IMG_RESOURCE_KEY = "RecycleBinImage";
        public const string NEXT_REMOVE_IMG_RESOURCE_KEY = "TrashCanImage";
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
        bool IsContentAddPausedByAccount { get; }

        MpContentCapInfo LastCapInfo { get; }
        string AccountStateInfo { get; }
        MpUserAccountType CurrentAccountType { get; }

        Task InitAsync();
        Task<MpContentCapInfo> RefreshCapInfoAsync();
        int GetContentCapacity(MpUserAccountType acctType);
        int GetTrashCapacity(MpUserAccountType acctType);
        decimal GetAccountRate(MpUserAccountType acctType, bool isMonthly);
        void SetAccountType(MpUserAccountType newType);
    }
}
