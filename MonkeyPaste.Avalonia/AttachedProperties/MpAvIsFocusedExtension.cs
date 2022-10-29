using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public static class MpAvIsFocusedExtension {
        static MpAvIsFocusedExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            //IsReadOnlyProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsReadOnlyChanged(x, y));
        }
        #region Properties

        #region IsFocused AvaloniaProperty
        public static bool GetIsFocused(AvaloniaObject obj) {
            return obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(AvaloniaObject obj, bool value) {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly AttachedProperty<bool> IsFocusedProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsFocused",
                false,
                false,
                BindingMode.TwoWay);

        #endregion

        #region IsReadOnly AvaloniaProperty
        public static bool GetIsReadOnly(AvaloniaObject obj) {
            return obj.GetValue(IsReadOnlyProperty);
        }

        public static void SetIsReadOnly(AvaloniaObject obj, bool value) {
            obj.SetValue(IsReadOnlyProperty, value);
        }

        public static readonly AttachedProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsReadOnly",
                false,
                false,
                BindingMode.TwoWay);

        #endregion

        #region SelectAllOnFocus AvaloniaProperty
        public static bool GetSelectAllOnFocus(AvaloniaObject obj) {
            return obj.GetValue(SelectAllOnFocusProperty);
        }

        public static void SetSelectAllOnFocus(AvaloniaObject obj, bool value) {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }

        public static readonly AttachedProperty<bool> SelectAllOnFocusProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "SelectAllOnFocus",
                false);

        #endregion

        #region SelectViewModelOnFocus AvaloniaProperty
        public static bool GetSelectViewModelOnFocus(AvaloniaObject obj) {
            return obj.GetValue(SelectViewModelOnFocusProperty);
        }

        public static void SetSelectViewModelOnFocus(AvaloniaObject obj, bool value) {
            obj.SetValue(SelectViewModelOnFocusProperty, value);
        }

        public static readonly AttachedProperty<bool> SelectViewModelOnFocusProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "SelectViewModelOnFocus",
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

            void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
                if (s is Control control) {
                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }
                    control.DetachedFromVisualTree += DetachedToVisualHandler;
                    control.GotFocus += Control_GotFocus;
                    control.LostFocus += Control_LostFocus;   
                    if(control is TextBox tb) {
                        tb.PropertyChanged += Tb_PropertyChanged;
                    }
                }
            }
            void DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;
                    control.GotFocus -= Control_GotFocus;
                    control.LostFocus -= Control_LostFocus;
                    if(control is TextBox tb) {
                        tb.PropertyChanged -= Tb_PropertyChanged;
                    }
                }
            }
        }

        private static void Tb_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            if(sender is TextBox tb) {
                if(e.Property.Name == "IsReadOnly") {
                    //HandleIsReadOnlyChanged(tb, e);
                } else if(e.Property.Name == "IsKeyboardFocusWithin") {
                    HandleIsKeyboardFocusWithinChanged(tb, e);
                }
            }
        }

        private static void Control_LostFocus(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if(sender is Control control) {
                LostFocus(control);
            }
        }

        private static void Control_GotFocus(object sender, global::Avalonia.Input.GotFocusEventArgs e) {
            if (sender is Control control) {
                GotFocus(control);
            }
        }

        private static void HandleIsKeyboardFocusWithinChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(element is Control control) {
                if(!GetIsEnabled(control)) {
                    // control must not be registered with this attached property
                    //Debugger.Break();
                    return;
                }
                if(e.NewValue is bool isKeyboardFocusWithin) {
                    if(isKeyboardFocusWithin) {
                        GotFocus(control);
                    } else {
                        LostFocus(control);
                    }
                }
            }
        }

        private static void HandleIsReadOnlyChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(element is TextBox tb) {
                if (!GetIsEnabled(tb)) {
                    // control must not be registered with this attached property
                    //Debugger.Break();
                    return;
                }
                bool isFocused = GetIsFocused(tb);
                if(isFocused != tb.IsFocused) {
                    // shouldn't happen
                    //Debugger.Break();
                }
                if(isFocused) {
                    if (tb.IsReadOnly) {
                        // when focused textbox becomes readonly need to make sure static IsAnyTextBoxFocused is toggled
                        LostFocus(tb);
                    } else {
                        GotFocus(tb);
                    }
                } else {
                    LostFocus(tb);
                }
            }
        }

        private static void GotFocus(Control control) {
            SetIsFocused(control, true);
            // NOTE avalonia keyboard focus is TBD
            //Keyboard.Focus(control);
            if(control.DataContext is MpISelectableViewModel svm &&
               GetSelectViewModelOnFocus(control)) {
                svm.IsSelected = true;
            }
            if(control is TextBox tb) {
                if(tb.IsReadOnly) {
                    MpAvMainWindowViewModel.Instance.IsAnyTextBoxFocused = false;
                } else {
                    MpAvMainWindowViewModel.Instance.IsAnyTextBoxFocused = true;
                    //
                }
                //if (GetSelectAllOnFocus(control)) {
                //    tb.CaretIndex = 0;
                //    SetIsReadOnly(control, false);
                //    KeyboardDevice.Instance.SetFocusedElement(control, NavigationMethod.Unspecified, KeyModifiers.Default);
                //    tb.SelectAll();
                //}

            }
        }

        private static void LostFocus(Control control) {
            if(control is TextBox tb) {
                MpAvMainWindowViewModel.Instance.IsAnyTextBoxFocused = false;
            }
            SetIsFocused(control, false);
        }

        #endregion

        #endregion
    }
}
