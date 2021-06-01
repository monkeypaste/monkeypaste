using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemDetailPageViewModel : MpViewModelBase {
        #region Properties
        public MpCopyItem CopyItem { get; set; }

        public string EditorHtml { get; set; }

        private Func<string, Task<string>> _evaluateJavascript;
        public Func<string, Task<string>> EvaluateJavascript {
            get { return _evaluateJavascript; }
            set { _evaluateJavascript = value; }
        }
        #endregion

        #region Public Methods
        public MpCopyItemDetailPageViewModel() : base() {
        }
        public MpCopyItemDetailPageViewModel(MpCopyItem ci) : this() {
            CopyItem = ci;
            Initialize();
        }
        #endregion

        #region Private Methods
        private void Initialize() {
            //while (CopyItem == null) {
            //    await Task.Delay(100);
            //}
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpCopyItemDetailPageViewModel)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("MonkeyPaste.Resources.Html.Editor.Editor.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                var html = reader.ReadToEnd();
                string contentTag = @"<div id='editor'>";
                html = html.Replace(contentTag, contentTag + CopyItem.ItemPlainText);
                EditorHtml = html;
            }
        }
        #endregion

        #region Commands

        #endregion
    }
}
