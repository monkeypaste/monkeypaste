using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISearchCriteriaBuilder {
        //Task<MpSearchCriteriaItem>
    }
    public interface MpICopyItemBuilder {
        Task<MpCopyItem> BuildAsync(
            MpPortableDataObject pdo,
            bool suppressWrite = false,
            MpTransactionType transType = MpTransactionType.None,
            bool force_ext_sources = true);
    }
}
