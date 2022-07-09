using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Windows.Input;
using System.Linq;
using System;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using System.Runtime.Intrinsics.Arm;
using Avalonia.Media.Immutable;
using System.Windows.Threading;
using System.Diagnostics;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public static class MpAvKinematicScrollExtension {
        #region Private Variables

        #endregion
        static MpAvKinematicScrollExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #region Timer AvaloniaProperty
        public static DispatcherTimer GetTimer(AvaloniaObject obj) {
            return obj.GetValue(TimerProperty);
        }

        public static void SetTimer(AvaloniaObject obj, DispatcherTimer value) {
            obj.SetValue(TimerProperty, value);
        }

        public static readonly AttachedProperty<DispatcherTimer> TimerProperty =
            AvaloniaProperty.RegisterAttached<object, Control, DispatcherTimer>(
                "Timer",
                null,
                false);

        #endregion

        #region LastWheelDelta AvaloniaProperty
        public static int GetLastWheelDelta(AvaloniaObject obj) {
            return obj.GetValue(LastWheelDeltaProperty);
        }

        public static void SetLastWheelDelta(AvaloniaObject obj, int value) {
            obj.SetValue(LastWheelDeltaProperty, value);
        }

        public static readonly AttachedProperty<int> LastWheelDeltaProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "LastWheelDelta",
                0,
                false);

        #endregion

        #region Velocity AvaloniaProperty
        public static double GetVelocity(AvaloniaObject obj) {
            return obj.GetValue(VelocityProperty);
        }

        public static void SetVelocity(AvaloniaObject obj, double value) {
            obj.SetValue(VelocityProperty, value);
        }

        public static readonly AttachedProperty<double> VelocityProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "Velocity",
                0.0d,
                false);

        #endregion

        #region ScrollOffset AvaloniaProperty
        public static double GetScrollOffset(AvaloniaObject obj) {
            return obj.GetValue(ScrollOffsetProperty);
        }

        public static void SetScrollOffset(AvaloniaObject obj, double value) {
            obj.SetValue(ScrollOffsetProperty, value);
        }

        public static readonly AttachedProperty<double> ScrollOffsetProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "ScrollOffset",
                0.0d,
                false,
                BindingMode.TwoWay);

        #endregion

        #region Friction AvaloniaProperty
        public static double GetFriction(AvaloniaObject obj) {
            return obj.GetValue(FrictionProperty);
        }

        public static void SetFriction(AvaloniaObject obj, double value) {
            obj.SetValue(FrictionProperty, value);
        }

        public static readonly AttachedProperty<double> FrictionProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "Friction",
                0.85d,
                false);

        #endregion

        #region WheelDampening AvaloniaProperty
        public static double GetWheelDampening(AvaloniaObject obj) {
            return obj.GetValue(WheelDampeningProperty);
        }

        public static void SetWheelDampening(AvaloniaObject obj, double value) {
            obj.SetValue(WheelDampeningProperty, value);
        }

        public static readonly AttachedProperty<double> WheelDampeningProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "WheelDampening",
                0.08d,
                false);

        #endregion

        #region MaxSpringDist AvaloniaProperty
        public static double GetMaxSpringDist(AvaloniaObject obj) {
            return obj.GetValue(MaxSpringDistProperty);
        }

        public static void SetMaxSpringDist(AvaloniaObject obj, double value) {
            obj.SetValue(MaxSpringDistProperty, value);
        }

        public static readonly AttachedProperty<double> MaxSpringDistProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "MaxSpringDist",
                400.0d,
                false);

        #endregion

        #region TranslateOffsetX AvaloniaProperty
        public static double GetTranslateOffsetX(AvaloniaObject obj) {
            return obj.GetValue(TranslateOffsetXProperty);
        }

        public static void SetTranslateOffsetX(AvaloniaObject obj, double value) {
            obj.SetValue(TranslateOffsetXProperty, value);
        }

        public static readonly AttachedProperty<double> TranslateOffsetXProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "TranslateOffsetX",
                0d,
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
            if(e.NewValue is bool isEnabledVal && isEnabledVal) {
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

            void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.DetachedFromVisualTree += DetachedToVisualHandler;
                    control.PointerWheelChanged += PointerMouseWheelHandler;

                    control.AddHandler(
                        Control.PointerPressedEvent,
                        PreviewPointerPressedHandler,
                        RoutingStrategies.Tunnel);

                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }

                    var timer = new DispatcherTimer(DispatcherPriority.Normal);
                    timer.Tag = control;
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                    timer.Tick += HandleWorldTimerTick;

                    SetTimer(control, timer);

                    if(GetScrollViewer(control) == null) {
                        var sv = control.GetVisualParent<ScrollViewer>();
                        SetScrollViewer(control, sv);
                    }

                    timer.Start();
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;
                    control.PointerWheelChanged -= PointerMouseWheelHandler;
                    control.RemoveHandler(Control.PointerPressedEvent, PreviewPointerPressedHandler);
                }
            }

            void PreviewPointerPressedHandler(object? s, PointerPressedEventArgs e) {
                if(s is Control control && Math.Abs(GetVelocity(control)) > 0) {
                    SetVelocity(control, 0);
                    //MpClipTrayViewModel.Instance.HasScrollVelocity = false;
                }
            }

            void PointerMouseWheelHandler(object? s, global::Avalonia.Input.PointerWheelEventArgs e) {
                if (s is Control control) {

                    if (//MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
                        //MpClipTrayViewModel.Instance.IsAnyTileExpanded ||
                        //MpClipTrayViewModel.Instance.IsScrollingIntoView ||
                        MpAvMainWindowViewModel.Instance.IsMainWindowOpening
                        //!MpAvClipTrayViewModel.Instance.CanScroll
                        ) {
                        //e.Handled = true;
                        return;
                    }
                    double v = e.Delta.Y > 0 ? 120 : -120; 
                    var sv = GetScrollViewer(control);
                    double maxOffset = sv.Extent.Width - sv.Viewport.Width;
                    double scrollOffset = GetScrollOffset(control);

                    if ((v < 0 && scrollOffset >= maxOffset) ||
                        (v > 0 && scrollOffset <= 0)) {
                        SetVelocity(control, 0);
                        return;
                    }

                    int lastWheelDelta = GetLastWheelDelta(control);

                    if ((lastWheelDelta < 0 && v > 0) ||
                       (lastWheelDelta > 0 && v < 0)) {
                        //when changing wheel direction clear velocity for this wheel event
                        SetVelocity(control, 0);
                    }

                    SetLastWheelDelta(control, (int)v);

                    double new_v = GetVelocity(control);
                    double damp = GetWheelDampening(control);
                    new_v -= v * damp;
                    SetVelocity(control, new_v);

                    
                } else {
                    return;
                }
                
            }
            double spring_v = 0;

            void HandleWorldTimerTick(object sender, EventArgs e) {
                if(sender is DispatcherTimer timer && 
                   timer.Tag is Control control &&
                   GetScrollViewer(control) is ScrollViewer sv) {

                    //if (MpClipTrayViewModel.Instance.IsRequery ||
                    //   !MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                    //    return;
                    //}
                    double maxOffset = sv.Extent.Width - sv.Viewport.Width;
                    double v = GetVelocity(control);                    
                    double scrollOffset = GetScrollOffset(control);

                    if (scrollOffset < 0) {
                        scrollOffset = 0;
                        v = 0;
                    } else if (scrollOffset > maxOffset) {
                        scrollOffset = maxOffset;
                        v = 0;
                    }

                    //if (MpClipTrayViewModel.Instance.IsThumbDragging) {
                    //    return;
                    //}

                    if (Math.Abs(v) < 0.1d) {
                        v = 0;
                    } else {
                        scrollOffset += v;
                        double friction = GetFriction(control);
                        v *= friction;
                    }

                    scrollOffset = Math.Max(0, Math.Min(maxOffset, scrollOffset));
                    SetScrollOffset(control, scrollOffset);
                    SetVelocity(control, v);

                    sv.ScrollToHorizontalOffset(scrollOffset);
                }
            }
        }

        #endregion
    }

}
