using CefSharp;
using CefSharp.Wpf;
using Microsoft.Web.WebView2.Wpf;
using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpDocumentRtfExtension : DependencyObject {
        private static string _editorHtml;
        private static bool _isCefLoaded = false;

        public static string GetDocumentRtf(DependencyObject obj) {
            return (string)obj.GetValue(DocumentRtfProperty);
        }
        public static void SetDocumentRtf(DependencyObject obj, string value) {
            obj.SetValue(DocumentRtfProperty, value);
        }
        public static readonly DependencyProperty DocumentRtfProperty =
          DependencyProperty.RegisterAttached(
            "DocumentRtf",
            typeof(string),
            typeof(MpDocumentRtfExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback =  (obj, e) => {
                    string rtf = string.Empty;
                    if (string.IsNullOrEmpty((string)e.NewValue)) {
                        rtf = string.Empty;
                    } else {
                        rtf = (string)e.NewValue;
                    }
                    var fe = (FrameworkElement)obj;
                    var fd = ((string)e.NewValue).ToFlowDocument(out Size docSize);

                    if (fe.DataContext is MpContentItemViewModel civm) {
                        civm.UnformattedContentSize = docSize;

                        if (fe is RichTextBox rtb) {

                            rtb.Document = fd;
                            rtb.FitDocToRtb();
                        } else if (fe is FlowDocumentPageViewer fdr) {
                            fdr.Document = fd;
                            fdr.UpdateLayout();
                        } else if (fe is FlowDocumentScrollViewer fdsv) {
                            fdsv.Document = fd;
                            fdsv.UpdateLayout();
                        } else if (fe is WebView2 wv) {
                            string itemHtml = JsonConvert.SerializeObject(MpRtfToHtmlConverter.ConvertFlowDocumentToHtml(fd));

                            var qev = wv.GetVisualAncestor<MpQuillEditorView>();
                            MpHelpers.RunOnMainThread(async() => {
                                while(qev == null) {
                                    await Task.Delay(100);
                                    qev = wv.GetVisualAncestor<MpQuillEditorView>();
                                }
                                while(qev.IsDomContentLoaded == false) {
                                    await Task.Delay(100);
                                }

                                await wv.ExecuteScriptAsync("setWpfEnv()");

                                var initCmd = $"init({itemHtml})";

                                string encodedResult = await wv.ExecuteScriptAsync(initCmd);

                                string decodedResult = JsonConvert.DeserializeObject<string>(encodedResult);
                                return;
                            });

                        } else if (fe is ChromiumWebBrowser cwb) {
                            string itemHtml = JsonConvert.SerializeObject(MpRtfToHtmlConverter.ConvertFlowDocumentToHtml(fd));

                            cwb.FrameLoadEnd += async(sender, args) => {
                                if (args.Frame.IsMain) {
                                    await cwb.EvaluateScriptAsync("setWpfEnv()");
                                    var initCmd = $"init({itemHtml})";
                                    var result = await cwb.EvaluateScriptAsync(initCmd);
                                    return;
                                }
                            };
                        }
                    }
                }
            });
        
    }
}