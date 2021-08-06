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

        public Func<string, Task<string>> EvaluateEditorJavaScript { get; set; }

        public event EventHandler OnEditorLoaded;

        public string EditorHtml { get; set; }

        public string Html { get; set; }
        public string Text { get; set; }
        public string Templates { get; set; }

        public System.Timers.Timer UpdateTimer { get; set; }

        #endregion

        #region Public Methods
        public MpCopyItemDetailPageViewModel() : base() { }

        public MpCopyItemDetailPageViewModel(MpCopyItem ci) : this() {
            CopyItem = ci;
            Initialize();
        }

        public void InitEditor(bool fillTemplates = false) {
            return;
        }
        
        private void SetToolbarTop(int y) {
            EvaluateEditorJavaScript($"moveToolbarTop({y})");
        } 

        public async Task<string> GetEditorText() {
            var itemText = await EvaluateEditorJavaScript($"getText()");
            return itemText.Replace("\"", string.Empty);
        }

        public async Task<string> GetEditorHtml() {
            var itemText = await EvaluateEditorJavaScript($"getHtml()");
            return itemText.Replace("\"", string.Empty);
        }

        public void SetEditorText(string content) {
            EvaluateEditorJavaScript($"setText('{content}')");
        }

        public void SetEditorHtml(string html) {
            EvaluateEditorJavaScript($"setHtml('{html}')");
        }

        private void LayoutService_OnKeyboardHeightChanged(object sender, float e) {
            if (EvaluateEditorJavaScript != null) {
                MpConsole.WriteLine(@"Kb Top: " + e);
                SetToolbarTop((int)e);
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
                var data = string.IsNullOrEmpty(CopyItem.ItemHtml) ? CopyItem.ItemText : CopyItem.ItemHtml;
                foreach(var cit in CopyItem.Templates) {
                    data = data.Replace(cit.ToHtml(), cit.ToQuillEncoded());
                }
                html = html.Replace(contentTag, contentTag + data) ;
                EditorHtml = html;
            }
            

            UpdateTimer = new System.Timers.Timer();
            UpdateTimer.Interval = 100;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Start();
        }

        private async void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            if (EvaluateEditorJavaScript == null) {
                return;
            }
            Text = await EvaluateEditorJavaScript($"getText()");
            if (EvaluateEditorJavaScript == null) {
                return;
            }
            Html = await EvaluateEditorJavaScript($"getHtml()");
            if (EvaluateEditorJavaScript == null) {
                return;
            }
            Templates = await EvaluateEditorJavaScript($"getTemplates()");
        }

        public void Dispose() {
            //StopMessageListener();
            EvaluateEditorJavaScript = null;
            (Application.Current.MainPage as MpMainShell).LayoutService.OnKeyboardHeightChanged -= LayoutService_OnKeyboardHeightChanged;
        }
        #endregion

        #region Commands
        #endregion
    }
}
