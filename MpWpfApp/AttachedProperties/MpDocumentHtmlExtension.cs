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
using Xamarin.Essentials;

namespace MpWpfApp {
    public class MpDocumentHtmlExtension : DependencyObject {
        private static string _editorHtml;
        private static bool _isCefLoaded = false;

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
            typeof(MpDocumentHtmlExtension),
            new FrameworkPropertyMetadata() {
                PropertyChangedCallback = async (s, e) => {
                    if (e.NewValue == null) {
                        return;
                    }
                    var fe = s as FrameworkElement;
                    bool isReadOnly = (bool)e.NewValue;
                    if (fe is ChromiumWebBrowser cwb && cwb.CanExecuteJavascriptInMainFrame) {
                       // MpHelpers.RunOnMainThread(async () => {
                            if (isReadOnly) {
                                await cwb.EvaluateScriptAsync("enableReadOnly()");
                            } else {
                                await cwb.EvaluateScriptAsync("disableReadOnly()");
                                if (cwb.DataContext is MpContentItemViewModel civm) {
                                    var response = await cwb.EvaluateScriptAsync("getHtml()");
                                    civm.CopyItemData = response.Result.ToString();
                                }
                            }
                    //    });
                    }
                }
            });

        #endregion

        #region DocumentRtf

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
            typeof(MpDocumentHtmlExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback =  (obj, e) => {
                    string itemData = e.NewValue != null ? (string)e.NewValue:string.Empty;
                    string itemHtml = string.Empty;
                    if (!string.IsNullOrEmpty((string)e.NewValue)) {
                        if(itemData.IsStringRichText()) {
                            itemHtml = JsonConvert.SerializeObject(MpRtfToHtmlConverter.ConvertRtfToHtml(itemData));
                        } else {
                            itemHtml = JsonConvert.SerializeObject(itemHtml);
                        }
                    }
                    var fe = (FrameworkElement)obj;

                    if (fe.DataContext is MpContentItemViewModel civm) {
                        var fd = itemData.ToFlowDocument(out Size docSize);
                        civm.UnformattedContentSize = docSize;

                        if (fe is ChromiumWebBrowser cwb) {
                            cwb.FrameLoadEnd += async(sender, args) => {
                                if (args.Frame.IsMain) {
                                    await cwb.EvaluateScriptAsync("setWpfEnv()");
                                    var initCmd = $"init({itemHtml})";
                                    var result = await cwb.EvaluateScriptAsync(initCmd);
                                }
                            };
                        }
                    }
                }
            });

        #endregion
    }
}