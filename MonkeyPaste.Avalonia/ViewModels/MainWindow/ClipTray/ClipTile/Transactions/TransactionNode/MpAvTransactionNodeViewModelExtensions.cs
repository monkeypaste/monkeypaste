using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvTransactionNodeViewModelExtensions {
        public static int GetNodeTabIdx(this MpAvITransactionNodeViewModel tnvm) {
            IEnumerable<MpITreeItemViewModel> test = null;
            if (tnvm != null) {
                test = tnvm.SelfAndAllAncestors();
            }
            if (tnvm.SelfAndAllAncestors().OfType<MpAvTransactionSourceViewModel>().Any()) {
                return 0;
            }
            if (tnvm.SelfAndAllAncestors().OfType<MpAvTransactionMessageViewModelBase>().FirstOrDefault() is MpAvTransactionMessageViewModelBase tmvm) {
                if (tmvm.TransactionMessageType == MpTransactionMessageType.Request) {
                    return 1;
                } else {
                    return 2;
                }
            }
            return 0;
        }
    }
}
