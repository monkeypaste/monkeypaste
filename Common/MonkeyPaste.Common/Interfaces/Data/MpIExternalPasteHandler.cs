using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIExternalPasteHandler {
        Task PasteDataObject(MpPortableDataObject mpdo, MpPortableProcessInfo processInfo);
    }
}
