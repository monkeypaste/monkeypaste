using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformDataObjectHelper {
        MpPortableDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5);
        object ConvertToPlatformClipboardDataObject(MpPortableDataObject portableObj);
        void SetPlatformClipboard(MpPortableDataObject portableObj, bool ignoreClipboardChange);
        MpPortableDataObject GetPlatformClipboardDataObject();
    }

    public interface MpIPlatformDataObjectHelperAsync {
        Task<MpPortableDataObject> ReadDragDropDataObject(object nativeDataObj, int retryCount = 5);
        Task<object> WriteDragDropDataObject(MpPortableDataObject portableObj);
        Task SetPlatformClipboardAsync(MpPortableDataObject portableObj);
        Task<MpPortableDataObject> GetPlatformClipboardDataObjectAsync();
    }
}
