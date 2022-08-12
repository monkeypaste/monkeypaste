using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvClipTileView_AttachedToVisualTree;
            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void MpAvClipTileView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            //MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayLayoutChanged);
        }


        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayLayoutChanged:
                    break;
            }
        }
    }
}
