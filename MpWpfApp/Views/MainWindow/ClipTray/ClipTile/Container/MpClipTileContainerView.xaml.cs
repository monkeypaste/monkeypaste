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

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileContainerView.xaml
    /// </summary>
    public partial class MpClipTileContainerView : MpUserControl<MpClipTileViewModel> {
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
                    TileResizeBehavior.Reattach();
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
            rtb.FitDocToRtb();
        }
    }
}
