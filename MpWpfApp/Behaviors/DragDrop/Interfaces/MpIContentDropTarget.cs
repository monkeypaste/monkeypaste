using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public interface MpIContentDropTarget {
        bool IsEnabled { get; set; }
        int DropIdx { get; set; }
        int DropPriority { get; }
        int TargetId { get; set; }

        void AutoScrollByMouse(MouseEventArgs e);
        
        bool IsDragDataValid(object dragData);
        
        Task StartDrop();
        Task Drop(bool isCopy, object dragData);
        void CancelDrop();

        List<Rect> DropRects { get; }
        List<Rect> GetDropTargetRects();
        int GetDropTargetRectIdx(MouseEventArgs e);
        void ContinueDragOverTarget(MouseEventArgs e);

        MpDropLineAdorner DropLineAdorner { get; set; }
        Orientation AdornerOrientation { get; }
        void InitAdorner();
        void UpdateAdorner();
        void EnableDebugMode();


        void Reset();

    }

}
