using System;
using System.Windows;
using System.Windows.Threading;

namespace MpWpfApp {
    public static class MpFocusExtension {
        public static bool GetIsFocused(DependencyObject obj) {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value) {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(MpFocusExtension),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e) {
            var uie = (UIElement)d;
            if ((bool)e.NewValue && uie.Dispatcher != null) {
                uie.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal, 
                    (Action)(() => {
                        uie.Focus();
                        System.Windows.Input.Keyboard.Focus(uie);
                    })); // invoke behaves nicer, if e.g. you have some additional handler attached to 'GotFocus' of UIE.   uie.SetValue(IsFocusedProperty, false); // reset bound value if possible, to allow setting again ...
             }
        }
    }
}
