using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
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

        public MpAvPlainHtmlConverterWebView ConverterWebView { get; private set; }

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            if (!MpPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                return;
            }
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
                ConverterWebView.AttachedToVisualTree += (s, e) => {
                    if (OperatingSystem.IsWindows()) {
                        // hide converter window from windows alt-tab menu
                        MpAvToolWindow_Win32.InitToolWindow(quillWindow.TryGetPlatformHandle().Handle);
                    }
                    quillWindow.Hide();
                    quillWindow.WindowState = WindowState.Minimized;
                };
                quillWindow.Show();

                while (!ConverterWebView.IsEditorInitialized) {
                    MpConsole.WriteLine("[loader] waiting for html converter init...");
                    await Task.Delay(100);
                }
                MpConsole.WriteLine("Html converter initialized");
            } else if (App.MainView is MpAvMainView mv) {
                ConverterWebView.AttachedToLogicalTree += (s, e) => {

                    ConverterWebView.IsVisible = false;
                };
                mv.RootGrid.Children.Add(ConverterWebView);
            }

            IsBusy = false;
        }
        public async Task<MpAvHtmlClipboardData> ParseAsync(
            string inputStr,
            string inputFormatType,
            MpCsvFormatProperties csvProps = null) {
            string htmlDataStr = inputStr;
            if (htmlDataStr == null) {
                return null;
            }
            if (!MpPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                var result = MpAvHtmlClipboardData.Parse(htmlDataStr.ToString());
                return result;
            }

            if (ConverterWebView == null) {
                MpConsole.WriteLine("Cannot parse html. Waiting for Html converter to load...");
                while (ConverterWebView == null) {
                    // should only occur when creating test data from db init
                    await Task.Delay(100);
                }
            }
            if (!ConverterWebView.IsEditorInitialized) {
                MpConsole.WriteLine("Cannot parse html. Waiting for Html converter to initialize...");
                while (!ConverterWebView.IsEditorInitialized) {
                    MpConsole.WriteLine("[parser] waiting...");
                    await Task.Delay(100);
                }
                MpConsole.WriteLine("Html converter initialized");
            }
            if (inputFormatType == "rtf") {
                // create 'dirty' quill html w/ internal converter and treat as plain for quill to parse
                htmlDataStr = htmlDataStr.ToRichHtmlText(MpPortableDataFormats.WinRtf);
                inputFormatType = "rtf2html";
            } else if (inputFormatType == "csv") {
                htmlDataStr = htmlDataStr.CsvStrToRichHtmlTable(csvProps);
                inputFormatType = "html";
            } else if (inputFormatType == "text") {
                htmlDataStr = htmlDataStr.Replace(Environment.NewLine, "\n");
            }
            htmlDataStr = htmlDataStr.ToString().ToBase64String();

            if (string.IsNullOrWhiteSpace(htmlDataStr)) {
                MpConsole.WriteTraceLine("Error parsing html data obj, no data found");
                //Debugger.Break();
                return null;
            }

            var req = new MpQuillConvertPlainHtmlToQuillHtmlRequestMessage() {
                data = htmlDataStr,
                dataFormatType = inputFormatType,
                isBase64 = true
            };
            //string respStr = await ConverterWebView.SendMessageAsync($"convertPlainHtml_ext_ntf('{req.SerializeJsonObjectToBase64()}')");
            //var resp = MpJsonConverter.DeserializeBase64Object<MpQuillConvertPlainHtmlToQuillHtmlResponseMessage>(respStr);

            ConverterWebView.LastPlainHtmlResp = null;
            ConverterWebView.SendMessage($"convertPlainHtml_ext_ntf('{req.SerializeJsonObjectToBase64()}')");
            while (ConverterWebView.LastPlainHtmlResp == null) {
                await Task.Delay(100);
            }
            MpQuillConvertPlainHtmlToQuillHtmlResponseMessage resp = ConverterWebView.LastPlainHtmlResp;
            ConverterWebView.LastPlainHtmlResp = null;

            return new MpAvHtmlClipboardData() {
                Html = resp.html.ToStringFromBase64(),
                RichHtml = resp.quillHtml.ToStringFromBase64(),
                Delta = resp.quillDelta.ToStringFromBase64(),
                SourceUrl = resp.sourceUrl
            };
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands

        public ICommand ShowConverterDevTools => new MpCommand(
            () => {
                ConverterWebView.ShowDevTools();
            });

        #endregion
    }
}
