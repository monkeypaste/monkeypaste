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
    /// Interaction logic for MpClipTileContainerView.xaml
    /// </summary>
    public partial class MpClipTileContainerView : ListBoxItem {
        int minDragDist = 5;

        public MpClipTileContainerView() : base() {
            InitializeComponent();
            //ClipBorder.PreviewMouseUp += ClipBorder_PreviewMouseUp_DragDrop;
            //ClipBorder.MouseMove += ClipBorder_MouseMove_DragDrop;
        }

        public void Resize(
            double deltaWidth,
            double deltaHeight,
            double deltaEditToolbarTop) {
            var ctvm = DataContext as MpClipTileViewModel;

            ctvm.TileBorderWidth += deltaWidth;
            ctvm.TileContentWidth += deltaWidth;

            ctvm.TileBorderHeight += deltaHeight;
            ctvm.TileContentHeight += deltaHeight;

            ctvm.EditRichTextBoxToolbarViewModel.Resize(deltaEditToolbarTop, deltaWidth);

            ctvm.RichTextBoxViewModelCollection.Resize(deltaEditToolbarTop, deltaWidth, deltaHeight);

            ctvm.EditTemplateToolbarViewModel.Resize(deltaHeight);

            ctvm.PasteTemplateToolbarViewModel.Resize(deltaHeight);
        }

        #region Event Handlers

        private void ClipBorder_Loaded(object sender, RoutedEventArgs e) {

        }

        private void ClipBorder_MouseEnter(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;            

            ctvm.IsHovering = true;
        }

        private void ClipBorder_MouseMove_Selection(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            var mwvm = (Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel;

            if (!ctvm.IsSelected || mwvm.ClipTrayViewModel.SelectedClipTiles.Count <= 1) {
                return;
            }
            //var mp = e.GetPosition(ctvm.RichTextBoxViewModelCollection.ListBox);
            bool isOverSubSelectedDragButton = false;
            foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                //var itemRect = ctvm.RichTextBoxViewModelCollection.GetListBoxItemRect(ctvm.RichTextBoxViewModelCollection.IndexOf(rtbvm));
                //rtbvm.IsSubHovering = itemRect.Contains(mp);
                if (rtbvm.IsSubHovering) {
                    var irmp = e.GetPosition(rtbvm.Rtbc);
                    var dragButtonRect = rtbvm.DragButtonRect;
                    if (dragButtonRect.Contains(irmp)) {
                        isOverSubSelectedDragButton = true;
                        ctvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(true, e.MouseDevice.LeftButton == MouseButtonState.Pressed);
                    }
                }
            }
            if (!isOverSubSelectedDragButton) {
                ctvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(false, e.MouseDevice.LeftButton == MouseButtonState.Pressed);
            }
        }

        private void ClipBorder_PreviewMouseUp_Selection(object sender, MouseButtonEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            
            var mwvm = (Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel;

            if (!ctvm.IsSelected || mwvm.ClipTrayViewModel.SelectedClipTiles.Count <= 1) {
                return;
            }
            var mp = e.GetPosition(ctvm.RichTextBoxViewModelCollection.ListBox);
            bool isSubSelection = false;
            if (ctvm.IsSelected && ctvm.RichTextBoxViewModelCollection.Count > 1) {
                foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                    if (rtbvm.IsSubHovering) {
                        isSubSelection = true;
                        ctvm.RichTextBoxViewModelCollection.UpdateExtendedSelection(ctvm.RichTextBoxViewModelCollection.IndexOf(rtbvm));
                    }
                }
            }
            if (isSubSelection) {
                e.Handled = true;
            }
        }

        private void ClipBorder_PreviewMouseUp_DragDrop(object sender, MouseButtonEventArgs e) {
            Application.Current.MainWindow.ForceCursor = false;
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.MouseDownPosition = new Point();
            ctvm.IsClipDragging = false;
            foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                rtbvm.IsSubDragging = false;
            }
            ctvm.DragDataObject = null;
            if (e.MouseDevice.DirectlyOver != null &&
                e.MouseDevice.DirectlyOver.GetType().IsSubclassOf(typeof(UIElement))) {
                if (((UIElement)e.MouseDevice.DirectlyOver).GetType() == typeof(Thumb)) {
                    //ensures scrollbar interaction isn't treated as drag and drop
                    var sb = (ScrollBar)((Thumb)e.MouseDevice.DirectlyOver).TemplatedParent;
                    if (sb.Orientation == Orientation.Vertical) {
                        ctvm.RichTextBoxViewModelCollection.IsMouseOverVerticalScrollBar = false;
                    } else {
                        ctvm.RichTextBoxViewModelCollection.IsMouseOverHorizontalScrollBar = false;
                    }
                    return;
                }
            }
        }

        private void ClipBorder_MouseLeave(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (ctvm != null && !ctvm.IsClipDragging) {
                ctvm.IsHovering = false;
                foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                    rtbvm.IsSubHovering = false;
                }
            }
        }

        private void ClipBorder_LostFocus(object sender, RoutedEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (!ctvm.IsSelected) {
                ctvm.IsEditingTitle = false;
            }
        }

        private void ClipBorder_MouseDown(object sender, MouseButtonEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (e.MouseDevice.DirectlyOver != null &&
                    e.MouseDevice.DirectlyOver.GetType().IsSubclassOf(typeof(UIElement))) {
                if (((UIElement)e.MouseDevice.DirectlyOver).GetType() == typeof(Thumb)) {
                    //ensures scrollbar interaction isn't treated as drag and drop
                    var sb = (ScrollBar)((Thumb)e.MouseDevice.DirectlyOver).TemplatedParent;
                    if (sb.Orientation == Orientation.Vertical) {
                        ctvm.RichTextBoxViewModelCollection.IsMouseOverVerticalScrollBar = true;
                    } else {
                        ctvm.RichTextBoxViewModelCollection.IsMouseOverHorizontalScrollBar = true;
                    }
                    return;
                }
            }
        }


        private void ClipBorder_MouseMove_DragDrop(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel; 
            var mwvm = (Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel;

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                if (ctvm.IsExpanded ||
                    ctvm.RichTextBoxViewModelCollection.IsMouseOverScrollBar ||
                    ctvm.IsAnySubItemDragging) {
                    return;
                }
                if (ctvm.MouseDownPosition == new Point()) {
                    ctvm.MouseDownPosition = e.GetPosition(ClipBorder);
                }
                if (MpHelpers.Instance.DistanceBetweenPoints(ctvm.MouseDownPosition, e.GetPosition(ClipBorder)) < minDragDist) {
                    return;
                }

                if (ctvm.DragDataObject == null) {
                    ctvm.DragDataObject = mwvm.ClipTrayViewModel.GetDataObjectFromSelectedClips(true, false).Result;
                }
                DragDrop.DoDragDrop(
                               ((FrameworkElement)sender),
                               ctvm.DragDataObject,
                               DragDropEffects.Move | DragDropEffects.Copy);
            }
        }

        private void ClipBorder_DragLeave(object sender, DragEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsClipDropping = false;
            ctvm.RichTextBoxViewModelCollection.UpdateAdorners();
            ctvm.RichTextBoxViewModelCollection.ScrollViewer?.ScrollToHome();
            Console.WriteLine("ClipTile Dragleave");
        }

        private void ClipBorder_PreviewDragEnter(object sender, DragEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            var mwvm = (Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel;

            mwvm.ClipTrayViewModel.AutoScrollByMouse();
            Console.WriteLine("ClipTile Dragenter");
            if (!ctvm.IsDragDataValid(e.Data)) {
                Console.WriteLine(@"Drag data invalid from drag enter");
                e.Handled = true;
                return;
            }
        }

        private void ClipBorder_PreviewDragOver(object sender, DragEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            var mw = Application.Current.MainWindow as MpMainWindow;
            var mwvm = mw.DataContext as MpMainWindowViewModel;

            mwvm.ClipTrayViewModel.IsTrayDropping = false;
            mw.ClipTrayView.ClipTrayAdornerLayer?.Update();
            //MainWindowViewModel.ClipTrayViewModel.ClipTrayAdornerLayer.Update();


            ctvm.RichTextBoxViewModelCollection.UpdateAdorners();

            mwvm.ClipTrayViewModel.AutoScrollByMouse();

            if (ctvm.IsDragDataValid(e.Data)) {
                int dropIdx = ctvm.GetDropIdx(MpHelpers.Instance.GetMousePosition(ctvm.RichTextBoxViewModelCollection.ListBox));
                Console.WriteLine("DropIdx: " + dropIdx);
                if (dropIdx >= 0 && dropIdx <= ctvm.RichTextBoxViewModelCollection.Count) {
                    if (dropIdx < ctvm.RichTextBoxViewModelCollection.Count) {
                        if (!ctvm.RichTextBoxViewModelCollection.IsListBoxItemVisible(dropIdx)) {
                            ctvm.RichTextBoxViewModelCollection.ListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx]);
                        } else if (dropIdx > 0 && dropIdx - 1 < ctvm.RichTextBoxViewModelCollection.Count) {
                            ctvm.RichTextBoxViewModelCollection.ListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx - 1]);
                        }
                    } else {
                        //only can be count + 1
                        if (!ctvm.RichTextBoxViewModelCollection.IsListBoxItemVisible(dropIdx - 1)) {
                            ctvm.RichTextBoxViewModelCollection.ListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx - 1]);
                        }
                    }
                    ctvm.RichTextBoxViewModelCollection.DropLeftPoint = ctvm.RichTextBoxViewModelCollection.GetAdornerPoints(dropIdx)[0];
                    ctvm.RichTextBoxViewModelCollection.DropRightPoint = ctvm.RichTextBoxViewModelCollection.GetAdornerPoints(dropIdx)[1];
                    ctvm.IsClipDropping = true;
                    e.Effects = DragDropEffects.Move;
                    e.Handled = true;

                    mwvm.ClipTrayViewModel.IsTrayDropping = false;
                    mw.ClipTrayView.ClipTrayAdornerLayer?.Update();
                }
            } else {
                Console.WriteLine(@"Drag data invalid from drag over");
                ctvm.IsClipDropping = false;
                //e1.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            ctvm.RichTextBoxViewModelCollection.UpdateAdorners();
        }

        private void ClipBorder_PreviewDrop(object sender, DragEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            var mwvm = (Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel;

            bool wasDropped = false;
            var dctvml = new List<MpClipTileViewModel>();
            if (e.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                dctvml = (List<MpClipTileViewModel>)e.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                int dropIdx = ctvm.GetDropIdx(MpHelpers.Instance.GetMousePosition(ctvm.RichTextBoxViewModelCollection.ListBox));
                if (dropIdx >= 0) {
                    /*
                     On tile drop: 
                     0. order same as tray drop 
                     1. take all non composite copyitems from sctvm and remove empty tiles if NOT drop tile. 
                     2. Then insert at dropidx where copyitems will still be hctvm selected datetime order. 
                    */
                    var dcil = new List<MpCopyItem>();
                    foreach (var dctvm in dctvml) {
                        bool wasEmptySelection = dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0;
                        if (wasEmptySelection) {
                            dctvm.RichTextBoxViewModelCollection.SubSelectAll();
                        }
                        if (dctvm.RichTextBoxViewModelCollection.Count == 0) {
                            dcil.Add(dctvm.CopyItem);
                        } else {
                            foreach (var ssrtbvm in dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems) {
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
                mwvm.ClipTrayViewModel.ClearAllDragDropStates();
                ctvm.RichTextBoxViewModelCollection.ClearSubSelection();
            }
            e.Handled = true;
            ctvm.RichTextBoxViewModelCollection.UpdateAdorners();
        }

        private void ClipTileTitleDetailTextBlock_MouseEnter(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.OnPropertyChanged(nameof(ctvm.DetailText));
        }
        #endregion

        private void ClipBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed) {
                ContextMenu = new MpClipTileContextMenu();
                ContextMenu.PlacementTarget = this;
                ContextMenu.IsOpen = true;
            }
        }
    }
}
