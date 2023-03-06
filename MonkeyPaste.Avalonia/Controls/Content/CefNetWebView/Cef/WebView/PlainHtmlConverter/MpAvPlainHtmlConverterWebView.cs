using Avalonia.LogicalTree;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvPlainHtmlConverterWebView :
        //MpAvCefNetWebView,
        MpAvContentWebView,
        MpIPlainHtmlConverterView {
        #region Private Variables

        #endregion

        #region Statics
        public static string HTML_CONVERTER_PARAMS => "converter=true";

        static MpAvPlainHtmlConverterWebView() {

        }
        #endregion

        #region Interfaces

        #region MpIPlainHtmlConverterView Implementation

        public EventHandler OnViewAttached { get; }
        public bool IsViewInitialized { get; private set; }

        #endregion

        #endregion

        #region Properties
        public override string ContentUrl => base.ContentUrl + $"?{HTML_CONVERTER_PARAMS}";
        #endregion

        #region Constructors
        public MpAvPlainHtmlConverterWebView() : base() {
            Mp.Services.ContentViewLocator = this;

        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e) {
            base.OnAttachedToLogicalTree(e);
            OnViewAttached?.Invoke(this, e);
        }

        protected override void OnIsEditorInitializedChanged() {
            base.OnIsEditorInitializedChanged();
            IsViewInitialized = IsEditorInitialized;
        }
        #endregion
        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
