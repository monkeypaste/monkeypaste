using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPortableContentDataObject {
        Task<MpPortableDataObject> ConvertToDataObject(bool fillTemplates);
    }
}
