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
using Xamarin.Forms;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpRtbItemCollectionVIew.xaml
    /// </summary>
    public partial class MpRtbItemCollectionVIew : UserControl {
        public MpRtbItemCollectionVIew() {
            InitializeComponent();
            MpTemplateHyperlinkViewModel.OnTemplateSelected += MpTemplateHyperlinkViewModel_OnTemplateSelected;
        }

        private void MpTemplateHyperlinkViewModel_OnTemplateSelected(object sender, EventArgs e) {
            var thvm = sender as MpTemplateHyperlinkViewModel;
            var rtbcvm = DataContext as MpClipTileRichTextBoxViewModelCollection;
            if (thvm.HostRtbItemViewModel.HostClipTileViewModel == rtbcvm.HostClipTileViewModel) {
                EditTemplateView.SetActiveTemplate(thvm);

            }
        }

        private void MpRtbEditToolbarView_OnTileUnexpand(object sender, EventArgs e) {
            var rtbcvm = DataContext as MpClipTileRichTextBoxViewModelCollection;
            
            // TODO save models here
        }

        private void MpRtbEditToolbarView_OnTileExpand(object sender, EventArgs e) {
            var srtb = ClipTileRichTextBoxListBox.GetListBoxItem(0).GetDescendantOfType<RichTextBox>();
            EditToolbarView.SetCommandTarget(srtb);
            EditTemplateView.SetActiveRtb(srtb);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null) {
                var rtbcvm = DataContext as MpClipTileRichTextBoxViewModelCollection;
                rtbcvm.CollectionChanged += Rtbcvm_CollectionChanged;
                rtbcvm.HostClipTileViewModel.OnTileExpand += MpRtbEditToolbarView_OnTileExpand;
                rtbcvm.HostClipTileViewModel.OnTileUnexpand += MpRtbEditToolbarView_OnTileUnexpand;
            } 
        }

        private void Rtbcvm_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if(e.NewItems != null) {
                foreach(MpRtbListBoxItemRichTextBoxViewModel rtbvm in e.NewItems) {
                    rtbvm.PropertyChanged += Rtbvm_PropertyChanged; 
                }
            }

            if (e.OldItems != null) {
                foreach (MpRtbListBoxItemRichTextBoxViewModel rtbvm in e.OldItems) {
                    rtbvm.PropertyChanged -= Rtbvm_PropertyChanged;
                }
            }

        }

        private void Rtbvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var rtbvm = sender as MpRtbListBoxItemRichTextBoxViewModel;
            var rtbcvm = rtbvm.RichTextBoxViewModelCollection;
            switch (e.PropertyName) {
                case nameof(rtbvm.IsSubSelected):
                    var itemIdx = rtbcvm.IndexOf(rtbvm);
                    if (itemIdx >= 0) {
                        var ssrtb = ClipTileRichTextBoxListBox.GetListBoxItem(itemIdx).GetDescendantOfType<RichTextBox>();
                        EditToolbarView.SetCommandTarget(ssrtb);
                        EditTemplateView.SetActiveRtb(ssrtb);
                    }
                    break;
            }
        }
    }
}
