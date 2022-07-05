using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvMainWindowTitleMenuView : MpAvUserControl<MpAvMainWindowViewModel> {
        public MpAvMainWindowTitleMenuView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
