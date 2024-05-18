using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlainHtmlConverter :
        MpIUserAgentProvider,
        MpIAsyncObject,
        MpIAsyncCollectionObject {
        #region Private Variables
        private Window _convWindow;
        private bool do_hide = true;
        #endregion

        #region Constants
        // NOTE needs to match converter.js version
        public const string CONVERTER_CONTENT_HANDLE = "[CONVERTER]";
        #endregion

        #region Statics

        private static MpAvPlainHtmlConverter _instance;
        public static MpAvPlainHtmlConverter Instance => _instance ?? (_instance = new MpAvPlainHtmlConverter());
        #endregion

        #region MpIUserAgentProvider Implementation

        private string _userAgent = MpFileIo.GetDefaultUserAgent();
        public string UserAgent =>
            //ConverterWebView == null ||
            //string.IsNullOrEmpty(ConverterWebView.UserAgent) ?
            //    _userAgent :
            //    ConverterWebView.UserAgent;
            _userAgent;
        #endregion

        #region MpIAsyncObject Implementation

        public bool IsBusy { get; set; } = false;
        #endregion

        #region MpIAsyncCollectionObject Implementation

        bool MpIAsyncCollectionObject.IsAnyBusy => IsBusy;
        #endregion

        #region Properties

        #region State

        public bool IsLoaded { get; set; } =
#if SUGAR_WV || CEFNET_WV || OUTSYS_WV 
            false;
#else
            true;
#endif
        public bool IsWebViewConverterEnabled =>
#if CEFNET_WV
            MpAvCefNetApplication.IsCefNetLoaded;
#elif OUTSYS_WV
            true;
#elif SUGAR_WV && ANDROID 
            false;
#elif SUGAR_WV
            true;
#else
            false;
#endif

        bool IsWebViewConverterAvailable =>
            IsWebViewConverterEnabled &&
            ConverterWebView != null &&
            ConverterWebView.IsEditorInitialized;

        #endregion
        public MpAvPlainHtmlConverterWebView ConverterWebView { get; set; }

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            if (IsWebViewConverterEnabled) {
                await CreateWebViewConverterAsync();
            } else {
                IsLoaded = true;
                MpConsole.WriteLine("Plain Html converter has no webview for conversion. Using fallback.");
            }
        }

        public async Task<MpRichHtmlContentConverterResult> ConvertAsync(
            string inputStr,
            //string inputFormatType,
            MpDataFormatType inputFormatType,
            string verifyText = null,
            MpCsvFormatProperties csvProps = null) {
            string htmlDataStr = inputStr;
            if (htmlDataStr == null) {
                return null;
            }
            MpRichHtmlContentConverterResult result;
            if (IsWebViewConverterAvailable) {
                result = await ConvertWithWebViewAsync(inputFormatType, htmlDataStr, verifyText, csvProps);
            } else {
                result = ConvertWithFallback(htmlDataStr, verifyText);
            }


            return await FinishHtmlConversionAsync(result, verifyText);
        }

        #endregion

        #region Private Methods

        private async Task CreateWebViewConverterAsync() {
            IsBusy = true;
            if (OperatingSystem.IsBrowser()) {
                await MpAvDeviceWrapper.Instance.JsImporter.ImportAllAsync();
                if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime mobile
                        && mobile.MainView != null) {
                    Mp.Services.ScreenInfoCollection = new MpAvScreenInfoCollectionBase(new[] { new MpAvDesktopScreenInfo(mobile.MainView.GetVisualRoot().AsScreen()) });
                }
            }

            ConverterWebView = new MpAvPlainHtmlConverterWebView() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            ConverterWebView.AttachedToVisualTree += ConverterWebView_AttachedToVisualTree;


            if (Mp.Services.PlatformInfo.IsDesktop) {
                if(do_hide) {
                    _convWindow = new MpAvHiddenWindow();
                    _convWindow.Width = 1;
                    _convWindow.Height = 1;
                } else {
                    _convWindow = new Window(); 
                    _convWindow.Width = 50;
                    _convWindow.Height = 50;
                }
                if (OperatingSystem.IsWindows()) {
                    // hide converter window from windows alt-tab menu
                    MpAvToolWindow_Win32.SetAsToolWindow(_convWindow.TryGetPlatformHandle().Handle);
                }
                _convWindow.Title = "Hidden converter window".ToWindowTitleText();
                _convWindow.Content = ConverterWebView;
                _convWindow.Show();
            } else {
                // mobile

                Dispatcher.UIThread.Post(async () => {
                    // NOTE need to ntf loaded or mv won't be created
                    MpAvMainView mv = null;
                    while (true) {
                        mv = MpAvMainView.Instance;
                        if (mv != null) {
                            break;
                        }
                        await Task.Delay(100);
                    }
                    mv.RootGrid.Children.Add(ConverterWebView);
                });
            }

            IsBusy = false;
        }

        private async void ConverterWebView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var sw = Stopwatch.StartNew();

            while (!ConverterWebView.IsEditorInitialized) {
                if (sw.Elapsed > TimeSpan.FromSeconds(60)) {
                    // TODO should fallback here 
                }
                MpConsole.WriteLine("waiting for html converter init...");
                await Task.Delay(100);
            }
            if (do_hide) {
                if(_convWindow == null) {
                    // mobile
                    ConverterWebView.IsVisible = false;
                    
                } else {
                    _convWindow.Hide();
                    _convWindow.WindowState = WindowState.Minimized;
                }
            }
            MpConsole.WriteLine($"Html converter initialized. Load time: {sw.ElapsedMilliseconds}ms");
            IsLoaded = true;
        }


        private async Task<MpRichHtmlContentConverterResult> ConvertWithWebViewAsync(
            MpDataFormatType inputFormatType,
            string htmlDataStr,
            string verifyPlainText,
            MpCsvFormatProperties csvProps = null) {
            htmlDataStr = htmlDataStr.ToBase64String();

            if (string.IsNullOrWhiteSpace(htmlDataStr)) {
                MpConsole.WriteTraceLine("Error parsing html data obj, no data found");
                return null;
            }

            var req = new MpQuillConvertPlainHtmlToQuillHtmlRequestMessage() {
                data = htmlDataStr,
                dataFormatType = inputFormatType.ToString().ToLowerInvariant(),
                verifyText = verifyPlainText,
                isBase64 = true
            };

            ConverterWebView.LastPlainHtmlResp = null;
            ConverterWebView.SendMessage($"convertPlainHtml_ext_ntf('{req.SerializeObjectToBase64()}')");
            var sw = Stopwatch.StartNew();
            while (ConverterWebView.LastPlainHtmlResp == null) {
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds >= MpAvClipTrayViewModel.ADD_CONTENT_TIMEOUT_MS) {
                    // shouldn't happen, check converter dev tool console for errors..
                    MpConsole.WriteLine($"Error! Html converter timed out. Cannot convert");
                    return null;
                }
            }
            MpQuillConvertPlainHtmlToQuillHtmlResponseMessage resp = ConverterWebView.LastPlainHtmlResp;
            ConverterWebView.LastPlainHtmlResp = null;
            MpConsole.WriteLine($"{(resp.success ? "[SUCCESS]" : "[FAILED]")}Content Conversion Complete. Total Time {sw.ElapsedMilliseconds}ms");

            return new MpRichHtmlContentConverterResult() {
                InputHtml = resp.html.ToStringFromBase64(),
#if SUGAR_WV
                OutputData = resp.themedHtml.ToStringFromBase64(),
#else
                OutputData = resp.quillHtml.ToStringFromBase64(), 
#endif
                Delta = resp.quillDelta.ToStringFromBase64(),
                SourceUrl = resp.sourceUrl
            };
        }
        private async Task<MpRichHtmlContentConverterResult> FinishHtmlConversionAsync(MpRichHtmlContentConverterResult cr, string verifyStr) {
            if (cr == null ||
                string.IsNullOrWhiteSpace(cr.InputHtml) ||
                cr.DeterminedFormat != MpPortableDataFormats.Html) {
                return cr;
            }
            var html_doc = new HtmlDocument();
            html_doc.LoadHtml(cr.InputHtml);
            if (html_doc.DocumentNode != null &&
                html_doc.DocumentNode.FirstChild.Name == "img" &&
                html_doc.DocumentNode.FirstChild.GetAttributeValue("src", string.Empty) is string src_str &&
                !string.IsNullOrWhiteSpace(src_str)) {
                string img_base64 = null;

                if (src_str.ToLowerInvariant().StartsWith("data:image/")) {
                    string src_data_str = src_str.Substring("data:image/".Length);
                    if (src_data_str.ToLowerInvariant().StartsWith("svg")) {
                        MpDebug.Break($"Need to handle svg img src.");
                    } else {
                        img_base64 = src_data_str.Split(",")[1];
                    }
                } else if (Uri.IsWellFormedUriString(src_str, UriKind.Absolute)) {
                    // TODO could fallback to relative uri by combining source url...
                    byte[] img_bytes = await MpFileIo.ReadBytesFromUriAsync(src_str);
                    img_base64 = img_bytes.ToBase64String();
                }
                if (!img_base64.IsStringBase64()) {
                    MpDebug.Assert(img_base64 == null, $"What went wrong parsing img html: '{src_str}'");
                    return cr;
                }
                cr.DeterminedFormat = MpPortableDataFormats.Image;
                cr.OutputData = img_base64;
            }
            return cr;
        }
        private string ApplyTheme(string html) {
            if (html.ToHtmlDocument() is not { } doc ||
                doc.DocumentNode.SelectNodes("//*[@style]") is not { } styled_nodes) {
                return html;
            }
            bool is_dark = MpAvThemeViewModel.Instance.IsThemeDark;
            string fallback_fg = is_dark ?
                Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeWhiteColor.ToString()) :
                Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeBlackColor.ToString());
            foreach (var node in styled_nodes) {
                string style_val = node.GetAttributeValue("style", string.Empty).Trim();

                var style_props =
                    style_val
                    .SplitNoEmpty(";")
                    .Select(x => x.Trim());
                // fg
                if (style_props.FirstOrDefault(x => x.StartsWith("color")) is { } color_prop) {
                    string color_val = color_prop.SplitNoEmpty(":").LastOrDefault().ToStringOrEmpty().Trim();
                    var c = new MpColor(color_val, fallback_fg);
                    MpColorHelpers.ColorToHsl(c, out double h, out double s, out double l);
                    if (is_dark) {
                        l = Math.Max(l == 0 ? 1 : l, 0.75);
                    } else {
                        l = Math.Min(l == 1 ? 0 : 0, 0.25);
                    }
                    var tc = MpColorHelpers.ColorFromHsl(h, s, l);
                    tc.A = 255;
                    string new_color_prop = $"color: {tc.ToHex(true)}";
                    style_val = style_val.Replace(color_prop, new_color_prop);
                }
                // bg
                string new_bg_color_prop = "background-color: transparent";
                string bg_color_prop = style_props.FirstOrDefault(x => x.StartsWith("background-color"));
                if (!bg_color_prop.IsNullOrEmpty()) {
                    style_val = style_val.Replace(bg_color_prop, new_bg_color_prop);
                } else {
                    new_bg_color_prop += ";";
                    if (!style_val.IsNullOrEmpty() && !style_val.EndsWith(";")) {
                        style_val += ";";
                    }
                    style_val += new_bg_color_prop;
                }
                node.SetAttributeValue("style", style_val);
            }
            return doc.DocumentNode.OuterHtml;
        }

        private MpRichHtmlContentConverterResult ConvertWithFallback(string htmlDataStr, string verifyText) {
            var result = MpRichHtmlContentConverterResult.Parse(htmlDataStr.ToString());
            if (result != null && result.DeterminedFormat == MpPortableDataFormats.Html) {
                result.OutputData = ApplyTheme(result.OutputData);
            }
            return result;
        }

        #endregion

        #region Commands

        public ICommand ShowConverterDevTools => new MpCommand(
            () => {
#if SUGAR_WV && MAC
                // NOTE WKWebView doesn't have a 'show dev tools' only right-click context option so 
                // showing the conv window
                if (_convWindow is not MpAvHiddenWindow cw) {
                    return;
                }
                cw.Unhide();
#endif
                ConverterWebView.OpenDevTools();
            });

        #endregion
    }
}
