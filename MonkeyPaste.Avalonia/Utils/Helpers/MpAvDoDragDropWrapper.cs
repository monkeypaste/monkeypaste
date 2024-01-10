using Avalonia.Controls;
using Avalonia.Input;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDoDragDropWrapper {
        public static bool IsAnyDragging =>
            DragDataObject != null;
        public static IDataObject DragDataObject { get; private set; }
        public static Control SourceControl { get; private set; }

        public static async Task<DragDropEffects> DoDragDropAsync(Control source, PointerEventArgs triggerEvent, IDataObject data, DragDropEffects allowedEffects) {
            SourceControl = source;
            DragDataObject = data;
            DragDropEffects result = await DragDrop.DoDragDrop(triggerEvent, data, allowedEffects);
            SourceControl = null;
            DragDataObject = null;
            return result;
        }
    }
}
