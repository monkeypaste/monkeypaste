using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using PropertyChanged;
using Avalonia.Controls.Platform;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvGtkWebView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvGtkWebView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void LinuxContentView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            BindingContext.IsViewLoaded = true;
        }
    }
}
