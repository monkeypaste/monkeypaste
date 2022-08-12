
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;

namespace MpWpfApp {
    public class MpTextBoxSelectionRangeExtension : DependencyObject {
        #region TextSelectionRange DependencyProperty

        public static MpITextSelectionRange GetTextSelectionRange(DependencyObject obj) {
            return (MpITextSelectionRange)obj.GetValue(TextSelectionRangeProperty);
        }

        public static void SetTextSelectionRange(DependencyObject obj, MpITextSelectionRange value) {
            obj.SetValue(TextSelectionRangeProperty, value);
        }

        public static readonly DependencyProperty TextSelectionRangeProperty =
            DependencyProperty.RegisterAttached(
                "TextSelectionRange", 
                typeof(MpITextSelectionRange), 
                typeof(MpTextBoxSelectionRangeExtension), 
                new FrameworkPropertyMetadata(null));

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
            typeof(MpTextBoxSelectionRangeExtension),
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
                                Tbb_Loaded(tbb, null);                                
                            } else {
                                tbb.Loaded += Tbb_Loaded;
                            }
                            
                        } else {
                            Tbb_Unloaded(tbb, null);
                        }
                    }
                }
            });

        private static void Tbb_Loaded(object sender, RoutedEventArgs e) {
            var tbb = sender as TextBoxBase;
            if(e == null) {
                tbb.Loaded += Tbb_Loaded;
            }
            tbb.Unloaded += Tbb_Unloaded;
        }

        private static void Tbb_Unloaded(object sender, RoutedEventArgs e) {
            var tbb = sender as TextBoxBase;
            tbb.Loaded -= Tbb_Loaded;
            tbb.Unloaded -= Tbb_Unloaded;
        }
        #endregion

        public static string GetSelectedPlainText(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if(tbb is TextBox tb) {
                return tb.SelectedText;
            }
            return null;
        }

        public static string GetSelectedRichText(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is TextBox tb) {
                return tb.SelectedText;
            }
            return null;
        }

        public static int GetSelectionStart(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is TextBox tb) {
                return tb.SelectionStart;
            }
            return 0;
        }

        public static int GetSelectionLength(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is TextBox tb) {
                return tb.SelectionLength;
            }
            return 0;
        }

        public static void SetSelectionText(MpITextSelectionRange tsr, string text) {
            var tbb = FindTextBoxBase(tsr);
            if(tbb is TextBox tb) {
                tb.SelectedText = text;
            }
        }

        public static void SelectAll(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if(tbb == null) {
                Debugger.Break();
            }
            if(!tbb.IsFocused) {
                tbb.Focus();
            }
            if(!tbb.IsFocused) {
                Debugger.Break();
            }
            tbb.SelectAll();
        }

        public static bool IsAllSelected(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if(tbb is TextBox tb) {
                return tb.Text.Length == tb.SelectionLength;
            } 
            return false;
        }

        private static TextBoxBase FindTextBoxBase(MpITextSelectionRange tsr) {
            // next look for TextBox Param
            var textBoxParamView = Application.Current.MainWindow
                                               .GetVisualDescendents<MpAnalyticItemParameterView>()
                                               .FirstOrDefault(x => x.DataContext == tsr);
            if(textBoxParamView != null) {
                var tbpv = textBoxParamView.GetVisualDescendent<MpTextBoxParameterView>();
                if(tbpv != null) {
                    return tbpv.ParameterTextBox;
                }
            }

            //look for Editable List Param Value

            var editableListParamView = Application.Current.MainWindow
                                               .GetVisualDescendents<MpEditableListBoxParameterView>()
                                               .FirstOrDefault(x => x.EditableList.GetListBoxItem(tsr) != null);
            if (editableListParamView != null) {
                var tsr_lbi = editableListParamView.EditableList.GetListBoxItem(tsr);

                if (tsr_lbi != null) {
                    var cqtbv = tsr_lbi.GetVisualDescendent<MpContentQueryTextBoxView>();
                    if(cqtbv != null) {
                        return cqtbv.ContentQueryTextBox;
                    }
                }
            }

            return null;
        }
    }
}