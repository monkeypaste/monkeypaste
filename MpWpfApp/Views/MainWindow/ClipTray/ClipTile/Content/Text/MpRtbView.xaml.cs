using GongSolutions.Wpf.DragDrop.Utilities;
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
    /// Interaction logic for Mpxaml
    /// </summary>
    public partial class MpRtbView : UserControl {
        public MpRtbView() {
            InitializeComponent();            
        }
        


        public void SyncModels() {
            var rtbvm = DataContext as MpRtbItemViewModel;
            
            //clear any search highlighting when saving the document then restore after save
            rtbvm.HostClipTileViewModel.HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(rtbvm);

            rtbvm.HostClipTileViewModel.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(Rtb);
            
            Rtb.ClearHyperlinks();

            rtbvm.CopyItem.ItemData = Rtb.Document.ToRichText();

            Rtb.CreateHyperlinks();

            rtbvm.HostClipTileViewModel.HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(rtbvm);

            var scvml = MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == rtbvm.CopyItemId).ToList();
            if (scvml.Count > 0) {
                rtbvm.ShortcutKeyString = scvml[0].KeyString;
            }
        }


        private void Rtb_Loaded(object sender, RoutedEventArgs e) {
            if (DataContext != null && DataContext is MpRtbItemViewModel rtbivm) {
                rtbivm.OnRtbResetRequest += Rtbivm_OnRtbResetRequest;
                rtbivm.OnScrollWheelRequest += Rtbivm_OnScrollWheelRequest;
                rtbivm.OnUiUpdateRequest += Rtbivm_OnUiUpdateRequest;
                rtbivm.OnClearHyperlinksRequest += Rtbivm_OnClearHyperlinksRequest;
                rtbivm.OnCreateHyperlinksRequest += Rtbivm_OnCreateHyperlinksRequest;
                rtbivm.OnSyncModels += Rtbivm_OnSyncModels;

                if (rtbivm.HostClipTileViewModel.WasAddedAtRuntime) {
                    //force new items to have left alignment
                    Rtb.CaretPosition = Rtb.Document.ContentStart;
                    Rtb.Document.TextAlignment = TextAlignment.Left;
                    UpdateLayout();
                }

                SyncModels();
            }
        }

        private void Rtbivm_OnSyncModels(object sender, EventArgs e) {
            SyncModels();
        }

        private void Rtbivm_OnCreateHyperlinksRequest(object sender, EventArgs e) {
            Rtb.CreateHyperlinks();
        }

        private void Rtbivm_OnClearHyperlinksRequest(object sender, EventArgs e) {
            Rtb.ClearHyperlinks();
        }

        private void Rtbivm_OnUiUpdateRequest(object sender, EventArgs e) {
            Rtb.UpdateLayout();
        }

        private void Rtbivm_OnScrollWheelRequest(object sender, int e) {
            Rtb.ScrollToVerticalOffset(Rtb.VerticalOffset + e);
        }

        private void Rtbivm_OnRtbResetRequest(object sender, bool focusRtb) {
            Rtb.ScrollToHome();
            Rtb.CaretPosition = Rtb.Document.ContentStart;
            if(focusRtb) {
                Rtb.Focus();
            }
        }

        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            if (rtbvm.IsEditingContent && Rtb.IsFocused) {
            }
        }

        private void Rtb_TextChanged(object sender, TextChangedEventArgs e) {
            var rtblb = this.FindParentOfType<MpMultiSelectListBox>();
            rtblb?.UpdateLayout();
        }

        private void Rtb_GotFocus(object sender, RoutedEventArgs e) {
            var plv = this.GetVisualAncestor<MpContentListView>();
            if (plv != null) {
                var et = plv.GetVisualDescendent<MpRtbEditToolbarView>();
                if(et != null) {
                    et.SetActiveRtb(Rtb);
                }
            }
        }
    }
}
