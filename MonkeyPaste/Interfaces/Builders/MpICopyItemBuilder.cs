using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpICopyItemBuilder {
        Task<MpCopyItem> CreateAsync(MpPortableDataObject pdo, bool suppressWrite = false);
    }
}
