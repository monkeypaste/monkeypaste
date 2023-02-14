using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpITransactionReporter {
        Task<MpCopyItemTransaction> ReportTransactionAsync(
            int copyItemId,
            MpJsonMessageFormatType reqType,
            string req,
            MpJsonMessageFormatType respType,
            string resp,
            IEnumerable<string> ref_urls,
            MpTransactionType transType);

    }
}
