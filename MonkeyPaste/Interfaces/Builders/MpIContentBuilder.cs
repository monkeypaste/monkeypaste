using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIContentBuilder {
        Task<MpCopyItem> BuildFromDataObject(object avOrPortableDataObject, bool is_copy);
    }
}
