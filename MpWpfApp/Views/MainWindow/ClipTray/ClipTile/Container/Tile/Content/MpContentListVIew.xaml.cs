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
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpContentListVIew.xaml
    /// </summary>
    public partial class MpContentListView : MpUserControl<MpClipTileViewModel> {
        private MpContentItemSeperatorAdorner seperatorAdorner;
        public AdornerLayer SeperatorAdornerLayer;

        public MpContentListView() : base() {
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

            ContentListDropBehavior.Attach(this);
            
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
            }
            if(e.NewValue != null && e.NewValue is MpClipTileViewModel nctvm) {
                nctvm.OnUiUpdateRequest += Rtbcvm_OnUiUpdateRequest;
                nctvm.OnScrollIntoViewRequest += Rtbcvm_OnScrollIntoViewRequest;
                nctvm.OnScrollToHomeRequest += Rtbcvm_OnScrollToHomeRequest;
                nctvm.PropertyChanged += Rtbcvm_PropertyChanged;
                nctvm.OnListBoxRefresh += Octvm_OnListBoxRefresh;
            } 
        }

        public async Task RefreshContext() {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                for (int i = 0; i < ContentListBox.Items.Count; i++) {
                    var lbi = ContentListBox.GetListBoxItem(i);
                    if (lbi != null) {
                        lbi.DataContext = BindingContext.ItemViewModels[i];
                    }
                }
            });
        }

        private void Rtbcvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var ctvm = sender as MpClipTileViewModel;
            switch (e.PropertyName) {
                case nameof(ctvm.IsSelected):
                    break;
                case nameof(ctvm.IsBusy):
                    if(!ctvm.IsBusy) {
                        UpdateLayout();
                    }
                    break;
            }            
        }

        private void Octvm_OnListBoxRefresh(object sender, EventArgs e) {
            ContentListBox?.Items.Refresh();
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
                var sv = ContentListBox.GetScrollViewer();
                double yOffset = sv.VerticalOffset + e;
                sv.ScrollToVerticalOffset(yOffset);
            }
        }

        public void UpdateUi() {
            UpdateAdorner();
            if(ContentListBox.Items.Count > 1) {
                ContentListBox.Items.Refresh();
            }
            this.UpdateLayout();
        }

        private void ContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateAdorner();
        }

        private void ContentListBox_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            UpdateAdorner();
        }

        public async Task<FlowDocument> GetSeparatedCompositeFlowDocument(string separatorChar = "- ") {
            if(!BindingContext.IsTextItem) {
                return new FlowDocument();
            }

            int maxCols = int.MinValue;
            var rtbl = this.GetVisualDescendents<RichTextBox>().ToList();
            for (int i = 0; i < rtbl.Count; i++) {
                maxCols = Math.Max(maxCols, MpHelpers.Instance.GetColCount(rtbl[i].Document.ToPlainText()));
            }

            string separatorLine = string.Empty;
            for (int i = 0; i < maxCols; i++) {
                separatorLine += separatorChar;
            }
            var separatorDocument = separatorLine.ToRichText().ToFlowDocument();
            var fullDocument = string.Empty.ToRichText().ToFlowDocument();
            for (int i = 0; i < rtbl.Count; i++) {
                var rtb = rtbl[i];
                if (i != 0) {
                    await MpHelpers.Instance.CombineFlowDocumentsAsync(
                    separatorDocument,
                    fullDocument,
                    false);
                }
                await MpHelpers.Instance.CombineFlowDocumentsAsync(
                    (MpEventEnabledFlowDocument)rtb.Document,
                    fullDocument,
                    false);
            }

            var ps = fullDocument.GetDocumentSize();
            fullDocument.PageWidth = ps.Width;
            fullDocument.PageHeight = ps.Height;
            return fullDocument;
        }

        private void ContentListDockPanel_Unloaded(object sender, RoutedEventArgs e) {
            ContentListDropBehavior.Detach();

            if(BindingContext == null) {
                return;
            }
            BindingContext.OnUiUpdateRequest -= Rtbcvm_OnUiUpdateRequest;
            BindingContext.OnScrollIntoViewRequest -= Rtbcvm_OnScrollIntoViewRequest;
            BindingContext.OnScrollToHomeRequest -= Rtbcvm_OnScrollToHomeRequest;
            BindingContext.PropertyChanged -= Rtbcvm_PropertyChanged;
            BindingContext.OnListBoxRefresh -= Octvm_OnListBoxRefresh;
        }
    }
}
