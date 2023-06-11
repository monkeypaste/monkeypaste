//using Avalonia.Win32;

using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpContentCapInfo {
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
