using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpICopyItemBuilder {
        Task<MpCopyItem> BuildAsync(MpPortableDataObject pdo, bool suppressWrite = false, string createLabel = "", bool force_ext_sources = true);
    }
}
