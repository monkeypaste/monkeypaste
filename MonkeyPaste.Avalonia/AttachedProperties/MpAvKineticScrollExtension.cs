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
    public static class MpAvKineticScrollExtension {
        #region Private Variables

        #endregion

        #region Constructors

        static MpAvKineticScrollExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
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
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
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
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
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
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsThumbDragging",
                false,
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
            AvaloniaProperty.RegisterAttached<object, Control, ScrollViewer>(
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
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    if (control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    } else {
                        control.AttachedToVisualTree += AttachedToVisualHandler;

                    }
                }
            } else {
                DetachedToVisualHandler(element, null);
            }

            #region Control Events (ItemsRepeater)

            void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.DetachedFromVisualTree += DetachedToVisualHandler;

                    control.AddHandler(
                        InputElement.PointerWheelChangedEvent,
                        PointerMouseWheelHandler,
                        RoutingStrategies.Tunnel);

                    control.AddHandler(
                        InputElement.PointerPressedEvent,
                        PreviewControlPointerPressedHandler,
                        RoutingStrategies.Tunnel);

                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }

                    var timer = new DispatcherTimer(DispatcherPriority.Normal);
                    timer.Tag = control;
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                    timer.Tick += HandleWorldTimerTick;

                    Dispatcher.UIThread.Post(async () => {
                        var sv = GetScrollViewer(control);
                        while (sv == null) {
                            //sv = control.GetVisualAncestor<ScrollViewer>();
                            //if (sv == null) {
                                sv = control.GetVisualDescendant<ScrollViewer>();
                                //if (sv == null) {
                                //    sv = control.GetVisualDescendant<ScrollViewer>();

                                //}
                            //}
                            await Task.Delay(100);
                        }

                        SetScrollViewer(control, sv);

                        //MpDebuggerHelper.Break();
                        //var tracks = sv.GetVisualDescendants<Thumb>();
                        //while (tracks.Count() == 0) {
                        //    tracks = sv.GetVisualDescendants<Thumb>();
                        //    await Task.Delay(100);
                        //}
                        sv.Tag = control;


                        while (!BindScrollViewerAndTracks(control, null)) {
                            await Task.Delay(1000);
                        }
                        timer.Start();
                    });
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    if (GetScrollViewer(control) is ScrollViewer sv) {
                        if (sv.TryGetVisualDescendants<Track>(out var tracks)) {
                            foreach (var track in tracks) {
                                track.RemoveHandler(
                                    Control.PointerPressedEvent,
                                    PreviewTrackPointerPressedHandler);

                                //track.RemoveHandler(
                                //    Control.PointerMovedEvent,
                                //    PreviewTrackPointerMovedHandler);

                                track.RemoveHandler(
                                    Control.PointerReleasedEvent,
                                    PreviewTrackPointerReleasedHandler);

                                //if (track.Thumb is Thumb thumb) {
                                //    thumb.RemoveHandler(
                                //            Control.PointerPressedEvent,
                                //            PreviewThumbPointerPressedHandler);

                                //    thumb.RemoveHandler(
                                //        Control.PointerMovedEvent,
                                //        PreviewThumbPointerMovedHandler);

                                //    thumb.RemoveHandler(
                                //        Control.PointerReleasedEvent,
                                //        PreviewThumbPointerReleasedHandler);
                                //}
                            }
                        }
                    }
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;

                    control.RemoveHandler(
                        Control.PointerWheelChangedEvent,
                        PointerMouseWheelHandler);

                    control.RemoveHandler(
                        Control.PointerPressedEvent,
                        PreviewControlPointerPressedHandler);
                }
            }

            void PreviewControlPointerPressedHandler(object? s, PointerPressedEventArgs e) {
                // when user clicks always halt any animated scrolling
                if (s is Control control) {
                    if (e.GetCurrentPoint(control).Properties.IsLeftButtonPressed) {
                        SetVelocityX(control, 0);
                        SetVelocityY(control, 0);
                    }
                }
                e.Handled = false;
            }

            void PointerMouseWheelHandler(object? s, global::Avalonia.Input.PointerWheelEventArgs e) {
                if (s is Control control) {
                    e.Handled = true;

                    var sv = GetScrollViewer(control);
                    if (sv == null) {
                        Debugger.Break();
                        return;
                    }

                    //var htrack = sv.GetVisualDescendants<Track>().FirstOrDefault(x => x.Orientation == Orientation.Horizontal);
                    //var vtrack = sv.GetVisualDescendants<Track>().FirstOrDefault(x => x.Orientation == Orientation.Vertical);

                    //double extent_width = htrack.Maximum;
                    //double extent_height = vtrack.Maximum;
                    //double viewport_width = htrack.ViewportSize;
                    //double viewport_height = vtrack.ViewportSize;

                    double extent_width = sv.Extent.Width;
                    double extent_height = sv.Extent.Height;
                    double viewport_width = sv.Viewport.Width;
                    double viewport_height = sv.Viewport.Height;

                    bool canScrollX = extent_width > viewport_width && GetCanScrollX(control);
                    bool canScrollY = extent_height > viewport_height && GetCanScrollY(control);
                    bool canScrollBoth = canScrollX && canScrollY;

                    bool isScrollX = (canScrollBoth && e.KeyModifiers.HasFlag(KeyModifiers.Shift)) ||
                                     (canScrollX && !canScrollY);

                    bool isScrollY = !isScrollX && canScrollY;
                    //MpDebuggerHelper.Break();

                    if (!isScrollX && !isScrollY) {
                        return;
                    }

                    double maxOffset = 0;
                    double scrollOffset = 0;
                    double v0 = 0;
                    double damp = 0;
                    int lastWheelDelta = 0;
                    double vFactor = 120;

                    if (isScrollX) {
                        damp = GetWheelDampeningX(control);
                        v0 = e.Delta.Y * -vFactor;
                        if (v0 == 0 && e.Delta.X != 0) {
                            v0 = e.Delta.X * -vFactor;
                        }
                        maxOffset = extent_width - viewport_width;
                        scrollOffset = GetScrollOffsetX(control);
                        lastWheelDelta = MpAttachedPropertyHelpers.GetInstanceProperty<int>(control, "lastWheelDeltaX");
                    } else {
                        damp = GetWheelDampeningY(control);
                        v0 = e.Delta.Y * -vFactor;
                        maxOffset = extent_height - viewport_height;
                        scrollOffset = GetScrollOffsetY(control);
                        lastWheelDelta = MpAttachedPropertyHelpers.GetInstanceProperty<int>(control, "lastWheelDeltaY");
                    }

                    bool isDirChange = lastWheelDelta != 0 &&
                                       ((lastWheelDelta < 0 && v0 > 0) ||
                                       (lastWheelDelta > 0 && v0 < 0));
                    if (isDirChange) {
                        //v0 = 0;
                    }

                    double v = v0 - (v0 * damp);

                    if (isScrollX) {
                        SetVelocityX(control, v);
                        MpAttachedPropertyHelpers.AddOrReplaceInstanceProperty<int>(control, "lastWheelDeltaX", (int)v0);
                        MpAttachedPropertyHelpers.AddOrReplaceInstanceProperty<int>(control, "lastWheelDeltaY", 0);
                    } else {
                        SetVelocityY(control, v);
                        MpAttachedPropertyHelpers.AddOrReplaceInstanceProperty<int>(control, "lastWheelDeltaY", (int)v0);
                        MpAttachedPropertyHelpers.AddOrReplaceInstanceProperty<int>(control, "lastWheelDeltaX", 0);
                    }
                }
            }

            #endregion

            #region ScrollViewer Events
            bool BindScrollViewerAndTracks(object sender, EffectiveViewportChangedEventArgs? e) {
                if (sender is Control control &&
                    GetScrollViewer(control) is ScrollViewer sv) {
                    MpConsole.WriteLine("Viewport changed");
                    
                    if (sv.TryGetVisualDescendants<Track>(out var tracks) && tracks.Count() > 0) {
                        sv.Bind(
                        ScrollViewer.WidthProperty,
                        new Binding() {
                            Source = control.DataContext,
                            Path = nameof(MpAvClipTrayViewModel.Instance.ClipTrayTotalWidth),
                            //Mode = BindingMode.TwoWay,
                            Priority = BindingPriority.Style
                        });
                        sv.Bind(
                            ScrollViewer.HeightProperty,
                            new Binding() {
                                Source = control.DataContext,
                                Path = nameof(MpAvClipTrayViewModel.Instance.ClipTrayScreenHeight),
                            //Mode = BindingMode.TwoWay,
                                Priority = BindingPriority.Style
                            });
                        sv.Bind(
                            ScrollViewer.OffsetProperty,
                            new Binding() {
                                Source = control.DataContext,
                                Path = nameof(MpAvClipTrayViewModel.Instance.ScrollOffset),
                                //Mode = BindingMode.TwoWay,
                                Priority = BindingPriority.Style
                            });
                        //MpDebuggerHelper.Break();
                        MpConsole.WriteLine("There's tracks");
                        //Debugger.Break();
                        foreach (var track in tracks) {
                            track.Tag = control;
                            track.IsThumbDragHandled = false;

                            //track.Bind(
                            //        Track.ValueProperty,
                            //        new Binding() {
                            //            Source = control.DataContext, 
                            //            Path = track.Orientation == Orientation.Horizontal ?
                            //                    nameof(MpAvClipTrayViewModel.Instance.ScrollOffsetX) :
                            //                    nameof(MpAvClipTrayViewModel.Instance.ScrollOffsetY),
                            //            //Mode = BindingMode.TwoWay,
                            //            //Priority = BindingPriority.Style
                            //        });

                            track.Bind(
                                    Track.MaximumProperty,
                                    new Binding() {
                                        Source = control.DataContext,
                                        Path = track.Orientation == Orientation.Horizontal ?
                                                nameof(MpAvClipTrayViewModel.Instance.MaxScrollOffsetX) :
                                                nameof(MpAvClipTrayViewModel.Instance.MaxScrollOffsetY),
                                        //Mode = BindingMode.OneWay
                                        Priority = BindingPriority.Style
                                    });

                            //track.Minimum = 0;


                            //track.AddHandler(
                            //    Control.PointerPressedEvent,
                            //    PreviewTrackPointerPressedHandler,
                            //    RoutingStrategies.Tunnel);

                            ////track.AddHandler(
                            ////    Control.PointerMovedEvent,
                            ////    PreviewTrackPointerMovedHandler,
                            ////    RoutingStrategies.Tunnel);

                            //track.AddHandler(
                            //    Control.PointerReleasedEvent,
                            //    PreviewTrackPointerReleasedHandler,
                            //    RoutingStrategies.Tunnel);

                            //if (track.Thumb is Thumb thumb) {
                            //    thumb.Tag = control;

                            //    thumb.AddHandler(
                            //        Control.PointerPressedEvent,
                            //        PreviewThumbPointerPressedHandler,
                            //        RoutingStrategies.Tunnel);

                            //    thumb.AddHandler(
                            //        Control.PointerMovedEvent,
                            //        PreviewThumbPointerMovedHandler,
                            //        RoutingStrategies.Tunnel);

                            //    thumb.AddHandler(
                            //        Control.PointerReleasedEvent,
                            //        PreviewThumbPointerReleasedHandler,
                            //        RoutingStrategies.Tunnel);
                            //} else {
                            //    Debugger.Break();
                            //}
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
                    track.Tag is Control control) {
                    SetIsThumbDragging(control, true);
                }
                e.Handled = false;
                //if (s is Track track &&
                //    track.Tag is Control control) {

                //    // NOTE if pointer pressed on thumb this event shouldn't fire

                //    //if (GetIsThumbDragging(control)) {
                //    //    var track_mp = e.GetCurrentPoint(track).Position;
                //    //    SetTrackValue(track, track_mp);
                //    //    SetIsThumbDragging(control, false);
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
                //        MpConsole.WriteLine($"ScrollOffset (should match track value): {GetScrollOffsetX(control)}");

                //        SetScrollOffsetX(control, new_track_value);

                //        track.GetVisualAncestor<ScrollViewer>().ScrollToHorizontalOffset(new_track_value);
                //    }

                //    //e.Handled = true;
                //    return;
                //}
                //e.Handled = false;
            }

            void PreviewTrackPointerMovedHandler(object? s, PointerEventArgs e) {
                //if (s is Track track &&
                //    track.Tag is Control control &&
                //    GetIsThumbDragging(control)) {

                //    //e.GetCurrentPoint(track).Pointer.Capture(track);
                //    var track_mp = e.GetCurrentPoint(track).Position;
                //    SetTrackValue(track, track_mp);
                //}
                //e.Handled = true;
            }

            void PreviewTrackPointerReleasedHandler(object? s, PointerReleasedEventArgs e) {
                if (s is Track track &&
                    track.Tag is Control control) {
                    SetIsThumbDragging(control, false);
                }
                e.Handled = false;
            }

            #endregion

            #region Thumb Events

            void PreviewThumbPointerPressedHandler(object? s, PointerPressedEventArgs e) {
                if (s is Thumb thumb &&
                    thumb.Tag is Control control) {
                    e.Handled = true;

                    MpConsole.WriteLine("Thumb Mouse Down");

                    SetIsThumbDragging(control, true);
                    e.GetCurrentPoint(MpAvMainWindow.Instance).Pointer.Capture(MpAvMainWindow.Instance);

                    return;
                }
                e.Handled = false;
            }

            void PreviewThumbPointerMovedHandler(object? s, PointerEventArgs e) {
                if (s is Thumb thumb &&
                    thumb.Tag is Control control &&
                    GetIsThumbDragging(control)) {
                    e.Handled = true;

                    if (!MpAvGlobalInputHook.Instance.GlobalIsLeftButtonPressed) {
                        //should be pressed, probalem w/ IsThumbDragging prop
                        Debugger.Break();
                        SetIsThumbDragging(control, false);
                        return;
                    }

                    var track = thumb.GetVisualAncestor<Track>();
                    if (track.Orientation == Orientation.Horizontal) {
                        double deltaX = MpAvGlobalInputHook.Instance.GlobalMouseLocation.X -
                                        MpAvGlobalInputHook.Instance.GlobalMouseLeftButtonDownLocation.X;

                        double deltaValue = track.ValueFromDistance(deltaX, 0);

                        double scrollOffsetX = GetScrollOffsetX(control);
                        double newScrollOffset = scrollOffsetX + deltaValue;

                        MpConsole.WriteLine($"DeltaX: {deltaX} DeltaValue: {deltaValue} ScrollOffsetX: {scrollOffsetX} New ScrollOffsetX: {newScrollOffset}");
                        SetScrollOffsetX(control, newScrollOffset);
                    }


                    return;
                }
                e.Handled = false;
            }

            void PreviewThumbPointerReleasedHandler(object? s, PointerReleasedEventArgs e) {
                if (s is Thumb thumb &&
                    thumb.Tag is Control control &&
                    GetIsThumbDragging(control)) {
                    MpConsole.WriteLine("Thumb Mouse Up");

                    SetIsThumbDragging(control, false);
                }
            }

            #endregion

            #endregion

            void HandleWorldTimerTick(object sender, EventArgs e) {
                if (sender is DispatcherTimer timer &&
                   timer.Tag is Control control &&
                   GetScrollViewer(control) is ScrollViewer sv) {
                    if (GetIsThumbDragging(control)) {
                        SetVelocityX(control, 0);
                        SetVelocityY(control, 0);
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
                    double extent_width = sv.Extent.Width;
                    double extent_height = sv.Extent.Height;
                    double viewport_width = sv.Viewport.Width;
                    double viewport_height = sv.Viewport.Height;

                    double scrollOffsetX = GetScrollOffsetX(control);
                    double maxOffsetX = extent_width - viewport_width;

                    double scrollOffsetY = GetScrollOffsetY(control);
                    double maxOffsetY = extent_height - viewport_height;

                    double vx = GetVelocityX(control);
                    double vy = GetVelocityY(control);

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


                    sv.ScrollToHorizontalOffset(scrollOffsetX);
                    sv.ScrollToVerticalOffset(scrollOffsetY);

                    vx = Math.Abs(vx) < 0.1d ? 0 : vx;
                    vy = Math.Abs(vy) < 0.1d ? 0 : vy;

                    scrollOffsetX += vx;
                    scrollOffsetY += vy;

                    vx *= GetFrictionX(control);
                    vy *= GetFrictionY(control);

                    if (vx == 0) {
                        MpAttachedPropertyHelpers.AddOrReplaceInstanceProperty<int>(control, "lastWheelDeltaX", 0);
                    }
                    if (vy == 0) {
                        MpAttachedPropertyHelpers.AddOrReplaceInstanceProperty<int>(control, "lastWheelDeltaY", 0);
                    }

                    SetScrollOffsetX(control, scrollOffsetX);
                    SetScrollOffsetY(control, scrollOffsetY);

                    SetVelocityX(control, vx);
                    SetVelocityY(control, vy);
                }
            }


            void SetTrackValue(Track track, Point track_mp) {
                if (track.Tag is Control control) {
                    if (track.Orientation == Orientation.Horizontal) {
                        SetVelocityX(control, 0);

                        double new_x = (track_mp.X / track.Bounds.Width) * track.Maximum;
                        new_x = Math.Min(Math.Max(track.Minimum, new_x), track.Maximum);
                        SetScrollOffsetX(control, new_x);
                    } else {
                        SetVelocityY(control, 0);

                        double new_y = (track_mp.Y / track.Bounds.Height) * track.Maximum;
                        new_y = Math.Min(Math.Max(track.Minimum, new_y), track.Maximum);
                        SetScrollOffsetY(control, new_y);
                    }
                }
            }
        }

        #endregion
    }

}
