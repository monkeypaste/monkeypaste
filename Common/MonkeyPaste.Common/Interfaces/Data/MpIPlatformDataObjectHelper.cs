using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformDataObjectHelperAsync {
        Task<MpPortableDataObject> ReadDragDropDataObject(object nativeDataObj, int retryCount = 5);
        Task<object> WriteDragDropDataObject(MpPortableDataObject portableObj);
        Task SetPlatformClipboardAsync(MpPortableDataObject portableObj, bool ignoreClipboardChange);
        Task<MpPortableDataObject> GetPlatformClipboardDataObjectAsync();

        Task UpdateDragDropDataObjectAsync(MpPortableDataObject source, MpPortableDataObject target);
    }
}
