using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIExternalPasteHandler {
        Task<bool> PasteDataObjectAsync(MpPortableDataObject mpdo, MpPortableProcessInfo processInfo, bool fromKeyboard);
    }
}
