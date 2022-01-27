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
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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
                    if (e.NewValue == null) {
                        return;
                    }
                    bool isEnabled = (bool)e.NewValue;
                    if (isEnabled) {
                        (obj as FrameworkElement).IsKeyboardFocusedChanged += MpIsFocusedExtension_IsKeyboardFocusedChanged;
                    } else {
                        (obj as FrameworkElement).IsKeyboardFocusedChanged -= MpIsFocusedExtension_IsKeyboardFocusedChanged;
                    }
                }
            });

        #endregion

        private static void MpIsFocusedExtension_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var dpo = (DependencyObject)sender;
            bool isFocused = (bool)e.NewValue;

            SetIsFocused(dpo, isFocused);

            if(!isFocused) {
                if(GetIsSaveOnLostFocus(dpo)) {
                    var dbo = GetSaveModel(dpo);
                    if(dbo == null) {
                        return;
                    }
                    Task.Run(async () => {
                        await dbo.WriteToDatabaseAsync();
                    });
                }
            }
        }
    }
}