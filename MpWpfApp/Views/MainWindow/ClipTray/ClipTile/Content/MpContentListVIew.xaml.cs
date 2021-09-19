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
    /// Interaction logic for MpContentListVIew.xaml
    /// </summary>
    public partial class MpContentListView : UserControl {
        AdornerLayer RtbLbAdornerLayer;
        public MpLineAdorner RtbLbAdorner;

        public MpContentListView() {
            InitializeComponent();
        }

        public MpContentListItemView GetSubSelectedItemView() {
            if(ClipTileRichTextBoxListBox.Items.Count == 0) {
                return null;
            }
            var ccvm = DataContext as MpClipTileViewModel;
            if (ClipTileRichTextBoxListBox.SelectedIndex < 0) {
                ClipTileRichTextBoxListBox.SelectedItem = ClipTileRichTextBoxListBox.Items[0];
            }
            var sciLbi = ClipTileRichTextBoxListBox.GetListBoxItem(ClipTileRichTextBoxListBox.SelectedIndex);
            if(sciLbi == null) {
                throw new Exception("Cannot select content item");
            }
            return sciLbi.GetVisualDescendent<MpContentListItemView>();
        }

        public void UpdateAdorners() {
            RtbLbAdornerLayer.Update();
            for (int i = 0; i < ClipTileRichTextBoxListBox.Items.Count; i++) {
                var lbi = ClipTileRichTextBoxListBox.GetListBoxItem(i);
                if(lbi == null) {
                    MonkeyPaste.MpConsole.WriteTraceLine("No Listbox Item at idx: " + i);
                    continue;
                }
                var cliv = lbi.GetVisualDescendent<MpContentListItemView>();
                cliv?.UpdateAdorner();
            }
        }
        #region Rtb ListBox Events
        private void ClipTileRichTextBoxListBox_Loaded(object sender, RoutedEventArgs e) {
            RtbLbAdorner = new MpLineAdorner(ClipTileRichTextBoxListBox);
            RtbLbAdornerLayer = AdornerLayer.GetAdornerLayer(ClipTileRichTextBoxListBox);
            RtbLbAdornerLayer.Add(RtbLbAdorner);

            var mwvm = Application.Current.MainWindow.DataContext as MpMainWindowViewModel;
            mwvm.OnTileExpand += MpRtbEditToolbarView_OnTileExpand;
            mwvm.OnTileUnexpand += MpRtbEditToolbarView_OnTileUnexpand;

            UpdateUi();
        }

        private void ClipTileRichTextBoxListBox_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true; 
        }

        private void ClipTileRichTextBoxListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            var rtblbvm = DataContext as MpClipTileViewModel;
            // NOTE This is for selection changed from interface from VM is in another handler
            var srtb = (sender as FrameworkElement).GetVisualDescendent<RichTextBox>();
            if(srtb != null && rtblbvm.Parent.IsEditingContent) {
                EditToolbarView.SetActiveRtb(srtb);
                EditTemplateView.SetActiveRtb(srtb);
                PasteTemplateView.SetActiveRtb(srtb);
            }

            if (rtblbvm.Count > 1) {
                //order selected tiles by ascending datetime 
                var subSelectedRtbvmListBySelectionTime = rtblbvm.SelectedItems.OrderBy(x => x.LastSubSelectedDateTime).ToList();
                foreach (var srtbvm in subSelectedRtbvmListBySelectionTime) {
                    if (srtbvm == subSelectedRtbvmListBySelectionTime[0]) {
                        srtbvm.IsPrimarySubSelected = true;
                    } else {
                        srtbvm.IsPrimarySubSelected = false;
                    }
                }
            } else if (rtblbvm.SelectedItems.Count == 1) {
                rtblbvm.SelectedItems[0].IsPrimarySubSelected = false;
            }

            foreach (var osctvm in e.RemovedItems) {
                if (osctvm.GetType() == typeof(MpContentItemViewModel)) {
                    ((MpContentItemViewModel)osctvm).IsSelected = false;
                    ((MpContentItemViewModel)osctvm).IsPrimarySubSelected = false;
                }
            }
        }
        
        #endregion

        #region Toolbar Events

        private void MpRtbEditToolbarView_OnTileUnexpand(object sender, EventArgs e) {
            EditToolbarView.Visibility = Visibility.Collapsed;

            var rtbcvm = DataContext as MpContentItemViewModel;
            rtbcvm.SaveToDatabase();
            // TODO save models here
        }

        private void MpRtbEditToolbarView_OnTileExpand(object sender, EventArgs e) {
            EditToolbarView.Visibility = Visibility.Visible;
            UpdateUi();
        }
        #endregion

        #region ViewModel Events

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null) {
                var rtbcvm = DataContext as MpClipTileViewModel;
                rtbcvm.OnUiUpdateRequest += Rtbcvm_OnUiUpdateRequest;
                rtbcvm.OnScrollIntoViewRequest += Rtbcvm_OnScrollIntoViewRequest;
                rtbcvm.OnScrollToHomeRequest += Rtbcvm_OnScrollToHomeRequest;
                //rtbcvm.SyncItemsWithModel();
            } 
        }

        private void Rtbcvm_OnUiUpdateRequest(object sender, EventArgs e) {
            UpdateUi();
        }

        private void Rtbcvm_OnScrollToHomeRequest(object sender, EventArgs e) {
            ClipTileRichTextBoxListBox?.GetScrollViewer().ScrollToHome();
        }

        private void Rtbcvm_OnScrollIntoViewRequest(object sender, object e) {
            ClipTileRichTextBoxListBox?.ScrollIntoView(e);
        }

        #endregion


        public void UpdateUi() {
            this.UpdateLayout();
            ClipTileRichTextBoxListBox.Items.Refresh();
        }

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
