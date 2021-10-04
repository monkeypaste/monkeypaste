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
        private MpContentItemSeperatorAdorner seperatorAdorner;
        public AdornerLayer SeperatorAdornerLayer;

        public MpContentListView() {
            InitializeComponent();
        }

        public void UpdateAdorner() {
            if(seperatorAdorner == null) {
                //selection changed is called before load so adorner is null
                return;
            }
            List<int> selectedIdxList = new List<int>();
            if(ContentListBox.SelectedItem != null) {
                var ctvm = DataContext as MpClipTileViewModel;
                selectedIdxList = ctvm.SelectedItems.Select(x => ctvm.ItemViewModels.IndexOf(x)).ToList();
            }
            
            seperatorAdorner.Lines.Clear();
            for (int i = 0; i < ContentListBox.Items.Count-1; i++) {
                if(selectedIdxList.Contains(i) || selectedIdxList.Contains(i+1)) {
                    //hide dotted line if its going to cover selection box
                    continue;
                }
                Rect lbir = ContentListBox.GetListBoxItemRect(i);
                var l = new Point[] { lbir.BottomLeft, lbir.BottomRight };
                seperatorAdorner.Lines.Add(l.ToList());
            }
            seperatorAdorner.IsShowing = true;
            SeperatorAdornerLayer.Update();
        }

        public void HideToolbars() {
            EditToolbarView.Visibility = Visibility.Collapsed;
            EditTemplateView.Visibility = Visibility.Collapsed;
            PasteTemplateView.Visibility = Visibility.Collapsed;

        }
        #region Rtb ListBox Events
        private void ContentListBox_Loaded(object sender, RoutedEventArgs e) {
            seperatorAdorner = new MpContentItemSeperatorAdorner(ContentListBox);
            SeperatorAdornerLayer = AdornerLayer.GetAdornerLayer(ContentListBox);
            SeperatorAdornerLayer.Add(seperatorAdorner);
            UpdateAdorner();

            UpdateUi();
        }

        private void ContentListBox_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true; 
        }
                
        #endregion


        #region ViewModel Events

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null) {
                var rtbcvm = DataContext as MpClipTileViewModel;
                rtbcvm.OnUiUpdateRequest += Rtbcvm_OnUiUpdateRequest;
                rtbcvm.OnScrollIntoViewRequest += Rtbcvm_OnScrollIntoViewRequest;
                rtbcvm.OnScrollToHomeRequest += Rtbcvm_OnScrollToHomeRequest;
                rtbcvm.PropertyChanged += Rtbcvm_PropertyChanged;
            } 
        }

        private void Rtbcvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var rtbcvm = sender as MpClipTileViewModel;
            switch(e.PropertyName) {
                case nameof(rtbcvm.IsBusy):
                    if(!rtbcvm.IsBusy) {
                        UpdateLayout();
                    }
                    break;
            }
            
        }

        private void Rtbcvm_OnUiUpdateRequest(object sender, EventArgs e) {
            UpdateUi();
        }

        private void Rtbcvm_OnScrollToHomeRequest(object sender, EventArgs e) {
            ContentListBox?.GetScrollViewer().ScrollToHome();
        }

        private void Rtbcvm_OnScrollIntoViewRequest(object sender, object e) {
            ContentListBox?.ScrollIntoView(e);
        }

        #endregion


        public void UpdateUi() {
            this.UpdateLayout();
            //ContentListBox.Items.Refresh();
        }

        public void SyncMultiSelectDragButton(bool isOver, bool isDown) {
            string transBrush = Brushes.Transparent.ToString();
            string outerBrush = isOver ? "#FF7CA0CC" : isDown ? "#FF2E4E76" : transBrush;
            string innerBrush = isOver ? "#FFE4EFFD" : isDown ? "#FF116EE4" : transBrush;
            string innerBg = isOver ? "#FFDAE7F5" : isDown ? "#FF3272B8" : transBrush;

            //foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedItems) {
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

        private void ContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateAdorner();
        }
    }
}
