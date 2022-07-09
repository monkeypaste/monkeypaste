using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvClipTileView_AttachedToVisualTree;
        }

        private void MpAvClipTileView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            MpConsole.WriteLine("Loaded BContext" + BindingContext.CopyItem.Title + " DContext "+(DataContext as MpAvClipTileViewModel).CopyItemData);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
