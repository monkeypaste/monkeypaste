//using Avalonia.Win32;

using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpContentCapInfo {
        public const int MAX_CAP_NTF_SHOW_TIME_MS = 5_000;
        public const string NEXT_TRASH_IMG_RESOURCE_KEY = "GhostImage";
        public const string NEXT_REMOVE_IMG_RESOURCE_KEY = "SkullImage";

        public int ToBeTrashed_ciid { get; set; }
        public int NextToBeTrashed_ciid { get; set; }

        public int ToBeRemoved_ciid { get; set; }
        public int NextToBeRemoved_ciid { get; set; }

        public override string ToString() {
            return $"ToTrash: {ToBeTrashed_ciid} NextTrash: {NextToBeTrashed_ciid} ToRemove: {ToBeRemoved_ciid} NextRemove: {NextToBeRemoved_ciid}";
        }
    }
    public interface MpIAccountTools {
        Task<MpContentCapInfo> RefreshCapInfoAsync(MpUserAccountType acctType);
        bool IsContentAddPausedByAccount { get; }

        MpContentCapInfo LastCapInfo { get; }
    }
}
