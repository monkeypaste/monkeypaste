using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
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

            #region ListBox Events (ItemsRepeater)

            void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
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
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                    timer.Tick += HandleWorldTimerTick;

                    timer.Start();


                    Dispatcher.UIThread.Post(async () => {
                        var sv = GetScrollViewer(lb);
                        while (sv == null) {
                            sv = lb.GetVisualAncestor<ScrollViewer>();
                            await Task.Delay(100);
                        }

                        SetScrollViewer(lb, sv);
                        sv.Tag = lb;

                        while (!BindScrollViewerAndTracks(lb)) {
                            await Task.Delay(1000);
                        }

                        var lb_sv = lb.GetVisualDescendant<ScrollViewer>();

                        lb_sv.EffectiveViewportChanged += async (s, e) => {
                            sv.Tag = lb;
                            while (!BindScrollViewerAndTracks(lb)) {
                                await Task.Delay(1000);
                            }
                        };

                        sv.EffectiveViewportChanged += async(s, e) => {
                            sv.Tag = lb;
                            while (!BindScrollViewerAndTracks(lb)) {
                                await Task.Delay(1000);
                            }
                        };
                    });
                }
            }

            void DetachedFromVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is ListBox lb) {
                    if (GetScrollViewer(lb) is ScrollViewer sv) {
                        if (sv.TryGetVisualDescendants<Track>(out var tracks)) {
                            foreach (var track in tracks) {
                                track.RemoveHandler(
                                    ListBox.PointerReleasedEvent,
                                    PreviewTrackPointerReleasedHandler);

                                if (track.Thumb is Thumb thumb) {
                                    thumb.RemoveHandler(
                                            ListBox.PointerPressedEvent,
                                            PreviewThumbPointerPressedHandler);

                                    thumb.RemoveHandler(
                                        ListBox.PointerMovedEvent,
                                        PreviewThumbPointerMovedHandler);
                                }
                            }
                        }
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

            void PreviewControlPointerPressedHandler(object? s, PointerPressedEventArgs e) {
                // when user clicks always halt any animated scrolling
                if (s is ListBox lb) {
                    SetVelocityX(lb, 0);
                    SetVelocityY(lb, 0);
                }
                e.Handled = false;
            }

            void PointerMouseWheelHandler(object? s, global::Avalonia.Input.PointerWheelEventArgs e) {
                if (s is ListBox lb) {
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

                    double v0x = lb_orientation == Orientation.Horizontal && layout_type == MpAvClipTrayLayoutType.Stack
                                    ? e.Delta.Y * vFactor : e.Delta.X * vFactor;
                    double v0y = lb_orientation == Orientation.Horizontal && layout_type == MpAvClipTrayLayoutType.Stack
                                    ? e.Delta.X * vFactor : e.Delta.Y * vFactor;

                    double vx = v0x - (v0x * dampX);
                    double vy = v0y - (v0y * dampY);

                    SetVelocityX(lb, vx);
                    SetVelocityY(lb, vy);
                }
            }

            #endregion

            #region ScrollViewer Events

            bool BindScrollViewerAndTracks(ListBox lb) {
                if (GetScrollViewer(lb) is ScrollViewer sv &&
                    sv.DataContext is MpIPagingScrollViewerViewModel psvvm) {
                    
                    if (sv.TryGetVisualDescendants<Track>(out var tracks) && tracks.Count() > 0) {

                        var lb_sv = lb.GetVisualDescendant<ScrollViewer>();
                        lb_sv.Bind(
                            ScrollViewer.HorizontalScrollBarValueProperty,
                            new Binding() {
                                Source = lb.DataContext,
                                Path = nameof(psvvm.ScrollOffsetX)
                            });

                        lb_sv.Bind(
                            ScrollViewer.VerticalScrollBarValueProperty,
                            new Binding() {
                                Source = lb.DataContext,
                                Path = nameof(psvvm.ScrollOffsetY)
                            });

                        //sv.Bind(
                        //    ScrollViewer.HorizontalScrollBarValueProperty,
                        //    new Binding() {
                        //        Source = lb.DataContext,
                        //        Path = nameof(psvvm.ScrollOffsetX)
                        //    });

                        //sv.Bind(
                        //    ScrollViewer.VerticalScrollBarValueProperty,
                        //    new Binding() {
                        //        Source = lb.DataContext,
                        //        Path = nameof(psvvm.ScrollOffsetY)
                        //    });
                        //if(sv.TryGetVisualDescendants<ScrollBar>(out var scrollBars) && scrollBars.Count() > 0) {
                        //    foreach(var sb in scrollBars) {
                        //        sb.GetObservable(ScrollBar.BoundsProperty).Subscribe(value => {
                        //            if (sb.Orientation == Orientation.Horizontal) {
                        //                MpConsole.WriteLine("H Bounds change, size: " + value.Size);
                        //                psvvm.HorizontalScrollBarDesiredSize = value.Size;
                        //            } else {
                        //                MpConsole.WriteLine("V Bounds change, size: " + value.Size);
                        //                psvvm.VerticalScrollBarDesiredSize = value.Size;
                        //            }
                        //        });
                        //    }
                        //}
                        foreach (var track in tracks) {
                            track.Tag = lb;
                            track.IsThumbDragHandled = true;                            

                            track.Bind(
                                    Track.ValueProperty,
                                    new Binding() {
                                        Source = lb.DataContext,
                                        Path = track.Orientation == Orientation.Horizontal ?
                                                nameof(psvvm.ScrollOffsetX) :
                                                nameof(psvvm.ScrollOffsetY),
                                    });

                            track.Bind(
                                    Track.MaximumProperty,
                                    new Binding() {
                                        Source = lb.DataContext,
                                        Path = track.Orientation == Orientation.Horizontal ?
                                                nameof(psvvm.MaxScrollOffsetX) :
                                                nameof(psvvm.MaxScrollOffsetY)});

                            track.Minimum = 0;
                            
                            track.AddHandler(
                                Track.PointerReleasedEvent,
                                PreviewTrackPointerReleasedHandler,
                                RoutingStrategies.Tunnel);


                            if (track.Thumb is Thumb thumb) {
                                thumb.Tag = lb;

                                thumb.AddHandler(
                                    Thumb.PointerPressedEvent,
                                    PreviewThumbPointerPressedHandler,
                                    RoutingStrategies.Tunnel);

                                thumb.AddHandler(
                                    Thumb.PointerMovedEvent,
                                    PreviewThumbPointerMovedHandler,
                                    RoutingStrategies.Tunnel);
                            }


                        }

                        return true;
                    }
                }
                return false;
            }


            #region Track Events

            void PreviewTrackPointerReleasedHandler(object? s, PointerReleasedEventArgs e) {
                if (s is Track track &&
                    track.Tag is ListBox lb) {
                    //MpDebuggerHelper.Break();
                    MpConsole.WriteLine("Track Mouse Up");
                    if(GetIsThumbDragging(lb)) {
                        SetIsThumbDragging(lb, false);                        
                    } else {
                        var track_mp = e.GetPosition(track);
                        double new_track_value = track.ValueFromPoint(track_mp); 
                        if(track.Orientation == Orientation.Horizontal) {
                            SetScrollOffsetX(lb, new_track_value);
                        } else {
                            SetScrollOffsetY(lb, new_track_value);
                        }
                    }
                    
                }
                e.Handled = true;
            }

            #endregion

            #region Thumb Events

            Point lastMousePosition;

            void PreviewThumbPointerPressedHandler(object? s, PointerPressedEventArgs e) {
                if (s is Thumb thumb &&
                    thumb.Tag is ListBox lb) {

                    MpConsole.WriteLine("Thumb Mouse Down");

                    SetIsThumbDragging(lb, true);
                    e.Pointer.Capture(thumb);
                    lastMousePosition = e.GetPosition(MpAvMainWindow.Instance);
                }
                e.Handled = true;
            }

            void PreviewThumbPointerMovedHandler(object? s, PointerEventArgs e) {
                if (s is Thumb thumb &&
                    thumb.Tag is ListBox lb &&
                    GetIsThumbDragging(lb)) {
                    var mp = e.GetPosition(MpAvMainWindow.Instance);

                    var track = thumb.GetVisualAncestor<Track>();
                    if (track.Orientation == Orientation.Horizontal) {
                        double deltaX = mp.X - lastMousePosition.X;
                        double deltaValue = track.ValueFromDistance(deltaX, 0);

                        double scrollOffsetX = GetScrollOffsetX(lb);
                        double newScrollOffset = scrollOffsetX + deltaValue;

                        MpConsole.WriteLine($"DeltaX: {deltaX} DeltaValue: {deltaValue} ScrollOffsetX: {scrollOffsetX} New ScrollOffsetX: {newScrollOffset}");
                        SetScrollOffsetX(lb, newScrollOffset);
                    } else {
                        double deltaY = mp.Y - lastMousePosition.Y;
                        double deltaValue = track.ValueFromDistance(deltaY, 0);

                        double scrollOffsetY = GetScrollOffsetY(lb);
                        double newScrollOffset = scrollOffsetY + deltaValue;

                        MpConsole.WriteLine($"DeltaY: {deltaY} DeltaValue: {deltaValue} ScrollOffsetY: {scrollOffsetY} New ScrollOffsetY: {newScrollOffset}");
                        SetScrollOffsetY(lb, newScrollOffset);
                    }
                    lastMousePosition = e.GetPosition(MpAvMainWindow.Instance);
                }
                e.Handled = true;
            }

            #endregion

            #endregion

            void HandleWorldTimerTick(object sender, EventArgs e) {
                if (sender is DispatcherTimer timer &&
                   timer.Tag is ListBox lb &&
                   GetScrollViewer(lb) is ScrollViewer sv) {
                    if (GetIsThumbDragging(lb)) {
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

                    vx = Math.Abs(vx) < 0.1d ? 0 : vx;
                    vy = Math.Abs(vy) < 0.1d ? 0 : vy;

                    scrollOffsetX += vx;
                    scrollOffsetY += vy;

                    vx *= GetFrictionX(lb);
                    vy *= GetFrictionY(lb);

                    SetScrollOffsetX(lb, scrollOffsetX);
                    SetScrollOffsetY(lb, scrollOffsetY);

                    SetVelocityX(lb, vx);
                    SetVelocityY(lb, vy);
                    
                    //sv.ScrollToHorizontalOffset(scrollOffsetX);
                    //sv.ScrollToVerticalOffset(scrollOffsetY);

                    //var lb_sv = lb.GetVisualDescendant<ScrollViewer>();
                    //lb_sv.ScrollToHorizontalOffset(scrollOffsetX);
                    //lb_sv.ScrollToVerticalOffset(scrollOffsetY);
                }
            }
        }
               

        #endregion
    }

}
