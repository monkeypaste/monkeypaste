using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSidebarContainerView : MpAvUserControl<MpAvSidebarItemCollectionViewModel> {

        public MpAvSidebarContainerView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
