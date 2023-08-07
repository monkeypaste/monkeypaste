using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIDragSource : MpIDraggable {
        bool WasDragCanceled { get; set; }
        PointerEventArgs LastPointerPressedEventArgs { get; }
        //bool IsDragging { get; set; }
        void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc, bool meta);
        //void NotifyDragComplete(DragDropEffects dropEffect);
        Task<MpAvDataObject> GetDataObjectAsync(string[] formats = null, bool use_placeholders = true, bool ignore_selection = false);

        string[] GetDragFormats();
    }
}
