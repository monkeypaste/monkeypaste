using CefSharp;
using CefSharp.Wpf;
using Microsoft.Web.WebView2.Wpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;
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
                                await cwb.EvaluateScriptAsync("enableReadOnly()");

                                var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
                                if (ctcv != null && civm.Parent != null) {
                                    double nw = civm.Parent.ReadOnlyContentSize.Width;
                                    double nh = civm.EditorHeight;
                                    //if(nh == double.NaN) {
                                    //    nh = 
                                    //}
                                    ctcv.TileResizeBehvior.Resize(nw - ctcv.ActualWidth, 0);
                                }

                                var response = await cwb.EvaluateScriptAsync("getHtml()");
                                string itemHtml = response.Result.ToString();
                                civm.CopyItemData = MpHtmlToRtfConverter.ConvertHtmlToRtf(itemHtml);
                            } else {
                                MpConsole.WriteLine($"Tile '{civm.Parent.HeadItem.CopyItemTitle}' is editable");
                                await cwb.EvaluateScriptAsync("disableReadOnly()");

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
                    var fe = (FrameworkElement)obj;

                    if (fe.DataContext is MpContentItemViewModel civm) {
                        var fd = itemData.ToFlowDocument(out Size docSize);
                        civm.UnformattedContentSize = docSize;

                        if (fe is ChromiumWebBrowser cwb) {
                            cwb.FrameLoadEnd += async (sender, args) => {
                                if (args.Frame.IsMain) {
                                    await cwb.EvaluateScriptAsync("setWpfEnv()");
                                    var initCmd = $"init({itemHtml},{JsonConvert.SerializeObject(civm.IsReadOnly)})";
                                    var result = await cwb.EvaluateScriptAsync(initCmd);
                                }
                            };
                        }
                    }
                }
            });

        #endregion

        private static async Task HandleReadOnlyChange(DependencyObject dpo, bool isReadOnly) {
            if (dpo is ChromiumWebBrowser cwb && cwb.CanExecuteJavascriptInMainFrame) {
                if (cwb.DataContext is MpContentItemViewModel civm) {
                    if (isReadOnly) {
                        MpConsole.WriteLine($"Tile '{civm.Parent.HeadItem.CopyItemTitle}' is readonly");
                        await cwb.EvaluateScriptAsync("enableReadOnly()");

                        var response = await cwb.EvaluateScriptAsync("getHtml()");
                        string itemHtml = response.Result.ToString();
                        civm.CopyItemData = MpHtmlToRtfConverter.ConvertHtmlToRtf(itemHtml);
                    } else {
                        MpConsole.WriteLine($"Tile '{civm.Parent.HeadItem.CopyItemTitle}' is editable");
                        await cwb.EvaluateScriptAsync("disableReadOnly()");

                        var ctcv = dpo.GetVisualAncestor<MpClipTileContainerView>();
                        if (ctcv != null) {
                            double nw = MpMeasurements.Instance.ClipTileEditModeMinWidth;
                            if (nw > ctcv.ActualWidth) {
                                ctcv.TileResizeBehvior.Resize(nw - ctcv.ActualWidth, 0);
                            }
                        }
                    }
                }
            }
        }
    }
}