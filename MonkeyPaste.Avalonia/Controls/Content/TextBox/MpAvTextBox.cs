using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvTextBox :
        TextBox,
        MpAvIDragSource {
        #region Private Variables

        #endregion

        #region Statics

        static MpAvTextBox() {
            TextProperty.Changed.AddClassHandler<MpAvTextBox>((x, y) => HandleTextChanged(x, y));
        }

        private static void HandleTextChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvTextBox tb) {
                //RaisePropertyChanged(TextProperty, oldValue, value);
            }
        }


        #endregion

        #region Interfaces



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

        public PointerPressedEventArgs LastPointerPressedEventArgs { get; }

        public void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc, bool meta) {
            throw new NotImplementedException();
        }

        public void NotifyDropComplete(DragDropEffects dropEffect) {
            throw new NotImplementedException();
        }

        public Task<MpAvDataObject> GetDataObjectAsync(string[] formats = null, bool use_placeholders = true, bool ignore_selection = false) {
            throw new NotImplementedException();
        }
        public string[] GetDragFormats() {
            if (DataContext is MpAvClipTileViewModel ctvm) {
                return ctvm.GetOleFormats(true);
            }
            return new string[] { };
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

        public MpAvTextBox() : base() {
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
        }

        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
            if (DataContext is MpAvClipTileViewModel ctvm) {
                ctvm.IsEditorLoaded = true;
            }
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
