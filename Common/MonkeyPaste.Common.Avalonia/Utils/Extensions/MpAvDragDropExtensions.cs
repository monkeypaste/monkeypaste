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
using System.Diagnostics;

namespace MonkeyPaste.Common {
    public static class MpAvDragDropExtensions {

        public static IEnumerable<string> GetAllDataFormats(this IDataObject ido) {
            if(ido == null) {
                return null;
            }
            List<string> formats = ido.GetDataFormats().ToList();
            if(ido.GetFileNames() is IEnumerable<string> fps && 
                fps.Count() > 0) {
                formats.Add(MpPortableDataFormats.AvFileNames);
            }
            // return non-null (workaround since sysdo can't remove)
            return formats.Where(x=>ido.Get(x) != null);
        }

        public static object GetAllowFiles(this IDataObject ido, string format) {
            if(ido == null) {
                return null;
            }
            if(ido.Get(format) is object obj) {
                return obj;
            }
            if(format != MpPortableDataFormats.AvFileNames) {
                return null;
            }
            if(ido.GetFileNames() is IEnumerable<string> fpl &&
                fpl.Count() > 0) {
                return fpl;
            }
            return null;
        }

        public static IDataObject Clone(this IDataObject ido_source) {
            if(ido_source == null) { 
                return null;
            }
            var cavdo = new MpAvDataObject();
            var availableFormats = ido_source.GetAllDataFormats();
            availableFormats.ForEach(x => cavdo.SetData(x, ido_source.GetAllowFiles(x)));
            return cavdo;
        }

        public static void CopyFrom(this IDataObject ido, IDataObject other_ido) {
            if(ido == null || other_ido == null) {
                return;
            }
            var format_diff = ido.GetAllDataFormats().Difference(other_ido.GetAllDataFormats());
            if (format_diff.Count() > 0 && ido is DataObject) {
                // NOTE can't remove from sys dataobject
                Debugger.Break();
            }
            if (ido is DataObject sysdo) {
                other_ido.GetAllDataFormats().ForEach(x => sysdo.Set(x, other_ido.GetAllowFiles(x)));
            } else if(ido is MpPortableDataObject mpdo) {
                mpdo.DataFormatLookup.Clear();
                other_ido.GetAllDataFormats().ForEach(x => mpdo.SetData(x, other_ido.Get(x)));
            }
        }
        public static bool TryRemove(this IDataObject ido, string format) {
            if(ido == null) {
                return false;
            }
            if(ido is DataObject sysdo) {
                // probably exception
                sysdo.Set(format, null);
                return true;
            } else if(ido is MpPortableDataObject mpdo &&
                MpPortableDataFormats.GetDataFormat(format) is MpPortableDataFormat pdf) {
                mpdo.DataFormatLookup.Remove(pdf);
                return true;
            }
            return false;
        }

        public static void Set(this IDataObject ido, string format, object data) {
            if (ido == null) {
                return;
            }
            if (ido is DataObject sysdo) {
                // probably exception
                sysdo.Set(format, data);
            } else if (ido is MpPortableDataObject mpdo) {
                mpdo.SetData(format, data);
            }
        }
        public static bool TryClear(this IDataObject ido) {
            if(ido == null) {
                return false;
            }
            bool result = true;
            foreach(var format in ido.GetAllDataFormats()) {
                if(!ido.TryRemove(format)) {
                    result = false;
                }
            }
            return result;
        }

        public static bool ContainsInternalContentItem(this IDataObject ido) {
            if(ido == null) {
                return false;
            }
            return ido.GetDataFormats().Contains(MpPortableDataFormats.INTERNAL_CLIP_TILE_REF_FORMAT);
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

            MpRect bounds = relativeTo.Bounds.ToPortableRect(relativeTo,true);
            if(!bounds.Contains(gmp)) {
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
                    if(i == MpRect.LEFT_IDX || i == MpRect.RIGHT_IDX) {
                        scroll_delta.X = velAccumulators[i];
                    } else {
                        scroll_delta.Y = velAccumulators[i];
                    }
                }
            }

            if(performScroll) {
                sv.ScrollByPointDelta(scroll_delta);
            }
            return scroll_delta;
        }
    }
}
