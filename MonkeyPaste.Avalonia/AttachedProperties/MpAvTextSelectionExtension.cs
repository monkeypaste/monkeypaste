using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public static class MpAvTextBoxSelectionExtension {
        static MpAvTextBoxSelectionExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties


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
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                    control.DetachedFromVisualTree += DetachedFromVisualHandler;
                    if (control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    } 
                }
            } else {
                DetachedFromVisualHandler(element, null);
            }

            void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }
                    control.DetachedFromVisualTree += DetachedFromVisualHandler;
                }
            }
            void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedFromVisualHandler;
                }
            }
        }



        #endregion

        #endregion

        public static string GetSelectedPlainText(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is TextBox tb) {
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
                return tb.SelectionLength();
            }
            return 0;
        }

        public static void SetSelectionText(MpITextSelectionRange tsr, string text) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is TextBox tb) {
                tb.SelectedText = text;
            }
        }

        public static void SelectAll(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb == null) {
                Debugger.Break();
            }
            if (!tbb.IsFocused) {
                tbb.Focus();
            }
            if (!tbb.IsFocused) {
                Debugger.Break();
            }
            tbb.SelectAll();
        }

        public static bool IsAllSelected(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is TextBox tb) {
                return tb.Text.Length == tb.SelectionLength();
            }
            return false;
        }

        private static TextBox FindTextBoxBase(MpITextSelectionRange tsr) {
            // next look for TextBox Param
            var textBoxParamView = MpAvMainWindow.Instance
                                               .GetVisualDescendants<MpAvPluginParameterItemView>()
                                               .FirstOrDefault(x => x.DataContext == tsr);
            if (textBoxParamView != null) {
                var tbpv = textBoxParamView.GetVisualDescendant<MpAvTextBoxParameterView>();
                if (tbpv != null) {
                    return tbpv.GetVisualDescendant<TextBox>();
                }
            }

            //look for Editable List Param Value

            var editableListParamView = MpAvMainWindow.Instance
                                               .GetVisualDescendants<MpAvEditableListBoxParameterView>()
                                               .FirstOrDefault(x => x.BindingContext.Items.Contains(tsr));
                                              // .FirstOrDefault(x => x.EditableList.GetListBoxItem(tsr) != null);
            if (editableListParamView != null) {
                //var tsr_lbi = editableListParamView.EditableList.GetListBoxItem(tsr);
                var tsr_lbi_idx = editableListParamView.BindingContext.Items.IndexOf(tsr as MpAvEnumerableParameterValueViewModel);
                var tsr_lbi = editableListParamView.GetVisualDescendant<ListBox>().ItemContainerGenerator.ContainerFromIndex(tsr_lbi_idx);

                if (tsr_lbi is Control lbi) {
                    var cqtbv = lbi.GetVisualDescendant<MpAvContentQueryTextBoxView>();
                    if (cqtbv != null) {
                        return cqtbv.GetVisualDescendant<TextBox>();
                    }
                }
            }

            return null;
        }
    }
}
