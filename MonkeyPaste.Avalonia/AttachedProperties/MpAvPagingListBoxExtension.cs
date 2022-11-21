using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.Utilities;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPagingListBoxExtension {
        #region Private Variables

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

        #region CanScrollX AvaloniaProperty
        public static bool GetCanScrollX(AvaloniaObject obj) {
            return obj.GetValue(CanScrollXProperty);
        }

        public static void SetCanScrollX(AvaloniaObject obj, bool value) {
            obj.SetValue(CanScrollXProperty, value);
        }

        public static readonly AttachedProperty<bool> CanScrollXProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "CanScrollX",
                true,
                false);

        #endregion

        #region CanScrollY AvaloniaProperty
        public static bool GetCanScrollY(AvaloniaObject obj) {
            return obj.GetValue(CanScrollYProperty);
        }

        public static void SetCanScrollY(AvaloniaObject obj, bool value) {
            obj.SetValue(CanScrollYProperty, value);
        }

        public static readonly AttachedProperty<bool> CanScrollYProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "CanScrollY",
                true,
                false);

        #endregion

        #region IsHorizontalScrollBarVisibile AvaloniaProperty
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

        #region IsVerticalScrollBarVisibile AvaloniaProperty
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

        #region VelocityX AvaloniaProperty
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

        #region VelocityY AvaloniaProperty
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

        #region ScrollOffsetX AvaloniaProperty
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

        #region ScrollOffsetY AvaloniaProperty
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

        #region MaxScrollOffsetX AvaloniaProperty
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

        #region MaxScrollOffsetY AvaloniaProperty
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

        #region FrictionX AvaloniaProperty
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

        #region FrictionY AvaloniaProperty
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

        #region WheelDampeningX AvaloniaProperty
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

        #region WheelDampeningY AvaloniaProperty
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

        #region IsThumbDragging AvaloniaProperty
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

        #region IsThumbDraggingX AvaloniaProperty
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

        #region IsThumbDraggingY AvaloniaProperty
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

        #region CanThumbDrag AvaloniaProperty
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

        #region CanThumbDragX AvaloniaProperty
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

        #region CanThumbDragY AvaloniaProperty
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

        #region IsScrollJumping AvaloniaProperty
        public static bool GetIsScrollJumping(AvaloniaObject obj) {
            return obj.GetValue(IsScrollJumpingProperty);
        }

        public static void SetIsScrollJumping(AvaloniaObject obj, bool value) {
            obj.SetValue(IsScrollJumpingProperty, value);
        }

        public static readonly AttachedProperty<bool> IsScrollJumpingProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, bool>(
                "IsScrollJumping",
                false,
                false,
                BindingMode.TwoWay);

        #endregion

        #region ListOrientation AvaloniaProperty
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

        #region LayoutType AvaloniaProperty
        public static MpAvClipTrayLayoutType GetLayoutType(AvaloniaObject obj) {
            return obj.GetValue(LayoutTypeProperty);
        }

        public static void SetLayoutType(AvaloniaObject obj, MpAvClipTrayLayoutType value) {
            obj.SetValue(LayoutTypeProperty, value);
        }

        public static readonly AttachedProperty<MpAvClipTrayLayoutType> LayoutTypeProperty =
            AvaloniaProperty.RegisterAttached<object, ListBox, MpAvClipTrayLayoutType>(
                "LayoutType",
                MpAvClipTrayLayoutType.Stack,
                false);

        #endregion

        #region ScrollViewer AvaloniaProperty
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

        #region IsEnabled AvaloniaProperty
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

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            //MpPoint lastMousePosition = null;

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

        #region Internal Event Handlers

        private static void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is ListBox lb) {
                lb.DetachedFromVisualTree += DetachedFromVisualHandler;

                lb.AddHandler(
                    InputElement.PointerWheelChangedEvent,
                    PointerMouseWheelHandler,
                    RoutingStrategies.Tunnel);

                lb.AddHandler(
                    InputElement.PointerPressedEvent,
                    PreviewControlPointerPressedHandler,
                    RoutingStrategies.Tunnel);

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
                });
            }
        }

        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is ListBox lb) {
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
        }

        private static void PreviewControlPointerPressedHandler(object s, PointerPressedEventArgs e) {
            // when user clicks always halt any animated scrolling
            if (s is ListBox lb) {
                SetVelocityX(lb, 0);
                SetVelocityY(lb, 0);
            }
            e.Handled = false;
        }

        private static void PointerMouseWheelHandler(object s, global::Avalonia.Input.PointerWheelEventArgs e) {
            if (s is ListBox lb) {
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

                bool isScrollHorizontal = (lb_orientation == Orientation.Horizontal && layout_type == MpAvClipTrayLayoutType.Stack) ||
                                            (lb_orientation == Orientation.Vertical && layout_type == MpAvClipTrayLayoutType.Grid);
                double v0x = isScrollHorizontal
                                ? e.Delta.Y * vFactor : e.Delta.X * vFactor;
                double v0y = isScrollHorizontal
                                ? e.Delta.X * vFactor : e.Delta.Y * vFactor;

                double vx = v0x - (v0x * dampX);
                double vy = v0y - (v0y * dampY);

                SetVelocityX(lb, vx);
                SetVelocityY(lb, vy);
            }
        }
                
        private static void ScrollViewerPointerPressedHandler(object s, PointerPressedEventArgs e) {
            //BUG not sure why but track and thumb don't have tag set here after orientation changes
            var sv = s as ScrollViewer;
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
            //if(thumb.RenderTransform is TransformOperations tos) {
            //    tos.
            //}
            //thumb.RenderTransform = new TranslateTransform();

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
                if (sv.DataContext is MpViewModelBase vm &&
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

                scrollOffsetX += vx;
                scrollOffsetY += vy;

                vx *= GetFrictionX(lb);
                vy *= GetFrictionY(lb);

                // set scroll offset for container scroll viewer (bound to tracks)
                SetScrollOffsetX(lb, scrollOffsetX);
                SetScrollOffsetY(lb, scrollOffsetY);

                //if(!GetIsThumbDragging(lb)) 
                {
                    // manually set actual listbox scroll
                    // so thumb drag smoothly scrolls (updates visual container sv)
                    // and load more check doesn't occur to mouse up
                    var lb_sv = lb.GetVisualDescendant<ScrollViewer>();
                    lb_sv.ScrollToHorizontalOffset(scrollOffsetX);
                    lb_sv.ScrollToVerticalOffset(scrollOffsetY);

                    sv.ScrollToHorizontalOffset(scrollOffsetX);
                    sv.ScrollToVerticalOffset(scrollOffsetY);
                }

                SetVelocityX(lb, vx);
                SetVelocityY(lb, vy);

                //sv.ScrollToHorizontalOffset(scrollOffsetX);
                //sv.ScrollToVerticalOffset(scrollOffsetY);


            }
        }

        #endregion

        #region Private Helper Methods

        private static bool BindScrollViewerAndTracks(ListBox lb) {
            if (GetScrollViewer(lb) is ScrollViewer sv &&
                sv.DataContext is MpIPagingScrollViewerViewModel psvvm) {
                sv.Tag = lb;
                if (sv.TryGetVisualDescendants<Track>(out var tracks) && tracks.Count() == 2) {
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
                        track.IsThumbDragHandled = true;

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
                    return true;

                }
            }
            return false;
        }
        private static void AdjustThumbTransform(Track track, MpPoint track_mp, bool isThumbPress) {
            var attached_control = track.Tag as AvaloniaObject;

            var thumb = track.GetVisualDescendant<Thumb>();
            if (track.Orientation == Orientation.Horizontal) {
                SetIsThumbDraggingX(attached_control, true);

                if (thumb.RenderTransform is TranslateTransform tt) {
                    if (isThumbPress) {
                        tt.X = 0;
                    } else {
                        double hw = thumb.Bounds.Width / 2;
                        double tx_min = -thumb.Bounds.X;
                        double tx_max = track.Bounds.Width - hw - thumb.Bounds.X;
                        double tx = track_mp.X - hw - thumb.Bounds.X;
                        tt.X = Math.Max(tx_min, Math.Min(tx, tx_max));
                    }
                }
            } else {
                SetIsThumbDraggingY(attached_control, true);
                if (thumb.RenderTransform  is TranslateTransform tt) {
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
        }

        private static void FinishThumbDrag(ListBox lb, Track track) {
            var thumb = track.GetVisualDescendant<Thumb>();
            if (GetIsThumbDraggingX(lb)) {
                if (thumb.RenderTransform is TranslateTransform tt) {
                    double hw = thumb.Bounds.Width / 2;
                    double x = tt.X + thumb.Bounds.X + hw;
                    track.Value = track.ValueFromPoint(new Point(x, 0));
                    tt.X = 0;
                    SetScrollOffsetX(lb, track.Value);
                    SetIsThumbDraggingX(lb, false);
                }

            } else if (GetIsThumbDraggingY(lb)) {
                if (thumb.RenderTransform is TranslateTransform tt) {
                    double hh = thumb.Bounds.Height / 2;
                    double y = tt.Y + thumb.Bounds.Y + hh;
                    track.Value = track.ValueFromPoint(new Point(0, y));
                    tt.Y = 0;
                    SetScrollOffsetY(lb, track.Value);
                    SetIsThumbDraggingY(lb, false);
                }
            } else {
                // shouldn't happen
                Debugger.Break();
            }
        }
        #endregion

        #region Public Methods
        public static bool CheckAndDoAutoScrollJump(ScrollViewer sv, ListBox lb, MpPoint gmp) {
            if(!GetCanThumbDrag(lb)) {
                return false;
            }

            Track hit_track = null;
            var tracks = sv.GetVisualDescendants<Track>();
            foreach(var track in tracks) {
                var track_rect = track.Bounds.ToPortableRect(track, true);
                if(track_rect.Contains(gmp)) {
                    if (track.Orientation == Orientation.Horizontal &&
                        GetCanThumbDragX(lb)) {
                        hit_track = track;
                    } else if(track.Orientation == Orientation.Vertical &&
                        GetCanThumbDragY(lb)) {
                        hit_track = track;
                    }
                    break;
                }
            }

            if(hit_track == null) {
                if(GetIsThumbDragging(lb)) {
                    Track finish_track = null;
                    if(GetIsThumbDraggingX(lb)) {
                        finish_track = tracks.FirstOrDefault(x => x.Orientation == Orientation.Horizontal);
                    } else {
                        finish_track = tracks.FirstOrDefault(x => x.Orientation == Orientation.Vertical);
                    }
                    if(finish_track == null) {
                        // not sure how this could happen but probably can so clear all state here
                        SetIsThumbDraggingX(lb, false);
                        SetIsThumbDraggingY(lb, false);
                    } else {
                        // trigger jump if was thumb dragging
                        FinishThumbDrag(lb, finish_track);
                    }
                    if(finish_track != null) {
                        // flag actual jump as true so timer blocks until tray finishes requery
                        return true;
                    }
                }
                return false;
            }

            bool is_thumb_press = !GetIsThumbDragging(lb);
            if(is_thumb_press) {
                // must be initial hit
                if(hit_track.Orientation == Orientation.Horizontal) {
                    SetIsThumbDraggingX(lb, true);
                } else {
                    SetIsThumbDraggingY(lb, true);
                }
            }
            AdjustThumbTransform(hit_track, gmp.TranslatePoint(hit_track, false), is_thumb_press);

            return true;
        }
        #endregion
    }

}
