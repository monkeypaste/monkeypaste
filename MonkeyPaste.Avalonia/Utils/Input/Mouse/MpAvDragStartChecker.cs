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

        public static void DragCheckAndStart(this Control control, PointerPressedEventArgs e, Action<PointerPressedEventArgs> start, Action end, bool endOnEscape) {
            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();

            MpPoint dc_down_pos = e.GetClientMousePoint(control);
            bool is_pointer_dragging = false;
            bool was_drag_started = false;
            bool wasEscapeReleased = false;

            EventHandler<PointerReleasedEventArgs> dragControl_PointerReleased_Handler = null;
            EventHandler<PointerEventArgs> dragControl_PointerMoved_Handler = null;
            EventHandler dragControl_EscapeReleased_handler = null;

            // Drag Control PointerMoved Handler
            dragControl_PointerMoved_Handler = (s, e1) => {
                MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e1.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();

                if (wasEscapeReleased) {
                    MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = null;
                    control.PointerMoved -= dragControl_PointerMoved_Handler;
                    MpAvShortcutCollectionViewModel.Instance.OnGlobalEscapeReleased -= dragControl_EscapeReleased_handler;
                    control.PointerReleased -= dragControl_PointerReleased_Handler;
                    end?.Invoke();
                    return;
                }

                if(was_drag_started) {
                    return;
                }


                MpPoint dc_move_pos = e1.GetClientMousePoint(control);

                var drag_dist = dc_down_pos.Distance(dc_move_pos);
                is_pointer_dragging = drag_dist >= MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST;
                if (is_pointer_dragging) {
                    was_drag_started = true;
                    // DRAG START

                    //control.PointerMoved -= dragControl_PointerMoved_Handler;
                    start?.Invoke(e);
                }
            };

            // Drag Control PointerReleased Handler
            dragControl_PointerReleased_Handler = (s, e2) => {
                if (was_drag_started) {
                    // this should not happen, or release is called before drop (if its called at all during drop
                    // release should be removed after drop
                    //Debugger.Break();
                }
                MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = null;

                // DRAG END

                control.PointerMoved -= dragControl_PointerMoved_Handler;
                control.PointerReleased -= dragControl_PointerReleased_Handler;
                MpConsole.WriteLine("DragCheck pointer released (was not drag)");

                end?.Invoke();

            };

            dragControl_EscapeReleased_handler = (s, e) => {
                wasEscapeReleased = true;
            };

            control.PointerReleased += dragControl_PointerReleased_Handler;
            control.PointerMoved += dragControl_PointerMoved_Handler;

            if(endOnEscape) {
                MpAvShortcutCollectionViewModel.Instance.OnGlobalEscapeReleased += dragControl_EscapeReleased_handler;
            }
        }

    }
}
