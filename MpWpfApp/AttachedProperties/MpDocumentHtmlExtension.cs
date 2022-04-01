using Azure;
using CefSharp;
using CefSharp.SchemeHandler;
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
        public const string ENCODED_TEMPLATE_OPEN_TOKEN = "{{";
        public const string ENCODED_TEMPLATE_CLOSE_TOKEN = "}}";

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
                        if (cwb.DataContext is MpContentItemViewModel civm) {
                            if (isReadOnly) {
                                MpConsole.WriteLine($"Tile '{civm.Parent.HeadItem.CopyItemTitle}' is readonly");
                                var readOnlyResult = await cwb.EvaluateScriptAsync("enableReadOnly()");
                                if (readOnlyResult.Result != null && readOnlyResult.Result is string resultStr) {
                                    var qrm = JsonConvert.DeserializeObject<MpQuillEnableReadOnlyResponseMessage>(resultStr);

                                    civm.CopyItemData = MpHtmlToRtfConverter.ConvertHtmlToRtf(qrm.itemEncodedHtmlData);

                                    MpMasterTemplateModelCollection.Update(qrm.updatedAllAvailableTextTemplates,qrm.removedGuids).FireAndForgetSafeAsync(civm);
                                }

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
                                MpConsole.WriteLine($"Tile '{civm.Parent.HeadItem.CopyItemTitle}' is editable");

                                MpQuillDisableReadOnlyRequestMessage drorMsg = new MpQuillDisableReadOnlyRequestMessage() {
                                    allAvailableTextTemplates = MpMasterTemplateModelCollection.AllTemplates.ToList(),
                                    editorHeight = cwb.GetVisualAncestor<MpContentItemView>().ActualHeight
                                };

                                string drorMsgStr = JsonConvert.SerializeObject(drorMsg);
                                string drorMsgStr64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(drorMsgStr));
                                await cwb.EvaluateScriptAsync($"disableReadOnly('{drorMsgStr64}')");

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
                MpQuillLoadRequestMessage lrm = new MpQuillLoadRequestMessage() {
                    envName = "wpf",
                    guidOpenTag = ENCODED_TEMPLATE_OPEN_TOKEN,
                    guidCloseTag = ENCODED_TEMPLATE_CLOSE_TOKEN
                };

                string itemData = e.NewValue != null ? (string)e.NewValue : string.Empty;
                lrm.itemEncodedHtmlData = string.Empty;
                if (!string.IsNullOrEmpty((string)e.NewValue)) {
                    if (itemData.IsStringRichText()) {
                        lrm.itemEncodedHtmlData = MpRtfToHtmlConverter.ConvertRtfToHtml(itemData);
                    }
                }
                string[] itemTemplateGuids = GetTextTemplateGuids(lrm.itemEncodedHtmlData).Select(x => x.ToLower()).ToArray();

                lrm.usedTextTemplates = MpMasterTemplateModelCollection.AllTemplates
                                        .Where(x => itemTemplateGuids.Contains(x.Guid.ToLower()))
                                        .ToList();

                var fe = (FrameworkElement)obj;

                if (fe.DataContext is MpContentItemViewModel civm) {
                    lrm.isPasteRequest = MpClipTrayViewModel.Instance.IsPasting;
                    lrm.isReadOnlyEnabled = civm.IsReadOnly;

                    string lrmJsonStr = JsonConvert.SerializeObject(lrm);
                    string lrmJsonStr64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(lrmJsonStr));

                        var fd = itemData.ToFlowDocument(out Size docSize);
                        civm.UnformattedContentSize = docSize;

                        if (fe is ChromiumWebBrowser cwb) {
                            cwb.FrameLoadEnd += async (sender, args) => {
                                if (args.Frame.IsMain) { 
                                    //var test = await cwb.EvaluateScriptAsync(string.Format(@"reqMsgStr='{0}';", lrmJsonStr64));
                                    var initCmd = $"init('{lrmJsonStr64}')";
                                    var result = await cwb.EvaluateScriptAsync(initCmd);
                                }
                            };
                        }
                    }
                }
            });

        #endregion

        public static void Init() {
            InitCef();
        }

        private static void InitCef() {
            //var settings = new CefSettings();

            //// Increase the log severity so CEF outputs detailed information, useful for debugging
            //settings.LogSeverity = LogSeverity.Verbose;
            //// By default CEF uses an in memory cache, to save cached data e.g. to persist cookies you need to specify a cache path
            //// NOTE: The executing user must have sufficient privileges to write to this folder.
            //settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");

            //Cef.Initialize(settings);

            //To support High DPI this must be before CefSharp.BrowserSubprocess.SelfHost.Main so the BrowserSubprocess is DPI Aware
            Cef.EnableHighDPISupport();

            var exitCode = CefSharp.BrowserSubprocess.SelfHost.Main(new string[] { });

            if (exitCode >= 0) {
                return;
            }

            var settings = new CefSettings() {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                //CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                //BrowserSubprocessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName
            };
            settings.RegisterScheme(new CefCustomScheme {
                SchemeName = "localfolder",
                DomainName = "cefsharp",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(
                    rootFolder: Path.Combine(Environment.CurrentDirectory, "Resources/Html/Editor"),
                    hostName: "cefsharp",
                    defaultPage: "Editor2.html" // will default to index.html
                )
            });
            Cef.Initialize(settings, performDependencyCheck: false);
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