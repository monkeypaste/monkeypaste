using Azure;
using CefSharp;
using CefSharp.Wpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Xamarin.Essentials;

namespace MpWpfApp {
    public class MpDocumentHtmlExtension : DependencyObject {
        public const string ENCODED_TEMPLATE_OPEN_TOKEN = "{{";
        public const string ENCODED_TEMPLATE_CLOSE_TOKEN = "}}";

        public static string ENCODED_TEMPLATE_REGEXP_STR = string.Format(
            @"{0}{1}{2}",
            ENCODED_TEMPLATE_OPEN_TOKEN,
            ".*?",
            ENCODED_TEMPLATE_CLOSE_TOKEN);

        private static Regex _encodedTemplateRegEx;

        private static string _encodedTemplateRegExInfoStr;

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
                        if (cwb.DataContext is MpContentItemViewModel civm) {
                            if (isReadOnly) {
                                MpConsole.WriteLine($"Tile '{civm.Parent.HeadItem.CopyItemTitle}' is readonly");
                                var readOnlyResult = await cwb.EvaluateScriptAsync("enableReadOnly()");
                                if (readOnlyResult.Result != null) {
                                    MpMasterTemplateModelCollection.Update(readOnlyResult.Result.ToString()).FireAndForgetSafeAsync(civm);
                                }

                                var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
                                if (ctcv != null && civm.Parent != null) {
                                    double nw = civm.Parent.ReadOnlyContentSize.Width;
                                    double nh = civm.EditorHeight;
                                    //if(nh == double.NaN) {
                                    //    nh = 
                                    //}
                                    ctcv.TileResizeBehvior.Resize(nw - ctcv.ActualWidth, 0);
                                }

                                var response = await cwb.EvaluateScriptAsync("getEncodedHtml()");
                                string itemHtml = response.Result.ToString();
                                civm.CopyItemData = MpHtmlToRtfConverter.ConvertHtmlToRtf(itemHtml);
                            } else {
                                MpConsole.WriteLine($"Tile '{civm.Parent.HeadItem.CopyItemTitle}' is editable");

                                string availTextTemplatesStr = JsonConvert.SerializeObject(MpMasterTemplateModelCollection.AllTemplates.ToArray());
                                await cwb.EvaluateScriptAsync($"disableReadOnly({availTextTemplatesStr})");

                                var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
                                if (ctcv != null) {
                                    double nw = MpMeasurements.Instance.ClipTileEditModeMinWidth;
                                    double nh = civm.EditorHeight;
                                    if (nh == double.NaN) {
                                        nh = MpMeasurements.Instance.ClipTileEditToolbarHeight + civm.UnformattedContentSize.Height;
                                    } else {
                                        nh = civm.Parent.TileContentHeight;
                                    }
                                    //nh = 1000;
                                    //cwb.Height = nh;
                                    if (nw > ctcv.ActualWidth) {
                                        ctcv.TileResizeBehvior.Resize(nw - ctcv.ActualWidth, 0);
                                    }
                                }
                            }
                        }
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
                    string[] itemTemplateGuids = GetTextTemplateGuids(itemHtml).Select(x=>x.ToLower()).ToArray();

                    var itemTemplates = MpMasterTemplateModelCollection.AllTemplates
                                            .Where(x => itemTemplateGuids.Contains(x.Guid.ToLower()));

                    string itemTemplatesStr = JsonConvert.SerializeObject(itemTemplates);

                    if(_encodedTemplateRegExInfoStr == null) {
                        _encodedTemplateRegExInfoStr = JsonConvert.SerializeObject(new string[] { JsonConvert.SerializeObject(ENCODED_TEMPLATE_OPEN_TOKEN), JsonConvert.SerializeObject(ENCODED_TEMPLATE_REGEXP_STR), JsonConvert.SerializeObject(ENCODED_TEMPLATE_CLOSE_TOKEN) });
                    }

                    var fe = (FrameworkElement)obj;

                    if (fe.DataContext is MpContentItemViewModel civm) {
                        var fd = itemData.ToFlowDocument(out Size docSize);
                        civm.UnformattedContentSize = docSize;

                        if (fe is ChromiumWebBrowser cwb) {
                            cwb.FrameLoadEnd += async (sender, args) => {
                                if (args.Frame.IsMain) {
                                    await cwb.EvaluateScriptAsync("setWpfEnv()");
                                    var initCmd = string.Format(@"init({0},{1},{2})",
                                                            itemHtml,
                                                            JsonConvert.SerializeObject(civm.IsReadOnly),
                                                            itemTemplatesStr);//,
                                                            //_encodedTemplateRegExInfoStr);

                                    var result = await cwb.EvaluateScriptAsync(initCmd);
                                }
                            };
                        }
                    }
                }
            });

        #endregion

        private static string[] GetTextTemplateGuids(string itemData) {
            if(_encodedTemplateRegEx == null) {
                _encodedTemplateRegEx = new Regex(ENCODED_TEMPLATE_REGEXP_STR);
            }

            var etgl = new List<string>();

            var mc = _encodedTemplateRegEx.Matches(itemData);
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        string tguid = c.Value
                                            .Replace(ENCODED_TEMPLATE_OPEN_TOKEN, string.Empty)
                                            .Replace(ENCODED_TEMPLATE_CLOSE_TOKEN, string.Empty);
                        if(etgl.Contains(tguid)) {
                            continue;
                        }
                        etgl.Add(tguid);
                    }
                }
            }
            return etgl.ToArray();
        }
    }
}