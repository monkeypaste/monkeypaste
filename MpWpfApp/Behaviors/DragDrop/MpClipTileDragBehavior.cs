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
                ClipTrayView.DropBehavior.Drop();
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
                        if (CurrentDropContentListView != null) {
                            CurrentDropContentListView.DropBehavior.CancelDrop();
                            CurrentDropContentListView = null;
                        }
                        return;
                    }
                    MpClipTileView ctv = lbi.GetVisualDescendent<MpClipTileView>();
                    if (ctv.ContentListView != CurrentDropContentListView &&
                       CurrentDropContentListView != null) {
                        //when dragged onto a new tile cancel the previous tile drop 
                        CurrentDropContentListView.DropBehavior.CancelDrop();
                    }
                    CurrentDropContentListView = ctv.ContentListView;

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
                            new List<object> {
                                MpClipTrayViewModel.Instance.SelectedItems
                            },
                            ctvIdx);
                        if(CurrentDropContentListView != null) {
                            CurrentDropContentListView.DropBehavior.CancelDrop();
                            return;
                        }
                    } else {
                        if(CurrentDropContentListView.ContentListBox.Items.Count == 1) {
                            MpClipTileView cdclv_ctv = CurrentDropContentListView.GetVisualAncestor<MpClipTileView>();
                            if(cdclv_ctv == AssociatedObject) {
                                //when dragging a 1 item tile ignore reordering
                                CurrentDropContentListView.DropBehavior.CancelDrop();
                                return;
                            }
                        }
                        var cilb = CurrentDropContentListView.ContentListBox;
                        int ciIdx = cilb.GetItemIndexAtPoint(e.GetPosition(cilb));
                        Rect cir = cilb.GetListBoxItemRect(ciIdx);
                        double ciMidY = cir.Height / 2;
                        ListBoxItem cilbi = cilb.GetListBoxItem(ciIdx);

                        if (e.GetPosition(cilbi).Y > ciMidY) {
                            //when mouse is more than halfway across tile assume dropping after it
                            ciIdx = ciIdx + 1;
                        }
                        CurrentDropContentListView.DropBehavior.DragOver(
                            new List<object> {
                                MpClipTrayViewModel.Instance.SelectedItems
                            },
                            ctvIdx);
                        ClipTrayView.DropBehavior.CancelDrop();
                    }
                }
            };
        }
    }
}
