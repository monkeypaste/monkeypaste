using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                        var fe = obj as FrameworkElement;
                        if (isEnabled) {
                            if(fe.IsLoaded) {
                                Fe_Loaded(obj, null);
                            } else {
                                fe.Loaded += Fe_Loaded;
                            }
                        } else {
                            Fe_Unloaded(fe, null);
                        }
                    }
                }
            });

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            if(fe == null) {
                return;
            }
            fe.MouseEnter += Fe_MouseEnter;
            fe.MouseLeave += Fe_MouseLeave;
            fe.Unloaded += Fe_Unloaded;
        }
        private static void Fe_Unloaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;

            fe.MouseEnter -= Fe_MouseEnter;
            fe.MouseLeave -= Fe_MouseLeave;
            fe.Unloaded -= Fe_Unloaded;
        }

        #endregion

        private static void Fe_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            var fe = sender as FrameworkElement;
            SetIsHovering(fe, true);

            MpCursorType? hoverCursor = GetHoverCursor(fe);
            if (hoverCursor.HasValue) {
                MpCursor.SetCursor(fe.DataContext, hoverCursor.Value);
            }

            ImageSource hoverImageSource = GetHoverImageSource(fe);
            if(hoverImageSource != null && fe is Image i) {
                if(GetDefaultImageSource(fe) == null) {
                    SetDefaultImageSource(fe, i.Source);
                }

                i.Source = hoverImageSource;
            }
        }

        private static void Fe_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            var fe = sender as FrameworkElement; 

            SetIsHovering(fe, false);

            MpCursorType? hoverCursor = GetHoverCursor(fe);
            if (hoverCursor.HasValue) {
                MpCursor.UnsetCursor(fe.DataContext);
            }

            ImageSource defaultImageSource = GetDefaultImageSource(fe);
            if (GetHoverImageSource(fe) != null && defaultImageSource != null && fe is Image i) {
                i.Source = defaultImageSource;
            }
        }
    }
}