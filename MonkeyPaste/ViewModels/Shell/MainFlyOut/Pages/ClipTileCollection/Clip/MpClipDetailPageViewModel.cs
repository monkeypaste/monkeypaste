using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpClipDetailPageViewModel : MpViewModelBase {
        #region Properties
        public MpClip Clip { get; set; }

        public string EditorHtml { get; set; }

        public Func<string, Task<string>> EvaluateJavascript { get; set; }
        #endregion

        #region Public Methods
        public MpClipDetailPageViewModel() : base() {
        }
        public MpClipDetailPageViewModel(MpClip ci) : this() {
            Clip = ci;
            Initialize();
        }

        public async Task<string> GetEditorText() {
            var itemText = await EvaluateJavascript($"getText()");
            return itemText.Replace("\"", string.Empty);
        }

        public async Task<string> GetEditorHtml() {
            var itemText = await EvaluateJavascript($"getHtml()");
            return itemText.Replace("\"", string.Empty);
        }

        public async Task SetEditorText(string content) {
            await EvaluateJavascript($"setText('{content}')");
        }

        public async Task SetEditorHtml(string html) {
            await EvaluateJavascript($"setHtml('{html}')");
        }
        #endregion

        #region Private Methods
        private void Initialize() {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpClipDetailPageViewModel)).Assembly;
            var stream = assembly.GetManifestResourceStream("MonkeyPaste.Resources.Html.Editor.Editor.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                var html = reader.ReadToEnd();
                string contentTag = @"<div id='editor'>";
                html = html.Replace(contentTag, contentTag + Clip.ItemPlainText);
                EditorHtml = html;
            }
        }
        #endregion

        #region Commands

        #endregion
    }
}
