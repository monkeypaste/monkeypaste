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
    public class MpCopyItemDetailPageViewModel : MpViewModelBase, IDisposable {
        #region Properties
        public MpCopyItem CopyItem { get; set; }

        public MpJsMessageListener JsMessageListener { get; set; }

        public Func<string, Task<string>> EvaluateEditorJavaScript { get; set; }

        public event EventHandler OnEditorLoaded;

        public string EditorHtml { get; set; }
        #endregion

        #region Public Methods
        public MpCopyItemDetailPageViewModel() : base() { }

        public MpCopyItemDetailPageViewModel(MpCopyItem ci) : this() {
            PropertyChanged += MpCopyItemDetailPageViewModel_PropertyChanged;
            CopyItem = ci;
            
            //JsMessageListener = new MpJsMessageListener(EvaluateEditorJavaScript);
            Initialize();
        }

        private void MpCopyItemDetailPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(EvaluateEditorJavaScript):
                    if(EvaluateEditorJavaScript != null) {
                        InitEditor();
                        //JsMessageListener = new MpJsMessageListener(EvaluateEditorJavaScript);
                        //StartMessageListener();
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

        private async Task SetToolbarTop(int y) {
            await EvaluateEditorJavaScript($"moveToolbarTop({y})");
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

        public async Task InitEditor() {
            (Application.Current.MainPage as MpMainShell).LayoutService.OnKeyboardHeightChanged += LayoutService_OnKeyboardHeightChanged;
            string content = CopyItem.ItemHtml;
            if(string.IsNullOrEmpty(content)) {
                content = CopyItem.ItemText;
            }
            //await EvaluateEditorJavaScript($"init('{content}',null,null,null,null)");
        }

        private async void LayoutService_OnKeyboardHeightChanged(object sender, float e) {
            if (EvaluateEditorJavaScript != null) {
                MpConsole.WriteLine(@"Kb Top: " + e);
                await SetToolbarTop((int)e);
            }
        }
        #endregion

        #region Private Methods
        private void Initialize() {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpCopyItemDetailPageViewModel)).Assembly;
            var stream = assembly.GetManifestResourceStream("MonkeyPaste.Resources.Html.Editor.Editor2.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                var html = reader.ReadToEnd();
                string contentTag = @"<div id='editor'>";
                html = html.Replace(contentTag, contentTag + CopyItem.ItemText);
                EditorHtml = html;
            }
        }

        public void Dispose() {
            EvaluateEditorJavaScript = null;
            (Application.Current.MainPage as MpMainShell).LayoutService.OnKeyboardHeightChanged -= LayoutService_OnKeyboardHeightChanged;
        }
        #endregion

        #region Commands
        public ICommand CreateTemplateCommand => new Command<object>(async (args) => {
            if (args == null) {
                return;
            }
            await Task.Delay(1);
        });
        #endregion
    }
}
