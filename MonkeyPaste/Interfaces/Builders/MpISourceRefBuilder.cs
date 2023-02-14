using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISourceRefBuilder {
        Task<List<MpTransactionSource>> AddTransactionSourcesAsync(int copyItemTransactionId, IEnumerable<Tuple<MpISourceRef, string>> transactionSources);
        Task<MpISourceRef> FetchOrCreateSourceAsync(string uri);

        string ParseRefArgs(string ref_url);
        Task<IEnumerable<MpISourceRef>> GatherSourceRefsAsync(MpPortableDataObject mpdo, bool forceExtSources = false);
        bool IsAnySourceRejected(IEnumerable<MpISourceRef> refs);
        string ConvertToRefUrl(MpISourceRef sr, string base64Args = null);
        byte[] ToUrlAsciiBytes(MpISourceRef sr);
    }
}
