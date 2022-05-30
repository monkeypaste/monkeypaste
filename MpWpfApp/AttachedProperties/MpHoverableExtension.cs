using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpHoverableExtension : DependencyObject {
        #region IsHovering DependencyProperty

        public static bool GetIsHovering(DependencyObject obj) {
            return (bool)obj.GetValue(IsHoveringProperty);
        }

        public static void SetIsHovering(DependencyObject obj, bool value) {
            obj.SetValue(IsHoveringProperty, value);
        }

        public static readonly DependencyProperty IsHoveringProperty =
            DependencyProperty.Register(
                "IsHovering", typeof(bool), 
                typeof(MpHoverableExtension), 
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region IsSelected DependencyProperty

        public static bool GetIsSelected(DependencyObject obj) {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value) {
            obj.SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                "IsSelected", typeof(bool),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(false));

        #endregion

        #region HoverBrush DependencyProperty

        public static Brush GetHoverBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(HoverBrushProperty);
        }

        public static void SetHoverBrush(DependencyObject obj, Brush value) {
            obj.SetValue(HoverBrushProperty, value);
        }

        public static readonly DependencyProperty HoverBrushProperty =
            DependencyProperty.Register(
                "HoverBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region SelectedBrush DependencyProperty

        public static Brush GetSelectedBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(SelectedBrushProperty);
        }

        public static void SetSelectedBrush(DependencyObject obj, Brush value) {
            obj.SetValue(SelectedBrushProperty, value);
        }

        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register(
                "SelectedBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region DefaultBrush DependencyProperty

        public static Brush GetDefaultBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(DefaultBrushProperty);
        }

        public static void SetDefaultBrush(DependencyObject obj, Brush value) {
            obj.SetValue(DefaultBrushProperty, value);
        }

        public static readonly DependencyProperty DefaultBrushProperty =
            DependencyProperty.Register(
                "DefaultBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region HoverCursor DependencyProperty

        public static MpCursorType? GetHoverCursor(DependencyObject obj) {
            return (MpCursorType?)obj.GetValue(HoverCursorProperty);
        }

        public static void SetHoverCursor(DependencyObject obj, MpCursorType? value) {
            obj.SetValue(HoverCursorProperty, value);
        }

        public static readonly DependencyProperty HoverCursorProperty =
            DependencyProperty.Register(
                "HoverCursor", 
                typeof(MpCursorType?), 
                typeof(MpHoverableExtension), 
                new PropertyMetadata(null));

        #endregion

        #region HoverImageSource DependencyProperty

        public static ImageSource GetHoverImageSource(DependencyObject obj) {
            return (ImageSource)obj.GetValue(HoverImageSourceProperty);
        }

        public static void SetHoverImageSource(DependencyObject obj, ImageSource value) {
            obj.SetValue(HoverImageSourceProperty, value);
        }

        public static readonly DependencyProperty HoverImageSourceProperty =
            DependencyProperty.Register(
                "HoverImageSource",
                typeof(ImageSource),
                typeof(MpHoverableExtension),
                new PropertyMetadata(null));

        #endregion

        #region DefaultImageSource DependencyProperty

        public static ImageSource GetDefaultImageSource(DependencyObject obj) {
            return (ImageSource)obj.GetValue(DefaultImageSourceProperty);
        }

        public static void SetDefaultImageSource(DependencyObject obj, ImageSource value) {
            obj.SetValue(DefaultImageSourceProperty, value);
        }

        public static readonly DependencyProperty DefaultImageSourceProperty =
            DependencyProperty.Register(
                "DefaultImageSource",
                typeof(ImageSource),
                typeof(MpHoverableExtension),
                new PropertyMetadata(null));

        #endregion

        #region IsEnabled DependencyProperty

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }
        public static readonly DependencyProperty IsEnabledProperty =
          DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MpHoverableExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if(e.NewValue is bool isEnabled) {
                        if(isEnabled) {
                            if (obj is FrameworkElement fe) {
                                if (fe.IsLoaded) {
                                    Fe_Loaded(obj, null);
                                } else {
                                    fe.Loaded += Fe_Loaded;
                                }
                            } else if (obj is FrameworkContentElement fce) {
                                if (fce.IsLoaded) {
                                    Fe_Loaded(obj, null);
                                } else {
                                    fce.Loaded += Fe_Loaded;
                                }
                            }
                        } else {
                            Fe_Unloaded(obj, null);
                        }
                    }
                }
            });

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement fe) {
                fe.MouseEnter += Fe_MouseEnter;
                fe.MouseLeave += Fe_MouseLeave;
                fe.Unloaded += Fe_Unloaded;
            } else if (sender is FrameworkContentElement fce) {
                fce.MouseEnter += Fe_MouseEnter;
                fce.MouseLeave += Fe_MouseLeave;
                fce.Unloaded += Fe_Unloaded;
                return;
            } 
        }
        private static void Fe_Unloaded(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement fe) {
                fe.MouseEnter -= Fe_MouseEnter;
                fe.MouseLeave -= Fe_MouseLeave;
                fe.Unloaded -= Fe_Unloaded;
            } else if (sender is FrameworkContentElement fce) {
                fce.MouseEnter -= Fe_MouseEnter;
                fce.MouseLeave -= Fe_MouseLeave;
                fce.Unloaded -= Fe_Unloaded;
                return;
            }
        }

        #endregion

        private static void Fe_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            DependencyObject dpo = null;
            object dc;
            if (sender is FrameworkElement fe) {
                dpo = fe;
                dc = fe.DataContext;
            } else if (sender is FrameworkContentElement fce) {
                dpo = fce;
                dc = fce.DataContext;
            } else {
                return;
            }

            SetIsHovering(dpo, true);

            MpCursorType? hoverCursor = GetHoverCursor(dpo);
            if (hoverCursor.HasValue) {
                MpCursor.SetCursor(dc, hoverCursor.Value);
            }

            ImageSource hoverImageSource = GetHoverImageSource(dpo);
            if(hoverImageSource != null && dpo is Image i) {
                if(GetDefaultImageSource(dpo) == null) {
                    SetDefaultImageSource(dpo, i.Source);
                }

                i.Source = hoverImageSource;
            } else if(GetHoverBrush(dpo) != null) {
                var hoverBrush = GetHoverBrush(dpo);
                Image img = null;
                if(dpo is Image) {
                    img = dpo as Image;
                } else if(dpo is FrameworkElement elm) {
                    img = elm.GetVisualDescendent<Image>();
                }
                if(img == null) {
                    if(dpo is Border b) {
                        if(b.Background is ImageBrush imgBrush) {
                            imgBrush.ImageSource = (imgBrush.ImageSource as BitmapSource).Tint(hoverBrush);
                        }
                    }
                    return;
                }
                img.Source = (img.Source as BitmapSource).Tint(hoverBrush);
            }
        }

        private static void Fe_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            DependencyObject dpo = null;
            object dc;
            if (sender is FrameworkElement fe) {
                dpo = fe;
                dc = fe.DataContext;
            } else if (sender is FrameworkContentElement fce) {
                dpo = fce;
                dc = fce.DataContext;
            } else {
                return;
            }

            SetIsHovering(dpo, false);

            MpCursorType? hoverCursor = GetHoverCursor(dpo);
            if (hoverCursor.HasValue) {
                MpCursor.UnsetCursor(dc);
            }

            ImageSource defaultImageSource = GetDefaultImageSource(dpo);
            if (GetHoverImageSource(dpo) != null && defaultImageSource != null && dpo is Image i) {
                i.Source = defaultImageSource;
            } else if (GetDefaultBrush(dpo) != null) {
                var defaultBrush = GetDefaultBrush(dpo);
                Image img = null;
                if (dpo is Image) {
                    img = dpo as Image;
                } else if (dpo is FrameworkElement elm) {
                    img = elm.GetVisualDescendent<Image>();
                }
                if (img == null) {
                    if (dpo is Border b) {
                        if (b.Background is ImageBrush imgBrush) {
                            if (GetIsSelected(dpo) && GetSelectedBrush(dpo) != null) {
                                imgBrush.ImageSource = (imgBrush.ImageSource as BitmapSource).Tint(GetSelectedBrush(dpo));
                            } else {
                                imgBrush.ImageSource = (imgBrush.ImageSource as BitmapSource).Tint(defaultBrush);
                            }
                        }
                    }
                    return;
                }
                if(GetIsSelected(dpo) && GetSelectedBrush(dpo) != null) {
                    img.Source = (img.Source as BitmapSource).Tint(GetSelectedBrush(dpo));
                } else {
                    img.Source = (img.Source as BitmapSource).Tint(defaultBrush);
                }
                
            }
        }
    }
}