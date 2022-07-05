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

        #region HoverCursor AvaloniaProperty
        public static MpCursorType? GetHoverCursor(AvaloniaObject obj) {
            return obj.GetValue(HoverCursorProperty);
        }

        public static void SetHoverCursor(AvaloniaObject obj, MpCursorType? value) {
            obj.SetValue(HoverCursorProperty, value);
        }

        public static readonly AttachedProperty<MpCursorType?> HoverCursorProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpCursorType?>(
                "HoverCursor",
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
                    control.PointerEnter += PointerEnterHandler;
                    control.PointerLeave += PointerLeaveHandler;

                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;
                    control.PointerEnter -= PointerEnterHandler;
                    control.PointerLeave -= PointerLeaveHandler;
                }
            }

            void PointerEnterHandler(object? s, PointerEventArgs e) {
                AvaloniaObject ao;
                object dc;
                if (s is StyledElement se) {
                    ao = se;
                    dc = se.DataContext; 
                } else {
                    return;
                }
                if(!GetCanHover(ao)) {
                    return;
                }
                SetIsHovering(ao, true);

                MpCursorType? hoverCursor = GetHoverCursor(ao);
                if (hoverCursor.HasValue) {
                    MpCursor.SetCursor(dc, hoverCursor.Value);
                }

                IImage hoverImageSource = GetHoverImageSource(ao);
                if (hoverImageSource != null && ao is Image i) {
                    if (GetDefaultImageSource(ao) == null) {
                        SetDefaultImageSource(ao, i.Source);
                    }

                    i.Source = hoverImageSource;
                } else if (GetHoverBrush(ao) != null) {
                    var hoverBrush = GetHoverBrush(ao);
                    Image img = null;
                    if (ao is Image) {
                        img = ao as Image;
                    } else if (ao is Control control) {
                        img = control.GetVisualDescendants().FirstOrDefault(x => x is Image) as Image;
                        //img = elm.GetVisualDescendent<Image>();
                    }
                    if (img == null) {
                        if (ao is Border b) {
                            if (b.Background is ImageBrush imgBrush) {
                               // imgBrush.Source = (imgBrush.Source as IImage).Tint(hoverBrush);
                            } else {
                                b.Background = hoverBrush;
                            }
                        }
                        return;
                    }
                    //img.Source = (img.Source as IImage).Tint(hoverBrush);
                }
            }
            void PointerLeaveHandler(object? s, PointerEventArgs e) {
                AvaloniaObject ao;
                object dc;
                if (s is StyledElement se) {
                    ao = se;
                    dc = se.DataContext;
                } else {
                    return;
                }
                SetIsHovering(ao, false);

                MpCursorType? hoverCursor = GetHoverCursor(ao);
                if (hoverCursor.HasValue) {
                    MpCursor.UnsetCursor(dc);
                }

                IImage defaultImageSource = GetDefaultImageSource(ao);
                if (GetHoverImageSource(ao) != null && defaultImageSource != null && ao is Image i) {
                    i.Source = defaultImageSource;
                } else if (GetDefaultBrush(ao) != null) {
                    var defaultBrush = GetDefaultBrush(ao);
                    Image img = null;
                    if (ao is Image) {
                        img = ao as Image;
                    } else if (ao is Control control) {
                        img = control.GetVisualDescendants().FirstOrDefault(x => x is Image) as Image;
                    }
                    if (img == null) {
                        if (ao is Border b) {
                            if (b.Background is ImageBrush imgBrush) {
                                if (GetIsSelected(ao) && GetSelectedBrush(ao) != null) {
                                    //imgBrush.ImageSource = (imgBrush.ImageSource as BitmapSource).Tint(GetSelectedBrush(ao));
                                } else {
                                    //imgBrush.ImageSource = (imgBrush.ImageSource as BitmapSource).Tint(defaultBrush);

                                }
                            } else {
                                b.Background = defaultBrush;
                            }
                        }
                        return;
                    }
                    if (GetIsSelected(ao) && GetSelectedBrush(ao) != null) {
                        //img.Source = (img.Source as BitmapSource).Tint(GetSelectedBrush(ao));
                    } else {
                        //img.Source = (img.Source as BitmapSource).Tint(defaultBrush);
                    }

                }
            }
        }

        #endregion
    }

}
