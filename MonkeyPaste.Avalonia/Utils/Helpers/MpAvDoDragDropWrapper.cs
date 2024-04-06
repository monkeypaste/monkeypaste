using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common.Plugin;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDoDragDropWrapper {
        public static bool IsAnyDragging =>
            DragDataObject != null;
        public static IDataObject DragDataObject { get; private set; }
        public static Control SourceControl { get; private set; }
        public static DateTime? LastDragCompletedDateTime { get; private set; }
        public static async Task<DragDropEffects> DoDragDropAsync(Control source, PointerEventArgs triggerEvent, IDataObject data, DragDropEffects allowedEffects) {
            SourceControl = source;
            DragDataObject = data;
            DragDropEffects result = DragDropEffects.None;
            try {

                result = await DragDrop.DoDragDrop(triggerEvent, data, allowedEffects);
                LastDragCompletedDateTime = DateTime.Now;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Dnd Error.", ex);
            }
            SourceControl = null;
            DragDataObject = null;
            return result;
        }
    }
}
