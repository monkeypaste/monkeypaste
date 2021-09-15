using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileView.xaml
    /// </summary>
    public partial class MpClipTileView : UserControl {
        int minDragDist = 5;

        public MpClipTileView() {
            InitializeComponent();
            PreviewMouseUp += MpClipTileView_PreviewMouseUp_DragDrop;
            MouseMove += MpClipTileView_MouseMove_DragDrop;            
        }


        #region Selection
        private void ClipTileClipBorder_MouseEnter(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsHovering = true;
        }

        private void ClipTileClipBorder_MouseMove(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel; 
            var rtblb = this.GetVisualDescendent<MpMultiSelectListBox>();
            var rtbcv = this.GetVisualDescendent<MpContentListVIew>();
            if (!ctvm.IsSelected || MpClipTrayViewModel.Instance.SelectedClipTiles.Count <= 1) {
                return;
            }
            var mp = e.GetPosition(rtblb);
            bool isOverSubSelectedDragButton = false;
            foreach (var rtbvm in ctvm.ContentContainerViewModel.ItemViewModels) {
                int lbiIdx = ctvm.ContentContainerViewModel.ItemViewModels.IndexOf(rtbvm);
                var lbi = rtblb.GetListBoxItem(lbiIdx);
                var rtbvm_canvas = lbi.GetVisualDescendent<Canvas>();
                var itemRect = rtblb.GetListBoxItemRect(lbiIdx);
                rtbvm.IsSubHovering = itemRect.Contains(mp);
                if (rtbvm.IsSubHovering) {
                    var rtbv = lbi.GetVisualDescendent<MpContentListItemView>();
                    var irmp = e.GetPosition(rtbv.DragButton);
                    var dragButtonRect = new Rect(0, 0, rtbv.DragButton.ActualWidth, rtbv.DragButton.ActualHeight);
                    if (dragButtonRect.Contains(irmp)) {
                        isOverSubSelectedDragButton = true; 
                        rtbcv.SyncMultiSelectDragButton(true, e.MouseDevice.LeftButton == MouseButtonState.Pressed);
                    }
                }
            }
            if (!isOverSubSelectedDragButton) {
                rtbcv.SyncMultiSelectDragButton(false, e.MouseDevice.LeftButton == MouseButtonState.Pressed);
            }
        }

        private void ClipTileClipBorder_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            var rtblb = this.GetVisualDescendent<MpMultiSelectListBox>();

            if (!ctvm.IsSelected || MpClipTrayViewModel.Instance.SelectedClipTiles.Count <= 1) {
                return;
            }
            var mp = e.GetPosition(rtblb);
            bool isSubSelection = false;
            if (ctvm.IsSelected && ctvm.ContentContainerViewModel.Count > 1) {
                foreach (var rtbvm in ctvm.ContentContainerViewModel.ItemViewModels) {
                    if (rtbvm.IsSubHovering) {
                        isSubSelection = true;
                        rtblb.UpdateExtendedSelection(ctvm.ContentContainerViewModel.ItemViewModels.IndexOf(rtbvm));
                    }
                }
            }
            if (isSubSelection) {
                e.Handled = true;
            }
        }

        private void ClipTileClipBorder_MouseLeave(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;

            if (ctvm != null && !ctvm.IsClipDragging) {
                ctvm.IsHovering = false;
                foreach (var rtbvm in ctvm.ContentContainerViewModel.ItemViewModels) {
                    rtbvm.IsSubHovering = false;
                }
            }
        }

        private void ClipTileClipBorder_LostFocus(object sender, RoutedEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;

            if (!ctvm.IsSelected) {
                ctvm.IsEditingTitle = false;
            }
        }
        #endregion

        #region Drag & Drop
        private void MpClipTileView_PreviewMouseUp_DragDrop(object sender, MouseButtonEventArgs e) {
            Application.Current.MainWindow.ForceCursor = false;
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.MouseDownPosition = new Point();
            ctvm.IsClipDragging = false;
            foreach (var rtbvm in ctvm.ContentContainerViewModel.ItemViewModels) {
                rtbvm.IsSubDragging = false;
            }
            ctvm.DragDataObject = null;
            if (e.MouseDevice.DirectlyOver != null &&
                e.MouseDevice.DirectlyOver.GetType().IsSubclassOf(typeof(UIElement))) {
                if (((UIElement)e.MouseDevice.DirectlyOver).GetType() == typeof(Thumb)) {
                    //ensures scrollbar interaction isn't treated as drag and drop
                    var sb = (ScrollBar)((Thumb)e.MouseDevice.DirectlyOver).TemplatedParent;
                    if (sb.Orientation == Orientation.Vertical) {
                        ctvm.ContentContainerViewModel.IsMouseOverVerticalScrollBar = false;
                    } else {
                        ctvm.ContentContainerViewModel.IsMouseOverHorizontalScrollBar = false;
                    }
                    return;
                }
            }
        }

        private void ClipTileClipBorder_MouseDown(object sender, MouseButtonEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (e.MouseDevice.DirectlyOver != null &&
                    e.MouseDevice.DirectlyOver.GetType().IsSubclassOf(typeof(UIElement))) {
                if (((UIElement)e.MouseDevice.DirectlyOver).GetType() == typeof(Thumb)) {
                    //ensures scrollbar interaction isn't treated as drag and drop
                    var sb = (ScrollBar)((Thumb)e.MouseDevice.DirectlyOver).TemplatedParent;
                    if (sb.Orientation == Orientation.Vertical) {
                        ctvm.ContentContainerViewModel.IsMouseOverVerticalScrollBar = true;
                    } else {
                        ctvm.ContentContainerViewModel.IsMouseOverHorizontalScrollBar = true;
                    }
                    return;
                }
            }
        }

        private void MpClipTileView_MouseMove_DragDrop(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                if (ctvm.IsExpanded ||
                    ctvm.ContentContainerViewModel.IsMouseOverScrollBar ||
                    ctvm.IsAnySubItemDragging) {
                    return;
                }
                if (ctvm.MouseDownPosition == new Point()) {
                    ctvm.MouseDownPosition = e.GetPosition(ClipTileClipBorder);
                }
                if (MpHelpers.Instance.DistanceBetweenPoints(ctvm.MouseDownPosition, e.GetPosition(ClipTileClipBorder)) < minDragDist) {
                    return;
                }

                if (ctvm.DragDataObject == null) {
                    ctvm.DragDataObject = MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(true, false).Result;
                }
                DragDrop.DoDragDrop(
                               this,
                               ctvm.DragDataObject,
                               DragDropEffects.Move | DragDropEffects.Copy);
            }
        }
        private void ClipTileClipBorder_DragLeave(object sender, DragEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            var rtblbv = this.GetVisualDescendent<MpContentListVIew>();
            var rtblb = this.GetVisualDescendent<MpMultiSelectListBox>();
            //ctvm.DragDataObject = null;
            ctvm.IsClipDropping = false;
            rtblbv.UpdateAdorners();
            rtblb.ScrollViewer.ScrollToHome();
            MonkeyPaste.MpConsole.WriteLine("ClipTile Dragleave");
        }

        private void ClipTileClipBorder_PreviewDragEnter(object sender, DragEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            var ctv = (Application.Current.MainWindow as MpMainWindow).ClipTrayView;
            ctv.AutoScrollByMouse();

            MonkeyPaste.MpConsole.WriteLine("ClipTile Dragenter");
            if (!ctvm.IsDragDataValid(e.Data)) {
                MonkeyPaste.MpConsole.WriteLine(@"Drag data invalid from drag enter");
                e.Handled = true;
                return;
            }
        }

        private void ClipTileClipBorder_PreviewDragOver(object sender, DragEventArgs e1) {
            var ctvm = DataContext as MpClipTileViewModel;
            var ctrv = (Application.Current.MainWindow as MpMainWindow).ClipTrayView;
            var rtblb = this.GetVisualDescendent<MpMultiSelectListBox>();
            var rtblbv = this.GetVisualDescendent<MpContentListVIew>();
            MpClipTrayViewModel.Instance.IsTrayDropping = false;
            ctrv.ClipTrayAdornerLayer.Update();


            rtblbv?.UpdateAdorners();

            ctrv.AutoScrollByMouse();

            if (ctvm.IsDragDataValid(e1.Data)) {
                int dropIdx = ctrv.GetDropIdx(MpHelpers.Instance.GetMousePosition(rtblb));
                MonkeyPaste.MpConsole.WriteLine("DropIdx: " + dropIdx);
                if (dropIdx >= 0 && dropIdx <= ctvm.ContentContainerViewModel.Count) {
                    if (dropIdx < ctvm.ContentContainerViewModel.Count) {
                        if (!rtblb.IsListBoxItemVisible(dropIdx)) {
                            rtblb.ScrollIntoView(ctvm.ContentContainerViewModel.ItemViewModels[dropIdx]);
                        } else if (dropIdx > 0 && dropIdx - 1 < ctvm.ContentContainerViewModel.Count) {
                            rtblb.ScrollIntoView(ctvm.ContentContainerViewModel.ItemViewModels[dropIdx - 1]);
                        }
                    } else {
                        //only can be count + 1
                        if (!rtblb.IsListBoxItemVisible(dropIdx - 1)) {
                            rtblb.ScrollIntoView(ctvm.ContentContainerViewModel.ItemViewModels[dropIdx - 1]);
                        }
                    }
                    rtblbv.RtbLbAdorner.Point1 = rtblb.GetAdornerPoints(dropIdx,false)[0];
                    rtblbv.RtbLbAdorner.Point2 = rtblb.GetAdornerPoints(dropIdx,false)[1];
                    ctvm.IsClipDropping = true;
                    e1.Effects = DragDropEffects.Move;
                    e1.Handled = true;

                    MpClipTrayViewModel.Instance.IsTrayDropping = false;
                    ctrv.ClipTrayAdornerLayer?.Update();
                }
            } else {
                MonkeyPaste.MpConsole.WriteLine(@"Drag data invalid from drag over");
                ctvm.IsClipDropping = false;
                //e1.Effects = DragDropEffects.None;
                e1.Handled = true;
            }
            rtblbv?.UpdateAdorners();
        }

        private void ClipTileClipBorder_PreviewDrop(object sender, DragEventArgs e2) {
            var ctvm = DataContext as MpClipTileViewModel;
            var ctv = (Application.Current.MainWindow as MpMainWindow).ClipTrayView;
            var rtblb = this.GetDescendantOfType<MpMultiSelectListBox>();
            var rtblbv = this.GetVisualDescendent<MpContentListVIew>();

            bool wasDropped = false;
            var dctvml = new List<MpClipTileViewModel>();
            if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                dctvml = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                int dropIdx = ctv.GetDropIdx(MpHelpers.Instance.GetMousePosition(rtblb));
                if (dropIdx >= 0) {
                    /*
                     On tile drop: 
                     0. order same as tray drop 
                     1. take all non composite copyitems from sctvm and remove empty tiles if NOT drop tile. 
                     2. Then insert at dropidx where copyitems will still be hctvm selected datetime order. 
                    */
                    var dcil = new List<MpCopyItem>();
                    foreach (var dctvm in dctvml) {
                        bool wasEmptySelection = dctvm.ContentContainerViewModel.SubSelectedContentItems.Count == 0;
                        if (wasEmptySelection) {
                            dctvm.ContentContainerViewModel.SubSelectAll();
                        }
                        if (dctvm.ContentContainerViewModel.Count == 0) {
                            dcil.Add(dctvm.CopyItem);
                        } else {
                            foreach (var ssrtbvm in dctvm.ContentContainerViewModel.SubSelectedContentItems) {
                                dcil.Add(ssrtbvm.CopyItem);
                            }
                        }
                    }
                    //dcil.Reverse();
                    ctvm.MergeCopyItemList(dcil, dropIdx);
                    wasDropped = true;
                }
            }
            if (wasDropped) {
                MpClipTrayViewModel.Instance.ClearAllDragDropStates();
                ctvm.ContentContainerViewModel.ClearSubSelection();
            }
            e2.Handled = true;
            rtblbv?.UpdateAdorners();
        }

        #endregion
    }
}
