using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
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
                        fe.Loaded += Fe_Loaded;
                        fe.Unloaded += Fe_Unloaded;
                        if (fe.IsLoaded) {
                            fe.IsKeyboardFocusedChanged += MpIsFocusedExtension_IsKeyboardFocusedChanged;
                        } else {
                            fe.Loaded += Fe_Loaded;
                        }
                    } else {
                        Fe_Unloaded(fe, null);
                    }
                }
            });

        #endregion


        #region IsSaveOnLostFocus

        public static bool GetIsSaveOnLostFocus(DependencyObject obj) {
            return (bool)obj.GetValue(IsSaveOnLostFocusProperty);
        }
        public static void SetIsSaveOnLostFocus(DependencyObject obj, bool value) {
            obj.SetValue(IsSaveOnLostFocusProperty, value);
        }
        public static readonly DependencyProperty IsSaveOnLostFocusProperty =
          DependencyProperty.RegisterAttached(
            "IsSaveOnLostFocus",
            typeof(bool),
            typeof(MpIsFocusedExtension),
            new FrameworkPropertyMetadata(false));

        #endregion

        #region SaveModel

        public static MpDbModelBase GetSaveModel(DependencyObject obj) {
            return (MpDbModelBase)obj.GetValue(SaveModelProperty);
        }
        public static void SetSaveModel(DependencyObject obj, MpDbModelBase value) {
            obj.SetValue(SaveModelProperty, value);
        }
        public static readonly DependencyProperty SaveModelProperty =
          DependencyProperty.RegisterAttached(
            "SaveModel",
            typeof(MpDbModelBase),
            typeof(MpIsFocusedExtension),
            new FrameworkPropertyMetadata(null));

        #endregion


        private static void Fe_Unloaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.IsKeyboardFocusedChanged -= MpIsFocusedExtension_IsKeyboardFocusedChanged;
            fe.Loaded -= Fe_Loaded;
            fe.Unloaded -= Fe_Unloaded;
        }

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.IsKeyboardFocusedChanged += MpIsFocusedExtension_IsKeyboardFocusedChanged;
            fe.GotFocus += Fe_GotFocus;
            fe.LostFocus += Fe_LostFocus;
        }



        private static void GotFocus(DependencyObject dpo) {
            IsAnyTextBoxFocused = true;
            SetIsFocused(dpo, true);
        }

        private static void LostFocus(DependencyObject dpo) {
            IsAnyTextBoxFocused = false;
            SetIsFocused(dpo, false);

            if (GetIsSaveOnLostFocus(dpo)) {
                var dbo = GetSaveModel(dpo);
                if (dbo == null) {
                    return;
                }
                Task.Run(async () => {
                    await dbo.WriteToDatabaseAsync();
                });
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
            GotFocus(dpo);
        }

        private static void MpIsFocusedExtension_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var dpo = (DependencyObject)sender;
            if(dpo == null) {
                return;
            }
            bool isFocused = (bool)e.NewValue;
            if(isFocused) {
                GotFocus(dpo);
            } else {
                LostFocus(dpo);
            }
        }
    }
}