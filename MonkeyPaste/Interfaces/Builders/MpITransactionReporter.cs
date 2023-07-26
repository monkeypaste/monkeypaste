using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpITransactionReporter {
        IEnumerable<int> CopyItemTransactionsInProgress { get; }
        Task<MpCopyItemTransaction> ReportTransactionAsync(
            int copyItemId,
            MpJsonMessageFormatType reqType = MpJsonMessageFormatType.None,
            string req = "",
            MpJsonMessageFormatType respType = MpJsonMessageFormatType.None,
            string resp = "",
            IEnumerable<string> ref_uris = null,
            MpTransactionType transType = MpTransactionType.None);

    }
}
