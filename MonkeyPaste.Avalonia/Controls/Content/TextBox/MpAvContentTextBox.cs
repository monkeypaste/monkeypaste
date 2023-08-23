using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvContentTextBox :
        TextBox,
        MpITextDocumentContainer,
        MpAvIContentDragSource,
        MpIContentView {
        #region Private Variables

        #endregion

        #region Statics

        static MpAvContentTextBox() {
            TextProperty.Changed.AddClassHandler<MpAvContentTextBox>((x, y) => HandleTextChanged(x, y));
        }

        private static void HandleTextChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvContentTextBox tb) {
                //RaisePropertyChanged(TextProperty, oldValue, value);
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
            this.InvalidateVisual();
            await Task.Delay(1);
        }

        public async Task ReloadAsync() {
            await LoadContentAsync();
        }

        public async Task<bool> UpdateContentAsync(MpJsonObject contentJsonObj) {
            // annotations not supported, so return false to not confuse transaction history

            await Task.Delay(1);
            return false;
        }

        public void ShowDevTools() {
#if !DEBUG
            return;
#endif
            // focus this control so its the dev tools focus
            this.Focus();
            MpConsole.WriteLine($"Showing dev tools with gesture '{MpAvWindow.DefaultDevToolOptions.Gesture.ToString()}'");
            Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(MpAvWindow.DefaultDevToolOptions.Gesture.ToString());

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
            if (!ignore_selection &&
                SelectionEnd - SelectionStart is int sel_len &&
                sel_len > 0) {
                // only use selection if range is selected
                avdo.SetData(MpPortableDataFormats.Text, this.Text.Substring(SelectionStart, sel_len));
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
        protected override Type StyleKeyOverride => typeof(TextBox);

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

        public MpAvContentTextBox() : base() {
            this.GetObservable(FontFamilyProperty).Subscribe(value => OnFontFamilyChanged());
        }

        #endregion

        #region Public Methods


        #endregion

        #region Protected Methods
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            Mp.Services.ContentViewLocator.AddView(this);
            this.FontFamily = MpAvStringToFontFamilyConverter.Instance.Convert(MpAvPrefViewModel.Instance.DefaultEditableFontFamily, null, null, null) as FontFamily;
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
        private void OnFontFamilyChanged() {
            Dispatcher.UIThread.Post(async () => {
                var tp = await this.GetVisualDescendantAsync<TextPresenter>();
                if (tp == null) {
                    return;
                }
                TextElement.SetFontFamily(tp, this.FontFamily);
            });

        }



        #endregion
    }
}
