using System.Threading.Tasks;
using MonkeyPaste.Common;

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
