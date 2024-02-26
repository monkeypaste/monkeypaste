using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TheArtOfDev.HtmlRenderer.Avalonia;


namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvContentHtmlControl :
        HtmlPanel,
        MpITextDocumentContainer,
        MpAvIContentDragSource,
        MpIContentView {
        #region Private Variables

        #endregion

        #region Statics

        static MpAvContentHtmlControl() {
            TextProperty.Changed.AddClassHandler<MpAvContentHtmlControl>((x, y) => HandleTextChanged(x, y));
        }

        private static void HandleTextChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvContentHtmlControl tb) {
                //RaisePropertyChanged(TextProperty, oldValue, paramValue);
            }
        }


        #endregion

        #region Interfaces

        #region MpITextDocumentContainer Implementation
        private MpTextRange _contentRange;
        public MpTextRange ContentRange {
            get {
                if (_contentRange == null) {
                    _contentRange = new MpTextRange(this);
                }
                return _contentRange;
            }
        }
        #endregion

        #region MpIContentView Implementation
        bool MpIContentView.IsViewInitialized =>
            IsInitialized;
        public bool IsContentLoaded =>
            IsInitialized && BindingContext != null;
        public bool IsSubSelectable =>
            BindingContext.IsSubSelectionEnabled;

        public async Task LoadContentAsync(bool isSearchEnabled = true) {
            // really dont need to do anything
            this.Redraw();
            await Task.Delay(1);
        }

        public async Task ReloadAsync() {
            await LoadContentAsync();
        }

        public async Task<bool> UpdateContentAsync(object contentJsonObj) {
            // annotations not supported, so return false to not confuse transaction history

            await Task.Delay(1);
            return false;
        }

        public void OpenDevTools() {
#if DEBUG
            // focus this control so its the dev tools focus
            this.Focus();
            MpConsole.WriteLine($"Showing dev tools with gesture '{MpAvWindow.DefaultDevToolOptions.Gesture.ToString()}'");
            Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(MpAvWindow.DefaultDevToolOptions.Gesture.ToString());
#endif
        }

        public void SendMessage(string msgJsonBase64Str) {
            throw new NotImplementedException();
        }


        #region MpIRecyclableLocatorItem Implementation
        int MpILocatorItem.LocationId =>
            BindingContext is MpILocatorItem ? (BindingContext as MpILocatorItem).LocationId : 0;
        DateTime? MpIRecyclableLocatorItem.LocatedDateTime { get; set; }
        #endregion

        #endregion

        #region MpAvIDragSource Implementation
        public bool IsDragging {
            get {
                if (BindingContext == null) {
                    return false;
                }
                return BindingContext.IsTileDragging;
            }
            set {
                if (IsDragging != value &&
                    BindingContext != null) {
                    BindingContext.IsTileDragging = value;
                }
            }
        }

        public async Task<MpAvDataObject> GetDataObjectAsync(string[] formats = null, bool use_placeholders = false, bool ignore_selection = false) {
            if (BindingContext == null ||
                BindingContext.IsPlaceholder ||
                BindingContext.CopyItem.ToAvDataObject(true, true) is not MpAvDataObject avdo ||
                avdo.DataFormatLookup == null) {
                return null;
            }

            if (!ignore_selection && SelectedText != null && SelectedText.Length > 0) {
                if (formats == null || formats.Contains(MpPortableDataFormats.Text)) {
                    // only use selection if range is selected
                    avdo.SetData(MpPortableDataFormats.Text, SelectedText);
                    avdo.SetData(MpPortableDataFormats.Text, SelectedText);
                }
                if (formats == null || formats.Contains(MpPortableDataFormats.Html)) {

                    // only use selection if range is selected
                    avdo.SetData(MpPortableDataFormats.Html, SelectedHtml);
                    avdo.SetData(MpPortableDataFormats.Html, SelectedHtml);
                }
            }
            await Task.Delay(1);
            return new MpAvDataObject(avdo);
        }
        public string[] GetDragFormats() {
            if (DataContext is not MpAvClipTileViewModel ctvm) {
                return new string[] { };
            }
            return ctvm.GetOleFormats(true);
        }
        #endregion
        #endregion

        #region Overrides
        protected override Type StyleKeyOverride => typeof(HtmlLabel);

        #endregion

        #region Properties

        public MpAvClipTileViewModel BindingContext {
            get {
                if (DataContext is MpAvClipTileViewModel) {
                    return DataContext as MpAvClipTileViewModel;
                }
                if (DataContext is MpAvNotificationViewModelBase nvmb) {
                    return nvmb.Body as MpAvClipTileViewModel;
                }
                return null;
            }
        }
        #endregion

        #region Constructors

        public MpAvContentHtmlControl() : base() {
        }

        #endregion

        #region Public Methods


        #endregion

        #region Protected Methods
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            Mp.Services.ContentViewLocator.AddView(this);
        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            Mp.Services.ContentViewLocator.RemoveView(this);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            if (e.IsRightDown(this)) {
                e.Handled = true;
                return;
            }
            base.OnPointerPressed(e);
        }
        #endregion

        #region Private Methods


        #endregion
    }
}
