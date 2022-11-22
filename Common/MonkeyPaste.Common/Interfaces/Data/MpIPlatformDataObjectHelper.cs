using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformDataObjectHelperAsync {
        bool IsOleBusy { get; }
        Task<object> ReadDragDropDataObject(object idoObj, int retryCount = 5);
        Task<object> WriteDragDropDataObject(object idoObj);
        Task SetPlatformClipboardAsync(object idoObj, bool ignoreClipboardChange);
        Task<object> GetPlatformClipboardDataObjectAsync();

        Task UpdateDragDropDataObjectAsync(object source, object target);
    }
}
