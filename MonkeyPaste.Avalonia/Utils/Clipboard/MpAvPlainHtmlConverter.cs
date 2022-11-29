using Avalonia.Controls;
using Avalonia.Layout;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlainHtmlConverter  {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvPlainHtmlConverter _instance;
        public static MpAvPlainHtmlConverter Instance => _instance ?? (_instance = new MpAvPlainHtmlConverter());
        #endregion

        #region Properties

        public MpAvCefNetWebView ConverterWebView { get; private set; }

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public void Init() {
            if (!MpAvCefNetApplication.UseCefNet) {
                return;
            }
            ConverterWebView = new MpAvCefNetWebView(MpAvCefNetWebView.HTML_CONVERTER_PARAMS) {
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

            quillWindow.AttachedToVisualTree += (s, e) => {
                if (OperatingSystem.IsWindows()) {
                    // hide converter window from windows alt-tab menu

                    MpAvToolWindow_Win32.InitToolWindow(quillWindow.PlatformImpl.Handle.Handle);
                }
                quillWindow.Hide();
            };
        }
        public async Task<MpAvHtmlClipboardData> ParseAsync(string htmlDataStr, string inputFormatType, MpCsvFormatProperties csvProps = null) {
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
                htmlDataStr = htmlDataStr.ToRichHtmlTable(csvProps);
                inputFormatType = "html";
            } else if (inputFormatType == "text") {
                htmlDataStr = htmlDataStr.Replace(Environment.NewLine, "\n");
            }
            htmlDataStr = htmlDataStr.ToString().ToBase64String();

            if (string.IsNullOrWhiteSpace(htmlDataStr)) {
                MpConsole.WriteTraceLine("Error parsing html data obj, no data found");
                Debugger.Break();
                return null;
            }

            var req = new MpQuillConvertPlainHtmlToQuillHtmlRequestMessage() {
                data = htmlDataStr,
                dataFormatType = inputFormatType,
                isBase64 = true
            };
            string respStr = await ConverterWebView.EvaluateJavascriptAsync($"convertPlainHtml_ext('{req.SerializeJsonObjectToBase64()}')");
            var resp = MpJsonObject.DeserializeBase64Object<MpQuillConvertPlainHtmlToQuillHtmlResponseMessage>(respStr);
            return new MpAvHtmlClipboardData() {
                Html = resp.quillHtml,
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
