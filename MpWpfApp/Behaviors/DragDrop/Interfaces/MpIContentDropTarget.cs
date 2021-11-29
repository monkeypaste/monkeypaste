using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public interface MpIContentDropTarget {
        int DropIdx { get;  }
        int DropPriority { get; }
        int TargetId { get; set; }

        void AutoScrollByMouse(MouseEventArgs e);
        
        bool IsDragDataValid(object dragData);
        
        void StartDrop();
        Task Drop(bool isCopy, object dragData);
        void CancelDrop();
        
        int GetDropTargetRectIdx(MouseEventArgs e);
        void ContinueDragOverTarget(MouseEventArgs e);
        List<Rect> GetDropTargetRects();

        Orientation AdornerOrientation { get; }
        void InitAdorner();
        void UpdateAdorner();
        void EnableDebugMode();


        void Reset();

    }

}
