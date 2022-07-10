using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using Avalonia.Input;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvClipTileView_AttachedToVisualTree;
            this.PointerPressed += MpAvClipTileView_PointerPressed;
        }

        private void MpAvClipTileView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if(e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
                //if(BindingContext is MpISelectableViewModel svm) {
                //    svm.IsSelected = true;
                //}
                if(BindingContext is MpISelectorItemViewModel<MpAvClipTileViewModel> sivm) {
                    sivm.Selector.SelectedItem = BindingContext;
                }
            }
        }

        private void MpAvClipTileView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
