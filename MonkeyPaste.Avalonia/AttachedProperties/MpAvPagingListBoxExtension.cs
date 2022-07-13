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
                DetachedToVisualHandler(element, null);
            }

            #region ListBox Events (ItemsRepeater)

            void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is ListBox lb) {
                    lb.DetachedFromVisualTree += DetachedToVisualHandler;

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

                    Dispatcher.UIThread.Post(async () => {
                        var sv = GetScrollViewer(lb);
                        while (sv == null) {
                            sv = lb.GetVisualAncestor<ScrollViewer>();
                            await Task.Delay(100);
                        }

                        SetScrollViewer(lb, sv);
                        sv.Tag = lb;
                        sv.EffectiveViewportChanged += Sv_EffectiveViewportChanged;

                        while (!BindScrollViewerAndTracks(lb)) {
                            await Task.Delay(1000);
                        }
                        timer.Start();
                    });
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is ListBox lb) {
                    if (GetScrollViewer(lb) is ScrollViewer sv) {
                        if (sv.TryGetVisualDescendants<Track>(out var tracks)) {
                            foreach (var track in tracks) {
                                track.RemoveHandler(
                                    ListBox.PointerPressedEvent,
                                    PreviewTrackPointerPressedHandler);

                                //track.RemoveHandler(
                                //    ListBox.PointerMovedEvent,
                                //    PreviewTrackPointerMovedHandler);

                                track.RemoveHandler(
                                    ListBox.PointerReleasedEvent,
                                    PreviewTrackPointerReleasedHandler);

                                //if (track.Thumb is Thumb thumb) {
                                //    thumb.RemoveHandler(
                                //            ListBox.PointerPressedEvent,
                                //            PreviewThumbPointerPressedHandler);

                                //    thumb.RemoveHandler(
                                //        ListBox.PointerMovedEvent,
                                //        PreviewThumbPointerMovedHandler);

                                //    thumb.RemoveHandler(
                                //        ListBox.PointerReleasedEvent,
                                //        PreviewThumbPointerReleasedHandler);
                                //}
                            }
                        }
                    }
                    lb.AttachedToVisualTree -= AttachedToVisualHandler;
                    lb.DetachedFromVisualTree -= DetachedToVisualHandler;

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
                    double vFactor = -120;

                    double v0x = lb_orientation == Orientation.Horizontal ? e.Delta.Y * vFactor : e.Delta.X * vFactor;
                    double v0y = lb_orientation == Orientation.Horizontal ? e.Delta.X * vFactor : e.Delta.Y * vFactor;

                    double vx = v0x - (v0x * dampX);
                    double vy = v0y - (v0y * dampY);

                    SetVelocityX(lb, vx);
                    SetVelocityY(lb, vy);
                }
            }

            #endregion

            #region ScrollViewer Events
            void Sv_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
                return;
            }

            bool BindScrollViewerAndTracks(ListBox lb) {
                if (GetScrollViewer(lb) is ScrollViewer sv) {                    
                    if (sv.TryGetVisualDescendants<Track>(out var tracks) && tracks.Count() > 0) {
                        foreach (var track in tracks) {
                            track.Tag = lb;
                            track.IsThumbDragHandled = true;
                            track.Bind(
                                    Track.ValueProperty,
                                    new Binding() {
                                        Source = lb.DataContext,
                                        Path = track.Orientation == Orientation.Horizontal ?
                                                nameof(MpAvClipTrayViewModel.Instance.ScrollOffsetX) :
                                                nameof(MpAvClipTrayViewModel.Instance.ScrollOffsetY),
                                        //Mode = BindingMode.TwoWay,
                                        //Priority = BindingPriority.Style
                                    });
                            ScrollViewer.HorizontalScrollBarMaximumProperty
                            track.Bind(
                                    Track.MaximumProperty,
                                    new Binding() {
                                        Source = lb.DataContext,
                                        Path = track.Orientation == Orientation.Horizontal ?
                                                nameof(MpAvClipTrayViewModel.Instance.MaxScrollOffsetX) :
                                                nameof(MpAvClipTrayViewModel.Instance.MaxScrollOffsetY),
                                        //Mode = BindingMode.OneWay
                                        //Priority = BindingPriority.Style
                                    });
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

                        return tracks.Count() > 0;
                    } else if (sv.TryGetVisualDescendants<Thumb>(out var thumbs) && thumbs.Count() > 0) {
                        Debugger.Break();
                    }
                }
                return false;
            }

            #region Track Events

            void PreviewTrackPointerPressedHandler(object? s, PointerPressedEventArgs e) {
                if (s is Track track &&
                    track.Tag is ListBox lb) {
                    MpConsole.WriteLine("Track Mouse Down");
                }
                e.Handled = true;
                //if (s is Track track &&
                //    track.Tag is ListBox lb) {

                //    // NOTE if pointer pressed on thumb this event shouldn't fire

                //    //if (GetIsThumbDragging(lb)) {
                //    //    var track_mp = e.GetCurrentPoint(track).Position;
                //    //    SetTrackValue(track, track_mp);
                //    //    SetIsThumbDragging(lb, false);
                //    //}
                //    //e.GetCurrentPoint(track).Pointer.Capture(null);

                //    var track_mp = e.GetPosition(track);

                //    // BUG for horizontal decrease button bounds width == 0 so use track height
                //    //var db_bounds = new Rect(
                //    //    track.DecreaseButton.Bounds.Position,
                //    //    new Size(
                //    //        track.Orientation == Orientation.Horizontal ? track.Bounds.Height : track.DecreaseButton.Bounds.Height,
                //    //        track.Orientation == Orientation.Horizontal ? track.DecreaseButton.Bounds.Width : track.Bounds.Width));

                //    //// BUG for horizontal increase button bounds width ==  so use track height
                //    //var ib_bounds = new Rect(
                //    //    track.IncreaseButton.Bounds.Position,
                //    //    new Size(
                //    //        track.Orientation == Orientation.Horizontal ? track.Bounds.Height : track.DecreaseButton.Bounds.Height,
                //    //        track.Orientation == Orientation.Horizontal ? track.DecreaseButton.Bounds.Width : track.Bounds.Width));

                //    //if (track.DecreaseButton.Bounds.Contains(track_mp) ||
                //    //    track.IncreaseButton.Bounds.Contains(track_mp)) {
                //    //    MpConsole.WriteLine("Track button pressed, ignoring jump");
                //    //    return;
                //    //}
                //    //if (track.Thumb.Bounds.Contains(e.GetPosition(track.Thumb.Parent))) {
                //    //    Debugger.Break();
                //    //    return;
                //    //}

                //    MpConsole.WriteLine($"Track Mouse Released ");

                //    if (track.Orientation == Orientation.Horizontal) {
                //        double adjusted_viewport_width = track.ViewportSize;// - track.DecreaseButton.Bounds.Width - track.IncreaseButton.Bounds.Width;

                //        // track bounds includes arrow buttons so offset from low bound
                //        // to find click spot
                //        double adjusted_track_mp_x = track_mp.X;// + track.DecreaseButton.Bounds.Width;

                //        // if click was near right arrow button clamp mp.x to visible viewport width
                //        //adjusted_track_mp_x = Math.Min(adjusted_track_mp_x, adjusted_viewport_width);

                //        // get normalized distance along adjusted track to project new track value
                //        double adjusted_ratio = adjusted_track_mp_x / adjusted_viewport_width;

                //        // minimum should be zero so check thesee are same
                //        double new_track_value_test = track.Maximum * adjusted_ratio;
                //        double new_track_value = track.ValueFromPoint(track_mp); //track.Minimum + ((track.Maximum - track.Minimum) * adjusted_ratio);

                //        MpConsole.WriteLine("");
                //        MpConsole.WriteLine($"Horizontal scroll jump");
                //        MpConsole.WriteLine($"Track Min: {track.Minimum} Track Max: {track.Maximum}");
                //        MpConsole.WriteLine($"Viewport size: {track.ViewportSize} Adjusted Viewport size: {adjusted_viewport_width}");
                //        MpConsole.WriteLine($"Decrease Button Width: {track.DecreaseButton.Bounds.Width}");
                //        MpConsole.WriteLine($"Viewport size: {track.ViewportSize} Adjusted Viewport size: {adjusted_viewport_width}");
                //        MpConsole.WriteLine($"Track Value: {track.Value} New Value: {new_track_value} New Value Check: {new_track_value_test}");
                //        MpConsole.WriteLine($"ScrollOffset (should match track value): {GetScrollOffsetX(lb)}");

                //        SetScrollOffsetX(lb, new_track_value);

                //        track.GetVisualAncestor<ScrollViewer>().ScrollToHorizontalOffset(new_track_value);
                //    }

                //    //e.Handled = true;
                //    return;
                //}
                //e.Handled = false;
            }


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

                    //var htrack = sv.GetVisualDescendants<Track>().FirstOrDefault(x => x.Orientation == Orientation.Horizontal);
                    //var vtrack = sv.GetVisualDescendants<Track>().FirstOrDefault(x => x.Orientation == Orientation.Vertical);
                    //if (htrack == null || vtrack == null) {
                    //    return;
                    //}
                    //double extent_width = htrack.Maximum;
                    //double extent_height = vtrack.Maximum;
                    //double viewport_width = htrack.ViewportSize;
                    //double viewport_height = vtrack.ViewportSize;
                    //sv.Extent = MpAvClipTrayViewModel.Instance.ClipTrayExtentSize;
                    //sv.Viewport = MpAvClipTrayViewModel.Instance.ClipTrayViewportSize;

                    //double extent_width = sv.Extent.Width;
                    //double extent_height = sv.Extent.Height;
                    //double viewport_width = sv.Viewport.Width;
                    //double viewport_height = sv.Viewport.Height;

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

                    //update listbox sv not the parent scroll vieweerr
                    var control_sv = lb.GetVisualDescendant<ScrollViewer>();
                    control_sv.ScrollToHorizontalOffset(scrollOffsetX);
                    control_sv.ScrollToVerticalOffset(scrollOffsetY);

                    SetVelocityX(lb, vx);
                    SetVelocityY(lb, vy);
                }
            }


            void SetTrackValue(Track track, Point track_mp) {
                if (track.Tag is ListBox lb) {
                    if (track.Orientation == Orientation.Horizontal) {
                        SetVelocityX(lb, 0);

                        double new_x = (track_mp.X / track.Bounds.Width) * track.Maximum;
                        new_x = Math.Min(Math.Max(track.Minimum, new_x), track.Maximum);
                        SetScrollOffsetX(lb, new_x);
                    } else {
                        SetVelocityY(lb, 0);

                        double new_y = (track_mp.Y / track.Bounds.Height) * track.Maximum;
                        new_y = Math.Min(Math.Max(track.Minimum, new_y), track.Maximum);
                        SetScrollOffsetY(lb, new_y);
                    }
                }
            }
        }
               

        #endregion
    }

}
