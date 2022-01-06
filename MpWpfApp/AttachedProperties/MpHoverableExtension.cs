using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpHoverableExtension : DependencyObject {
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

        #region ForegroundBrush DependencyProperty

        public static Brush GetForegroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(ForegroundBrushProperty);
        }

        public static void SetForegroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(ForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty ForegroundBrushProperty =
            DependencyProperty.Register(
                "ForegroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(Brushes.Black));

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

        #region BackgroundBrush DependencyProperty

        public static Brush GetBackgroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(BackgroundBrushProperty);
        }

        public static void SetBackgroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(BackgroundBrushProperty, value);
        }

        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register(
                "BackgroundBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(Brushes.White));

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
                new FrameworkPropertyMetadata(Brushes.Yellow));

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
                new FrameworkPropertyMetadata(Brushes.Red));

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
                new FrameworkPropertyMetadata(Brushes.Yellow));

        #endregion

        #region BorderBrush DependencyProperty

        public static Brush GetBorderBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(BorderBrushProperty);
        }

        public static void SetBorderBrush(DependencyObject obj, Brush value) {
            obj.SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(
                "BorderBrush", typeof(Brush),
                typeof(MpHoverableExtension),
                new FrameworkPropertyMetadata(Brushes.Black));

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
                        if(isEnabled) {
                            var fe = obj as FrameworkElement;
                            fe.MouseEnter += Fe_MouseEnter;
                            fe.MouseLeave += Fe_MouseLeave;
                        }
                    }
                }
            });

        #endregion

        private static void Fe_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            var dpo = sender as DependencyObject;
            SetIsHovering(dpo, false);
            MpCursorType? hoverCursor = GetHoverCursor(dpo);
            if (hoverCursor.HasValue) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
            }
            if (dpo is Control c) {
                //UpdateBrushes(c);
            }
        }

        private static void Fe_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            var dpo = sender as DependencyObject;
            SetIsHovering(dpo, true);
            MpCursorType? hoverCursor = GetHoverCursor(dpo);
            if(hoverCursor.HasValue) {
                MpCursorViewModel.Instance.CurrentCursor = hoverCursor.Value;
            }

            if (dpo is FrameworkElement fe) {
               // UpdateBrushes(fe);
            } 
        }

        private static void UpdateBrushes(FrameworkElement fe) {
            if(fe == null) {
                return;
            }
            bool isHovering = GetIsHovering(fe);
            bool isSelected = GetIsSelected(fe);

            if(fe is Border b) {
                b.Background = GetBrush(fe, isHovering, isSelected, "Background");
                b.BorderBrush = GetBrush(fe, isHovering, isSelected, "BorderBrush");
            }else if(fe is Control c) {
                c.Foreground = GetBrush(fe, isHovering, isSelected, "Foreground");
                c.Background = GetBrush(fe, isHovering, isSelected, "Background");
                c.BorderBrush = GetBrush(fe, isHovering, isSelected, "BorderBrush");
            } else {
                Debugger.Break();
            }
            
        }

        private static Brush GetBrush(DependencyObject dpo, bool isHovering, bool isSelected, string prefix) {
            Stack<string> propertyNameStack = new Stack<string>();
            propertyNameStack.Push(prefix + "Brush");

            if (isSelected) {
                propertyNameStack.Push("Selected" + propertyNameStack);
            }
            if (isHovering) {
                propertyNameStack.Push("Hover" + propertyNameStack);
            }
            DependencyPropertyDescriptor descriptor = null;
            while (descriptor == null) { 
                if(propertyNameStack.Count == 0) {
                    break;
                }
                string propertyName = propertyNameStack.Pop();
                descriptor = DependencyPropertyDescriptor.FromName(
                                propertyName,
                                typeof(MpHoverableExtension),
                                typeof(Brush));
            }
            if(descriptor == null) {
                return Brushes.Red;
            }

            return descriptor.GetValue(dpo) as Brush;
        }

    }
}