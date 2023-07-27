using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlainHtmlConverter :
        MpIAsyncObject,
        MpIAsyncCollectionObject {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvPlainHtmlConverter _instance;
        public static MpAvPlainHtmlConverter Instance => _instance ?? (_instance = new MpAvPlainHtmlConverter());
        #endregion

        #region MpIAsyncObject Implementation

        public bool IsBusy { get; private set; } = false;
        #endregion

        #region MpIAsyncCollectionObject Implementation

        bool MpIAsyncCollectionObject.IsAnyBusy => IsBusy;
        #endregion

        #region Properties

        #region State

        public bool IsWebViewConverterEnabled =>
            MpAvCefNetApplication.IsCefNetLoaded;

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
                MpConsole.WriteLine("Plain Html converter has no webview for conversion. Using fallback.");
            }
        }

        public async Task<MpAvRichHtmlConvertResult> ConvertAsync(
            string inputStr,
            string inputFormatType,
            MpCsvFormatProperties csvProps = null) {
            string htmlDataStr = inputStr;
            if (htmlDataStr == null) {
                return null;
            }
            if (inputFormatType == "rtf") {
                // create 'dirty' quill html w/ internal converter and treat as plain for quill to parse
                htmlDataStr = htmlDataStr.ToRichHtmlText(MpPortableDataFormats.WinRtf);
                inputFormatType = "rtf2html";
            }
            MpAvRichHtmlConvertResult result;
            if (IsWebViewConverterAvailable) {
                result = await ConvertWithWebViewAsync(inputFormatType, htmlDataStr, csvProps);
            } else {
                result = ConvertWithFallback(htmlDataStr);
            }
            return result;
        }

        #endregion

        #region Private Methods
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
                var sw = Stopwatch.StartNew();
                var quillWindow = new MpAvHiddenWindow();

                quillWindow.Content = ConverterWebView;
                ConverterWebView.AttachedToVisualTree += async (s, e) => {
                    if (OperatingSystem.IsWindows()) {
                        // hide converter window from windows alt-tab menu
                        MpAvToolWindow_Win32.InitToolWindow(quillWindow.TryGetPlatformHandle().Handle);
                    }

                    while (!ConverterWebView.IsEditorInitialized) {
                        MpConsole.WriteLine("[loader] waiting for html converter init...");
                        await Task.Delay(100);
                    }
                    quillWindow.Hide();
                    quillWindow.WindowState = WindowState.Minimized;
                    sw.Stop();
                    MpConsole.WriteLine($"Html converter initialized. Load time: {sw.ElapsedMilliseconds}ms");
                };
                quillWindow.Show();
            } else if (App.MainView is MpAvMainView mv) {
                ConverterWebView.AttachedToLogicalTree += (s, e) => {
                    ConverterWebView.IsVisible = false;
                };
                mv.RootGrid.Children.Add(ConverterWebView);
            }

            IsBusy = false;
        }

        private MpAvRichHtmlConvertResult ConvertWithFallback(string htmlDataStr) {
            var result = MpAvRichHtmlConvertResult.Parse(htmlDataStr.ToString());
            return result;
        }

        private async Task<MpAvRichHtmlConvertResult> ConvertWithWebViewAsync(
            string inputFormatType,
            string htmlDataStr,
            MpCsvFormatProperties csvProps = null) {
            if (!IsWebViewConverterAvailable) {
                MpDebug.Break($"Convert from webview called before available");
                return ConvertWithFallback(htmlDataStr);
            }
            if (inputFormatType == "csv") {
                htmlDataStr = htmlDataStr.CsvStrToRichHtmlTable(csvProps);
                inputFormatType = "html";
            } else if (inputFormatType == "text") {
                htmlDataStr = htmlDataStr.Replace(Environment.NewLine, "\n");
            }
            htmlDataStr = htmlDataStr.ToString().ToBase64String();

            if (string.IsNullOrWhiteSpace(htmlDataStr)) {
                MpConsole.WriteTraceLine("Error parsing html data obj, no data found");
                return null;
            }

            var req = new MpQuillConvertPlainHtmlToQuillHtmlRequestMessage() {
                data = htmlDataStr,
                dataFormatType = inputFormatType,
                isBase64 = true
            };

            ConverterWebView.LastPlainHtmlResp = null;
            ConverterWebView.SendMessage($"convertPlainHtml_ext_ntf('{req.SerializeJsonObjectToBase64()}')");
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
            if (resp.success) {
                return new MpAvRichHtmlConvertResult() {
                    InputHtml = resp.html.ToStringFromBase64(),
                    RichHtml = resp.quillHtml.ToStringFromBase64(),
                    Delta = resp.quillDelta.ToStringFromBase64(),
                    SourceUrl = resp.sourceUrl
                };
            }
            return null;
        }
        #endregion

        #region Commands

        public ICommand ShowConverterDevTools => new MpCommand(
            () => {
                ConverterWebView.ShowDevTools();
            });

        #endregion
    }
}
