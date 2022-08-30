using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Windows.Input;
using System.Linq;
using System;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using System.Runtime.Intrinsics.Arm;
using Avalonia.Media.Immutable;
using Avalonia.Controls.Shapes;
using System.Diagnostics;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste.Avalonia {
    public static class MpAvTextBoxSelectionExtension {
        static MpAvTextBoxSelectionExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #region TextSelectionRange AvaloniaProperty
        public static MpITextSelectionRange GetTextSelectionRange(AvaloniaObject obj) {
            return obj.GetValue(TextSelectionRangeProperty);
        }

        public static void SetTextSelectionRange(AvaloniaObject obj, MpITextSelectionRange value) {
            obj.SetValue(TextSelectionRangeProperty, value);
        }

        public static readonly AttachedProperty<MpITextSelectionRange> TextSelectionRangeProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpITextSelectionRange>(
                "TextSelectionRange",
                null,
                false,
                BindingMode.TwoWay);

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
            if(e.NewValue is bool isEnabledVal && isEnabledVal) {
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

        }

        #endregion

        #region Public Methods

        public static async Task<string> GetSelectedPlainTextAsync(MpITextSelectionRange tsr) {
            var control = FindControl(tsr);
            if (control is MpAvIContentView cv) {
                string text = await cv.Selection.GetTextAsync();
                return text;
            }

            return null;
        }

        public static int GetSelectionStart(MpITextSelectionRange tsr) {
            var control = FindControl(tsr);
            if (control is MpAvIContentView cv) {
                return cv.Selection.Start.Offset;
            }
            return 0;
        }

        public static int GetSelectionLength(MpITextSelectionRange tsr) {
            var control = FindControl(tsr);
            if (control is MpAvIContentView cv) {
                return cv.Selection.End.Offset - cv.Selection.Start.Offset;
            }
            return 0;
        }

        public static void SetSelectionText(MpITextSelectionRange tsr, string text) {
            var control = FindControl(tsr);
            if(control is MpAvIContentView cv) {
                cv.Selection.SetTextAsync(text).FireAndForgetSafeAsync((cv as Control).DataContext as MpAvClipTileViewModel);
            }
        }

        public static void SelectAll(MpITextSelectionRange tsr) {
            var control = FindControl(tsr);
            if (control == null) {
                Debugger.Break();
            }
            if (!control.IsFocused) {
                control.Focus();
            }
            if (!control.IsFocused) {
                Debugger.Break();
            }
            if(control is MpAvIContentView cv) {
                cv.SelectAll();
            }
        }

        public static async Task<bool> IsAllSelectedAsync(MpITextSelectionRange tsr) {
            var control = FindControl(tsr);
            if (control is TextBox tb) {
                return tb.Text.Length == tb.SelectedText.Length;
            } else if(control is MpAvCefNetWebView wv) {
                string resultStr = await wv.EvaluateJavascriptAsync("isAllSelected()");
                return resultStr.ToLower() == "true";

            }   
            return false;
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private static void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.DetachedFromVisualTree += DetachedToVisualHandler;

                if (e == null) {
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                }
            }
        }

        private static void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.AttachedToVisualTree -= AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DetachedToVisualHandler;
            }
        }

        #endregion


        private static Control FindControl(MpITextSelectionRange tsr) {
            // next look for TextBox Param
            //var textBoxParamView = Application.Current.MainWindow
            //                                   .GetVisualDescendents<MpAnalyticItemParameterView>()
            //                                   .FirstOrDefault(x => x.DataContext == tsr);
            //if (textBoxParamView != null) {
            //    var tbpv = textBoxParamView.GetVisualDescendent<MpTextBoxParameterView>();
            //    if (tbpv != null) {
            //        return tbpv.ParameterTextBox;
            //    }
            //}

            ////look for Editable List Param Value

            //var editableListParamView = Application.Current.MainWindow
            //                                   .GetVisualDescendents<MpEditableListBoxParameterView>()
            //                                   .FirstOrDefault(x => x.EditableList.GetListBoxItem(tsr) != null);
            //if (editableListParamView != null) {
            //    var tsr_lbi = editableListParamView.EditableList.GetListBoxItem(tsr);

            //    if (tsr_lbi != null) {
            //        var cqtbv = tsr_lbi.GetVisualDescendent<MpContentQueryTextBoxView>();
            //        if (cqtbv != null) {
            //            return cqtbv.ContentQueryTextBox;
            //        }
            //    }
            //}

            return null;
        }

        #endregion
    }

}
