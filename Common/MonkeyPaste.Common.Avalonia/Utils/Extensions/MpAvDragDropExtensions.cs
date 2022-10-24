using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MonkeyPaste.Common;
using Avalonia.Input;
using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using Avalonia;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpAvDragDropExtensions {
        public static async Task<bool> ContainsInternalContentItem_safe(this IDataObject ido, object lockObj) {
            if(ido == null) {
                return false;
            }
            var formats = await ido.GetDataFormats_safe(lockObj);
            return formats.Contains(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT);
        }

        public static void DragCheckAndStart(
            this Control control,
            PointerPressedEventArgs e,
            Action<PointerPressedEventArgs> start,
            Action<PointerEventArgs> move,
            Action<PointerReleasedEventArgs> end,
            MpIDndWindowPointerLocator wpl = null,
            MpIDndUserCancelNotifier ucn = null,
            double MIN_DISTANCE = 10) {

            // WINDOW POSITION INIT
            if(wpl is Window window) {
                wpl.DragPointerPosition = e.GetPosition(window).ToPortablePoint();
            }            

            // DRAG CYCLE

            MpPoint dc_down_pos = e.GetClientMousePoint(control);
            bool is_pointer_dragging = false;
            bool was_drag_started = false;

            EventHandler<PointerReleasedEventArgs> dragControl_PointerReleased_Handler = null;
            EventHandler<PointerEventArgs> dragControl_PointerMoved_Handler = null;

            // Drag Control PointerMoved Handler
            dragControl_PointerMoved_Handler = (s, e1) => {
                if (wpl is Window window) {
                    wpl.DragPointerPosition = e1.GetPosition(window).ToPortablePoint();
                }

                MpPoint dc_move_pos = e1.GetClientMousePoint(control);

                var drag_dist = dc_down_pos.Distance(dc_move_pos);
                is_pointer_dragging = drag_dist >= MIN_DISTANCE;
                if (is_pointer_dragging && !was_drag_started) {

                    // DRAG START

                    was_drag_started = true;
                    start?.Invoke(e);
                }

                if (was_drag_started) {
                    move?.Invoke(e1);
                }
            };

            // Drag Control PointerReleased Handler
            dragControl_PointerReleased_Handler = (s, e2) => {               

                // DRAG END

                control.PointerMoved -= dragControl_PointerMoved_Handler;
                control.PointerReleased -= dragControl_PointerReleased_Handler;
                MpConsole.WriteLine("DragCheck pointer released (was not drag)");

                end?.Invoke(e2);

            };

            control.PointerReleased += dragControl_PointerReleased_Handler;
            control.PointerMoved += dragControl_PointerMoved_Handler;

            // CANCEL HANDLER

            EventHandler userCancel_Handler = null;
            userCancel_Handler = (s, e3) => {
                if (ucn != null) {
                    ucn.OnGlobalEscKeyPressed -= userCancel_Handler;
                }
                //if (wpl is Window window) {
                //    // when this is null on end signifies a cancel

                //    wpl.DragPointerPosition = null;
                //}

                // when end handler args = null signifies cancel 
                dragControl_PointerReleased_Handler(s, null);
            };

            if (ucn != null) {
                ucn.OnGlobalEscKeyPressed += userCancel_Handler;
            }
        }
    }
}
