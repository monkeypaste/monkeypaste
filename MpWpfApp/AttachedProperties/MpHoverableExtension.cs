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
        #region Brush Properties

        #region ForegroundBrush Properties

        #region HoverSelectedForegroundBrush DependencyProperty

        public static Brush GetHoverSelectedForegroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(HoverSelectedForegroundBrushProperty);
        }

        public static void SetHoverSelectedForegroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(HoverSelectedForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty HoverSelectedForegroundBrushProperty =
            DependencyProperty.Register(
                "HoverSelectedForegroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region SelectedForegroundBrush DependencyProperty

        public static Brush GetSelectedForegroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(SelectedForegroundBrushProperty);
        }

        public static void SetSelectedForegroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(SelectedForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty SelectedForegroundBrushProperty =
            DependencyProperty.Register(
                "SelectedForegroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region HoverForegroundBrush DependencyProperty

        public static Brush GetHoverForegroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(HoverForegroundBrushProperty);
        }

        public static void SetHoverForegroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(HoverForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty HoverForegroundBrushProperty =
            DependencyProperty.Register(
                "HoverForegroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region InactiveForegroundBrush DependencyProperty

        public static Brush GetInactiveForegroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(InactiveForegroundBrushProperty);
        }

        public static void SetInactiveForegroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(InactiveForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty InactiveForegroundBrushProperty =
            DependencyProperty.Register(
                "InactiveForegroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #endregion

        #region BackgroundBrush Properties

        #region HoverSelectedBackgroundBrush DependencyProperty

        public static Brush GetHoverSelectedBackgroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(HoverSelectedBackgroundBrushProperty);
        }

        public static void SetHoverSelectedBackgroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(HoverSelectedBackgroundBrushProperty, value);
        }

        public static readonly DependencyProperty HoverSelectedBackgroundBrushProperty =
            DependencyProperty.Register(
                "HoverSelectedBackgroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region SelectedBackgroundBrush DependencyProperty

        public static Brush GetSelectedBackgroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(SelectedBackgroundBrushProperty);
        }

        public static void SetSelectedBackgroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(SelectedBackgroundBrushProperty, value);
        }

        public static readonly DependencyProperty SelectedBackgroundBrushProperty =
            DependencyProperty.Register(
                "SelectedBackgroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region HoverBackgroundBrush DependencyProperty

        public static Brush GetHoverBackgroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(HoverBackgroundBrushProperty);
        }

        public static void SetHoverBackgroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(HoverBackgroundBrushProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundBrushProperty =
            DependencyProperty.Register(
                "HoverBackgroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region InactiveBackgroundBrush DependencyProperty

        public static Brush GetInactiveBackgroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(InactiveBackgroundBrushProperty);
        }

        public static void SetInactiveBackgroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(InactiveBackgroundBrushProperty, value);
        }

        public static readonly DependencyProperty InactiveBackgroundBrushProperty =
            DependencyProperty.Register(
                "InactiveBackgroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #endregion

        #region BorderBrush Properties

        #region HoverSelectedBorderBrush DependencyProperty

        public static Brush GetHoverSelectedBorderBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(HoverSelectedBorderBrushProperty);
        }

        public static void SetHoverSelectedBorderBrush(DependencyObject obj, Brush value) {
            obj.SetValue(HoverSelectedBorderBrushProperty, value);
        }

        public static readonly DependencyProperty HoverSelectedBorderBrushProperty =
            DependencyProperty.Register(
                "HoverSelectedBorderBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region SelectedBorderBrush DependencyProperty

        public static Brush GetSelectedBorderBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(SelectedBorderBrushProperty);
        }

        public static void SetSelectedBorderBrush(DependencyObject obj, Brush value) {
            obj.SetValue(SelectedBorderBrushProperty, value);
        }

        public static readonly DependencyProperty SelectedBorderBrushProperty =
            DependencyProperty.Register(
                "SelectedBorderBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region HoverBorderBrush DependencyProperty

        public static Brush GetHoverBorderBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(HoverBorderBrushProperty);
        }

        public static void SetHoverBorderBrush(DependencyObject obj, Brush value) {
            obj.SetValue(HoverBorderBrushProperty, value);
        }

        public static readonly DependencyProperty HoverBorderBrushProperty =
            DependencyProperty.Register(
                "HoverBorderBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region InactiveBorderBrush DependencyProperty

        public static Brush GetInactiveBorderBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(InactiveBorderBrushProperty);
        }

        public static void SetInactiveBorderBrush(DependencyObject obj, Brush value) {
            obj.SetValue(InactiveBorderBrushProperty, value);
        }

        public static readonly DependencyProperty InactiveBorderBrushProperty =
            DependencyProperty.Register(
                "InactiveBorderBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #endregion

        #endregion

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
                            fe.MouseEnter += Fe_MouseEnter;
                            fe.MouseLeave += Fe_MouseLeave;

                            if(fe.IsLoaded) {
                                UpdateBrushes(fe);
                            } else {
                                fe.Loaded += Fe_Loaded;
                            }
                            fe.Unloaded += Fe_Unloaded;
                        } else {
                            Fe_Unloaded(fe, null);
                        }
                    }
                }
            });

        private static void Fe_Unloaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;

            fe.MouseEnter -= Fe_MouseEnter;
            fe.MouseLeave -= Fe_MouseLeave;
            fe.Loaded -= Fe_Loaded;
            fe.Unloaded -= Fe_Unloaded;
        }

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            UpdateBrushes(sender as FrameworkElement);
        }

        #endregion

        private static void Fe_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            var dpo = sender as DependencyObject;
            SetIsHovering(dpo, true);

            MpCursorType? hoverCursor = GetHoverCursor(dpo);
            if (hoverCursor.HasValue) {
                MpCursorStack.PushCursor(dpo, hoverCursor.Value);
            }

            if (dpo is FrameworkElement fe) {
                UpdateBrushes(fe);
            }
        }

        private static void Fe_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            var dpo = sender as DependencyObject; 

            SetIsHovering(dpo, false);
            MpCursorType? hoverCursor = GetHoverCursor(dpo);
            if (hoverCursor.HasValue) {
                MpCursorStack.PopCursor(dpo);
            }
            if (dpo is Control c) {
                UpdateBrushes(c);
            }
        }


        private static void UpdateBrushes(FrameworkElement fe) {
            if(fe == null || !HasBrushes(fe)) {
                return;
            }
            bool isHovering = GetIsHovering(fe);
            bool isSelected = GetIsSelected(fe);

            if (fe is Border b) {
                b.Background = GetBrush(fe, isHovering, isSelected, "Background");
                b.BorderBrush = GetBrush(fe, isHovering, isSelected, "Border");
            } else if (fe is Control c) {
                c.Foreground = GetBrush(fe, isHovering, isSelected, "Foreground");
                c.Background = GetBrush(fe, isHovering, isSelected, "Background");
                c.BorderBrush = GetBrush(fe, isHovering, isSelected, "Border");
            } else if (fe is Shape s) {
                s.Stroke = GetBrush(s, isHovering, isSelected, "Border");
            } else {
                Debugger.Break();
            }
            
        }

        private static bool HasBrushes(FrameworkElement fe) {
            return HasBrush(fe, "Foreground") || HasBrush(fe, "Background") || HasBrush(fe,"Border");
        }

        private static bool HasBrush(FrameworkElement fe, string prefix) {
            var b0 = GetBrush(fe, false, false, prefix);
            var b1 = GetBrush(fe, false, true, prefix);
            var b2 = GetBrush(fe, true, false, prefix);
            var b3 = GetBrush(fe, true, true, prefix);
            return b0 != null ||
                   b1 != null ||
                   b2 != null ||
                   b3 != null;
        }
        private static Brush GetBrush(DependencyObject dpo, bool isHovering, bool isSelected, string prefix) {
            prefix += "Brush";
            Stack<string> propertyNameStack = new Stack<string>();

            if(!isHovering && !isSelected) {
                prefix = "Inactive" + prefix;
                propertyNameStack.Push(prefix);
            } else {
                if (isHovering) {
                    propertyNameStack.Push("Hover" + prefix);
                }
                if (isSelected) {
                    propertyNameStack.Push("Selected" + prefix);
                }
                if (isHovering && isSelected) {
                    propertyNameStack.Push("HoverSelected" + prefix);
                }
            }

            while (propertyNameStack.Count > 0) { 
                string propertyName = propertyNameStack.Pop();
                var descriptor = DependencyPropertyDescriptor.FromName(propertyName,
                                typeof(MpHoverableExtension),
                                typeof(Brush));
                if (descriptor == null) {
                    continue;
                }

                var result = descriptor.GetValue(dpo) as Brush;
                if(result != null) {
                    return result;
                }
            }

            return null;
        }

    }
}