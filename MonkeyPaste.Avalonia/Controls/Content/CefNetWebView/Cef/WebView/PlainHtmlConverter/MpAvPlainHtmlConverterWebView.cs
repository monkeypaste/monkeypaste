using Avalonia.LogicalTree;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
#if PLAT_WV
using AvaloniaWebView;
#endif

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvPlainHtmlConverterWebView :
        MpAvContentWebView,
        MpIPlainHtmlConverterView {
        #region Private Variables

        private MpQuillConvertPlainHtmlToQuillHtmlResponseMessage _lastPlainHtmlConvertedResp = null;
        #endregion

        #region Statics

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

        public MpQuillConvertPlainHtmlToQuillHtmlResponseMessage LastPlainHtmlResp {
            get => _lastPlainHtmlConvertedResp;
            set => _lastPlainHtmlConvertedResp = value;
        }

#if PLAT_WV
        public override WebView WebView {
            get {
                if (_webView == null) {
                    _webView = new WebView() {
                        Url = MpAvClipTrayViewModel.Instance.EditorUri
                    };
                }
                return _webView;
            }
        } 
#endif

        #endregion

        #region Constructors
        public MpAvPlainHtmlConverterWebView() {
#if PLAT_WV
            this.Content = WebView; 
#endif
        }

        #endregion

        #region Public Methods

        public override void HandleBindingNotification(MpEditorBindingFunctionType notificationType, string msgJsonBase64Str, string contentHandle) {
            switch (notificationType) {
                case MpEditorBindingFunctionType.notifyPlainHtmlConverted:
                    var ntf = MpJsonConverter.DeserializeBase64Object<MpQuillConvertPlainHtmlToQuillHtmlResponseMessage>(msgJsonBase64Str);
                    if (ntf is MpQuillConvertPlainHtmlToQuillHtmlResponseMessage plainHtmlResp) {
                        _lastPlainHtmlConvertedResp = plainHtmlResp;
                    }
                    break;
                default:
                    base.HandleBindingNotification(notificationType, msgJsonBase64Str, contentHandle);
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

        protected override MpQuillInitMainRequestMessage GetInitMessage() {
            var msg = base.GetInitMessage();
            msg.isConverter = true;
            return msg;
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
