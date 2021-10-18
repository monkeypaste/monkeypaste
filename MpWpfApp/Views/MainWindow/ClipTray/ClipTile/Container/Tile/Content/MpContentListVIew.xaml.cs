using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private static int _ContentListViewCount = 0;
        private int _clvIdx = -1;

        private MpContentItemSeperatorAdorner seperatorAdorner;
        public AdornerLayer SeperatorAdornerLayer;

        public MpContentListView() {
            if(_clvIdx < 0) {
                _clvIdx = _ContentListViewCount++;
            }
            InitializeComponent();
        }

        public void UpdateAdorner() {
            //NOTE DISABLED IN ADORNER RENDER

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
            seperatorAdorner.IsShowing = ContentListBox.Items.Count > 1;
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
            if(e.OldValue != null && e.OldValue is MpClipTileViewModel octvm) {
                octvm.OnUiUpdateRequest -= Rtbcvm_OnUiUpdateRequest;
                octvm.OnScrollIntoViewRequest -= Rtbcvm_OnScrollIntoViewRequest;
                octvm.OnScrollToHomeRequest -= Rtbcvm_OnScrollToHomeRequest;
                octvm.PropertyChanged -= Rtbcvm_PropertyChanged;
                octvm.OnListBoxRefresh -= Octvm_OnListBoxRefresh;
                octvm.ItemViewModels.CollectionChanged -= ItemViewModels_CollectionChanged;
            }
            if(e.NewValue != null && e.NewValue is MpClipTileViewModel nctvm) {
                if(!nctvm.IsPlaceholder) {
                    nctvm.OnUiUpdateRequest += Rtbcvm_OnUiUpdateRequest;
                    nctvm.OnScrollIntoViewRequest += Rtbcvm_OnScrollIntoViewRequest;
                    nctvm.OnScrollToHomeRequest += Rtbcvm_OnScrollToHomeRequest;
                    nctvm.PropertyChanged += Rtbcvm_PropertyChanged;
                    nctvm.OnListBoxRefresh += Octvm_OnListBoxRefresh;
                    nctvm.ItemViewModels.CollectionChanged += ItemViewModels_CollectionChanged;
                }                
            } 
        }

        

        private void ItemViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            MpConsole.WriteLine($"Tile List ({_clvIdx}) collection changed event: " + Enum.GetName(typeof(NotifyCollectionChangedAction), e.Action));

            for (int i = 0; i < ContentListBox.Items.Count; i++) {
                ContentListBox.GetListBoxItem(i).DataContext = ctvm.ItemViewModels[i];
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

        private void Octvm_OnListBoxRefresh(object sender, EventArgs e) {
            UpdateLayout();
            ContentListBox.Items.Refresh();
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

        public void Civm_OnScrollWheelRequest(object sender, int e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (ctvm.IsExpanded) {
                ContentListBox.ScrollViewer.ScrollToVerticalOffset(ContentListBox.ScrollViewer.VerticalOffset + e);
            }
        }

        public void UpdateUi() {
            UpdateAdorner();
            this.UpdateLayout();
        }

        private void ContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateAdorner();
        }

        private void ContentListBox_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            UpdateAdorner();
        }
    }
}
