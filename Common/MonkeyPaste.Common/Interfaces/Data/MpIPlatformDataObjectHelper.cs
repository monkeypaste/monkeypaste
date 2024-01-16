using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformDataObjectTools {
        bool IsOleBusy { get; }
        Task<object> ReadDataObjectAsync(object idoObj, MpDataObjectSourceType sourceType);
        Task<object> ReadClipboardAsync(bool ignorePlugins);
        Task<object> WriteDragDropDataObjectAsync(object idoObj);
        Task WriteToClipboardAsync(object idoObj, bool ignoreClipboardChange);

        Task UpdateDragDropDataObjectAsync(object source, object target);
    }
}
