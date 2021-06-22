using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpClipDetailPageViewModel : MpViewModelBase {
        #region Properties
        public MpClip Clip { get; set; }

        public MpJsMessageListener JsMessageListener { get; set; }

        public Func<string, Task<string>> EvaluateEditorJavaScript { get; set; }

        public string EditorHtml { get; set; }
        #endregion

        #region Public Methods
        public MpClipDetailPageViewModel() : base() {      
        }
        public MpClipDetailPageViewModel(MpClip ci) : this() {
            PropertyChanged += MpClipDetailPageViewModel_PropertyChanged;
            Clip = ci;
            //JsMessageListener = new MpJsMessageListener(EvaluateEditorJavaScript);
            Initialize();
        }

        private void MpClipDetailPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(EvaluateEditorJavaScript):
                    if(EvaluateEditorJavaScript != null) {
                        JsMessageListener = new MpJsMessageListener(EvaluateEditorJavaScript);
                        StartMessageListener();
                    }
                    break;
            }
        }

        public void StartMessageListener() {
            JsMessageListener.Start();
        }

        public async Task<string> StopMessageListener() {
            string outStr = string.Empty;
            if(JsMessageListener != null) {
                JsMessageListener.Stop();
            }
            var itemText = await EvaluateEditorJavaScript($"getText()");
            itemText = itemText.Replace("\"", string.Empty);
            return itemText;
        }

        public async Task<string> GetEditorText() {
            var itemText = await EvaluateEditorJavaScript($"getText()");
            return itemText.Replace("\"", string.Empty);
        }

        public async Task<string> GetEditorHtml() {
            var itemText = await EvaluateEditorJavaScript($"getHtml()");
            return itemText.Replace("\"", string.Empty);
        }

        public async Task SetEditorText(string content) {
            await EvaluateEditorJavaScript($"setText('{content}')");
        }

        public async Task SetEditorHtml(string html) {
            await EvaluateEditorJavaScript($"setHtml('{html}')");
        }
        #endregion

        #region Private Methods
        private void Initialize() {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpClipDetailPageViewModel)).Assembly;
            var stream = assembly.GetManifestResourceStream("MonkeyPaste.Resources.Html.Editor.Editor2.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                var html = reader.ReadToEnd();
                string contentTag = @"<div id='editor'>";
                html = html.Replace(contentTag, contentTag + Clip.ItemText);
                EditorHtml = html;
            }
        }
        #endregion

        #region Commands
        public ICommand CreateTemplateCommand => new Command<object>(async (args) => {
            if (args == null) {
                return;
            }
        });
        #endregion
    }
}
