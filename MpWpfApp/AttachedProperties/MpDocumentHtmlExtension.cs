using Azure;
using CefSharp;
using CefSharp.SchemeHandler;
using CefSharp.Wpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
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
        public const string ENCODED_TEMPLATE_OPEN_TOKEN = "{t{";
        public const string ENCODED_TEMPLATE_CLOSE_TOKEN = "}t}";

        public static string ENCODED_TEMPLATE_REGEXP_STR = string.Format(
            @"{0}{1}{2}",
            ENCODED_TEMPLATE_OPEN_TOKEN,
            ".*?",
            ENCODED_TEMPLATE_CLOSE_TOKEN);

        private static Regex _encodedTemplateRegEx;


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
                        if (isReadOnly) {
                            var readOnlyResult = await cwb.EvaluateScriptAsync("enableReadOnly()");
                            ProcessReadOnlyResponse(fe, readOnlyResult);

                            //var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
                            //if (ctcv != null && civm.Parent != null) {
                            //    double nw = civm.Parent.ReadOnlyContentSize.Width;
                            //    double nh = civm.EditorHeight;
                            //    //if(nh == double.NaN) {
                            //    //    nh = 
                            //    //}
                            //    ctcv.TileResizeBehvior.Resize(nw - ctcv.ActualWidth, 0);
                            //}
                        } else {
                            MpQuillDisableReadOnlyRequestMessage drorMsg = CreateDisableReadOnlyMessage(fe);
                            await cwb.EvaluateScriptAsync($"disableReadOnly('{drorMsg.SerializeToByteString()}')");

                            //var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
                            //if (ctcv != null) {
                            //    double nw = MpMeasurements.Instance.ClipTileEditModeMinWidth;
                            //    double nh = civm.EditorHeight;
                            //    if (nh == double.NaN) {
                            //        nh = MpMeasurements.Instance.ClipTileEditToolbarHeight + civm.UnformattedContentSize.Height;
                            //    } else {
                            //        nh = civm.Parent.TileContentHeight;
                            //    }
                            //    //nh = 1000;
                            //    //cwb.Height = nh;
                            //    if (nw > ctcv.ActualWidth) {
                            //        ctcv.TileResizeBehvior.Resize(nw - ctcv.ActualWidth, 0);
                            //    }
                            //}
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
                PropertyChangedCallback = (obj, e) => {       

                if (obj is ChromiumWebBrowser cwb) {
                        var lrm = CreateLoadRequestMessage(cwb, e.NewValue);

                        cwb.FrameLoadEnd += async (sender, args) => {
                            if (args.Frame.IsMain) {
                                var initCmd = $"init('{lrm.SerializeToByteString()}')";
                                var result = await cwb.EvaluateScriptAsync(initCmd);
                            }
                        };
                    }
                }
            });

        #endregion

        public static void Init() {
            //called in bootstrap
            InitCef();
        }

        private static void InitCef() {
            //To support High DPI this must be before CefSharp.BrowserSubprocess.SelfHost.Main so the BrowserSubprocess is DPI Aware
            Cef.EnableHighDPISupport();

            var exitCode = CefSharp.BrowserSubprocess.SelfHost.Main(new string[] { });

            if (exitCode >= 0) {
                return;
            }

            var settings = new CefSettings() {
                LogSeverity = LogSeverity.Verbose
            };
            settings.RegisterScheme(new CefCustomScheme {                
                SchemeName = "localfolder",
                DomainName = "cefsharp",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(
                    rootFolder: Path.Combine(Environment.CurrentDirectory, "Resources/Html/Editor"),
                    hostName: "cefsharp",
                    defaultPage: "index.html"
                )
            });
            Cef.Initialize(settings, performDependencyCheck: false);
        }

        #region Messages

        private static MpQuillLoadRequestMessage CreateLoadRequestMessage(FrameworkElement fe, object newValue) {
            if (fe.DataContext is MpContentItemViewModel civm) {

                MpConsole.WriteLine($"Tile Content Item '{(fe.DataContext as MpContentItemViewModel).CopyItemTitle}' is loaded");

                string itemData = newValue == null ? string.Empty : (string)newValue;

                return new MpQuillLoadRequestMessage() {
                    envName = "wpf",
                    itemEncodedHtmlData = GetEncodedHtml(itemData,civm.CopyItemGuid),
                    usedTextTemplates = GetTextTemplates(itemData),
                    isPasteRequest = civm.IsPasting,
                    isReadOnlyEnabled = civm.IsReadOnly
                };
            }
            return null;
        }

        private static MpQuillDisableReadOnlyRequestMessage CreateDisableReadOnlyMessage(FrameworkElement fe) {
            MpConsole.WriteLine($"Tile content item '{(fe.DataContext as MpContentItemViewModel).CopyItemTitle}' is editable");

            MpQuillDisableReadOnlyRequestMessage drorMsg = new MpQuillDisableReadOnlyRequestMessage() {
                allAvailableTextTemplates = MpMasterTemplateModelCollection.AllTemplates.ToList(),
                editorHeight = fe.GetVisualAncestor<MpContentItemView>().ActualHeight
            };
            return drorMsg;
        }

        private static void ProcessReadOnlyResponse(FrameworkElement fe, JavascriptResponse readOnlyResult) {
            if(fe.DataContext is MpContentItemViewModel civm) {
                if (readOnlyResult.Result != null && readOnlyResult.Result is string resultStr) {
                    MpConsole.WriteLine($"Tile content item '{civm.CopyItemTitle}' is readonly");

                    var qrm = JsonConvert.DeserializeObject<MpQuillEnableReadOnlyResponseMessage>(resultStr);

                    civm.CopyItemData = MpHtmlToRtfConverter.ConvertHtmlToRtf(qrm.itemEncodedHtmlData);

                    MpMasterTemplateModelCollection.Update(qrm.updatedAllAvailableTextTemplates, qrm.removedGuids).FireAndForgetSafeAsync(civm);
                }
            }
        }
        #endregion

        private static string GetEncodedHtml(string itemData, string itemGuid) {
            if (itemData.IsStringRichText()) {
                return MpRtfToHtmlConverter.ConvertRtfToHtml(itemData, new Dictionary<string, string>() { { "copyItemGuid",itemGuid } });
            }
            return itemData;
        }

        private static List<MpTextTemplate> GetTextTemplates(string itemData) {
            string[] itemTemplateGuids = GetTextTemplateGuids(itemData).Select(x => x.ToLower()).ToArray();

            return MpMasterTemplateModelCollection.AllTemplates
                                    .Where(x => itemTemplateGuids.Contains(x.Guid.ToLower()))
                                    .ToList();
        }

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