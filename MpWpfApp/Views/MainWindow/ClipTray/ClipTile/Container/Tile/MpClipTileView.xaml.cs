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
        int minDragDist = 25;
        public List<TextBlock> Titles = new List<TextBlock>();

        public MpClipTileView() {
            InitializeComponent();
            PreviewMouseUp += MpClipTileView_PreviewMouseUp_DragDrop;
            MouseMove += MpClipTileView_MouseMove_DragDrop;     
        }

        private void ClipTileClipBorder_Loaded(object sender, RoutedEventArgs e) {
            var mwvm = Application.Current.MainWindow.DataContext as MpMainWindowViewModel;
            mwvm.OnTileExpand += Mwvm_OnTileExpand;
            mwvm.OnTileUnexpand += Mwvm_OnTileUnexpand;
        }

        private void ClipTileClipBorder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext != null && DataContext is MpClipTileViewModel ctvm) {
                ctvm.OnSearchRequest += Ctvm_OnSearchRequest;

                Titles.Clear();
                foreach(var civm in ctvm.ItemViewModels) {
                    Titles.Add(new TextBlock() {
                        Text = civm.CopyItem.Title
                    });
                }
            }
        }

        public async Task<MpHighlightTextRangeViewModelCollection> Search(string hlt) {
            var ctvm = DataContext as MpClipTileViewModel;
            var hltrcvm = new MpHighlightTextRangeViewModelCollection();

            var rtbl = this.GetVisualDescendents<RichTextBox>();
            var tl = new List<Tuple<TextBlock, RichTextBox>>();
            foreach(var rtb in rtbl) {
                var rtbvm = rtb.DataContext as MpContentItemViewModel;
                var tb = Titles.Where(x => x.Text == rtbvm.CopyItem.Title).FirstOrDefault();
                tl.Add(new Tuple<TextBlock, RichTextBox>(tb, rtb));
            }
            await hltrcvm.PerformHighlightingAsync(hlt, tl);

            return hltrcvm;
        }

        #region Selection
        private void ClipTileClipBorder_MouseEnter(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsHovering = true;
        }

        private void ClipTileClipBorder_MouseMove(object sender, MouseEventArgs e) {
            return;

            var ctvm = DataContext as MpClipTileViewModel; 
            var rtblb = this.GetVisualDescendent<MpMultiSelectListBox>();
            var rtbcv = this.GetVisualDescendent<MpContentListView>();
            if (!ctvm.IsSelected || MpClipTrayViewModel.Instance.SelectedItems.Count <= 1) {
                return;
            }
            var mp = e.GetPosition(rtblb);
            bool isOverSubSelectedDragButton = false;
            foreach (var rtbvm in ctvm.ItemViewModels) {
                int lbiIdx = ctvm.ItemViewModels.IndexOf(rtbvm);
                var lbi = rtblb.GetListBoxItem(lbiIdx);
                var rtbvm_canvas = lbi.GetVisualDescendent<Canvas>();
                var itemRect = rtblb.GetListBoxItemRect(lbiIdx);
                rtbvm.IsHovering = itemRect.Contains(mp);
                if (rtbvm.IsHovering) {
                    var rtbv = lbi.GetVisualDescendent<MpContentItemView>();
                    var irmp = e.GetPosition(rtbv.DragButton);
                    var dragButtonRect = rtbv.DragButton.RelativeBounds();// new Rect(0, 0, rtbv.DragButton.ActualWidth, rtbv.DragButton.ActualHeight);
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

        public void ClipTileClipBorder_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            var rtblb = this.GetVisualDescendent<MpMultiSelectListBox>();

            if (!ctvm.IsSelected || MpClipTrayViewModel.Instance.SelectedItems.Count <= 1) {
                return;
            }
            var mp = e.GetPosition(rtblb);
            bool isSubSelection = false;
            if (ctvm.IsSelected/* && ctvm.Count > 1*/) {
                foreach (var rtbvm in ctvm.ItemViewModels) {
                    if (rtbvm.IsHovering) {
                        isSubSelection = true;
                        rtblb.UpdateExtendedSelection(ctvm.ItemViewModels.IndexOf(rtbvm));
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
                foreach (var rtbvm in ctvm.ItemViewModels) {
                    rtbvm.IsHovering = false;
                }
            }
        }

        private void ClipTileClipBorder_LostFocus(object sender, RoutedEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;

            if (!ctvm.IsSelected) {
                ctvm.ClearEditing();
            }
        }
        #endregion

        #region Drag & Drop
        private void MpClipTileView_PreviewMouseUp_DragDrop(object sender, MouseButtonEventArgs e) {
            Application.Current.MainWindow.ForceCursor = false;
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.MouseDownPosition = new Point();
            ctvm.IsClipDragging = false;
            foreach (var rtbvm in ctvm.ItemViewModels) {
                rtbvm.IsSubDragging = false;
            }
            ctvm.DragDataObject = null;
            if (e.MouseDevice.DirectlyOver != null &&
                e.MouseDevice.DirectlyOver.GetType().IsSubclassOf(typeof(UIElement))) {
                if (((UIElement)e.MouseDevice.DirectlyOver).GetType() == typeof(Thumb)) {
                    //ensures scrollbar interaction isn't treated as drag and drop
                    var sb = (ScrollBar)((Thumb)e.MouseDevice.DirectlyOver).TemplatedParent;
                    if (sb.Orientation == Orientation.Vertical) {
                        ctvm.IsMouseOverVerticalScrollBar = false;
                    } else {
                        ctvm.IsMouseOverHorizontalScrollBar = false;
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
                        ctvm.IsMouseOverVerticalScrollBar = true;
                    } else {
                        ctvm.IsMouseOverHorizontalScrollBar = true;
                    }
                    return;
                }
            }
        }

        private void MpClipTileView_MouseMove_DragDrop(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                if (ctvm.IsExpanded ||
                    ctvm.IsMouseOverScrollBar ||
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
            var rtblbv = this.GetVisualDescendent<MpContentListView>();
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
            if (!IsDragDataValid(e.Data)) {
                MonkeyPaste.MpConsole.WriteLine(@"Drag data invalid from drag enter");
                e.Handled = true;
                return;
            }
        }

        private void ClipTileClipBorder_PreviewDragOver(object sender, DragEventArgs e1) {
            var ctvm = DataContext as MpClipTileViewModel;
            var ctrv = (Application.Current.MainWindow as MpMainWindow).ClipTrayView;
            var rtblb = this.GetVisualDescendent<MpMultiSelectListBox>();
            var rtblbv = this.GetVisualDescendent<MpContentListView>();
            MpClipTrayViewModel.Instance.IsTrayDropping = false;
            ctrv.ClipTrayAdornerLayer.Update();


            rtblbv?.UpdateAdorners();

            ctrv.AutoScrollByMouse();

            if (IsDragDataValid(e1.Data)) {
                int dropIdx = ctrv.GetDropIdx(MpHelpers.Instance.GetMousePosition(rtblb));
                MonkeyPaste.MpConsole.WriteLine("DropIdx: " + dropIdx);
                if (dropIdx >= 0 && dropIdx <= ctvm.Count) {
                    if (dropIdx < ctvm.Count) {
                        if (!rtblb.IsListBoxItemVisible(dropIdx)) {
                            rtblb.ScrollIntoView(ctvm.ItemViewModels[dropIdx]);
                        } else if (dropIdx > 0 && dropIdx - 1 < ctvm.Count) {
                            rtblb.ScrollIntoView(ctvm.ItemViewModels[dropIdx - 1]);
                        }
                    } else {
                        //only can be count + 1
                        if (!rtblb.IsListBoxItemVisible(dropIdx - 1)) {
                            rtblb.ScrollIntoView(ctvm.ItemViewModels[dropIdx - 1]);
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
            var rtblbv = this.GetVisualDescendent<MpContentListView>();

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
                        bool wasEmptySelection = dctvm.SelectedItems.Count == 0;
                        if (wasEmptySelection) {
                            dctvm.SubSelectAll();
                        }
                        if (dctvm.Count == 0) {
                            dcil.AddRange(dctvm.ItemViewModels.Select(x=>x.CopyItem).ToList());
                        } else {
                            foreach (var ssrtbvm in dctvm.SelectedItems) {
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
                ctvm.ClearClipSelection();
            }
            e2.Handled = true;
            rtblbv?.UpdateAdorners();
        }

        public bool IsDragDataValid(IDataObject data) {
            var ctvm = DataContext as MpClipTileViewModel;

            if (!ctvm.IsTextItem) {
                return false;
            }
            if (data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                var dctvml = (List<MpClipTileViewModel>)data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                foreach (var dctvm in dctvml) {
                    if ((dctvm == ctvm && !ctvm.IsAnySubItemDragging) ||
                       !dctvm.IsTextItem) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        #endregion



        private void Mwvm_OnTileUnexpand(object sender, EventArgs e) {
            if (sender == DataContext) {
                TileDetailView.Visibility = Visibility.Visible;
            }
        }

        private void Mwvm_OnTileExpand(object sender, EventArgs e) {
            if(sender == DataContext) {
                //TileDetailView.Visibility = Visibility.Collapsed;
            }
        }        

        private void Ctvm_OnSearchRequest(object sender, string e) {
            throw new NotImplementedException();
        }
    }
}
