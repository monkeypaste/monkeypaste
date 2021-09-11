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
    /// Interaction logic for MpClipTrayView.xaml
    /// </summary>
    public partial class MpClipTrayView : UserControl {
        public AdornerLayer ClipTrayAdornerLayer;
        public MpClipTrayAdorner ClipTrayAdorner;
        public VirtualizingStackPanel ClipTrayVirtualizingStackPanel;

        public MpClipTrayView() {
            InitializeComponent();

            ClipTrayAdorner = new MpClipTrayAdorner(ClipTray);
            ClipTrayAdornerLayer = AdornerLayer.GetAdornerLayer(ClipTray);
            ClipTrayAdornerLayer.Add(ClipTrayAdorner);
        }
        private void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            ClipTray.ScrollViewer.Margin = new Thickness(5, 0, 5, 0);
        }

        private void ClipTrayVirtualizingStackPanel_Loaded(object sender, RoutedEventArgs e) {
            ClipTrayVirtualizingStackPanel = sender as VirtualizingStackPanel;
        }

        #region Drag & Drop

        private void ClipTray_DragLeave(object sender, DragEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            ctrvm.IsTrayDropping = false;
            ClipTrayAdornerLayer.Update();
        }

        private void ClipTray_DragOver(object sender, DragEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            ctrvm.IsTrayDropping = false;
            ClipTrayAdornerLayer.Update();
            if (ctrvm.IsAnyClipDropping) {
                return;
            }
            AutoScrollByMouse();
            if (e.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                int dropIdx = GetDropIdx(MpHelpers.Instance.GetMousePosition(ClipTray));
                if (dropIdx >= 0/* && (dropIdx >= ctrvm.ClipTileViewModels.Count || (dropIdx < ctrvm.ClipTileViewModels.Count && !this[dropIdx].IsClipOrAnySubItemDragging))*/) {
                    var adornerPoints = GetAdornerPoints(dropIdx);
                    ClipTrayAdorner.DropTopPoint = adornerPoints[0];
                    ClipTrayAdorner.DropBottomPoint = adornerPoints[1];
                    ctrvm.IsTrayDropping = true;
                    e.Effects = DragDropEffects.Move;
                }
            }
            ClipTrayAdornerLayer.Update();
        }

        private void ClipTray_Drop(object sender, DragEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            if (!ctrvm.IsTrayDropping) {
                return;
            }
            bool wasDropped = false;
            var dctvml = new List<MpClipTileViewModel>();
            if (e.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                dctvml = (List<MpClipTileViewModel>)e.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                dctvml = dctvml.OrderByDescending(x => x.SortOrderIdx).ToList();
                int dropIdx = GetDropIdx(MpHelpers.Instance.GetMousePosition(ClipTray));
                if (dropIdx >= 0 && (dropIdx >= ctrvm.ClipTileViewModels.Count || (dropIdx < ctrvm.ClipTileViewModels.Count && !ctrvm.ClipTileViewModels[dropIdx].IsClipDragging))) {
                    if (dropIdx < ctrvm.ClipTileViewModels.Count && ctrvm.ClipTileViewModels[dropIdx].IsClipDragging) {
                        //ignore dropping dragged tile onto itself
                        //e2.Effects = DragDropEffects.None;
                        e.Handled = true;
                        ctrvm.IsTrayDropping = false;
                        ClipTrayAdornerLayer.Update();
                        return;
                    }
                    /* 
                     On tray drop: 
                     1. if all rtbvm of sctvm are selected or rtbvm count is 0, do move to dropidx, 
                     2. if partial selection, remove from parent and make new composite in merge then insert at dropidx. 
                     3.Order sctvml by asc hctvm.selecttime then subsort composites by asc rtbvm subselectdatetime
                    */
                    dctvml.Reverse();
                    foreach (var dctvm in dctvml) {
                        int dragCtvmIdx = ctrvm.ClipTileViewModels.IndexOf(dctvm);
                        bool wasEmptySelection = dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0;
                        if (wasEmptySelection) {
                            dctvm.RichTextBoxViewModelCollection.SubSelectAll();
                        }
                        if (dctvm.RichTextBoxViewModelCollection.Count == 0 ||
                            wasEmptySelection ||
                            dctvm.RichTextBoxViewModelCollection.Count == dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count) {
                            //1. if all rtbvm of sctvm are selected or rtbvm count is 0, do move to dropidx
                            if (dragCtvmIdx < dropIdx) {
                                ctrvm.ClipTileViewModels.Move(dragCtvmIdx, dropIdx - 1);
                            } else {
                                ctrvm.ClipTileViewModels.Move(dragCtvmIdx, dropIdx);
                            }
                            wasDropped = true;
                        } else {
                            //2. if partial selection, remove from parent and make new
                            //   composite in merge then insert at dropidx.
                            var drtbvm = dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.OrderBy(x => x.CompositeSortOrderIdx).ToList()[0];
                            dctvm.RichTextBoxViewModelCollection.Remove(drtbvm, true);
                            var nctvm = new MpClipTileViewModel(drtbvm.CopyItem);
                            foreach (var ssrtbvm in dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.OrderBy(x => x.CompositeSortOrderIdx).ToList()) {
                                nctvm.MergeCopyItemList(new List<MpCopyItem>() { ssrtbvm.CopyItem });
                            }
                            ctrvm.Add(nctvm, dropIdx);
                            nctvm.OnPropertyChanged(nameof(nctvm.CopyItem));
                            wasDropped = true;
                        }
                    }
                }
            }
            e.Handled = true;
            ctrvm.ClearAllDragDropStates();
            ClipTrayAdornerLayer.Update();
        }

        public void AutoScrollByMouse() {
            double minScrollDist = 20;
            double autoScrollOffset = 15;
            var mp = MpHelpers.Instance.GetMousePosition(ClipTray);
            double leftDiff = MpHelpers.Instance.DistanceBetweenValues(mp.X, 0);
            double rightDiff = MpHelpers.Instance.DistanceBetweenValues(mp.X, ActualWidth); 
            if (leftDiff < minScrollDist) {
                autoScrollOffset += Math.Pow(leftDiff, 2);
                ClipTray.ScrollViewer.ScrollToHorizontalOffset(ClipTray.ScrollViewer.HorizontalOffset - autoScrollOffset);
            } else if (rightDiff < minScrollDist) {
                autoScrollOffset += Math.Pow(rightDiff, 2);
                ClipTray.ScrollViewer.ScrollToHorizontalOffset(ClipTray.ScrollViewer.HorizontalOffset + autoScrollOffset);
            }
        }

        public int GetDropIdx(Point mp) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            double mdx = mp.X;
            double minDist = double.MaxValue;
            int dropIdx = -1;
            for (int i = 0; i < ClipTray.Items.Count; i++) {
                if(!ClipTray.IsListBoxItemVisible(i)) {
                    continue;
                }
                Rect lbir = ClipTray.GetListBoxItemRect(i);

                double lbilx = lbir.Left;
                double lbirx = lbir.Right;
                double lDist = Math.Abs(mdx - lbilx);
                double rDist = Math.Abs(mdx - lbirx);
                double dist = Math.Min(lDist, rDist);
                if (dist < minDist) {
                    minDist = dist;
                    if (minDist == lDist) {
                        dropIdx = i;
                    } else {
                        dropIdx = i + 1;
                    }

                }
            }
            //var overRect = this[dropIdx].TileRect;
            //double overMidX = overRect.Left + (overRect.Right / 2);
            //if (mp.X > overMidX) {
            //    dropIdx++;
            //}
            return dropIdx;
        }

        #endregion

        #region Selection
        private void ClipTray_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            ctrvm.MergeClipsCommandVisibility = ctrvm.MergeSelectedClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;

            MpTagTrayViewModel.Instance.UpdateTagAssociation();

            if (ctrvm.PrimarySelectedClipTile != null) {
                ctrvm.PrimarySelectedClipTile.OnPropertyChanged(nameof(ctrvm.PrimarySelectedClipTile.TileBorderBrush));
            }

            MpAppModeViewModel.Instance.RefreshState();
        }

        private void ClipTray_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            if (!ctrvm.IsAnyTileExpanded) {
                return;
            }
            var selectedClipTilesHoveringOnMouseDown = ctrvm.SelectedClipTiles.Where(x => x.IsHovering).ToList();
            if (selectedClipTilesHoveringOnMouseDown.Count == 0 &&
               !MpSearchBoxViewModel.Instance.IsTextBoxFocused) {
                ctrvm.ClearClipEditing();
            }
        }
        #endregion

        public Point[] GetAdornerPoints(int index) {
            var points = new Point[2];
            var itemRect = index >= ClipTray.Items.Count ? 
                ClipTray.GetListBoxItemRect(ClipTray.Items.Count -1) : ClipTray.GetListBoxItemRect(index);

            if (index < ClipTray.Items.Count) {
                points[0] = itemRect.TopLeft;
                points[1] = itemRect.BottomLeft;
            } else {
                points[0] = itemRect.TopRight;
                points[1] = itemRect.BottomRight;
            }
            var sv = ClipTray.ScrollViewer;
            if (sv != null &&
                (sv.HorizontalOffset > 0 || sv.VerticalOffset > 0)) {
                points[0].X += sv.Margin.Right;
                //points[0].Y += ScrollViewer.VerticalOffset;
                points[1].X += sv.Margin.Right;
                //points[1].Y += ScrollViewer.VerticalOffset;
            }
            return points;
        }

        
    }
}
