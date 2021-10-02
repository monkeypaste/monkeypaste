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
        private AdornerLayer adornerLayer;

        public MpContentListView() {
            InitializeComponent();
        }

        public void UpdateAdorner() {
            seperatorAdorner.Lines.Clear();
            for (int i = 0; i < ContentListBox.Items.Count-1; i++) {
                Rect lbir = ContentListBox.GetListBoxItemRect(i);
                var l = new Point[] { lbir.BottomLeft, lbir.BottomRight };
                seperatorAdorner.Lines.Add(l.ToList());
            }
            //seperatorAdorner.IsShowing = true;
           // adornerLayer.Update();
        }

        public void HideToolbars() {
            EditToolbarView.Visibility = Visibility.Collapsed;
            EditTemplateView.Visibility = Visibility.Collapsed;
            PasteTemplateView.Visibility = Visibility.Collapsed;

        }
        #region Rtb ListBox Events
        private void ContentListBox_Loaded(object sender, RoutedEventArgs e) {

            

            seperatorAdorner = new MpContentItemSeperatorAdorner(ContentListBox);
            adornerLayer = AdornerLayer.GetAdornerLayer(ContentListBox);
            adornerLayer.Add(seperatorAdorner);
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
                        var ctv = this.GetVisualAncestor<MpClipTileView>();
                        if (ctv != null) {
                            ctv.OnExpandCompleted += Ctv_OnExpandCompleted;
                            ctv.OnUnexpandCompleted += Ctv_OnUnexpandCompleted;
                        }
                    }
                    break;
            }
            
        }

        private void Ctv_OnUnexpandCompleted(object sender, EventArgs e) {
            UpdateAdorner();
        }

        private void Ctv_OnExpandCompleted(object sender, EventArgs e) {
            UpdateAdorner();
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
            ContentListBox.Items.Refresh();
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
    }
}
