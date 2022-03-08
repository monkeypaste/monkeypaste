using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpTextSelectionRangeExtension : DependencyObject {
        #region TextSelectionRange DependencyProperty

        public static MpITextSelectionRangeViewModel GetTextSelectionRange(DependencyObject obj) {
            return (MpITextSelectionRangeViewModel)obj.GetValue(TextSelectionRangeProperty);
        }

        public static void SetTextSelectionRange(DependencyObject obj, MpITextSelectionRangeViewModel value) {
            obj.SetValue(TextSelectionRangeProperty, value);
        }

        public static readonly DependencyProperty TextSelectionRangeProperty =
            DependencyProperty.Register(
                "TextSelectionRange", typeof(MpITextSelectionRangeViewModel), 
                typeof(MpTextSelectionRangeExtension), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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
            typeof(MpTextSelectionRangeExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if(e.NewValue is bool isEnabled) {
                        var tbb = obj as TextBoxBase;
                        if(tbb == null) {
                            if(obj == null) {
                                return;
                            }
                            throw new System.Exception("This extension must be attach to a textbox control");
                        }

                        if (isEnabled) {
                            if(tbb.IsLoaded) {
                                tbb.SelectionChanged += Tbb_SelectionChanged;
                            } else {
                                tbb.Loaded += Tbb_Loaded;
                            }
                            tbb.Unloaded += Tbb_Unloaded;
                        } else {
                            Tbb_Unloaded(tbb, null);
                        }
                    }
                }
            });

        private static void Tbb_Loaded(object sender, RoutedEventArgs e) {
            var tbb = sender as TextBoxBase;
            tbb.SelectionChanged += Tbb_SelectionChanged;
        }

        private static void Tbb_Unloaded(object sender, RoutedEventArgs e) {
            var tbb = sender as TextBoxBase;
            tbb.Loaded -= Tbb_Loaded;
            tbb.SelectionChanged -= Tbb_SelectionChanged;
            tbb.Unloaded -= Tbb_Unloaded;
        }


        private static void Tbb_SelectionChanged(object sender, RoutedEventArgs e) {
            if(sender is TextBox tb) {
                var tsrvm = GetTextSelectionRange(tb);
                tsrvm.SelectionStart = tb.SelectionStart;
                tsrvm.SelectionLength = tb.SelectionLength;
                //SetTextSelectionRange(tb, tsrvm);
            }
        }

        #endregion
    }
}