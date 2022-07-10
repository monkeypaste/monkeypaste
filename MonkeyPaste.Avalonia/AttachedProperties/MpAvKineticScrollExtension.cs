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
using System.Diagnostics;
using MonkeyPaste.Common;
using MonoMac.ObjCRuntime;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Controls.Primitives;

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

                    control.AddHandler(
                        Control.PointerWheelChangedEvent,
                        PointerMouseWheelHandler,
                        RoutingStrategies.Tunnel);

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

                    if(GetScrollViewer(control) == null) {
                        var sv = control.GetVisualParent<ScrollViewer>();
                        SetScrollViewer(control, sv);
                        var thumbs = sv.GetVisualDescendants().Where(x => x is Thumb);
                        if (thumbs.Count() == 0) {
                            Debugger.Break();
                        }
                    }

                    timer.Start();
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;

                    control.RemoveHandler(
                        Control.PointerWheelChangedEvent,
                        PointerMouseWheelHandler);

                    control.RemoveHandler(
                        Control.PointerPressedEvent, 
                        PreviewPointerPressedHandler);
                }
            }

            void PreviewPointerPressedHandler(object? s, PointerPressedEventArgs e) {
                // when user clicks always halt any animated scrolling
                if(s is Control control) {
                    if(e.GetCurrentPoint(control).Properties.IsLeftButtonPressed) {
                        SetVelocityX(control, 0);
                        SetVelocityY(control, 0);
                    }
                }
            }

            void PointerMouseWheelHandler(object? s, global::Avalonia.Input.PointerWheelEventArgs e) {
                if (s is Control control) {
                    e.Handled = true;

                    var sv = GetScrollViewer(control);
                    if(sv == null) {
                        Debugger.Break();
                        return;
                    }

                    bool canScrollX = sv.Extent.Width > sv.Viewport.Width && GetCanScrollX(control);
                    bool canScrollY = sv.Extent.Height > sv.Viewport.Height && GetCanScrollY(control);
                    bool canScrollBoth = canScrollX && canScrollY;

                    bool isScrollX = (canScrollBoth && e.KeyModifiers.HasFlag(KeyModifiers.Shift)) ||
                                     (canScrollX && !canScrollY);

                    bool isScrollY = !isScrollX && canScrollY;

                    if(!isScrollX && !isScrollY) {
                        return;
                    }

                    double maxOffset = 0;
                    double scrollOffset = 0;
                    double v0 = 0;
                    double damp = 0;
                    int lastWheelDelta = 0;
                    
                    if(isScrollX) {                        
                        damp = GetWheelDampeningX(control);
                        v0 = e.Delta.Y > 0 ? 120 : -120;
                        if (v0 == 0 && e.Delta.X != 0) {
                            v0 = e.Delta.X > 0 ? 120 : -120;
                        }
                        maxOffset = sv.Extent.Width - sv.Viewport.Width;
                        scrollOffset = GetScrollOffsetX(control);
                        lastWheelDelta = MpAttachedPropertyHelpers.GetInstanceProperty<int>(control, "lastWheelDeltaX");
                    } else {
                        damp = GetWheelDampeningY(control);
                        v0 = e.Delta.Y > 0 ? -120 : 120;
                        maxOffset = sv.Extent.Height - sv.Viewport.Height;
                        scrollOffset = GetScrollOffsetY(control);
                        lastWheelDelta = MpAttachedPropertyHelpers.GetInstanceProperty<int>(control, "lastWheelDeltaY");
                    }

                    bool isDirChange = lastWheelDelta != 0 &&
                                       ((lastWheelDelta < 0 && v0 > 0) ||
                                       (lastWheelDelta > 0 && v0 < 0));
                    if (isDirChange) {
                        v0 = 0;
                    }

                    double v = v0 - (v0 * damp);

                    if(isScrollX) {
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

            void HandleWorldTimerTick(object sender, EventArgs e) {
                if(sender is DispatcherTimer timer && 
                   timer.Tag is Control control &&
                   GetScrollViewer(control) is ScrollViewer sv) {
                    if(control.IsUnsetValue()) {
                        Debugger.Break();
                    }

                    double scrollOffsetX = GetScrollOffsetX(control);
                    double maxOffsetX = sv.Extent.Width - sv.Viewport.Width;

                    double scrollOffsetY = GetScrollOffsetY(control);
                    double maxOffsetY = sv.Extent.Height - sv.Viewport.Height;

                    double vx = GetVelocityX(control);
                    double vy = GetVelocityY(control);

                    //MpConsole.WriteLine("vx: " + vx);
                    //MpConsole.WriteLine("vy: " + vy);

                    if(scrollOffsetX < 0 || scrollOffsetX > maxOffsetX) {
                        scrollOffsetX = Math.Min(maxOffsetX, Math.Max(0, scrollOffsetX));
                        vx = 0;
                    }
                    if(scrollOffsetY < 0 || scrollOffsetY > maxOffsetY) {
                        scrollOffsetY = Math.Min(maxOffsetY, Math.Max(0, scrollOffsetY));
                        vy = 0;
                    }

                    //if (MpClipTrayViewModel.Instance.IsThumbDragging) {
                    //    return;
                    //}
                    sv.ScrollToHorizontalOffset(scrollOffsetX);
                    sv.ScrollToVerticalOffset(scrollOffsetY);

                    vx = Math.Abs(vx) < 0.1d ? 0 : vx;
                    vy = Math.Abs(vy) < 0.1d ? 0 : vy;

                    scrollOffsetX += vx;
                    scrollOffsetY += vy;

                    vx *= GetFrictionX(control);
                    vy *= GetFrictionY(control);

                    if(vx == 0) {
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
        }

        #endregion
    }

}
