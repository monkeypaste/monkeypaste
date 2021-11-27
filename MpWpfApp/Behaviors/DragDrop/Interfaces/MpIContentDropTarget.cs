using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public interface MpIContentDropTarget {
        int DropPriority { get; }
        int TargetId { get; set; }

        void AutoScrollByMouse(MouseEventArgs e);
        
        bool IsDragDataValid(object dragData);
        void CancelDrop();
        int GetDropTargetRectIdx(MouseEventArgs e);
        void ContinueDragOverTarget(MouseEventArgs e);
        List<Rect> GetDropTargetRects();

        void InitAdorner();
        void UpdateAdorner();
        void EnableDebugMode();

        Task Drop(object dragData);

        void Reset();

    }

}
