using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvContentTextBox :
        TextBox,
        MpIContentView,
        MpAvIDragSource {
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

        #region MpIContentView Implementation

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
            Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(MpAvWindow.DefaultDevToolOptions.Gesture.ToString()).FireAndForgetSafeAsync();

        }

        public void SendMessage(string msgJsonBase64Str) {
            throw new NotImplementedException();
        }
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
        public bool WasDragCanceled { get; set; } = false;

        public PointerPressedEventArgs LastPointerPressedEventArgs { get; private set; }

        public void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc, bool meta) {
            return;
        }


        public async Task<MpAvDataObject> GetDataObjectAsync(string[] formats = null, bool use_placeholders = true, bool ignore_selection = false) {
            if (BindingContext == null ||
                BindingContext.IsPlaceholder ||
                BindingContext.CopyItem.ToPortableDataObject(formats, true, true) is not MpPortableDataObject mpdo ||
                mpdo.DataFormatLookup == null) {
                return null;
            }
            await Task.Delay(1);
            return new MpAvDataObject(mpdo.DataFormatLookup.ToDictionary(x => x.Key.Name, x => x.Value));
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
            if (DataContext is MpAvClipTileViewModel ctvm) {
                ctvm.IsEditorLoaded = true;
            }
            this.FontFamily = MpAvStringToFontFamilyConverter.Instance.Convert(MpPrefViewModel.Instance.DefaultEditableFontFamily, null, null, null) as FontFamily;
        }

        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
            if (DataContext is MpAvClipTileViewModel ctvm) {
                ctvm.IsEditorLoaded = true;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            LastPointerPressedEventArgs = e;
            base.OnPointerPressed(e);
        }
        #endregion

        #region Private Methods
        private void OnFontFamilyChanged() {
            Dispatcher.UIThread.Post(async () => {
                var tp = await this.GetVisualDescendantAsync<TextPresenter>();
                TextElement.SetFontFamily(tp, this.FontFamily);
            });

        }



        #endregion
    }
}
