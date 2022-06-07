using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpIsFocusedExtension : DependencyObject {

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

        #region SelectViewModelOnFocus

        public static bool GetSelectViewModelOnFocus(DependencyObject obj) {
            return (bool)obj.GetValue(SelectViewModelOnFocusProperty);
        }
        public static void SetSelectViewModelOnFocus(DependencyObject obj, bool value) {
            obj.SetValue(SelectViewModelOnFocusProperty, value);
        }
        public static readonly DependencyProperty SelectViewModelOnFocusProperty =
          DependencyProperty.RegisterAttached(
            "SelectViewModelOnFocus",
            typeof(bool),
            typeof(MpIsFocusedExtension),
            new FrameworkPropertyMetadata(true));

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
                    } else {
                        Fe_Unloaded(fe, null);
                    }
                }
            });

        #endregion
        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.IsKeyboardFocusedChanged += MpIsFocusedExtension_IsKeyboardFocusedChanged;
            fe.GotFocus += Fe_GotFocus;
            fe.LostFocus += Fe_LostFocus;
            if (fe is TextBoxBase tbb) {
                var descriptor = DependencyPropertyDescriptor.FromProperty(TextBoxBase.IsReadOnlyProperty, fe.GetType());
                if (descriptor == null) {
                    return;
                }
                descriptor.AddValueChanged(tbb, Tbb_OnReadOnlyChanged);
            } 
        }

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
            e.Handled = true;
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
            if (sender is TextBoxBase tbb) {
                bool isFocused = GetIsFocused(tbb);
                if (isFocused) {
                    if (tbb.IsReadOnly) {
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

        public static void GotFocus(DependencyObject dpo) {
            SetIsFocused(dpo, true);

            if (dpo is FrameworkElement fe) {
                Keyboard.Focus(fe);
                if (fe.DataContext is MpISelectableViewModel svm && 
                    GetSelectViewModelOnFocus(dpo)) {
                    svm.IsSelected = true;
                }
            }
            
            if (dpo is TextBoxBase tbb) {
                if (tbb.IsReadOnly) {
                    MpMainWindowViewModel.Instance.IsAnyTextBoxFocused = false;
                } else {
                    MpMainWindowViewModel.Instance.IsAnyTextBoxFocused = true;
                    //Keyboard.Focus(tbb);
                    if (tbb is TextBox tb) {
                        tb.CaretIndex = 0;
                    } else if (tbb is RichTextBox rtb) {
                        rtb.CaretPosition = rtb.Document.ContentStart;
                    }
                    if (GetSelectAllOnFocus(dpo)) {                        
                        tbb.SelectAll();
                    }
                }
            }
        }

        private static void LostFocus(DependencyObject dpo) {
            if (dpo is TextBoxBase tbb) {
                MpMainWindowViewModel.Instance.IsAnyTextBoxFocused = false;
            }
            
            SetIsFocused(dpo, false);
        }
    }

    public class SelectTextOnFocus : DependencyObject {
        public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached(
            "Active",
            typeof(bool),
            typeof(SelectTextOnFocus),
            new PropertyMetadata(false, ActivePropertyChanged));

        private static void ActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is TextBox) {
                TextBox textBox = d as TextBox;
                if ((e.NewValue as bool?).GetValueOrDefault(false)) {
                    textBox.GotKeyboardFocus += OnKeyboardFocusSelectText;
                    textBox.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                } else {
                    textBox.GotKeyboardFocus -= OnKeyboardFocusSelectText;
                    textBox.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DependencyObject dependencyObject = GetParentFromVisualTree(e.OriginalSource);

            if (dependencyObject == null) {
                return;
            }

            var textBox = (TextBox)dependencyObject;
            if (!textBox.IsKeyboardFocusWithin) {
                textBox.Focus();
                e.Handled = true;
            }
        }

        private static DependencyObject GetParentFromVisualTree(object source) {
            DependencyObject parent = source as UIElement;
            while (parent != null && !(parent is TextBox)) {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

        private static void OnKeyboardFocusSelectText(object sender, KeyboardFocusChangedEventArgs e) {
            TextBox textBox = e.OriginalSource as TextBox;
            if (textBox != null) {
                textBox.SelectAll();
            }
        }

        [AttachedPropertyBrowsableForChildrenAttribute(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetActive(DependencyObject @object) {
            return (bool)@object.GetValue(ActiveProperty);
        }

        public static void SetActive(DependencyObject @object, bool value) {
            @object.SetValue(ActiveProperty, value);
        }
    }
}