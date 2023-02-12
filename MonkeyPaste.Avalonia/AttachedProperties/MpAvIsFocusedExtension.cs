using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;
using System;
using Avalonia.Threading;
using System.Threading.Tasks;
using Avalonia.VisualTree;

namespace MonkeyPaste.Avalonia {
    public static class MpAvIsFocusedExtension {
        static MpAvIsFocusedExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            IsFocusedProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsFocusedBindingChanged(x, y));
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
                if (ie.IsFocused == isFocusedVal ||
                    ie.IsKeyboardFocusWithin == isFocusedVal) {
                    MpConsole.WriteLine($"IsFocused IGNORED. Was already '{isFocusedVal}' on '{ie}' from binding on '{control}'");
                    return;
                }
                Dispatcher.UIThread.Post(async () => {
                    if (isFocusedVal) {
                        if(control is AutoCompleteBox acb) {
                            var tb = acb.FindDescendantOfType<TextBox>();
                            if(tb != null) {
                                control = tb;
                            }
                        }
                        bool success = await control.TrySetFocusAsync();
                        if(success != ie.IsKeyboardFocusWithin) {
                            // huh? result mismatch
                            //Debugger.Break();
                        }
                        MpConsole.WriteLine($"Focus {(ie.IsKeyboardFocusWithin ? "SUCCEEDED" : "FAILED")}  on '{ie}' from binding on '{control}'");
                    } else {
                        bool success = await control.TryKillFocusAsync();
                        MpConsole.WriteLine($"Kill Focus {(success ? "SUCCEEDED" : "FAILED")}  on '{ie}' from binding on '{control}'");
                    }
                });
                
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

        #region AcceptFocusWithin AvaloniaProperty
        public static bool GetAcceptFocusWithin(AvaloniaObject obj) {
            return obj.GetValue(AcceptFocusWithinProperty);
        }

        public static void SetAcceptFocusWithin(AvaloniaObject obj, bool value) {
            obj.SetValue(AcceptFocusWithinProperty, value);
        }

        public static readonly AttachedProperty<bool> AcceptFocusWithinProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "AcceptFocusWithin",
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
            if(sender is Control control) {
                if(GetAcceptFocusWithin(control) && control.IsKeyboardFocusWithin) {
                    return;
                }
                SetIsFocused(control, false);
            }
        }

        private static void FocusableControl_GotFocus(object sender, global::Avalonia.Input.GotFocusEventArgs e) {
            if (sender is Control focusableControl &&
                focusableControl.Tag is Control attachedControl) {
               
                if(GetSelectViewModelOnFocus(attachedControl) &&
                    attachedControl.DataContext is MpISelectableViewModel svm) {
                    svm.IsSelected = true;
                }
                SetIsFocused(attachedControl, true);

                if (GetSelectAllOnFocus(attachedControl)) {
                    if (focusableControl is TextBox tb) {
                        Dispatcher.UIThread.Post(async () => {
                            await Task.Delay(300);

                            tb.SelectAll();
                        });
                    } else if (focusableControl is AutoCompleteBox acb) {
                        // dunno how
                        Debugger.Break();
                    }
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
