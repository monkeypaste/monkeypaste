using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformDataObjectHelperAsync {
        bool IsOleBusy { get; }
        Task<object> ReadDragDropDataObjectAsync(object idoObj);
        Task<object> ProcessDragDropDataObjectAsync(object idoObj);
        Task WriteToClipboardAsync(object idoObj, bool ignoreClipboardChange);
        Task<object> ReadClipboardAsync(bool ignorePlugins);

        Task UpdateDragDropDataObjectAsync(object source, object target);
    }
}
