using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileDetailView : MpAvUserControl<MpAvClipTileViewModel> {

        public MpAvClipTileDetailView() {
            InitializeComponent();
        }

        private void MpAvClipTileDetailView_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.CycleDetailCommand.Execute(null);
        }

        private void MpAvClipTileDetailView_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e) {
            
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
