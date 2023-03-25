using Avalonia.LogicalTree;
using MonkeyPaste.Common;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvPlainHtmlConverterWebView :
        MpAvContentWebView,
        MpIPlainHtmlConverterView {
        #region Private Variables

        private MpQuillConvertPlainHtmlToQuillHtmlResponseMessage _lastPlainHtmlConvertedResp = null;
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

        public MpQuillConvertPlainHtmlToQuillHtmlResponseMessage LastPlainHtmlResp {
            get => _lastPlainHtmlConvertedResp;
            set => _lastPlainHtmlConvertedResp = value;
        }

        #endregion

        #region Constructors
        public MpAvPlainHtmlConverterWebView() : base() {
            Mp.Services.ContentViewLocator = this;

        }

        #endregion

        #region Public Methods

        public override void HandleBindingNotification(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str) {
            base.HandleBindingNotification(notificationType, msgJsonBase64Str);
            switch (notificationType) {
                case MpAvEditorBindingFunctionType.notifyPlainHtmlConverted:
                    var ntf = MpJsonConverter.DeserializeBase64Object<MpQuillConvertPlainHtmlToQuillHtmlResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillConvertPlainHtmlToQuillHtmlResponseMessage plainHtmlResp) {
                        _lastPlainHtmlConvertedResp = plainHtmlResp;
                    }
                    break;
            }
        }
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
