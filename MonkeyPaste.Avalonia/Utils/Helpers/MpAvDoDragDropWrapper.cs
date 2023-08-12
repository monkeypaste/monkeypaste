using Avalonia.Input;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDoDragDropWrapper {
        public static IDataObject DragDataObject { get; private set; }
        public static object Source { get; private set; }

        public static async Task<DragDropEffects> DoDragDropAsync(object source, PointerEventArgs triggerEvent, IDataObject data, DragDropEffects allowedEffects) {
            Source = source;
            DragDataObject = data;
            DragDropEffects result = await DragDrop.DoDragDrop(triggerEvent, data, allowedEffects);
            Source = null;
            DragDataObject = null;
            return result;
        }
    }
}
