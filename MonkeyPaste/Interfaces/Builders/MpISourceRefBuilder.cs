using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISourceRefBuilder {
        Task<List<MpTransactionSource>> AddTransactionSourcesAsync(int copyItemTransactionId, IEnumerable<MpISourceRef> transactionSources);
        Task<MpISourceRef> FetchOrCreateSourceAsync(string uri);
        Task<IEnumerable<MpISourceRef>> GatherSourceRefsAsync(MpPortableDataObject mpdo);
        bool IsAnySourceRejected(IEnumerable<MpISourceRef> refs);
        string ConvertToRefUrl(MpISourceRef sr);
        byte[] ToUrlAsciiBytes(MpISourceRef sr);
    }
}
