using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpITransactionBuilder {
        Task<MpCopyItemTransaction> PerformTransactionAsync(int copyItemId, MpJsonMessageFormatType reqType, string req, MpJsonMessageFormatType respType, string resp, IEnumerable<string> ref_urls);

    }
}
