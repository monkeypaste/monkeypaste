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
    /// Interaction logic for MpContentListVIew.xaml
    /// </summary>
    public partial class MpContentListVIew : UserControl {
        AdornerLayer RtbLbAdornerLayer;
        public MpRtbListBoxAdorner RtbLbAdorner;

        public MpContentListVIew() {
            InitializeComponent();
            MpTemplateHyperlinkViewModel.OnTemplateSelected += MpTemplateHyperlinkViewModel_OnTemplateSelected;
        }

        public void UpdateAdorners() {
            RtbLbAdornerLayer.Update();
            for (int i = 0; i < ClipTileRichTextBoxListBox.Items.Count; i++) {
               ClipTileRichTextBoxListBox
                    .GetListBoxItem(i)
                    .GetVisualDescendent<MpContentListItemView>()
                    .UpdateAdorner();

            }
        }
        #region Rtb ListBox Events
        private void ClipTileRichTextBoxListBox_Loaded(object sender, RoutedEventArgs e) {
            RtbLbAdorner = new MpRtbListBoxAdorner(ClipTileRichTextBoxListBox);
            RtbLbAdornerLayer = AdornerLayer.GetAdornerLayer(ClipTileRichTextBoxListBox);
            RtbLbAdornerLayer.Add(RtbLbAdorner);
        }

        private void ClipTileRichTextBoxListBox_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true; 
        }

        private void ClipTileRichTextBoxListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            // NOTE This is for selection changed from interface from VM is in another handler
            var rtblbvm = DataContext as MpRtbItemCollectionViewModel;
            if (rtblbvm.Count > 1) {
                //order selected tiles by ascending datetime 
                var subSelectedRtbvmListBySelectionTime = rtblbvm.SubSelectedContentItems.OrderBy(x => x.LastSubSelectedDateTime).ToList();
                foreach (var srtbvm in subSelectedRtbvmListBySelectionTime) {
                    if (srtbvm == subSelectedRtbvmListBySelectionTime[0]) {
                        srtbvm.IsPrimarySubSelected = true;
                    } else {
                        srtbvm.IsPrimarySubSelected = false;
                    }
                }
            } else if (rtblbvm.SubSelectedContentItems.Count == 1) {
                rtblbvm.SubSelectedContentItems[0].IsPrimarySubSelected = false;
            }

            foreach (var osctvm in e.RemovedItems) {
                if (osctvm.GetType() == typeof(MpRtbItemViewModel)) {
                    ((MpRtbItemViewModel)osctvm).IsSubSelected = false;
                    ((MpRtbItemViewModel)osctvm).IsPrimarySubSelected = false;
                }
            }
        }
        
        #endregion

        #region Toolbar Events
        private void MpTemplateHyperlinkViewModel_OnTemplateSelected(object sender, EventArgs e) {
            var thvm = sender as MpTemplateHyperlinkViewModel;
            var rtbcvm = DataContext as MpRtbItemCollectionViewModel;
            if (thvm.HostRtbItemViewModel.HostClipTileViewModel == rtbcvm.HostClipTileViewModel) {
                EditTemplateView.SetActiveTemplate(thvm);

            }
        }

        private void MpRtbEditToolbarView_OnTileUnexpand(object sender, EventArgs e) {
            var rtbcvm = DataContext as MpRtbItemCollectionViewModel;
            
            // TODO save models here
        }

        private void MpRtbEditToolbarView_OnTileExpand(object sender, EventArgs e) {
            var srtb = ClipTileRichTextBoxListBox.GetListBoxItem(0).GetDescendantOfType<RichTextBox>();
            EditToolbarView.SetCommandTarget(srtb);
            EditTemplateView.SetActiveRtb(srtb);
        }
        #endregion

        #region ViewModel

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null) {
                var rtbcvm = DataContext as MpContentContainerViewModel;
                rtbcvm.OnScrollIntoViewRequest += Rtbcvm_OnScrollIntoViewRequest;
                rtbcvm.OnScrollToHomeRequest += Rtbcvm_OnScrollToHomeRequest;
                rtbcvm.OnSubSelectionChanged += Rtbcvm_OnSubSelectionChanged;
                rtbcvm.HostClipTileViewModel.OnTileExpand += MpRtbEditToolbarView_OnTileExpand;
                rtbcvm.HostClipTileViewModel.OnTileUnexpand += MpRtbEditToolbarView_OnTileUnexpand;

                //rtbcvm.SyncItemsWithModel();
            } 
        }

        private void Rtbcvm_OnSubSelectionChanged(object sender, object e) {
            var rtbcvm = DataContext as MpContentContainerViewModel;
            var itemIdx = rtbcvm.ItemViewModels.IndexOf(e as MpContentItemViewModel);
            if (itemIdx >= 0) {
                var ssrtb = ClipTileRichTextBoxListBox.GetListBoxItem(itemIdx).GetDescendantOfType<RichTextBox>();
                EditToolbarView.SetCommandTarget(ssrtb);
                EditTemplateView.SetActiveRtb(ssrtb);
            }
        }

        private void Rtbcvm_OnScrollToHomeRequest(object sender, EventArgs e) {
            ClipTileRichTextBoxListBox?.GetScrollViewer().ScrollToHome();
        }

        private void Rtbcvm_OnScrollIntoViewRequest(object sender, object e) {
            ClipTileRichTextBoxListBox?.ScrollIntoView(e);
        }

        #endregion

        public void SyncMultiSelectDragButton(bool isOver, bool isDown) {
            string transBrush = Brushes.Transparent.ToString();
            string outerBrush = isOver ? "#FF7CA0CC" : isDown ? "#FF2E4E76" : transBrush;
            string innerBrush = isOver ? "#FFE4EFFD" : isDown ? "#FF116EE4" : transBrush;
            string innerBg = isOver ? "#FFDAE7F5" : isDown ? "#FF3272B8" : transBrush;

            //foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedClipTiles) {
            //    foreach (var srtbvm in sctvm.ContentContainerViewModel.SubSelectedContentItems) {
            //        var outerBorder = (Border)srtbvm.DragButton.Template.FindName("OuterBorder", srtbvm.DragButton);
            //        if (outerBorder != null) {
            //            outerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString(outerBrush);
            //        }
            //        var innerBorder = (Border)srtbvm.DragButton.Template.FindName("InnerBorder", srtbvm.DragButton);
            //        if (innerBorder != null) {
            //            innerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString(innerBrush);
            //            innerBorder.Background = (Brush)new BrushConverter().ConvertFromString(innerBg);
            //        }
            //    }
            //}
        }

    }
}
