using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPinTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        public MpAvPinTrayDropBehavior PinTrayDropBehavior { get; set; }
        public MpAvPinTrayView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvClipTileTitleView_AttachedToVisualTree;
        }

        private void MpAvClipTileTitleView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            MpAvViewBehaviorFactory.BuildAllViewBehaviors(this, this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
