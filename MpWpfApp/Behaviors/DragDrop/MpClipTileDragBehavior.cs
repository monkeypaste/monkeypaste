using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTileDragBehavior : Behavior<MpClipTileView> {
        private const double MINIMUM_DRAG_DISTANCE = 20;
        private static MpClipTrayView ClipTrayView;

        private double maxTileDropDist;
        private Point elementStartPosition;
        private Point mouseStartPosition;
        private TranslateTransform transform = new TranslateTransform();

        private MpContentListView CurrentDropContentListView;

        protected override void OnAttached() {
            AssociatedObject.Loaded += (s,e)=>{ maxTileDropDist = AssociatedObject.ActualWidth * 0.4; };

            Window parent = Application.Current.MainWindow;
            if (ClipTrayView == null) {
                ClipTrayView = parent.GetVisualDescendent<MpClipTrayView>();
            }
            
            AssociatedObject.RenderTransform = transform;

            AssociatedObject.MouseLeftButtonDown += (sender, e) => {
                elementStartPosition = AssociatedObject.TranslatePoint(new Point(), parent);
                mouseStartPosition = e.GetPosition(parent);
                AssociatedObject.CaptureMouse();
            };

            AssociatedObject.MouseLeftButtonUp += (sender, e) => {
                AssociatedObject.ReleaseMouseCapture();
                if(CurrentDropContentListView == null) {
                    ClipTrayView.DropBehavior.Drop();
                } else {
                    CurrentDropContentListView.DropBehavior.Drop();
                }
                FinishDrop();
            };

            AssociatedObject.MouseMove += (sender, e) => {
                Vector diff = e.GetPosition(parent) - mouseStartPosition;
                if (AssociatedObject.IsMouseCaptured &&
                    diff.Length >= MINIMUM_DRAG_DISTANCE) {
                    //if mouse down was on tile
                    ListBox ctlb = ClipTrayView.ClipTray;
                    ListBoxItem lbi = ctlb.GetItemAtPoint(e.GetPosition(ctlb));
                    if (lbi == null) {
                        //if dragged off of tray
                        ClipTrayView.DropBehavior.CancelDrop();
                        CancelContentListDrop();
                        return;
                    }
                    MpClipTileView ctv = lbi.GetVisualDescendent<MpClipTileView>();
                    if (ctv.ContentListView != CurrentDropContentListView &&
                       CurrentDropContentListView != null) {
                        //when dragged onto a new tile cancel the previous tile drop 
                        CancelContentListDrop();
                        //CurrentDropContentListView = null;
                    }

                    int ctvIdx = ctlb.GetItemIndexAtPoint(e.GetPosition(ctlb));
                    Rect ctvr = ctlb.GetListBoxItemRect(ctvIdx);
                    double ctvMidX = ctvr.Width / 2;

                    double test = e.GetPosition(ctv).X;
                    double mouseDistFromMidX = Math.Abs(test - ctvMidX);
                    if(mouseDistFromMidX > maxTileDropDist) {
                        //assume this is a tile resort
                        
                        if (ctvIdx == ClipTrayView.ClipTray.Items.IndexOf(AssociatedObject)) {
                            //ignore resorting a tile to its same position
                            ClipTrayView.DropBehavior.CancelDrop();
                            return;
                        }

                        if (e.GetPosition(ctv).X > ctvMidX) {
                            //when mouse is more than halfway across tile assume dropping after it
                            ctvIdx = ctvIdx + 1;
                        }

                        ClipTrayView.DropBehavior.DragOver(
                            MpClipTrayViewModel.Instance.SelectedItems,
                            ctvIdx);

                        CancelContentListDrop();
                    } else {
                        StartContentListDrop(ctv.ContentListView);

                        MpClipTileView cdclv_ctv = CurrentDropContentListView.GetVisualAncestor<MpClipTileView>();
                        if (CurrentDropContentListView.ContentListBox.Items.Count == 1 &&
                            cdclv_ctv == AssociatedObject) {
                            CancelContentListDrop();
                        } else {
                            var cilb = CurrentDropContentListView.ContentListBox;
                            int ciIdx = cilb.GetItemIndexAtPoint(e.GetPosition(cilb));
                            Rect cir = cilb.GetListBoxItemRect(ciIdx);
                            double ciMidY = cir.Height / 2;
                            ListBoxItem cilbi = cilb.GetListBoxItem(ciIdx);

                            if (e.GetPosition(cilbi).Y > ciMidY) {
                                //when mouse is more than halfway across tile assume dropping after it
                                ciIdx = ciIdx + 1;
                            }
                            //even if drag data is drop target we must update drop target to check if its valid
                            CurrentDropContentListView.DropBehavior.DragOver(
                                    MpClipTrayViewModel.Instance.SelectedItems,
                                    ctvIdx);

                            ClipTrayView.DropBehavior.CancelDrop();
                        }
                        
                    }
                }
            };
        }

        public void StartContentListDrop(MpContentListView clv) {
            CurrentDropContentListView = clv;
            var ctvm = CurrentDropContentListView.DataContext as MpClipTileViewModel;
            ctvm.DoCommandSelection();
            ctvm.IsClipDragging = true;

            MpConsole.WriteLine($"{DateTime.Now} Starting content drop on tile: " + ctvm.HeadItem.CopyItem.ItemData.ToPlainText());
        }
        private void CancelContentListDrop() {
            if(CurrentDropContentListView == null) {
                return;
            }
            CurrentDropContentListView.DropBehavior.CancelDrop();
            var ctvm = CurrentDropContentListView.DataContext as MpClipTileViewModel;
            ctvm.IsClipDragging = false;
            CurrentDropContentListView = null;
            MpConsole.WriteLine($"{DateTime.Now} Canceling content drop on tile: " + ctvm.HeadItem.CopyItem.ItemData.ToPlainText());
        }

        private void FinishDrop() {
            if(CurrentDropContentListView != null) {
                var ctvm = CurrentDropContentListView.DataContext as MpClipTileViewModel;
                MpConsole.WriteLine($"{DateTime.Now} Finishing content drop on tile: " + ctvm.HeadItem.CopyItem.ItemData.ToPlainText());
            }
            var ctrvm = ClipTrayView.DataContext as MpClipTrayViewModel;
            foreach (var ctvm in ctrvm.SelectedItems) {
                ctvm.IsClipDragging = false;
            }
            CancelContentListDrop();
        }
    }
}
