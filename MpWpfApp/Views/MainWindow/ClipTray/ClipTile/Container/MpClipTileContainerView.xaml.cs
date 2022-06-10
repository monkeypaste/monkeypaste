using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileContainerView.xaml
    /// </summary>
    public partial class MpClipTileContainerView : MpUserControl<MpClipTileViewModel> {
        public bool IsPinTrayTile { get; set; } = false;

        public MpClipTileContainerView() {
            InitializeComponent();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e) {
            MpMessenger.Register<MpMessageType>(
                BindingContext,
                ReceivedClipTileViewModelMessage,
                BindingContext);
        }


        private void ReceivedClipTileViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ContentItemsChanged:
                    TileResizeBehavior.Reattach();//.FireAndForgetSafeAsync(BindingContext);
                    break;
                case MpMessageType.IsEditable:
                case MpMessageType.IsReadOnly:
                    MpClipTrayViewModel.Instance.RequestScrollIntoView(BindingContext);
                    break;
            }
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e) {
            var rtb = this.GetVisualDescendent<RichTextBox>();
            if(rtb == null) {
                return;
            }
            rtb.FitDocToRtb(BindingContext.IsCurrentDropTarget);
        }
    }
}
