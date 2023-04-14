using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISourceRefTools {
        Task<List<MpTransactionSource>> AddTransactionSourcesAsync(int copyItemTransactionId, IEnumerable<MpISourceRef> transactionSources);
        Task<MpISourceRef> FetchOrCreateSourceAsync(string uri);

        string ParseRefArgs(string ref_url);
        Task<IEnumerable<MpISourceRef>> GatherSourceRefsAsync(object mpOrAvDataObj, bool forceExtSources = false);
        bool IsAnySourceRejected(IEnumerable<MpISourceRef> refs);
        string ConvertToRefUrl(MpISourceRef sr);
        byte[] ToUrlAsciiBytes(MpISourceRef sr);

        Tuple<MpTransactionSourceType, int> ParseUriForSourceRef(string uri);
    }
}
