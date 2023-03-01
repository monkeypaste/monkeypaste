using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;

namespace MonkeyPaste.Common {
    public static class MpAvDragDropExtensions {
        private static double[] _autoScrollAccumulators;
        public static int GetDropIdx(this ItemsControl ic, MpPoint ic_mp, Orientation orientation) {
            if (ic == null) {
                return -1;
            }
            string excluded_sides =
                orientation == Orientation.Horizontal ? "b,t" : "r,l";

            if (!ic.Bounds.Contains(ic_mp.ToAvPoint())) {
                return -1;
            }

            if (ic.ItemCount == 0) {
                return 0;
            }
            MpRectSideHitTest closet_side_ht = null;
            int closest_side_lbi_idx = -1;
            for (int i = 0; i < ic.ItemCount; i++) {
                var lbi = ic.ContainerFromIndex(i);
                if (lbi == null) {
                    //offscreen
                    continue;
                }

                var lbi_rect = lbi.Bounds.ToPortableRect();
                var cur_tup = lbi_rect.GetClosestSideToPoint(ic_mp, excluded_sides);
                if (closet_side_ht == null || cur_tup.ClosestSideDistance < closet_side_ht.ClosestSideDistance) {
                    closet_side_ht = cur_tup;
                    closest_side_lbi_idx = i;
                }
            }

            string tail_side = orientation == Orientation.Horizontal ? "r" : "b";
            if (closet_side_ht.ClosestSideLabel == tail_side) {
                return closest_side_lbi_idx + 1;
            }
            return closest_side_lbi_idx;
        }

        public static int GetTreeDropIdx(
            this ItemsControl ic,
            MpPoint ic_mp,
            Orientation orientation,
            out ItemsControl drop_parent_ic,
            ItemsControl root_ic = null) {
            drop_parent_ic = null;

            if (ic == null) {
                return -1;
            }
            string excluded_sides =
                orientation == Orientation.Horizontal ? "b,t" : "r,l";

            if (!ic.Bounds.Contains(ic_mp.ToAvPoint())) {
                return -1;
            }

            drop_parent_ic = ic;
            if (ic.ItemCount == 0) {
                return 0;
            }

            root_ic = root_ic == null ? ic : root_ic;
            MpRectSideHitTest closet_side_ht = null;
            int closest_side_lbi_idx = -1;
            for (int i = 0; i < ic.ItemCount; i++) {
                var lbi = ic.ContainerFromIndex(i);
                if (lbi == null) {
                    //offscreen
                    continue;
                }

                var lbi_rect = lbi.Bounds.ToPortableRect(root_ic);
                var cur_tup = lbi_rect.GetClosestSideToPoint(ic_mp, excluded_sides);
                if (closet_side_ht == null || cur_tup.ClosestSideDistance < closet_side_ht.ClosestSideDistance) {
                    closet_side_ht = cur_tup;
                    closest_side_lbi_idx = i;

                    if (lbi_rect.Contains(ic_mp) &&
                        lbi is ItemsControl cur_ic &&
                        cur_ic.ItemCount > 0) {
                        // descend into items control
                        drop_parent_ic = cur_ic;
                        int result_drop_idx = cur_ic.GetTreeDropIdx(ic_mp, orientation, out ItemsControl result_drop_parent_ic, root_ic);
                        if (result_drop_idx >= 0) {
                            drop_parent_ic = result_drop_parent_ic;
                            return result_drop_idx;
                        }
                    }
                }
            }

            string tail_side = orientation == Orientation.Horizontal ? "r" : "b";
            if (closet_side_ht.ClosestSideLabel == tail_side) {
                return closest_side_lbi_idx + 1;
            }
            return closest_side_lbi_idx;
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

            MpPoint dc_down_pos = e.GetClientMousePoint(control);
            bool is_pointer_dragging = false;
            bool was_drag_started = false;

            EventHandler<PointerReleasedEventArgs> dragControl_PointerReleased_Handler = null;
            EventHandler<PointerEventArgs> dragControl_PointerMoved_Handler = null;
            EventHandler userCancel_Handler = null;

            // WINDOW POSITION INIT
            if (wpl is Window window) {
                wpl.DragPointerPosition = e.GetPosition(window).ToPortablePoint();
            }

            // MOVE

            dragControl_PointerMoved_Handler = (s, e1) => {
                if (wpl is Window window) {
                    wpl.DragPointerPosition = e1.GetPosition(window).ToPortablePoint();
                }

                MpPoint dc_move_pos = e1.GetClientMousePoint(control);

                var drag_dist = dc_down_pos.Distance(dc_move_pos);
                is_pointer_dragging = drag_dist >= MIN_DISTANCE;
                if (is_pointer_dragging && !was_drag_started) {

                    // DRAG START
                    if (ucn != null) {
                        ucn.OnGlobalEscKeyPressed += userCancel_Handler;
                    }
                    was_drag_started = true;
                    start?.Invoke(e);
                }

                if (was_drag_started) {
                    move?.Invoke(e1);
                }
            };

            // RELEASE

            dragControl_PointerReleased_Handler = (s, e2) => {

                // DRAG END
                if (ucn != null) {
                    ucn.OnGlobalEscKeyPressed -= userCancel_Handler;
                }

                control.PointerMoved -= dragControl_PointerMoved_Handler;
                control.PointerReleased -= dragControl_PointerReleased_Handler;
                MpConsole.WriteLine("DragCheck pointer released (was not drag)");

                end?.Invoke(e2);

            };

            // CANCEL

            userCancel_Handler = (s, e3) => {
                if (ucn != null) {
                    ucn.OnGlobalEscKeyPressed -= userCancel_Handler;
                }
                if (wpl is Window window) {
                    // when this is null on end signifies a cancel

                    wpl.DragPointerPosition = null;
                }

                // when end handler args = null signifies cancel 
                dragControl_PointerReleased_Handler(s, null);
            };

            control.PointerReleased += dragControl_PointerReleased_Handler;
            control.PointerMoved += dragControl_PointerMoved_Handler;
        }


