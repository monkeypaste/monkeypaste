using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace MonkeyPaste.Avalonia {
    public static class MpAvIsHoveringExtension {
        static MpAvIsHoveringExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #region CanHover AvaloniaProperty
        public static bool GetCanHover(AvaloniaObject obj) {
            return obj.GetValue(CanHoverProperty);
        }

        public static void SetCanHover(AvaloniaObject obj, bool value) {
            obj.SetValue(CanHoverProperty, value);
        }

        public static readonly AttachedProperty<bool> CanHoverProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "CanHover",
                true,
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

        #region IsSelected AvaloniaProperty
        public static bool GetIsSelected(AvaloniaObject obj) {
            return obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(AvaloniaObject obj, bool value) {
            obj.SetValue(IsSelectedProperty, value);
        }

        public static readonly AttachedProperty<bool> IsSelectedProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsSelected",
                false,
                false);

        #endregion

        #region HoverBrush AvaloniaProperty
        public static IBrush GetHoverBrush(AvaloniaObject obj) {
            return obj.GetValue(HoverBrushProperty);
        }

        public static void SetHoverBrush(AvaloniaObject obj, IBrush value) {
            obj.SetValue(HoverBrushProperty, value);
        }

        public static readonly AttachedProperty<IBrush> HoverBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "HoverBrush",
                null,
                false);

        #endregion

        #region SelectedBrush AvaloniaProperty
        public static IBrush GetSelectedBrush(AvaloniaObject obj) {
            return obj.GetValue(SelectedBrushProperty);
        }

        public static void SetSelectedBrush(AvaloniaObject obj, IBrush value) {
            obj.SetValue(SelectedBrushProperty, value);
        }

        public static readonly AttachedProperty<IBrush> SelectedBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "SelectedBrush",
                null,
                false);

        #endregion

        #region DefaultBrush AvaloniaProperty
        public static IBrush GetDefaultBrush(AvaloniaObject obj) {
            return obj.GetValue(DefaultBrushProperty);
        }

        public static void SetDefaultBrush(AvaloniaObject obj, IBrush value) {
            obj.SetValue(DefaultBrushProperty, value);
        }

        public static readonly AttachedProperty<IBrush> DefaultBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "DefaultBrush",
                null,
                false);

        #endregion

        #region HoverImageSource AvaloniaProperty
        public static IImage GetHoverImageSource(AvaloniaObject obj) {
            return obj.GetValue(HoverImageSourceProperty);
        }

        public static void SetHoverImageSource(AvaloniaObject obj, IImage value) {
            obj.SetValue(HoverImageSourceProperty, value);
        }

        public static readonly AttachedProperty<IImage> HoverImageSourceProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IImage>(
                "HoverImageSource",
                null,
                false);

        #endregion

        #region DefaultImageSource AvaloniaProperty
        public static IImage GetDefaultImageSource(AvaloniaObject obj) {
            return obj.GetValue(DefaultImageSourceProperty);
        }

        public static void SetDefaultImageSource(AvaloniaObject obj, IImage value) {
            obj.SetValue(DefaultImageSourceProperty, value);
        }

        public static readonly AttachedProperty<IImage> DefaultImageSourceProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IImage>(
                "DefaultImageSource",
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
            if (s is Control control) {
                if (!GetCanHover(control)) {
                    return;
                }
                SetIsHovering(control, true);

                if (GetHoverBrush(control) is IBrush hoverBrush && control is Border border) {
                    if (GetDefaultBrush(control) == null) {
                        SetDefaultBrush(control, border.Background);
                    }
                    border.Background = hoverBrush;
                }
            }
        }
        private static void PointerLeaveHandler(object s, PointerEventArgs e) {
            if (s is Control control) {
                SetIsHovering(control, false);
                if (GetDefaultBrush(control) is IBrush defaultBrush && control is Border border) {
                    border.Background = defaultBrush;
                }
            }
        }
        #endregion
    }

}
