using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public static class MpAvIsHoveringExtension {
        static MpAvIsHoveringExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

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

        #region Private Methods

        private static void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {
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
            }
        }

        private static void Tc_PointerMoved(object sender, PointerEventArgs e) {
            if (sender is not Control c) {
                return;
            }
            Brush bb = null;

            if (sender is TemplatedControl tc &&
                tc.BorderBrush is Brush tc_bb) {
                bb = tc_bb;
            } else if (sender is Border b &&
                b.BorderBrush is Brush b_bb) {
                bb = b_bb;
            }
            if (bb == null) {
                return;
            }
            var mp = e.GetPosition(c).ToPortablePoint();
            var rel_mp = mp / c.Bounds.Size.ToPortableSize().ToPortablePoint();

            //if (bb is LinearGradientBrush lgb) {
            //    var rel_mp = mp / c.Bounds.Size.ToPortableSize().ToPortablePoint();
            //    lgb.StartPoint = new RelativePoint(rel_mp.ToAvPoint(), RelativeUnit.Relative);
            //}


            if (bb.Transform is RotateTransform rt) {
                var center = c.Bounds.ToPortableRect().Centroid();
                //rt.Angle = mp.AngleBetween(c.Bounds.BottomRight.ToPortablePoint());//.Wrap(0, 120);
                rt.Angle = center.AngleBetween(mp);//.Wrap(0, 120);
                //MpConsole.WriteLine($"new angle: {rt.Angle}");
            }

            //if (bb.Transform is TranslateTransform tt) {
            //    tt.X = mp.X;
            //    tt.Y = mp.Y;
            //}
            Dispatcher.UIThread.Post(c.InvalidateVisual);
        }
        #endregion
    }

}
