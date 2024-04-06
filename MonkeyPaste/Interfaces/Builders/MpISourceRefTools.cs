using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISourceRefTools {
        string InternalSourceBaseUri { get; }
        string ContentItemQueryUriPrefix { get; }
        Task<List<MpTransactionSource>> AddTransactionSourcesAsync(int copyItemTransactionId, IEnumerable<MpISourceRef> transactionSources);
        Task<MpISourceRef> FetchOrCreateSourceAsync(string uri, object arg = null);

        Task<IEnumerable<MpISourceRef>> GatherSourceRefsAsync(object mpOrAvDataObj, bool enforce_rejection = false);
        bool IsAnySourceRejected(IEnumerable<MpISourceRef> refs);
        string ConvertToInternalUrl(MpISourceRef sr);
        string ConvertToInternalUrl(MpTransactionSourceType sourceType, int sourceId);
        string ConvertToAbsolutePath(MpISourceRef sr);
        byte[] ToUrlAsciiBytes(MpISourceRef sr);

        Tuple<MpTransactionSourceType, int> ParseUriForSourceRef(string uri);
        bool IsInternalUrl(string url);
        bool IsExternalSource(MpISourceRef sr);
        bool IsSourceRejected(MpISourceRef sr);

        Task<MpApp> FetchOrCreateAppRefAsync(MpPortableProcessInfo ppi);
        Task<string> FetchOrCreateAppRefUrlAsync(MpPortableProcessInfo ppi);
    }
}
