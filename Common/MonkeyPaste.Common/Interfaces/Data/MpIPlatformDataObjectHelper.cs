using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformDataObjectHelper {
        MpPortableDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5);
        object ConvertToPlatformClipboardDataObject(MpPortableDataObject portableObj);
        void SetPlatformClipboard(MpPortableDataObject portableObj, bool ignoreClipboardChange);
        MpPortableDataObject GetPlatformClipboardDataObject();
    }

    public interface MpIPlatformDataObjectHelperAsync {
        Task<MpPortableDataObject> ConvertToSupportedPortableFormatsAsync(object nativeDataObj, int retryCount = 5);
        Task<object> ConvertToPlatformClipboardDataObjectAsync(MpPortableDataObject portableObj);
        Task SetPlatformClipboardAsync(MpPortableDataObject portableObj, bool ignoreClipboardChange);
        Task<MpPortableDataObject> GetPlatformClipboardDataObjectAsync();
    }
}
