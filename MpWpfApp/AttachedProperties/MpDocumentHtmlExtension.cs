using Azure;
using CefSharp;
using CefSharp.JavascriptBinding;
using CefSharp.SchemeHandler;
using CefSharp.Wpf;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using MpWpfApp.Properties;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public interface MpIProcessWatcher {

    }
    public class MpDocumentHtmlExtension : DependencyObject {
        #region Private Variables

        #endregion
        public const string ENCODED_TEMPLATE_OPEN_TOKEN = MpTextTemplate.TextTemplateOpenToken;
        public const string ENCODED_TEMPLATE_CLOSE_TOKEN = MpTextTemplate.TextTemplateCloseToken;

        public static string ENCODED_TEMPLATE_REGEXP_STR = string.Format(
            @"{0}{1}{2}",
            ENCODED_TEMPLATE_OPEN_TOKEN,
            ".*?",
            ENCODED_TEMPLATE_CLOSE_TOKEN);

        private static Regex _encodedTemplateRegEx;


        #region ReadOnlyWidth

        public static double GetReadOnlyWidth(DependencyObject obj) {
            return (double)obj.GetValue(ReadOnlyWidthProperty);
        }
        public static void SetReadOnlyWidth(DependencyObject obj, double value) {
            obj.SetValue(ReadOnlyWidthProperty, value);
        }
        public static readonly DependencyProperty ReadOnlyWidthProperty =
          DependencyProperty.RegisterAttached(
            "ReadOnlyWidth",
            typeof(double),
            typeof(MpDocumentHtmlExtension),
            new FrameworkPropertyMetadata(MpClipTileViewModel.DefaultBorderWidth));

        #endregion

        #region IsSelected

        public static bool GetIsSelected(DependencyObject obj) {
            return (bool)obj.GetValue(IsSelectedProperty);
        }
        public static void SetIsSelected(DependencyObject obj, bool value) {
            obj.SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(MpDocumentHtmlExtension),
            new FrameworkPropertyMetadata(false));

        #endregion

        #region IsContentFocused

        public static bool GetIsContentFocused(DependencyObject obj) {
            return (bool)obj.GetValue(IsContentFocusedProperty);
        }
        public static void SetIsContentFocused(DependencyObject obj, bool value) {
            obj.SetValue(IsContentFocusedProperty, value);
        }
        public static readonly DependencyProperty IsContentFocusedProperty =
          DependencyProperty.RegisterAttached(
            "IsContentFocused",
            typeof(bool),
            typeof(MpDocumentHtmlExtension),
            new FrameworkPropertyMetadata() {
                PropertyChangedCallback = async (s, e) => {
                    if (e.NewValue == null) {
                        return;
                    }
                    var fe = s as FrameworkElement;
                    bool isContentFocused = (bool)e.NewValue;
                    if (isContentFocused && !GetIsContentReadOnly(fe)) {
                        SetIsSelected(fe, true);
                        if (fe is ChromiumWebBrowser cwb) {
                            if (cwb.CanExecuteJavascriptInMainFrame) {
                                await cwb.EvaluateScriptAsync("document.getElementsByTagName('textarea')[0].focus();");
                                //await cwb.EvaluateScriptAsync("focusEditor();");
                            }
                        }
                    }
                }
            });

        #endregion

        #region IsContentReadOnly

        public static bool GetIsContentReadOnly(DependencyObject obj) {
            return (bool)obj.GetValue(IsContentReadOnlyProperty);
        }
        public static void SetIsContentReadOnly(DependencyObject obj, bool value) {
            obj.SetValue(IsContentReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsContentReadOnlyProperty =
          DependencyProperty.RegisterAttached(
            "IsContentReadOnly",
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
                            cwb.JavascriptObjectRepository.ResolveObject -= JavascriptObjectRepository_ResolveObject;
                            cwb.JavascriptObjectRepository.ObjectBoundInJavascript -= JavascriptObjectRepository_ObjectBoundInJavascript;

                            var enableReadOnlyResp = await cwb.EvaluateScriptAsync("enableReadOnly()");
                            ProcessEnableReadOnlyResponse(fe, enableReadOnlyResp);
                        } else {
                            cwb.JavascriptObjectRepository.ResolveObject += JavascriptObjectRepository_ResolveObject;
                            cwb.JavascriptObjectRepository.ObjectBoundInJavascript += JavascriptObjectRepository_ObjectBoundInJavascript;

                            MpQuillDisableReadOnlyRequestMessage drorMsg = CreateDisableReadOnlyMessage(fe);
                            var disableReadOnlyResponse = await cwb.EvaluateScriptAsync(null,$"disableReadOnly", drorMsg.SerializeJsonObject());
                            ProcessDisableReadOnlyResponse(fe, disableReadOnlyResponse);

                            SetIsContentFocused(fe, true);
                        }
                    }
                }
            });

        private static void JavascriptObjectRepository_ObjectBoundInJavascript(object sender, CefSharp.Event.JavascriptBindingCompleteEventArgs e) {
            Debug.WriteLine($"Object {e.ObjectName} was bound successfully.");
        }

        private static void JavascriptObjectRepository_ResolveObject(object sender, CefSharp.Event.JavascriptBindingEventArgs e) {
            var repo = e.ObjectRepository;
            if (e.ObjectName == "jsComAdapter") {
                repo.NameConverter = new CamelCaseJavascriptNameConverter();
                repo.Register("jsComAdapter", new MpJsComAdapter(), isAsync: true);
            }
        }

        #endregion

        #region DocumentHtml

        public static string GetDocumentHtml(DependencyObject obj) {
            return (string)obj.GetValue(DocumentHtmlProperty);
        }
        public static void SetDocumentHtml(DependencyObject obj, string value) {
            obj.SetValue(DocumentHtmlProperty, value);
        }
        public static readonly DependencyProperty DocumentHtmlProperty =
          DependencyProperty.RegisterAttached(
            "DocumentHtml",
            typeof(string),
            typeof(MpDocumentHtmlExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {       

                if (obj is ChromiumWebBrowser cwb) {
                        var lrm = CreateLoadRequestMessage(cwb, e.NewValue);
                        cwb.FrameLoadEnd += async (sender, args) => {
                            if (args.Frame.IsMain) {
                                await cwb.EvaluateScriptAsync(null, "init", lrm);
                            }
                        };
                    }
                }
            });

        #endregion

        #region Messages

        public static MpQuillLoadContentRequestMessage CreateConvertStandardHtmlMessage(string standardHtml) {
            return new MpQuillLoadContentRequestMessage() {
                envName = "wpf",
                itemData = standardHtml
            };
        }

        public static MpQuillLoadContentRequestMessage CreateLoadRequestMessage(FrameworkElement fe, object newValue) {
            if (fe.DataContext is MpClipTileViewModel ctvm) {

                MpConsole.WriteLine($"Tile Content Item '{(fe.DataContext as MpClipTileViewModel).CopyItemTitle}' is loaded");

                string itemData = newValue == null ? string.Empty : (string)newValue;

                return new MpQuillLoadContentRequestMessage() {
                    envName = "wpf",
                    itemData = GetEncodedHtml(itemData,ctvm.CopyItemGuid),
                    usedTextTemplates = GetTextTemplates(itemData),
                    isPasteRequest = ctvm.IsPasting,
                    isReadOnlyEnabled = ctvm.IsContentReadOnly
                };
            }
            return null;
        }

        public static string ProcessEnableReadOnlyResponse(FrameworkElement fe, JavascriptResponse enableReadOnlyResponse) {
            if (fe.DataContext is MpClipTileViewModel ctvm) {
                if (enableReadOnlyResponse.Result != null && enableReadOnlyResponse.Result is string resultStr) {
                    MpConsole.WriteLine($"Tile content item '{ctvm.CopyItemTitle}' is readonly");

                    var qrm = JsonConvert.DeserializeObject<MpQuillEnableReadOnlyResponseMessage>(resultStr);//.ToStringFromBase64());

                    //civm.CopyItemData = qrm.itemEncodedHtmlData;
                    MpConsole.WriteLine("Skipping writing updated item data: ");
                    MpConsole.WriteLine(qrm.itemData);

                    var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
                    if (ctcv != null) {
                        ctcv.TileResizeBehavior.ResizeWidth(GetReadOnlyWidth(fe));
                    }

                    MpMasterTemplateModelCollectionViewModel.Instance.UpdateAsync(qrm.updatedAllAvailableTextTemplates, qrm.userDeletedTemplateGuids).FireAndForgetSafeAsync(ctvm);
                    
                    return qrm.itemData;
                }
            }
            return null;
        }

        private static MpQuillDisableReadOnlyRequestMessage CreateDisableReadOnlyMessage(FrameworkElement fe) {
            var ctvm = fe.DataContext as MpClipTileViewModel;
            MpConsole.WriteLine($"Tile content item '{ctvm.CopyItemTitle}' is editable");

            MpQuillDisableReadOnlyRequestMessage drorMsg = new MpQuillDisableReadOnlyRequestMessage() {
                allAvailableTextTemplates = MpMasterTemplateModelCollectionViewModel.Instance.AllTemplates.ToList(),
                editorHeight = ctvm.EditorHeight//fe.GetVisualAncestor<MpRtbContentView>().ActualHeight
            };
            return drorMsg;
        }

        private static void ProcessDisableReadOnlyResponse(FrameworkElement fe, JavascriptResponse disableReadOnlyResponse) {
            if (fe.DataContext is MpClipTileViewModel civm) {
                if (disableReadOnlyResponse.Result != null && disableReadOnlyResponse.Result is string resultStr) {
                    MpConsole.WriteLine($"Tile content item '{civm.CopyItemTitle}' is editable");

                    var qrm = JsonConvert.DeserializeObject<MpQuillDisableReadOnlyResponseMessage>(resultStr);

                    var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
                    if (ctcv != null) {
                        SetReadOnlyWidth(fe, ctcv.ActualWidth);

                        if (ctcv.ActualWidth < 900) {
                            ctcv.TileResizeBehavior.ResizeWidth(900);// qrm.editorWidth - ctcv.ActualWidth, 0);
                        }
                    } else {
                        SetReadOnlyWidth(fe,  MpClipTileViewModel.DefaultBorderWidth);
                    }
                }
            }
        }
        #endregion

        private static string GetEncodedHtml(string itemData, string itemGuid) {
            //if (itemData.IsStringRichText()) {
            //    return MpRtfToHtmlConverter.ConvertRtfToHtml(
            //        itemData,
            //        new Dictionary<string, string>() { { "copyItemBlockGuid",itemGuid } },
            //        new Dictionary<string, string>() { { "copyItemInlineGuid", itemGuid } });
            //}
            return itemData;
        }

        private static List<MpTextTemplate> GetTextTemplates(string itemData) {
            string[] itemTemplateGuids = GetTextTemplateGuids(itemData).Select(x => x.ToLower()).ToArray();

            return MpMasterTemplateModelCollectionViewModel.Instance.AllTemplates
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

        public static void Init() {
            //called in bootstrap
            InitCef();
        }

        private static void InitCef() {
#if ANYCPU
            //Only required for PlatformTarget of AnyCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
#endif
            //To support High DPI this must be before CefSharp.BrowserSubprocess.SelfHost.Main so the BrowserSubprocess is DPI Aware
            Cef.EnableHighDPISupport();

            var exitCode = CefSharp.BrowserSubprocess.SelfHost.Main(new string[] { });

            if (exitCode >= 0) {
                return;
            }

            CefSharpSettings.ConcurrentTaskExecution = true;

            var settings = new CefSettings() {
                LogSeverity = LogSeverity.Fatal
            };
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");
            settings.CefCommandLineArgs.Add(@"--disable-component-update");
            settings.RemoteDebuggingPort = 8080;
            //settings.ExternalMessagePump = true;
            //NOTE: WebRTC Device Id's aren't persisted as they are in Chrome see https://bitbucket.org/chromiumembedded/cef/issues/2064/persist-webrtc-deviceids-across-restart
            //settings.CefCommandLineArgs.Add("enable-media-stream");
            //https://peter.sh/experiments/chromium-command-line-switches/#use-fake-ui-for-media-stream
            //settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            //For screen sharing add (see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access#comment-58677180)
            //settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");

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

    }
}