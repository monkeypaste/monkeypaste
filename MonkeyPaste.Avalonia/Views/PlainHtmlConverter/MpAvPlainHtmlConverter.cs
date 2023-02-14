using Avalonia.Controls;
using Avalonia.Layout;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlainHtmlConverter : MpIBootstrappedItem, MpIAsyncObject {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvPlainHtmlConverter _instance;
        public static MpAvPlainHtmlConverter Instance => _instance ?? (_instance = new MpAvPlainHtmlConverter());
        #endregion

        #region MpIAsyncBootstrappedItem Implementation

        public string Label => "Web Converter";
        public bool IsBusy { get; private set; } = false;
        #endregion

        #region Properties

        public MpAvCefNetPlainHtmlConverterWebView ConverterWebView { get; private set; }

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            if (!MpAvCefNetApplication.UseCefNet) {
                return;
            }
            IsBusy = true;

            ConverterWebView = new MpAvCefNetPlainHtmlConverterWebView() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var quillWindow = new Window() {
                Width = 300,
                Height = 300,
                ShowInTaskbar = false,
                SystemDecorations = SystemDecorations.None,
                //Position = new PixelPoint(808080, 808080)
            };

            quillWindow.Content = ConverterWebView;
            ConverterWebView.AttachedToVisualTree += (s, e) => {
                if (OperatingSystem.IsWindows()) {
                    // hide converter window from windows alt-tab menu
                    MpAvToolWindow_Win32.InitToolWindow(quillWindow.PlatformImpl.Handle.Handle);
                }
                quillWindow.Hide();
            };
            quillWindow.Show();

            MpConsole.WriteLine("Waiting for Html converter to initialize...");
            while (!ConverterWebView.IsEditorInitialized) {
                await Task.Delay(100);
            }
            MpConsole.WriteLine("Html converter initialized");


            IsBusy = false;
        }
        public async Task<MpAvHtmlClipboardData> ParseAsync(string inputStr, string inputFormatType, MpCsvFormatProperties csvProps = null) {
            string htmlDataStr = inputStr;
            if (htmlDataStr == null) {
                return null;
            }
            if (!MpAvCefNetApplication.UseCefNet) {
                return MpAvHtmlClipboardData.Parse(htmlDataStr.ToString());
            }
            if (inputFormatType == "rtf") {
                // create 'dirty' quill html w/ internal converter and treat as plain for quill to parse
                htmlDataStr = htmlDataStr.ToRichHtmlText(MpPortableDataFormats.AvRtf_bytes);
                inputFormatType = "html";
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
            string respStr = await ConverterWebView.EvaluateJavascriptAsync($"convertPlainHtml_ext('{req.SerializeJsonObjectToBase64()}')");
            var resp = MpJsonConverter.DeserializeBase64Object<MpQuillConvertPlainHtmlToQuillHtmlResponseMessage>(respStr);
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
