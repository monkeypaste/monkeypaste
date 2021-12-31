using System.Windows;
using System.Windows.Controls;

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
        }

        private static void Fe_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            var dpo = sender as DependencyObject;
            SetIsHovering(dpo, true);
            MpCursorType? hoverCursor = GetHoverCursor(dpo);
            if(hoverCursor.HasValue) {
                MpCursorViewModel.Instance.CurrentCursor = hoverCursor.Value;
            }
        }
    }
}