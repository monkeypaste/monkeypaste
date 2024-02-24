using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.VisualTree;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlainHtmlConverter :
        MpIUserAgentProvider,
        MpIAsyncObject,
        MpIAsyncCollectionObject {
        #region Private Variables

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
            ConverterWebView == null ||
            string.IsNullOrEmpty(ConverterWebView.UserAgent) ?
                _userAgent :
                ConverterWebView.UserAgent;
        #endregion

        #region MpIAsyncObject Implementation

        public bool IsBusy { get; private set; } = false;
        #endregion

        #region MpIAsyncCollectionObject Implementation

        bool MpIAsyncCollectionObject.IsAnyBusy => IsBusy;
        #endregion

        #region Properties

        #region State

        public bool IsLoaded { get; set; } =
#if CEFNET_WV || OUTSYS_WV || MOBILE
            false;
#else
            true;
#endif
        public bool IsWebViewConverterEnabled =>
#if CEFNET_WV
            MpAvCefNetApplication.IsCefNetLoaded;
#elif OUTSYS_WV
            true;
#else
            true;
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
        private async Task CreateWebViewConverterAsync() {
            IsBusy = true;
            if (OperatingSystem.IsBrowser()) {
                await MpDeviceWrapper.Instance.JsImporter.ImportAllAsync();
                if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime mobile
                        && mobile.MainView != null) {
                    Mp.Services.ScreenInfoCollection = new MpAvScreenInfoCollectionBase(new[] { new MpAvDesktopScreenInfo(mobile.MainView.GetVisualRoot().AsScreen()) });
                }
            }

            ConverterWebView = new MpAvPlainHtmlConverterWebView() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };


            if (Mp.Services.PlatformInfo.IsDesktop) {
                var quillWindow = new MpAvHiddenWindow();

                quillWindow.Content = ConverterWebView;
                ConverterWebView.AttachedToVisualTree += async (s, e) => {
                    var sw = Stopwatch.StartNew();
                    if (OperatingSystem.IsWindows()) {
                        // hide converter window from windows alt-tab menu
                        MpAvToolWindow_Win32.SetAsToolWindow(quillWindow.TryGetPlatformHandle().Handle);
                    }

                    while (!ConverterWebView.IsEditorInitialized) {
                        if (sw.Elapsed > TimeSpan.FromSeconds(60)) {
                            // TODO should fallback here 
                        }
                        MpConsole.WriteLine("[loader] waiting for html converter init...");
                        await Task.Delay(100);
                    }
                    quillWindow.Hide();
                    quillWindow.WindowState = WindowState.Minimized;
                    sw.Stop();
                    MpConsole.WriteLine($"Html converter initialized. Load time: {sw.ElapsedMilliseconds}ms");
                    IsLoaded = true;
                };
                quillWindow.Show();
            } else if (App.PrimaryView is MpAvMainView mv) {
                ConverterWebView.AttachedToLogicalTree += (s, e) => {
                    ConverterWebView.IsVisible = false;
                };
                mv.RootGrid.Children.Add(ConverterWebView);
            }

            IsBusy = false;
        }

        private MpRichHtmlContentConverterResult ConvertWithFallback(string htmlDataStr, string verifyText) {
            var result = MpRichHtmlContentConverterResult.Parse(htmlDataStr.ToString());
            return result;
        }

        private async Task<MpRichHtmlContentConverterResult> ConvertWithWebViewAsync(
            MpDataFormatType inputFormatType,
            string htmlDataStr,
            string verifyPlainText,
            MpCsvFormatProperties csvProps = null) {
            if (!IsWebViewConverterAvailable) {
                MpDebug.Break($"Convert from webview called before available");
                return ConvertWithFallback(htmlDataStr, verifyPlainText);
            }
            htmlDataStr = htmlDataStr.ToString().ToBase64String();

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
                OutputData = resp.quillHtml.ToStringFromBase64(),
                Delta = resp.quillDelta.ToStringFromBase64(),
                SourceUrl = resp.sourceUrl
            };
        }
        #endregion

        #region Commands

        public ICommand ShowConverterDevTools => new MpCommand(
            () => {
                ConverterWebView.OpenDevTools();
            });

        #endregion
    }
}
