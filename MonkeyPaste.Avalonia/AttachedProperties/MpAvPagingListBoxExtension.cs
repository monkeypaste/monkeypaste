using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPagingListBoxExtension {
        #region Private Variables

        private static MpPoint _touch_accel;
        private static MpPoint _last_scroll_offset;

        private static MpPoint _down_touch_loc;
        private static MpPoint _downOffset;

        private static MpPoint _last_touch_loc;
        private static MpPoint _last_v;

        private static DateTime? _last_touch_dt;
        #endregion

        #region Constants

        public const int SCROLL_TICK_INTERVAL_MS = 20;
        public const double MIN_SCROLL_VELOCITY_MAGNITUDE = 0.1d;

        #endregion

        #region Constructors

        static MpAvPagingListBoxExtension() {
            IsEnabledProperty.Changed.AddClassHandler<ListBox>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion

        #region Properties

        #region Kinetic Properties

        #region VelocityX 
        public static double GetVelocityX(AvaloniaObject obj) {
            return obj.GetValue(VelocityXProperty);
        }

        public static void SetVelocityX(AvaloniaObject obj, double value) {
            obj.SetValue(VelocityXProperty, value);
        }

        public static readonly AttachedProperty<double> VelocityXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "VelocityX",
                0.0d,
                false,
                BindingMode.TwoWay);

        #endregion

        #region VelocityY 
        public static double GetVelocityY(AvaloniaObject obj) {
            return obj.GetValue(VelocityYProperty);
        }

        public static void SetVelocityY(AvaloniaObject obj, double value) {
            obj.SetValue(VelocityYProperty, value);
        }

        public static readonly AttachedProperty<double> VelocityYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "VelocityY",
                0.0d,
                false,
                BindingMode.TwoWay);

        #endregion

        #region FrictionX 
        public static double GetFrictionX(AvaloniaObject obj) {
            return obj.GetValue(FrictionXProperty);
        }

        public static void SetFrictionX(AvaloniaObject obj, double value) {
            obj.SetValue(FrictionXProperty, value);
        }

        public static readonly AttachedProperty<double> FrictionXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "FrictionX",
                0.85d,
                false);

        #endregion

        #region FrictionY 
        public static double GetFrictionY(AvaloniaObject obj) {
            return obj.GetValue(FrictionYProperty);
        }

        public static void SetFrictionY(AvaloniaObject obj, double value) {
            obj.SetValue(FrictionYProperty, value);
        }

        public static readonly AttachedProperty<double> FrictionYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "FrictionY",
                0.85d,
                false);

        #endregion

        #region WheelDampeningX 
        public static double GetWheelDampeningX(AvaloniaObject obj) {
            return obj.GetValue(WheelDampeningXProperty);
        }

        public static void SetWheelDampeningX(AvaloniaObject obj, double value) {
            obj.SetValue(WheelDampeningXProperty, value);
        }

        public static readonly AttachedProperty<double> WheelDampeningXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "WheelDampeningX",
                0.08d,
                false);

        #endregion

        #region WheelDampeningY 
        public static double GetWheelDampeningY(AvaloniaObject obj) {
            return obj.GetValue(WheelDampeningYProperty);
        }

        public static void SetWheelDampeningY(AvaloniaObject obj, double value) {
            obj.SetValue(WheelDampeningYProperty, value);
        }

        public static readonly AttachedProperty<double> WheelDampeningYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "WheelDampeningX",
                0.08d,
                false);

        #endregion

        #endregion

        #region Scrollbar Properties

        #region IsHorizontalScrollBarVisibile 
        public static bool? GetIsHorizontalScrollBarVisibile(AvaloniaObject obj) {
            return obj.GetValue(IsHorizontalScrollBarVisibileProperty);
        }

        public static void SetIsHorizontalScrollBarVisibile(AvaloniaObject obj, bool? value) {
            obj.SetValue(IsHorizontalScrollBarVisibileProperty, value);
        }

        public static readonly AttachedProperty<bool?> IsHorizontalScrollBarVisibileProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool?>(
                "IsHorizontalScrollBarVisibile",
                true,
                false);

        #endregion

        #region IsVerticalScrollBarVisibile 
        public static bool? GetIsVerticalScrollBarVisibile(AvaloniaObject obj) {
            return obj.GetValue(IsVerticalScrollBarVisibileProperty);
        }

        public static void SetIsVerticalScrollBarVisibile(AvaloniaObject obj, bool? value) {
            obj.SetValue(IsVerticalScrollBarVisibileProperty, value);
        }

        public static readonly AttachedProperty<bool?> IsVerticalScrollBarVisibileProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool?>(
                "IsVerticalScrollBarVisibile",
                true,
                false);

        #endregion

        #region Thumb Properties
        #region IsThumbDragging 
        public static bool GetIsThumbDragging(AvaloniaObject obj) {
            return obj.GetValue(IsThumbDraggingProperty);
        }
        public static void SetIsThumbDragging(AvaloniaObject obj, bool value) {
            obj.SetValue(IsThumbDraggingProperty, value);
        }
        public static readonly AttachedProperty<bool> IsThumbDraggingProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "IsThumbDragging",
                false,
                false);

        #endregion

        #region IsThumbDraggingX 
        public static bool GetIsThumbDraggingX(AvaloniaObject obj) {
            return obj.GetValue(IsThumbDraggingXProperty);
        }

        public static void SetIsThumbDraggingX(AvaloniaObject obj, bool value) {
            obj.SetValue(IsThumbDraggingXProperty, value);
        }

        public static readonly AttachedProperty<bool> IsThumbDraggingXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "IsThumbDraggingX",
                false,
                false,
                BindingMode.TwoWay);

        #endregion

        #region IsThumbDraggingY 
        public static bool GetIsThumbDraggingY(AvaloniaObject obj) {
            return obj.GetValue(IsThumbDraggingYProperty);
        }

        public static void SetIsThumbDraggingY(AvaloniaObject obj, bool value) {
            obj.SetValue(IsThumbDraggingYProperty, value);
        }

        public static readonly AttachedProperty<bool> IsThumbDraggingYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "IsThumbDraggingY",
                false,
                false,
                BindingMode.TwoWay);

        #endregion

        #region CanThumbDrag 
        public static bool GetCanThumbDrag(AvaloniaObject obj) {
            return obj.GetValue(CanThumbDragProperty);
        }

        public static void SetCanThumbDrag(AvaloniaObject obj, bool value) {
            obj.SetValue(CanThumbDragProperty, value);
        }

        public static readonly AttachedProperty<bool> CanThumbDragProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "CanThumbDrag",
                false,
                false,
                BindingMode.TwoWay);

        #endregion

        #region CanThumbDragX 
        public static bool GetCanThumbDragX(AvaloniaObject obj) {
            return obj.GetValue(CanThumbDragXProperty);
        }

        public static void SetCanThumbDragX(AvaloniaObject obj, bool value) {
            obj.SetValue(CanThumbDragXProperty, value);
        }

        public static readonly AttachedProperty<bool> CanThumbDragXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "CanThumbDragX",
                false,
                false,
                BindingMode.TwoWay);

        #endregion

        #region CanThumbDragY 
        public static bool GetCanThumbDragY(AvaloniaObject obj) {
            return obj.GetValue(CanThumbDragYProperty);
        }

        public static void SetCanThumbDragY(AvaloniaObject obj, bool value) {
            obj.SetValue(CanThumbDragYProperty, value);
        }

        public static readonly AttachedProperty<bool> CanThumbDragYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "CanThumbDragY",
                false,
                false,
                BindingMode.TwoWay);

        #endregion

        #endregion

        #endregion

        #region ScrollViewer Properties

        #region CanScrollX 
        public static bool GetCanScrollX(AvaloniaObject obj) {
            return obj.GetValue(CanScrollXProperty);
        }

        public static void SetCanScrollX(AvaloniaObject obj, bool value) {
            obj.SetValue(CanScrollXProperty, value);
        }

        public static readonly AttachedProperty<bool> CanScrollXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "CanScrollX",
                true);

        #endregion

        #region CanScrollY 
        public static bool GetCanScrollY(AvaloniaObject obj) {
            return obj.GetValue(CanScrollYProperty);
        }

        public static void SetCanScrollY(AvaloniaObject obj, bool value) {
            obj.SetValue(CanScrollYProperty, value);
        }

        public static readonly AttachedProperty<bool> CanScrollYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "CanScrollY",
                true);

        #endregion

        #region ScrollOffsetX 
        public static double GetScrollOffsetX(AvaloniaObject obj) {
            return obj.GetValue(ScrollOffsetXProperty);
        }

        public static void SetScrollOffsetX(AvaloniaObject obj, double value) {
            obj.SetValue(ScrollOffsetXProperty, value);
        }

        public static readonly AttachedProperty<double> ScrollOffsetXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "ScrollOffsetX",
                0.0d,
                false,
                BindingMode.TwoWay);

        #endregion

        #region ScrollOffsetY 
        public static double GetScrollOffsetY(AvaloniaObject obj) {
            return obj.GetValue(ScrollOffsetYProperty);
        }

        public static void SetScrollOffsetY(AvaloniaObject obj, double value) {
            obj.SetValue(ScrollOffsetYProperty, value);
        }

        public static readonly AttachedProperty<double> ScrollOffsetYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "ScrollOffsetY",
                0.0d,
                false,
                BindingMode.TwoWay);

        #endregion

        #region MaxScrollOffsetX 
        public static double GetMaxScrollOffsetX(AvaloniaObject obj) {
            return obj.GetValue(MaxScrollOffsetXProperty);
        }

        public static void SetMaxScrollOffsetX(AvaloniaObject obj, double value) {
            obj.SetValue(MaxScrollOffsetXProperty, value);
        }

        public static readonly AttachedProperty<double> MaxScrollOffsetXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "MaxScrollOffsetX",
                0.0d,
                false);

        #endregion

        #region MaxScrollOffsetY 
        public static double GetMaxScrollOffsetY(AvaloniaObject obj) {
            return obj.GetValue(MaxScrollOffsetYProperty);
        }

        public static void SetMaxScrollOffsetY(AvaloniaObject obj, double value) {
            obj.SetValue(MaxScrollOffsetYProperty, value);
        }

        public static readonly AttachedProperty<double> MaxScrollOffsetYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, double>(
                "MaxScrollOffsetY",
                0.0d,
                false);

        #endregion       

        #region ScrollViewer Control
        public static ScrollViewer GetScrollViewer(AvaloniaObject obj) {
            return obj.GetValue(ScrollViewerProperty);
        }

        public static void SetScrollViewer(AvaloniaObject obj, ScrollViewer value) {
            obj.SetValue(ScrollViewerProperty, value);
        }

        public static readonly AttachedProperty<ScrollViewer> ScrollViewerProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, ScrollViewer>(
                "ScrollViewer",
                null,
                false);

        #endregion

        #endregion

        #region List Properties

        #region ListOrientation 
        public static Orientation GetListOrientation(AvaloniaObject obj) {
            return obj.GetValue(ListOrientationProperty);
        }

        public static void SetListOrientation(AvaloniaObject obj, Orientation value) {
            obj.SetValue(ListOrientationProperty, value);
        }

        public static readonly AttachedProperty<Orientation> ListOrientationProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, Orientation>(
                "ListOrientation",
                Orientation.Horizontal,
                false);

        #endregion

        #region LayoutType 
        public static MpClipTrayLayoutType GetLayoutType(AvaloniaObject obj) {
            return obj.GetValue(LayoutTypeProperty);
        }

        public static void SetLayoutType(AvaloniaObject obj, MpClipTrayLayoutType value) {
            obj.SetValue(LayoutTypeProperty, value);
        }

        public static readonly AttachedProperty<MpClipTrayLayoutType> LayoutTypeProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, MpClipTrayLayoutType>(
                "LayoutType",
                MpClipTrayLayoutType.Stack,
                false);

        #endregion

        #endregion

        #region Touch Properties

        #region CanTouchScroll 
        public static bool GetCanTouchScroll(AvaloniaObject obj) {
            return obj.GetValue(CanTouchScrollProperty);
        }

        public static void SetCanTouchScroll(AvaloniaObject obj, bool value) {
            obj.SetValue(CanTouchScrollProperty, value);
        }

        public static readonly AttachedProperty<bool> CanTouchScrollProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "CanTouchScroll",
                defaultValue: true,
                defaultBindingMode: BindingMode.TwoWay);

        #endregion

        #region IsTouchScrolling 
        public static bool GetIsTouchScrolling(AvaloniaObject obj) {
            return obj.GetValue(IsTouchScrollingProperty);
        }

        public static void SetIsTouchScrolling(AvaloniaObject obj, bool value) {
            obj.SetValue(IsTouchScrollingProperty, value);
        }

        public static readonly AttachedProperty<bool> IsTouchScrollingProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "IsTouchScrolling",
                defaultValue: false,
                defaultBindingMode: BindingMode.TwoWay);

        #endregion

        #endregion

        #region IsEnabled 
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is ListBox lb) {
                    if (lb.IsInitialized) {
                        AttachedToVisualHandler(lb, null);
                    } else {
                        lb.AttachedToVisualTree += AttachedToVisualHandler;

                    }
                }
            } else {
                //DetachedFromVisualHandler(element, VisualTreeAttachmentEventArgs.Empty);
            }


        }


        #endregion

        #endregion

        #region Internal Event Handlers

        private static void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is ListBox lb) {
                lb.DetachedFromVisualTree += DetachedFromVisualHandler;

                if(MpAvThemeViewModel.Instance.IsMultiWindow) {
                    lb.AddHandler(
                    InputElement.PointerWheelChangedEvent,
                    PointerMouseWheelHandler,
                    RoutingStrategies.Tunnel);

                    lb.AddHandler(
                           InputElement.PointerPressedEvent,
                           PreviewControlPointerPressedHandler,
                           RoutingStrategies.Tunnel);
                }

                //if (GetCanTouchScroll(lb)) {

                //    lb.AddHandler(
                //        InputElement.PointerMovedEvent,
                //        PreviewControlPointerMovedHandler,
                //        RoutingStrategies.Tunnel);

                //    lb.AddHandler(
                //        InputElement.PointerReleasedEvent,
                //        PreviewControlPointerReleasedHandler,
                //        RoutingStrategies.Tunnel);
                //}


                if (e == null) {
                    lb.AttachedToVisualTree += AttachedToVisualHandler;
                }

                var timer = new DispatcherTimer(DispatcherPriority.Normal);
                timer.Tag = lb;
                timer.Interval = new TimeSpan(0, 0, 0, 0, SCROLL_TICK_INTERVAL_MS);
                timer.Tick += HandleWorldTimerTick;

                timer.Start();


                Dispatcher.UIThread.Post(async () => {
                    var sv = GetScrollViewer(lb);
                    while (sv == null) {
                        sv = lb.GetVisualAncestor<ScrollViewer>();
                        await Task.Delay(100);
                    }

                    SetScrollViewer(lb, sv);

                    while (!BindScrollViewerAndTracks(lb)) {
                        await Task.Delay(1000);
                    }


                    //sv.EffectiveViewportChanged += async (s, e) => {
                    //    while (!BindScrollViewerAndTracks(lb)) {
                    //        await Task.Delay(1000);
                    //    }
                    //}; 
                    //if (GetCanTouchScroll(lb)) {
                    //    sv.IsScrollInertiaEnabled = MpAvThemeViewModel.Instance.IsMobileOrWindowed;
                    //    if(sv.IsScrollInertiaEnabled && 
                    //    sv is MpAvPagingScrollViewer psv &&
                    //    psv.InnerGrid is { } ig) {
                    //        ig.AddHandler(Gestures.ScrollGestureEvent, (s, e) => {
                    //            SetIsTouchScrolling(lb, true);
                    //        }, RoutingStrategies.Tunnel);
                    //        ig.AddHandler(Gestures.ScrollGestureEndedEvent, (s, e) => {
                    //            SetIsTouchScrolling(lb, false);
                    //        }, RoutingStrategies.Tunnel);
                    //    }
                    //}
                });
            }
        }

        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is not ListBox lb) {
                return;
            }

            if (GetScrollViewer(lb) is ScrollViewer sv) {
                sv.RemoveHandler(
                            ScrollViewer.PointerPressedEvent,
                            ScrollViewerPointerPressedHandler);

                sv.RemoveHandler(
                    ScrollViewer.PointerMovedEvent,
                    ScrollViewerPointerMovedHandler);
                sv.RemoveHandler(
                    ScrollViewer.PointerReleasedEvent,
                    ScrollViewerPointerReleasedHandler);
            }
            lb.AttachedToVisualTree -= AttachedToVisualHandler;
            lb.DetachedFromVisualTree -= DetachedFromVisualHandler;

            lb.RemoveHandler(
                ListBox.PointerWheelChangedEvent,
                PointerMouseWheelHandler);

            lb.RemoveHandler(
                ListBox.PointerPressedEvent,
                PreviewControlPointerPressedHandler);
        }

        private static void PreviewControlPointerPressedHandler(object s, PointerPressedEventArgs e) {
            // when user clicks always halt any animated scrolling
            var lb = s as ListBox;
            if (lb == null) {
                return;
            }

            SetVelocityX(lb, 0);
            SetVelocityY(lb, 0);
            e.Handled = false;
            if (!GetCanTouchScroll(lb)) {
                return;
            }
            e.PreventGestureRecognition();
            //e.Pointer.Capture(lb);
            _down_touch_loc = e.GetPosition(lb).ToPortablePoint();
            //MpConsole.WriteLine($"Touch down loc: {_down_touch_loc}", true);
            _downOffset = new MpPoint(GetScrollOffsetX(lb), GetScrollOffsetY(lb));
            _last_scroll_offset = _downOffset;
        }

        private static void PreviewControlPointerMovedHandler(object s, PointerEventArgs e) {
            var lb = s as ListBox;
            if (lb == null) {
                return;
            }
            if (!GetCanTouchScroll(lb) ||
                !e.IsLeftDown(lb) ||
                _last_scroll_offset == null) {
                _last_scroll_offset = null;
                SetIsTouchScrolling(lb, false);
                return;
            }
            e.PreventGestureRecognition();
            SetIsTouchScrolling(lb, true);

            var cur_touch_loc = e.GetPosition(lb).ToPortablePoint();
            if (_down_touch_loc == null) {
                _down_touch_loc = cur_touch_loc;
            }
            if (_last_touch_loc == null) {
                _last_touch_loc = cur_touch_loc;
            }

            var cur_touch_dt = DateTime.Now;
            if (_last_touch_dt == null) {
                _last_touch_dt = DateTime.Now;
            }
            var cur_offset = new MpPoint(GetScrollOffsetX(lb), GetScrollOffsetY(lb));
            var new_offset = cur_offset - (cur_touch_loc - _down_touch_loc);

            //MpConsole.WriteLine($"Cur Offset: {cur_offset} New Offset: {new_offset}", true);
            //MpConsole.WriteLine($"Cur Loc: {cur_touch_loc} Down Loc: {_down_touch_loc}", false, true);

            ApplyScrollOffset(lb, new_offset.X, new_offset.Y);
            if (_last_v == null) {
                _last_v = MpPoint.Zero;
            } else {
                //_last_v = (cur_touch_loc - _last_v) / (cur_touch_dt - _last_touch_dt.Value).TotalMilliseconds;
                
                _last_v = (cur_touch_loc - _last_touch_loc) / (cur_touch_dt - _last_touch_dt.Value).TotalMilliseconds;
            }

            _last_touch_loc = cur_touch_loc;
            _last_touch_dt = cur_touch_dt;
            _last_scroll_offset = new_offset;
        }

        private static void PreviewControlPointerReleasedHandler(object s, PointerReleasedEventArgs e) {
            var lb = s as ListBox;
            if (lb == null) {
                return;
            }
            if (!GetCanTouchScroll(lb) ||
                _last_v == null) {
                return;
            }
            e.PreventGestureRecognition();

            var cur_touch_loc = e.GetPosition(lb).ToPortablePoint();
            var cur_touch_dt = DateTime.Now;
            var final_v = (cur_touch_loc - _last_touch_loc) / (cur_touch_dt - _last_touch_dt.Value).TotalMilliseconds;
            // x = v*t + 1/2*a*t^2.
            // vf^2=vi^2 + 2*a*d
            _touch_accel = ((final_v ^ 2) - (_last_v ^ 2)) / (2 * (cur_touch_loc - _last_touch_loc).Length);
            //MpConsole.WriteLine($"Touch Move Accel: {_touch_accel}");
            //_last_v *= 100;
            if (GetCanScrollY(lb)) {
                SetVelocityY(lb, _last_v.Y);
                MpConsole.WriteLine($"Y Vel: {_last_v.Y}");
            }
            if (GetCanScrollX(lb)) {
                SetVelocityX(lb, _last_v.X);
                MpConsole.WriteLine($"Y Vel: {_last_v.X}");
            }
            if (_touch_accel != null) {
                MpConsole.WriteLine($"Touch UP Accel: {_touch_accel}");
            }
            SetIsTouchScrolling(lb, false);
            _last_v = null;
            _last_scroll_offset = null;
            _last_touch_loc = null;
            _last_touch_dt = null;
        }

        private static void PointerMouseWheelHandler(object s, global::Avalonia.Input.PointerWheelEventArgs e) {
            if (s is not ListBox lb) {
                return;
            }
            if (e.KeyModifiers == KeyModifiers.Control) {
                // allow content zoom
                e.Handled = false;
                return;
            }

            bool canScroll = GetCanScrollX(lb) || GetCanScrollY(lb);
            if (!canScroll) {
                SetVelocityX(lb, 0);
                SetVelocityY(lb, 0);
                e.Handled = false;
                return;
            }

            e.Handled = true;

            double scrollOffsetX = GetScrollOffsetX(lb);
            double scrollOffsetY = GetScrollOffsetY(lb);
            double maxScrollOffsetX = GetMaxScrollOffsetX(lb);
            double maxScrollOffsetY = GetMaxScrollOffsetY(lb);
            double dampX = GetWheelDampeningX(lb);
            double dampY = GetWheelDampeningY(lb);
            var lb_orientation = GetListOrientation(lb);
            var layout_type = GetLayoutType(lb);
            double vFactor = -120;

            bool isScrollHorizontal = (lb_orientation == Orientation.Horizontal && layout_type == MpClipTrayLayoutType.Stack) ||
                                        (lb_orientation == Orientation.Vertical && layout_type == MpClipTrayLayoutType.Grid);
            double v0x = isScrollHorizontal
                            ? e.Delta.Y * vFactor : e.Delta.X * vFactor;
            double v0y = isScrollHorizontal
                            ? e.Delta.X * vFactor : e.Delta.Y * vFactor;

            double vx = v0x - (v0x * dampX);
            double vy = v0y - (v0y * dampY);

            SetVelocityX(lb, vx);
            SetVelocityY(lb, vy);
        }

        private static void ScrollViewerPointerPressedHandler(object s, PointerPressedEventArgs e) {
            //BUG not sure why but track and thumb don't have tag set here after orientation changes
            var sv = s as ScrollViewer;
            if (sv.Tag is ListBox lb &&
                GetIsTouchScrolling(lb)) {
                return;
            }
            Track track = (e.Source as Control).GetVisualAncestor<Track>();
            if (track == null) {
                return;
            }
            track.Tag = sv.Tag;
            var track_mp = e.GetPosition(track).ToPortablePoint();

            Thumb thumb = (e.Source as Control).GetVisualAncestor<Thumb>();
            bool isThumbPress = thumb != null;
            if (thumb == null) {
                thumb = track.GetVisualDescendant<Thumb>();
            }
            thumb.Tag = sv.Tag;

            AdjustThumbTransform(track, track_mp, isThumbPress);

            e.Pointer.Capture(thumb);
            e.Handled = true;
        }

        private static void ScrollViewerPointerMovedHandler(object s, PointerEventArgs e) {
            if (//s is Track track &&
                //s is ScrollBar sb &&
                s is ScrollViewer sv &&
                sv.Tag is ListBox lb &&
                GetIsThumbDragging(lb) &&
                sv.TryGetVisualDescendants<Track>(out var tracks)) {
                Track track = null;
                if (GetIsThumbDraggingX(lb)) {
                    track = tracks.FirstOrDefault(x => x.Orientation == Orientation.Horizontal);
                } else {
                    track = tracks.FirstOrDefault(x => x.Orientation == Orientation.Vertical);
                }
                var thumb = track.GetVisualDescendant<Thumb>();

                var track_mp = e.GetPosition(track).ToPortablePoint();
                AdjustThumbTransform(track, track_mp, false);

                e.Handled = true;
            }
        }
        private static void ScrollViewerPointerReleasedHandler(object s, PointerReleasedEventArgs e) {
            //MpConsole.WriteLine("ScrollViewer Release");
            if (s is ScrollViewer sv &&
                sv.Tag is ListBox lb) {
                if (GetIsThumbDragging(lb) &&
                    e.Source is Thumb thumb &&
                    thumb.GetVisualAncestor<Track>() is Track track) {
                    //finish thumb drag
                    FinishThumbDrag(lb, track);
                    e.Pointer.Capture(null);
                    e.Handled = true;
                    return;
                }
            }
        }

        private static void HandleWorldTimerTick(object sender, EventArgs e) {
            if (sender is DispatcherTimer timer &&
               timer.Tag is ListBox lb &&
               GetScrollViewer(lb) is ScrollViewer sv) {

                if (MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                    // force single axis scrolling (really hard to understand where/what causes)
                    if(!GetCanScrollX(lb)) {
                        SetAllOffsets(lb, x: 0);
                    }
                    if(!GetCanScrollY(lb)) {
                        SetAllOffsets(lb, y: 0);
                    }
                    // ntf ctrvm of scroll for query,etc.
                    SetScrollOffsetX(lb, sv.Offset.X);
                    SetScrollOffsetY(lb, sv.Offset.Y);
                    return;
                }
                if (sv.DataContext is MpAvViewModelBase vm &&
                    vm.IsBusy) {
                    return;
                }
                bool isThumbDragging = GetIsThumbDragging(lb);
                bool canScroll = GetCanScrollX(lb) || GetCanScrollY(lb);
                bool is_scroll_frozen = isThumbDragging || !canScroll;
                if (is_scroll_frozen) {
                    SetVelocityX(lb, 0);
                    SetVelocityY(lb, 0);
                    return;
                }

                double scrollOffsetX = GetScrollOffsetX(lb);
                double maxOffsetX = GetMaxScrollOffsetX(lb);

                double scrollOffsetY = GetScrollOffsetY(lb);
                double maxOffsetY = GetMaxScrollOffsetY(lb);

                double vx = GetVelocityX(lb);
                double vy = GetVelocityY(lb);

                //MpConsole.WriteLine("vx: " + vx);
                //MpConsole.WriteLine("vy: " + vy);

                if (scrollOffsetX < 0 || scrollOffsetX > maxOffsetX) {
                    scrollOffsetX = Math.Min(maxOffsetX, Math.Max(0, scrollOffsetX));
                    vx = 0;
                }
                if (scrollOffsetY < 0 || scrollOffsetY > maxOffsetY) {
                    scrollOffsetY = Math.Min(maxOffsetY, Math.Max(0, scrollOffsetY));
                    vy = 0;
                }

                vx = Math.Abs(vx) < MIN_SCROLL_VELOCITY_MAGNITUDE ? 0 : vx;
                vy = Math.Abs(vy) < MIN_SCROLL_VELOCITY_MAGNITUDE ? 0 : vy;


                //if (_touch_accel == null) {
                //    vx *= GetFrictionX(lb);
                //    vy *= GetFrictionY(lb);
                //} else {
                //    _touch_accel *= new MpPoint(GetFrictionX(lb), GetFrictionY(lb));
                //    if(_last_v == null) {
                //        _last_v = MpPoint.Zero;
                //    }

                //    //vx *= GetFrictionX(lb);
                //    //vy *= GetFrictionY(lb);

                //    // x = v*t + 1/2*a*t^2.
                //    // vf^2=vi^2 + 2*a*d
                //    // vf^2 = vi^2 + 2ad
                //    //var new_v = new MpPoint(vx, vy) + (_touch_accel * (0.5d * (double)(SCROLL_TICK_INTERVAL_MS ^ 2)));
                //    var new_v = _last_v + (_touch_accel * (double)SCROLL_TICK_INTERVAL_MS);
                //    vx = new_v.X;
                //    vy = new_v.Y;
                //    _last_v = new_v;
                //    if (_touch_accel.Length.IsFuzzyZero()) {
                //        _touch_accel = null;
                //    }
                //}
                scrollOffsetX += vx;
                scrollOffsetY += vy;


                ApplyScrollOffset(lb, scrollOffsetX, scrollOffsetY);

                SetVelocityX(lb, vx);
                SetVelocityY(lb, vy);
            }
        }

        #endregion

        #region Private Helper Methods

        private static void ApplyScrollOffset(ListBox lb, double x, double y) {
            var sv = GetScrollViewer(lb);
            if (sv == null ||
                (sv.DataContext is MpIAsyncObject vm &&
                    vm.IsBusy)) {
                return;
            }

            // set scroll offset for container scroll viewer (bound to tracks)
            var lb_sv = lb.GetVisualDescendant<ScrollViewer>();
            if (GetCanScrollX(lb)) {
                SetScrollOffsetX(lb, x);
                // manually set actual listbox scroll
                // so thumb drag smoothly scrolls (updates visual container sv)
                // and load more check doesn't occur to mouse up
                lb_sv.ScrollToHorizontalOffset(x);
                sv.ScrollToHorizontalOffset(x);

            }
            if (GetCanScrollY(lb)) {
                SetScrollOffsetY(lb, y);
                lb_sv.ScrollToVerticalOffset(y);
                sv.ScrollToVerticalOffset(y);
            }
        }
        
        private static void SetAllOffsets(ListBox lb, double x = -1, double y = -1) {
            if(GetScrollViewer(lb) is not { } sv ||
                lb.GetVisualDescendant<ScrollViewer>() is not { } lb_sv) {
                return;
            }
            if(x >= 0) {
                SetScrollOffsetX(lb, x);
                // manually set actual listbox scroll
                // so thumb drag smoothly scrolls (updates visual container sv)
                // and load more check doesn't occur to mouse up
                lb_sv.ScrollToHorizontalOffset(x);
                sv.ScrollToHorizontalOffset(x);
            }

            if(y >= 0) {
                SetScrollOffsetY(lb, y);
                lb_sv.ScrollToVerticalOffset(y);
                sv.ScrollToVerticalOffset(y);
            }
        }

        private static bool BindScrollViewerAndTracks(ListBox lb) {
            if (GetScrollViewer(lb) is MpAvPagingScrollViewer sv &&
                sv.Tracks is IReadOnlyList<Track> tracks &&
                tracks.Count == 2 &&
                sv.DataContext is MpIPagingScrollViewerViewModel psvvm) {
                sv.Tag = lb;
                sv.AddHandler(
                       ScrollViewer.PointerPressedEvent,
                       ScrollViewerPointerPressedHandler,
                       RoutingStrategies.Tunnel);
                sv.AddHandler(
                    ScrollViewer.PointerMovedEvent,
                    ScrollViewerPointerMovedHandler,
                    RoutingStrategies.Tunnel);
                sv.AddHandler(
                    ScrollViewer.PointerReleasedEvent,
                    ScrollViewerPointerReleasedHandler,
                    RoutingStrategies.Tunnel);

                foreach (var track in tracks) {
                    track.Tag = lb;
                    track.IgnoreThumbDrag = true;

                    track.Bind(
                            Track.MaximumProperty,
                            new Binding() {
                                Source = lb.DataContext,
                                Path = track.Orientation == Orientation.Horizontal ?
                                        nameof(psvvm.MaxScrollOffsetX) :
                                        nameof(psvvm.MaxScrollOffsetY)
                            });

                    track.Minimum = 0;

                    if (track.Thumb is Thumb thumb) {
                        thumb.Tag = lb;
                        thumb.RenderTransform = new TranslateTransform();
                    }

                }

                sv.ScrollBars
                    .Select(x => x.LineUpButton)
                    .Union(sv.ScrollBars.Select(x => x.LineDownButton))
                    .ForEach(x => {
                        x.Command = RepeatButtonCommand;
                        x.CommandParameter = x;
                        x.PointerReleased += (s, e) => {
                            x.Tag = null;
                        };
                    });
                return true;
            }
            return false;
        }
        private static ICommand RepeatButtonCommand => new MpCommand<object>(
            (args) => {
                if (args is not RepeatButton rb ||
                    rb.GetVisualAncestor<ScrollBar>() is not ScrollBar sb ||
                    sb.GetVisualAncestor<ScrollViewer>() is not ScrollViewer sv ||
                    sv.Tag is not ListBox lb) {
                    return;
                }

                MpPoint dir = MpPoint.Zero;
                if (sb.Orientation == Orientation.Horizontal) {
                    dir.X = rb.Name.ToLowerInvariant().Contains("down") ? 1 : -1;
                } else {
                    dir.Y = rb.Name.ToLowerInvariant().Contains("down") ? 1 : -1;
                }
                double repeat_val = 0;
                if (rb.Tag is double) {
                    repeat_val = (double)rb.Tag;
                }
                double repeat_inc = 10;
                repeat_val += repeat_inc;
                rb.Tag = repeat_val;
                var delta = dir * repeat_val;
                double scroll_x = GetScrollOffsetX(lb);
                double scroll_y = GetScrollOffsetY(lb);

                ApplyScrollOffset(lb, scroll_x + delta.X, scroll_y + delta.Y);
            });

        private static void AdjustThumbTransform(Track track, MpPoint track_mp, bool isThumbPress) {
            var attached_control = track.Tag as AvaloniaObject;
            if (attached_control == null ||
                track == null ||
                track.GetVisualDescendant<Thumb>() is not { } thumb ||
                thumb.RenderTransform is not TranslateTransform tt) {
                // BUG this happened when clicking an editor link, 
                // which opened an cef browser window for the link, 
                // then after closing the window, attached_control was null
                // when mw finished hiding in vertical orientation on the right
                return;
            }

            if (track.Orientation == Orientation.Horizontal) {
                SetIsThumbDraggingX(attached_control, true);

                if (isThumbPress) {
                    tt.X = 0;
                } else {
                    double hw = thumb.Bounds.Width / 2;
                    double tx_min = -thumb.Bounds.X;
                    double tx_max = track.Bounds.Width - hw - thumb.Bounds.X;
                    double tx = track_mp.X - hw - thumb.Bounds.X;
                    tt.X = Math.Max(tx_min, Math.Min(tx, tx_max));
                }
            } else {
                SetIsThumbDraggingY(attached_control, true);
                if (isThumbPress) {
                    tt.Y = 0;
                } else {
                    double hh = thumb.Bounds.Height / 2;
                    double ty_min = -thumb.Bounds.Y;
                    double ty_max = track.Bounds.Height - hh - thumb.Bounds.Y;
                    double ty = track_mp.Y - hh - thumb.Bounds.Y;
                    tt.Y = Math.Max(ty_min, Math.Min(ty, ty_max));
                }
            }
        }

        private static void FinishThumbDrag(ListBox lb, Track track) {
            var thumb = track.GetVisualDescendant<Thumb>();

            var tt = thumb.RenderTransform as TranslateTransform;

            if (GetIsThumbDraggingX(lb)) {
                double hw = thumb.Bounds.Width / 2;
                double x = tt.X + thumb.Bounds.X + hw;

                double final_val = track.ValueFromPoint(new Point(x, 0));
                if (track.GetVisualAncestor<ScrollBar>() is ScrollBar sb) {
                    sb.SetCurrentValue(ScrollBar.ValueProperty, final_val);
                }

                tt.X = 0;
                SetScrollOffsetX(lb, final_val);
                SetIsThumbDraggingX(lb, false);
            } else if (GetIsThumbDraggingY(lb)) {
                double hh = thumb.Bounds.Height / 2;
                double y = tt.Y + thumb.Bounds.Y + hh;

                double final_val = track.ValueFromPoint(new Point(0, y));
                if (track.GetVisualAncestor<ScrollBar>() is ScrollBar sb) {
                    sb.SetCurrentValue(ScrollBar.ValueProperty, final_val);
                }
                tt.Y = 0;
                SetScrollOffsetY(lb, track.Value);
                SetIsThumbDraggingY(lb, false);
            } else {
                // shouldn't happen
                MpDebug.Break();
            }
        }
       
        #endregion

        #region Public Methods
        public static bool CheckAndDoAutoScrollJump(ScrollViewer sv, ListBox lb, MpPoint gmp) {
            if (!GetCanThumbDrag(lb)) {
                return false;
            }

            Track hit_track = null;
            var tracks = sv.GetVisualDescendants<Track>();
            foreach (var track in tracks) {
                var track_rect = track.Bounds.ToPortableRect(track, true);
                if (track_rect.Contains(gmp)) {
                    if (track.Orientation == Orientation.Horizontal &&
                        GetCanThumbDragX(lb)) {
                        hit_track = track;
                    } else if (track.Orientation == Orientation.Vertical &&
                        GetCanThumbDragY(lb)) {
                        hit_track = track;
                    }
                    break;
                }
            }

            if (hit_track == null) {
                if (GetIsThumbDragging(lb)) {
                    Track finish_track = null;
                    if (GetIsThumbDraggingX(lb)) {
                        finish_track = tracks.FirstOrDefault(x => x.Orientation == Orientation.Horizontal);
                    } else {
                        finish_track = tracks.FirstOrDefault(x => x.Orientation == Orientation.Vertical);
                    }
                    if (finish_track == null) {
                        // not sure how this could happen but probably can so clear all state here
                        SetIsThumbDraggingX(lb, false);
                        SetIsThumbDraggingY(lb, false);
                    } else {
                        // trigger jump if was thumb dragging
                        FinishThumbDrag(lb, finish_track);
                    }
                    if (finish_track != null) {
                        // flag actual jump as true so timer blocks until tray finishes requery
                        return true;
                    }
                }
                return false;
            }

            bool is_thumb_press = !GetIsThumbDragging(lb);
            if (is_thumb_press) {
                // must be initial hit
                if (hit_track.Orientation == Orientation.Horizontal) {
                    SetIsThumbDraggingX(lb, true);
                } else {
                    SetIsThumbDraggingY(lb, true);
                }
            }
            AdjustThumbTransform(hit_track, gmp.TranslatePoint(hit_track, false), is_thumb_press);

            return true;
        }

        public static void ForceScrollOffset(MpPoint offset) {
            if (MpAvMainView.Instance.GetVisualDescendant<MpAvQueryTrayView>() is not MpAvQueryTrayView qtv ||
                qtv.FindControl<ListBox>("ClipTrayListBox") is not ListBox qt_lb) {
                return;
            }
            ApplyScrollOffset(qt_lb, offset.X, offset.Y);
        }
        #endregion
    }

}
