using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public interface MpIContentDropTarget {
        object DataContext { get; }
        bool IsEnabled { get; set; }

        int DropIdx { get; set; }
        MpDropType DropType { get; }

        MpCursorType MoveCursor { get; }
        MpCursorType CopyCursor { get; }

        void AutoScrollByMouse();
        
        bool IsDragDataValid(bool isCopy, object dragData);
        
        Task StartDrop();
        Task Drop(bool isCopy, object dragData);
        void CancelDrop();

        UIElement RelativeToElement { get; }
        List<Rect> DropRects { get; }
        List<Rect> GetDropTargetRects();
        int GetDropTargetRectIdx();
        void ContinueDragOverTarget();

        MpDropLineAdorner DropLineAdorner { get; set; }
        Orientation AdornerOrientation { get; }
        void InitAdorner();
        void UpdateAdorner();
        void EnableDebugMode();


        void Reset();
    }

}
