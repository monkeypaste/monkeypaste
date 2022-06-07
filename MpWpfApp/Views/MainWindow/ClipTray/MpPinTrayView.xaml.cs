using MonkeyPaste;
using MonkeyPaste.Common;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpPinTrayView.xaml
    /// </summary>
    public partial class MpPinTrayView : MpUserControl<MpClipTrayViewModel> {
        private double _preDragWidth = -1;
        public MpPinTrayView() {
            InitializeComponent();
        }

        private void PinTrayListBox_Loaded(object sender, RoutedEventArgs e) {
            if(BindingContext == null) {
                return;
            }
            BindingContext.OnScrollIntoViewRequest += BindingContext_OnScrollIntoViewRequest;

            //MpMessenger.Register<MpMessageType>(nameof(MpDragDropManager), ReceivedDragDropManagerMessage);
        }

        private void ReceivedDragDropManagerMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.ItemDragBegin:
                    if(_preDragWidth < 0) {
                        _preDragWidth = PinTrayListBox.ActualWidth;
                        PinTrayListBox.MinWidth = _preDragWidth + BindingContext.MinClipOrPinTrayScreenWidth;

                        PinTrayListBox.InvalidateMeasure();
                        UpdateLayout();
                    }
                    
                    break;
                case MpMessageType.ItemDragEnd:
                    if(_preDragWidth > 0) {
                        PinTrayListBox.MinWidth = BindingContext.MinClipOrPinTrayScreenWidth;
                        //BindingContext.PinTrayBoundWidth = _preDragWidth;
                        _preDragWidth = -1;

                        PinTrayListBox.Items.Refresh();
                        PinTrayListBox.InvalidateMeasure();
                        UpdateLayout();
                    }
                    break;
            }
        }
        private void BindingContext_OnScrollIntoViewRequest(object sender, object e) {
            PinTrayListBox.ScrollIntoView(e);
        }

        public void UpdateAdorners() {
            if (PinTrayListBox == null) {
                return;
            }
            var ptlb_a = AdornerLayer.GetAdornerLayer(PinTrayListBox);
            if (ptlb_a == null) {
                return;
            }
            ptlb_a.Update();
        }
    }
}
