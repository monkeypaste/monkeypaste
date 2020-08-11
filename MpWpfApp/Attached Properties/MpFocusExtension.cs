﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            if((bool)e.NewValue) {
                uie.Focus(); // Don't care about false values.
                System.Windows.Input.Keyboard.Focus(uie);
            }
        }
    }
}
