using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDragHelperExtensions {

        public static void DragCheckAndStart(
            this Control control, 
            PointerPressedEventArgs e, 
            Action<PointerPressedEventArgs> start, 
            Action<PointerEventArgs> @continue, 
            Action end, 
            double MIN_DISTANCE = MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST) {
            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();

            MpPoint dc_down_pos = e.GetClientMousePoint(control);
            bool is_pointer_dragging = false;
            bool was_drag_started = false;

            EventHandler<PointerReleasedEventArgs> dragControl_PointerReleased_Handler = null;
            EventHandler<PointerEventArgs> dragControl_PointerMoved_Handler = null;

            // Drag Control PointerMoved Handler
            dragControl_PointerMoved_Handler = (s, e1) => {        
                MpPoint dc_move_pos = e1.GetClientMousePoint(control);

                var drag_dist = dc_down_pos.Distance(dc_move_pos);
                is_pointer_dragging = drag_dist >= MIN_DISTANCE;
                if (is_pointer_dragging && !was_drag_started) {
                    was_drag_started = true;
                    // DRAG START

                    start?.Invoke(e);
                }

                @continue?.Invoke(e1);
            };

            // Drag Control PointerReleased Handler
            dragControl_PointerReleased_Handler = (s, e2) => {
                MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = null;

                // DRAG END

                control.PointerMoved -= dragControl_PointerMoved_Handler;
                control.PointerReleased -= dragControl_PointerReleased_Handler;
                MpConsole.WriteLine("DragCheck pointer released (was not drag)");

                end?.Invoke();

            };

            control.PointerReleased += dragControl_PointerReleased_Handler;
            control.PointerMoved += dragControl_PointerMoved_Handler;

        }

    }
}