        public static void AutoScrollItemsControl(this ItemsControl lb, DragEventArgs e) {
            var sv = lb.GetVisualDescendant<ScrollViewer>();

            if (e == null) {
                // terminating case
                _autoScrollAccumulators = null;
                return;
            }
            sv.AutoScroll(
                lb.PointToScreen(e.GetPosition(lb)).ToPortablePoint(lb.VisualPixelDensity()),
                lb,
                ref _autoScrollAccumulators);
        }
        public static MpPoint AutoScroll(
            this ScrollViewer sv,
            MpPoint gmp,
            Control relativeTo,
            ref double[] velAccumulators,
            bool performScroll = true,
            double minVel = 5,
            double[] sideThresholds = null) {
            if (sv == null) {
                return MpPoint.Zero;
            }

            sideThresholds = sideThresholds == null ? Enumerable.Repeat<double>(25, 4).ToArray() : sideThresholds;

            if (velAccumulators == null) {
                velAccumulators = new double[4];
            }

            relativeTo = relativeTo == null ? sv : relativeTo;

            //var rt_mp = VisualExtensions.PointToClient(relativeTo, rt_mp.ToAvPixelPoint(1.0d)).ToPortablePoint();

            MpRect bounds = relativeTo.Bounds.ToPortableRect(relativeTo, true);
            if (!bounds.Contains(gmp)) {
                // outside of bounds, clear v's
                velAccumulators.ForEach(x => x = 0);
                return MpPoint.Zero;
            }

            double l_dist = Math.Abs(gmp.X - bounds.Left);
            double r_dist = Math.Abs(bounds.Right - gmp.X);
            double t_dist = Math.Abs(gmp.Y - bounds.Top);
            double b_dist = Math.Abs(bounds.Bottom - gmp.Y);

            //MpConsole.WriteLine(string.Format(@"L:{0} R:{1} T:{2} B:{3}", l_dist, r_dist, t_dist, b_dist));

            double[] side_deltas = Enumerable.Repeat<double>(0, 4).ToArray();
            if (l_dist <= sideThresholds[MpRect.LEFT_IDX]) {
                side_deltas[MpRect.LEFT_IDX] = -minVel;
            } else if (r_dist <= sideThresholds[MpRect.RIGHT_IDX]) {
                side_deltas[MpRect.RIGHT_IDX] = minVel;
            }

            if (t_dist <= sideThresholds[MpRect.TOP_IDX]) {
                side_deltas[MpRect.TOP_IDX] = -minVel;
            } else if (b_dist <= sideThresholds[MpRect.BOTTOM_IDX]) {
                side_deltas[MpRect.BOTTOM_IDX] = minVel;
            }

            MpPoint scroll_delta = MpPoint.Zero;
            for (int i = 0; i < side_deltas.Length; i++) {
                if (side_deltas[i] == 0) {
                    // clear accumaltors of sides not changing
                    velAccumulators[i] = 0;
                } else {
                    // accumulate sides with delta
                    velAccumulators[i] += side_deltas[i];
                    if (i == MpRect.LEFT_IDX || i == MpRect.RIGHT_IDX) {
                        scroll_delta.X = velAccumulators[i];
                    } else {
                        scroll_delta.Y = velAccumulators[i];
                    }
                }
            }

            if (performScroll) {
                sv.ScrollByPointDelta(scroll_delta);
                MpConsole.WriteLine($"Auto-scroll delta: '{scroll_delta}'");
            }
            return scroll_delta;
        }

    }
}
