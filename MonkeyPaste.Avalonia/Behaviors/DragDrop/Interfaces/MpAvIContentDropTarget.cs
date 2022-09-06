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
    public interface MpAvIContentDropTargetAsync {
        object DataContext { get; }
        bool IsDropEnabled { get; set; }
        bool IsDebugEnabled { get; set; }
        int DropIdx { get; set; }
        MpDropType DropType { get; }

        MpCursorType MoveCursor { get; }
        MpCursorType CopyCursor { get; }

        void AutoScrollByMouse();

        void CancelDrop();

        MpAvContentDragDropAdorner DropAdorner { get; set; }
        Orientation AdornerOrientation { get; }
        void InitAdorner();
        void UpdateAdorner();
        void Reset();

        Control RelativeToElement { get; }
        //List<MpRect> DropRects { get; }
        Task<bool> IsDragDataValidAsync(bool isCopy, object dragData);
        
        Task StartDropAsync();
        Task DropAsync(bool isCopy, object dragData);

        Task<List<MpRect>> GetDropTargetRectsAsync();
        Task<int> GetDropTargetRectIdxAsync();
        Task<MpShape[]> GetDropTargetAdornerShapeAsync();
        Task ContinueDragOverTargetAsync();

        int GetDropTargetRectIdx();
        bool IsDragDataValid(bool isCopy, object dragData);
    }

    public interface MpAvIContentDropTarget: MpAvIContentDropTargetAsync {
        bool IsDragDataValid(bool isCopy, object dragData);

        void StartDrop();
        void Drop(bool isCopy, object dragData);

        List<MpRect> GetDropTargetRects();
        int GetDropTargetRectIdx();
        MpShape[] GetDropTargetAdornerShape();
        void ContinueDragOverTarget();
    }

}
