using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Windows.Threading;

namespace MonkeyPaste.Avalonia {
    public static class MpAvIsHoveringExtension {
        #region Private Variables
        private static DispatcherTimer _timer;
        private static MpPoint _last_mp;
        #endregion

        #region Statics
        static MpAvIsHoveringExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            IsBorderTimerEnabledProperty.Changed.AddClassHandler<Control>((x, y) => OnIsBorderTimerEnabledChanged(x, y));
        }

        #endregion

        #region Properties

        #region IsBorderFollowEnabled AvaloniaProperty
        public static bool GetIsBorderFollowEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsBorderFollowEnabledProperty);
        }

        public static void SetIsBorderFollowEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsBorderFollowEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsBorderFollowEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsBorderFollowEnabled",
                false,
                false,
                BindingMode.TwoWay);
        #endregion

        #region IsBorderTimerEnabled AvaloniaProperty
        public static bool GetIsBorderTimerEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsBorderTimerEnabledProperty);
        }

        public static void SetIsBorderTimerEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsBorderTimerEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsBorderTimerEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsBorderTimerEnabled",
                false);

        private static void OnIsBorderTimerEnabledChanged(Control c, AvaloniaPropertyChangedEventArgs e) {
            if (GetIsBorderTimerEnabled(c)) {
                StartFollowTimer(c);
            } else {
                StopFollowTimer(c);
            }
        }
        #endregion

        #region TimerDeltaAngle AvaloniaProperty
        public static double GetTimerDeltaAngle(AvaloniaObject obj) {
            return obj.GetValue(TimerDeltaAngleProperty);
        }

        public static void SetTimerDeltaAngle(AvaloniaObject obj, double value) {
            obj.SetValue(TimerDeltaAngleProperty, value);
        }

        public static readonly AttachedProperty<double> TimerDeltaAngleProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "TimerDeltaAngle",
                5);

        #endregion

        #region IsHovering AvaloniaProperty
        public static bool GetIsHovering(AvaloniaObject obj) {
            return obj.GetValue(IsHoveringProperty);
        }

        public static void SetIsHovering(AvaloniaObject obj, bool value) {
            obj.SetValue(IsHoveringProperty, value);
        }

        public static readonly AttachedProperty<bool> IsHoveringProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsHovering",
                false,
                false,
                BindingMode.TwoWay);

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

        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
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


        }

        #endregion

        #endregion

        #region Private Methods

        private static void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {

                if (GetIsBorderTimerEnabled(control)) {
                    StartFollowTimer(control);
                }
                control.DetachedFromVisualTree += DetachedToVisualHandler;

                control.PointerEntered += PointerEnterHandler;
                control.PointerExited += PointerLeaveHandler;
                if (e == null) {
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                }
            }
        }

        private static void DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {
                control.AttachedToVisualTree -= AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DetachedToVisualHandler;
                control.PointerEntered -= PointerEnterHandler;
                control.PointerExited -= PointerLeaveHandler;

                if (GetIsBorderTimerEnabled(control)) {
                    StopFollowTimer(control);
                }
            }
        }

        private static void PointerEnterHandler(object s, PointerEventArgs e) {
            if (s is not Control control) {
                return;
            }
            SetIsHovering(control, true);
            if (GetIsBorderFollowEnabled(control)) {
                control.PointerMoved += Tc_PointerMoved;
            }
        }


        private static void PointerLeaveHandler(object s, PointerEventArgs e) {
            if (s is not Control control) {
                return;
            }
            SetIsHovering(control, false);
            if (GetIsBorderFollowEnabled(control)) {
                control.PointerMoved -= Tc_PointerMoved;
                _last_mp = null;
            }
        }

        private static void Tc_PointerMoved(object sender, PointerEventArgs e) {
            if (sender is not Control c ||
                !GetIsBorderFollowEnabled(c)) {
                return;
            }

            var mp = e.GetPosition(c).ToPortablePoint();
            _last_mp = _last_mp == null ? mp : _last_mp;

            var center = c.Bounds.ToPortableRect().Centroid();
            double last_angle = center.AngleBetween(_last_mp);
            double angle = center.AngleBetween(mp);
            RotateBorderBrush(c, angle - last_angle, angle);

            _last_mp = mp;
        }
        #endregion

        private static void StartFollowTimer(Control c) {
            void _timer_Tick(object sender, EventArgs e) {
                RotateBorderBrush(_timer.Tag as Control, GetTimerDeltaAngle(c));
            }
            if (_timer == null) {
                _timer = new DispatcherTimer() {
                    Interval = TimeSpan.FromMilliseconds(20),
                    IsEnabled = true
                };
                _timer.Tick += _timer_Tick;
            }
            _timer.Tag = c;
            _timer.Start();
        }


        private static void StopFollowTimer(Control c) {
            if (_timer == null) {
                return;
            }
            _timer.Stop();
        }

        private static void RotateBorderBrush(Control c, double angle_delta, double? force_angle = null) {
            Brush bb;

            if (c is TemplatedControl tc &&
                tc.BorderBrush is Brush tc_bb) {
                bb = tc_bb;
            } else if (c is Border b &&
                b.BorderBrush is Brush b_bb) {
                bb = b_bb;
            } else {
                MpDebug.Break($"Unhandled border follow control type '{c.GetType()}'");
                return;
            }

            if (bb.Transform is not RotateTransform rt) {
                MpDebug.Break($"Border follow error, bb must have rotate transform");
                return;
            }

            if (force_angle.HasValue) {
                rt.Angle = force_angle.Value;
            } else {
                rt.Angle += angle_delta;
            }
            rt.Angle = rt.Angle.Wrap(0, 360);
            //MpConsole.WriteLine($"Brush angle: {rt.Angle}");

            c.Redraw();
        }
    }

}
