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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpContentItemView.xaml
    /// </summary>
    public partial class MpContentItemView : UserControl {
        private int _minDragDist = 25;

        //AdornerLayer RtbItemAdornerLayer;
        //MpLineAdorner RtbItemAdorner;

        public MpContentItemView() : base() {
            InitializeComponent();            
        }


        private void ContentListItemView_Loaded(object sender, RoutedEventArgs e) {
            //RtbItemAdorner = new MpLineAdorner(EditorView);
            //RtbItemAdornerLayer = AdornerLayer.GetAdornerLayer(EditorView);
            //RtbItemAdornerLayer?.Add(RtbItemAdorner);

            //UpdateAdorner();

            var mwvm = Application.Current.MainWindow.DataContext as MpMainWindowViewModel;
            mwvm.OnTileExpand += MainWindowViewModel_OnTileExpand;
            mwvm.OnTileUnexpand += MainWindowViewModel_OnTileUnexpand;

            var civm = DataContext as MpContentItemViewModel;
            var scvml = MpShortcutCollectionViewModel.Instance.Shortcuts.Where(x => x.CopyItemId == civm.CopyItem.Id).ToList();
            if (scvml.Count > 0) {
                civm.ShortcutKeyString = scvml[0].KeyString;
            } else {
                civm.ShortcutKeyString = string.Empty;
            }
        }

        private void MainWindowViewModel_OnTileUnexpand(object sender, EventArgs e) {
            EditorView.Rtb.FitDocToRtb();
        }

        private void MainWindowViewModel_OnTileExpand(object sender, EventArgs e) {
            EditorView.Rtb.FitRtbToDoc();
        }

        private void ContentListItemView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null && DataContext is MpContentItemViewModel civm) {
                civm.OnScrollWheelRequest += Civm_OnScrollWheelRequest;
                civm.OnUiUpdateRequest += Civm_OnUiUpdateRequest;
            }
        }

        public void UpdateAdorner() {
            //RtbItemAdorner.Points = 
            //RtbItemAdornerLayer.Update();
        }

        #region Event Handlers

        #region View Model Ui Requests

        private void Civm_OnUiUpdateRequest(object sender, EventArgs e) {
            this.UpdateLayout();
        }

        private void Civm_OnScrollWheelRequest(object sender, int e) {
            
        }

        #endregion

        #region Drag & Drop

        private void ContentListItemView_MouseEnter(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpContentItemViewModel;
            rtbvm.IsHovering = true;
        }

        private void ContentListItemView_MouseLeave(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpContentItemViewModel;
            rtbvm.IsHovering = false;
        }
        
        private void DragButton_PreviewGiveFeedback(object sender, GiveFeedbackEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                Application.Current.MainWindow.Cursor = Cursors.Cross;
                Application.Current.MainWindow.ForceCursor = true;
            }
        }

        private void DragButton_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var rtbvm = DataContext as MpContentItemViewModel;
            Application.Current.MainWindow.ForceCursor = false;
            rtbvm.MouseDownPosition = new Point();
            rtbvm.DragDataObject = null;
            rtbvm.IsSubDragging = false;
           // SyncMultiSelectDragButton(true, false);
        }


        private void DragButton_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var rtbvm = DataContext as MpContentItemViewModel;
            //SyncMultiSelectDragButton(true, true);
        }

        private void DragButton_PreviewMouseMove(object sender, MouseEventArgs e7) {
            var rtbvm = DataContext as MpContentItemViewModel;
            if (e7.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                if (rtbvm.IsEditingContent ||
                  (rtbvm.Parent.IsExpanded && rtbvm.Parent.Count == 1)) {
                    //cannot resort w/ only 1 item and its relative location is not clear
                    //since its isolated
                    return;
                }
                //SyncMultiSelectDragButton(false, true);
                if (rtbvm.MouseDownPosition == new Point()) {
                    rtbvm.MouseDownPosition = e7.GetPosition(ContentListItemViewGrid);
                }
                if (MpHelpers.Instance.DistanceBetweenPoints(rtbvm.MouseDownPosition, e7.GetPosition(ContentListItemViewGrid)) < _minDragDist) {
                    return;
                }
                rtbvm.IsSubDragging = true;
                rtbvm.IsSelected = true;
                if (rtbvm.DragDataObject == null) {
                    rtbvm.DragDataObject = MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(true, false).Result;//RichTextBoxViewModelCollection.GetDataObjectFromSubSelectedItems(true).Result;
                }
                DragDrop.DoDragDrop(
                            ContentListItemViewGrid,
                            rtbvm.DragDataObject,
                            DragDropEffects.Copy | DragDropEffects.Move);
                e7.Handled = true;
            }
        }

        private void DragButton_MouseEnter(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpContentItemViewModel;
            rtbvm.IsOverDragButton = true;
            //SyncMultiSelectDragButton(true, false);
        }

        private void DragButton_MouseLeave(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpContentItemViewModel;
            rtbvm.IsOverDragButton = false;
           // SyncMultiSelectDragButton(false, false);
        }

        //public bool IsDragDataValid(IDataObject data) {
        //    var rtbvm = DataContext as MpContentItemViewModel;
        //    if (rtbvm.CopyItem.ItemType == MpCopyItemType.Image || rtbvm.CopyItem.ItemType == MpCopyItemType.FileList) {
        //    return false;
        //    }
        //    if (data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
        //        var dctvml = (List<MpClipTileViewModel>)data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
        //        foreach (var dctvm in dctvml) {
        //            if ((dctvm == this && !IsAnySubItemDragging) ||
        //               dctvm.CopyItem.ItemType == MpCopyItemType.Image ||
        //               dctvm.CopyItemType == MpCopyItemType.FileList) {
        //                return false;
        //            }
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        #endregion

        #endregion
    }
}
