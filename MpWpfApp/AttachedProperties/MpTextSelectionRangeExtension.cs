using Microsoft.Office.Interop.Outlook;
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
    public class MpTextSelectionRangeExtension : DependencyObject {
        #region TextSelectionRange DependencyProperty

        public static MpITextSelectionRange GetTextSelectionRange(DependencyObject obj) {
            return (MpITextSelectionRange)obj.GetValue(TextSelectionRangeProperty);
        }

        public static void SetTextSelectionRange(DependencyObject obj, MpITextSelectionRange value) {
            obj.SetValue(TextSelectionRangeProperty, value);
        }

        public static readonly DependencyProperty TextSelectionRangeProperty =
            DependencyProperty.Register(
                "TextSelectionRange", 
                typeof(MpITextSelectionRange), 
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
            if(tbb is RichTextBox rtb) {
                return rtb.Selection.Text;
            }
            return null;
        }

        public static int GetSelectionStart(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is TextBox tb) {
                return tb.SelectionStart;
            }
            if (tbb is RichTextBox rtb) {
                TextRange start = new TextRange(rtb.Document.ContentStart, rtb.Selection.Start);
                return start.Text.Length;
            }
            return 0;
        }

        public static int GetSelectionLength(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is TextBox tb) {
                return tb.SelectionLength;
            }
            if (tbb is RichTextBox rtb) {
                return rtb.Selection.Text.Length;
            }
            return 0;
        }

        public static bool IsSelectionContainTemplate(MpITextSelectionRange tsr) {
            return SelectedTextTemplates(tsr).Count() > 0;
        }

        public static ObservableCollection<MpTextTemplateViewModel> SelectedTextTemplates(MpITextSelectionRange tsr) {
            var sttl = new ObservableCollection<MpTextTemplateViewModel>();
            if (tsr is MpClipTileViewModel ctvm) {
                var tbb = FindTextBoxBase(tsr);
                if (tbb == null) {
                    return sttl;
                }
                if (tbb is RichTextBox rtb) {
                    IEnumerable<MpTextTemplate> citl = null;
                    if(rtb.Selection.IsEmpty) {
                        citl = rtb.Document.GetAllTextElements()
                                            .Where(x => x is InlineUIContainer)
                                            .Select(x => x.Tag as MpTextTemplate);
                    } else {
                        citl = rtb.Selection.GetAllTextElements()
                                            .Where(x => x is InlineUIContainer)
                                            .Select(x => x.Tag as MpTextTemplate);
                    }
                    return new ObservableCollection<MpTextTemplateViewModel>(ctvm.TemplateCollection.Items.Where(x => citl.Any(y => y.Id == x.TextTemplateId)));
                }
            }
            return sttl;
        }


        public static void SetTextSelection(MpITextSelectionRange tsr, TextRange tr) {
            var tbb = FindTextBoxBase(tsr);
            if(tbb is RichTextBox rtb) {
                if (!rtb.Document.ContentStart.IsInSameDocument(tr.Start) ||
                !rtb.Document.ContentStart.IsInSameDocument(tr.End)) {
                    return;
                }
                rtb.Selection.Select(tr.Start, tr.End);

                if (tr.Start.Parent is FrameworkContentElement fce) {
                    fce.BringIntoView();
                }
            }            
        }

        public static void SetSelectionText(MpITextSelectionRange tsr, string text) {
            var tbb = FindTextBoxBase(tsr);
            if(tbb is TextBox tb) {
                tb.SelectedText = text;
            }
            else if (tbb is RichTextBox rtb) {
                rtb.Selection.Text = text;
            }
        }

        public static void SelectAll(MpITextSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if(!tbb.IsFocused) {
                Debugger.Break();
            }
            tbb.SelectAll();
        }

        private static TextBoxBase FindTextBoxBase(MpITextSelectionRange tsr) {
            // NOTE for performance this needs to be updated if new view's use this extension
            

            // first look for content tile rtb
            if(tsr is MpClipTileViewModel ctvm) {
                var cv = Application.Current.MainWindow
                                            .GetVisualDescendents<MpContentView>()
                                            .FirstOrDefault(x => x.DataContext == ctvm);
                if(cv == null) {
                    return null;
                }
                return cv.Rtb;
            }

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