using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Input;
using System.Diagnostics;
using System.Security.Cryptography;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using System.Linq;

using Avalonia.Threading;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileContentView : 
        MpAvUserControl<MpAvClipTileViewModel> {

        public MpAvIContentView ContentView { get; private set; }

        public MpAvClipTileContentView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void ContentTemplate_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is UserControl contentContainer) {
                ContentView = contentContainer.Content as MpAvIContentView;
            }
        }
    }
}
