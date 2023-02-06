using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;
using System;

namespace MonkeyPaste.Avalonia {
    public static class MpAvIsFocusedExtension {
        static MpAvIsFocusedExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            IsFocusedProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsFocusedBindingChanged(x, y));
            IsReadOnlyProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsReadOnlyBindingChanged(x, y));
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

        private static void HandleIsFocusedBindingChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isFocusedVal && 
                element is Control control && 
                control.GetFocusableDescendant() is IInputElement ie) {
                if(ie.IsFocused == isFocusedVal) {
                    MpConsole.WriteLine($"IsFocused IGNORED. Was already '{isFocusedVal}' on '{ie}' from binding on '{control}'");
                    return;
                }
                if(isFocusedVal) {
                    ie.Focus();
                    MpConsole.WriteLine($"Focus {(ie.IsFocused ? "SUCCEEDED":"FAILED")}  on '{ie}' from binding on '{control}'");
                } else {
                    bool result = control.KillFocus();
                    MpConsole.WriteLine($"Kill Focus {(result ? "SUCCEEDED" : "FAILED")}  on '{ie}' from binding on '{control}'");
                }
            } 
        }

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

        private static void HandleIsReadOnlyBindingChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is Control control &&
                GetIsEnabled(control) &&
                e.NewValue is bool isReadOnlyVal &&
                control.GetVisualDescendant<TextBox>() is TextBox tb) {
                if(isReadOnlyVal == tb.IsReadOnly) {
                    MpConsole.WriteLine($"IsReadOnly IGNORED. Was already '{isReadOnlyVal}' on '{tb}' from binding on '{control}'");
                    return;
                }
                
                if(isReadOnlyVal) {
                    tb.IsReadOnly = true;
                    bool result = tb.IsReadOnly == true;
                    MpConsole.WriteLine($"ReadOnly {(isReadOnlyVal ? "ENABLED" : "DISABLED")} {(result ? "SUCCEEDED" : "FAILED")} on '{tb}' from binding on '{control}'");
                    // kill focus for mw show logic
                    SetIsFocused(control, false);
                    return;
                }
                SetIsFocused(control, true);
                tb.IsReadOnly = false;
            }
        }
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
            if (e.NewValue is bool isEnabledVal &&
                element is Control control) {
                if(isEnabledVal) {
                    control.Initialized += Control_Initialized;
                    control.DetachedFromVisualTree += Control_DetachedToVisualHandler;
                    if (control.IsInitialized) {
                        Control_Initialized(control, null);
                    }
                } else {
                    Control_DetachedToVisualHandler(element, null);
                }
            }            
        }

        private static void Control_Initialized(object sender, EventArgs e) {
            if (sender is Control control) {
                if (control.GetFocusableDescendant() is Control focusableControl) {
                    focusableControl.Tag = control;
                    focusableControl.PropertyChanged += FocusableControl_PropertyChanged;
                    focusableControl.GotFocus += FocusableControl_GotFocus;
                    focusableControl.LostFocus += FocusableControl_LostFocus;
                } else {
                    // what's wrong?
                    Debugger.Break();
                }
            }
        }
        private static void Control_DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {
                control.Initialized -= Control_Initialized;
                control.DetachedFromVisualTree -= Control_DetachedToVisualHandler;
                if(control.GetFocusableDescendant() is Control focuableControl) {
                    focuableControl.GotFocus -= FocusableControl_GotFocus;
                    focuableControl.LostFocus -= FocusableControl_LostFocus;
                    focuableControl.PropertyChanged -= FocusableControl_PropertyChanged;
                }
                
            }
        }
        private static void FocusableControl_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            if(sender is Control focusableControl) {
                if(e.Property.Name == "IsKeyboardFocusWithin") {
                    HandleIsKeyboardFocusWithinChanged(focusableControl, e);
                }
            }
        }

        private static void FocusableControl_LostFocus(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if(sender is Control focusableControl &&
                focusableControl.Tag is Control attachedControl) {
                SetIsFocused(attachedControl, false);
            }
        }

        private static void FocusableControl_GotFocus(object sender, global::Avalonia.Input.GotFocusEventArgs e) {
            if (sender is Control focusableControl &&
                focusableControl.Tag is Control attachedControl) {
                SetIsFocused(attachedControl, true);

                if (GetSelectAllOnFocus(attachedControl)) {
                    if(focusableControl is TextBox tb) {
                        tb.SelectAll();
                    } else if(focusableControl is AutoCompleteBox acb) {
                        // dunno how
                        Debugger.Break();
                    }
                }
                if(GetSelectViewModelOnFocus(attachedControl) &&
                    attachedControl.DataContext is MpISelectableViewModel svm) {
                    svm.IsSelected = true;
                }
            }
        }

        private static void HandleIsKeyboardFocusWithinChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(e.NewValue is bool isKeyboardFocused &&
                element is Control focusableControl &&
                focusableControl.Tag is Control attachedControl) {
                SetIsFocused(attachedControl, isKeyboardFocused);
            }
        }

        #endregion


        #endregion
    }
}
