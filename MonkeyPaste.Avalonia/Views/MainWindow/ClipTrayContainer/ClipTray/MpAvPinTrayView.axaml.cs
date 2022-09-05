using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Avalonia.Behaviors._Factory;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPinTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        public MpAvPinTrayDropBehavior PinTrayDropBehavior { 
            get {
                if (this.Resources.TryGetResource("PinTrayDropBehavior", out object value)) {
                    return value as MpAvPinTrayDropBehavior;
                }
                return null;
            }
        }
        public MpAvPinTrayView() {
            InitializeComponent();
            PinTrayListBox = this.FindControl<ListBox>("PinTrayListBox");
            //MpAvViewBehaviorFactory.BuildAllViewBehaviors(this, this);

            //this.AttachedToVisualTree += MpAvClipTileTitleView_AttachedToVisualTree;
        }

        private void MpAvClipTileTitleView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            MpAvViewBehaviorFactory.BuildAllViewBehaviors(this, this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
