using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformDataObjectHelperAsync {
        bool IsOleBusy { get; }
        Task<object> ReadDragDropDataObjectAsync(object idoObj);
        Task SetPlatformClipboardAsync(object idoObj, bool ignoreClipboardChange);
        Task<object> GetPlatformClipboardDataObjectAsync(bool ignorePlugins);

        Task UpdateDragDropDataObjectAsync(object source, object target);
    }
}
