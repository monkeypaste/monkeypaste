using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public interface MpIContentDropTarget {
        object DataContext { get; }
        bool IsDropEnabled { get; set; }
        bool IsDebugEnabled { get; set; }
        int DropIdx { get; set; }
        MpDropType DropType { get; }

        MpCursorType MoveCursor { get; }
        MpCursorType CopyCursor { get; }

        void AutoScrollByMouse();
        
        bool IsDragDataValid(bool isCopy, object dragData);
        
        Task StartDrop(PointerEventArgs e);
        Task Drop(bool isCopy, object dragData);
        void CancelDrop();

        Control RelativeToElement { get; }
        List<MpRect> DropRects { get; }
        List<MpRect> GetDropTargetRects();
        int GetDropTargetRectIdx();
        MpShape[] GetDropTargetAdornerShape();
        void ContinueDragOverTarget();

        MpAvContentAdorner DropLineAdorner { get; set; }
        Orientation AdornerOrientation { get; }
        void InitAdorner();
        void UpdateAdorner();
        void Reset();
    }

}
