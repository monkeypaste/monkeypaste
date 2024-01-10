using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIContentBuilder {
        Task<MpCopyItem> BuildFromDataObjectAsync(object avOrPortableDataObject, bool is_copy, MpDataObjectSourceType sourceType);
    }
}
