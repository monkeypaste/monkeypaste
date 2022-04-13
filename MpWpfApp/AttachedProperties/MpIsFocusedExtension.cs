using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpIsFocusedExtension : DependencyObject {
        public static bool IsAnyTextBoxFocused = false;

        #region IsFocused

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
            typeof(MpIsFocusedExtension),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                DefaultValue = false,
                PropertyChangedCallback = (s,e) => {
                    if(e.NewValue == null) {
                        return;
                    }
                    bool isFocused = (bool)e.NewValue;
                    if(isFocused) {
                        GotFocus(s as FrameworkElement);
                    } else {
                        LostFocus(s as FrameworkElement);
                    }
                }
            });

        #endregion

        #region IsReadOnly

        public static bool GetIsReadOnly(DependencyObject obj) {
            return (bool)obj.GetValue(IsReadOnlyProperty);
        }
        public static void SetIsReadOnly(DependencyObject obj, bool value) {
            obj.SetValue(IsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
          DependencyProperty.RegisterAttached(
            "IsReadOnly",
            typeof(bool),
            typeof(MpIsFocusedExtension),
            new FrameworkPropertyMetadata(false));

        #endregion

        #region SelectAllOnFocus

        public static bool GetSelectAllOnFocus(DependencyObject obj) {
            return (bool)obj.GetValue(SelectAllOnFocusProperty);
        }
        public static void SetSelectAllOnFocus(DependencyObject obj, bool value) {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }
        public static readonly DependencyProperty SelectAllOnFocusProperty =
          DependencyProperty.RegisterAttached(
            "SelectAllOnFocus",
            typeof(bool),
            typeof(MpIsFocusedExtension),
            new FrameworkPropertyMetadata {
                DefaultValue = false
            });

        #endregion

        #region IsEnabled

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
            typeof(MpIsFocusedExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    var fe = obj as FrameworkElement;
                    if(fe == null) {
                        return;
                    }

                    if ((bool)e.NewValue == true) {
                        fe.Unloaded += Fe_Unloaded;
                        fe.IsKeyboardFocusedChanged += MpIsFocusedExtension_IsKeyboardFocusedChanged;
                        if (!fe.IsLoaded) {
                            fe.Loaded += Fe_Loaded;
                        } else {
                            Fe_Loaded(fe, null);
                        }

                        if(fe.GetType().IsSubclassOf(typeof(TextBoxBase))) {
                            var tbb = fe as TextBoxBase;

                            var descriptor = DependencyPropertyDescriptor.FromProperty(TextBoxBase.IsReadOnlyProperty,fe.GetType());
                            
                            if (descriptor == null) {
                                return;
                            }

                            descriptor.AddValueChanged(tbb,Tbb_OnReadOnlyChanged);
                        }
                    } else {
                        Fe_Unloaded(fe, null);
                    }
                }
            });

        #endregion

        private static void Fe_Unloaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.IsKeyboardFocusedChanged -= MpIsFocusedExtension_IsKeyboardFocusedChanged;
            fe.Loaded -= Fe_Loaded;
            fe.Unloaded -= Fe_Unloaded;

            if (fe.GetType().IsSubclassOf(typeof(TextBoxBase))) {
                var tbb = fe as TextBoxBase;

                var descriptor = DependencyPropertyDescriptor.FromProperty(TextBoxBase.IsReadOnlyProperty, fe.GetType());

                if (descriptor == null) {
                    return;
                }

                descriptor.RemoveValueChanged(tbb, Tbb_OnReadOnlyChanged);
            }
        }

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.IsKeyboardFocusedChanged += MpIsFocusedExtension_IsKeyboardFocusedChanged;
            fe.GotFocus += Fe_GotFocus;
            fe.LostFocus += Fe_LostFocus;
        }

        private static void Fe_LostFocus(object sender, RoutedEventArgs e) {
            var dpo = (DependencyObject)sender;
            if (dpo == null) {
                return;
            }
            LostFocus(dpo);
        }

        private static void Fe_GotFocus(object sender, RoutedEventArgs e) {
            var dpo = (DependencyObject)sender;
            if (dpo == null) {
                return;
            }
            GotFocus(dpo);
        }

        private static void MpIsFocusedExtension_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var dpo = (DependencyObject)sender;
            if (dpo == null) {
                return;
            }
            bool isFocused = (bool)e.NewValue;
            if (isFocused) {
                GotFocus(dpo);
            } else {
                LostFocus(dpo);
            }
        }

        private static void Tbb_OnReadOnlyChanged(object sender, EventArgs e) {
            var tbb = sender as TextBoxBase;
            if (tbb != null) {
                bool isFocused = GetIsFocused(tbb);
                if(isFocused) {
                    if(tbb.IsReadOnly) {
                        // when focused textbox becomes readonly need to make sure static IsAnyTextBoxFocused is toggled
                        LostFocus(tbb);
                    } else {
                        GotFocus(tbb);
                    }
                } else {
                    LostFocus(tbb);
                }
            }
        }

        private static void GotFocus(DependencyObject dpo) {            
            SetIsFocused(dpo, true);
            if(dpo is FrameworkElement fe) {
                Keyboard.Focus(fe);
                if(fe.DataContext is MpISelectableViewModel svm) {
                    svm.IsSelected = true;
                }
            }
            if (dpo is TextBoxBase tbb) {
                if(tbb.IsReadOnly) {
                    IsAnyTextBoxFocused = false;
                } else {
                    IsAnyTextBoxFocused = true;
                    Keyboard.Focus(tbb);
                    if (GetSelectAllOnFocus(dpo)) {
                        tbb.SelectAll();
                    } else {
                        if(tbb is TextBox tb) {
                            tb.CaretIndex = 0;
                        } else if(tbb is RichTextBox rtb) {
                            rtb.CaretPosition = rtb.Document.ContentStart;
                        }
                    }
                }  
            }          
            
        }

        private static void LostFocus(DependencyObject dpo) {
            if (dpo is TextBoxBase tbb) {
                IsAnyTextBoxFocused = false;
            }
            
            SetIsFocused(dpo, false);
        }
    }
}