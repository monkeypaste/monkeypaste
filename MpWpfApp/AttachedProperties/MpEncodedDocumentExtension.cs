using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpEncodedDocumentExtension : DependencyObject {
        #region Private Variables
        private static string _regexOpenCloseMatchPattern = ".*?";

        private static string _encodedTemplateOpenToken = "{t{";
        private static string _encodedTemplateCloseToken = "}t}";
        private static Regex _encodedTemplateRegEx = new Regex(
            string.Format(
                @"{0}{1}{2}",
                _encodedTemplateOpenToken,
                _regexOpenCloseMatchPattern,
                _encodedTemplateCloseToken));

        private static string _encodedDocumentOpenToken = "{c{";
        private static string _encodedDocumentCloseToken = "}c}";
        private static Regex _encodedDocumentRegEx = new Regex(
            string.Format(
                @"{0}{1}{2}",
                _encodedDocumentOpenToken,
                _regexOpenCloseMatchPattern,
                _encodedDocumentCloseToken));

        #endregion

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
                typeof(MpEncodedDocumentExtension), 
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
            typeof(MpEncodedDocumentExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if(e.NewValue is bool isEnabled) {
                        var rtb = obj as RichTextBox;
                        if(rtb == null) {
                            if(obj == null) {
                                return;
                            }
                            throw new System.Exception("This extension must be attach to a richtextbox control");
                        }

                        if (isEnabled) {
                            if(rtb.IsLoaded) {
                                Rtb_Loaded(rtb, null);
                            } else {
                                rtb.Loaded += Rtb_Loaded;
                            }
                            rtb.Unloaded += Rtb_Unloaded;
                        } else {
                            Rtb_Unloaded(rtb, null);
                        }
                    }
                }
            });

        private static void Rtb_Unloaded(object sender, RoutedEventArgs e) {
            var tbb = sender as TextBoxBase;
            tbb.Loaded -= Rtb_Loaded;
            tbb.Unloaded -= Rtb_Unloaded;
        }

        private static void Rtb_Loaded(object sender, RoutedEventArgs e) {
            var tbb = sender as TextBoxBase;
        }

        private static async Task DecodeDocument(RichTextBox rtb) {
            await MpHelpers.RunOnMainThreadAsync(() => {

            });
        }

        private static List<MpTextTemplate> GetTextTemplates(string itemPlainText) {
            string[] itemTemplateGuids = GetTextTemplateGuids(itemPlainText).Select(x => x.ToLower()).ToArray();

            return MpMasterTemplateModelCollection.AllTemplates
                                    .Where(x => itemTemplateGuids.Contains(x.Guid.ToLower()))
                                    .ToList();
        }

        private static string[] GetTextTemplateGuids(string itemPlainText) {

            var etgl = new List<string>();

            var mc = _encodedTemplateRegEx.Matches(itemPlainText);
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        string tguid = c.Value
                                            .Replace(_encodedTemplateOpenToken, string.Empty)
                                            .Replace(_encodedTemplateCloseToken, string.Empty);
                        if (etgl.Contains(tguid)) {
                            continue;
                        }
                        etgl.Add(tguid);
                    }
                }
            }
            return etgl.ToArray();
        }

        #endregion
    }
}