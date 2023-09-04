using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public interface MpIPlatformDataObjectTools {
        bool IsOleBusy { get; }
        Task<object> ReadDragDropDataObjectAsync(object idoObj);
        Task<object> ProcessDragDropDataObjectAsync(object idoObj, int[] force_writer_preset_ids);
        Task WriteToClipboardAsync(object idoObj, bool ignoreClipboardChange, int[] force_writer_preset_ids);
        Task<object> ReadClipboardAsync(bool ignorePlugins);

        Task UpdateDragDropDataObjectAsync(object source, object target, int[] force_writer_preset_ids);
    }
}
