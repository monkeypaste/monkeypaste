using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticToolbarTreeView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaItemView : 
        MpAvUserControl<MpAvSearchCriteriaItemViewModel>, 
        MpIDndUserCancelNotifier {
        public MpAvSearchCriteriaItemView() {
            InitializeComponent();
            var db = this.FindControl<Control>("CriteriaDragButton");
            db.AddHandler(PointerPressedEvent, Db_PointerPressed, RoutingStrategies.Tunnel);
            OnGlobalEscKeyPressed += MpAvSearchCriteriaItemView_OnGlobalEscKeyPressed;
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        #region Criteria Row Drag

        #region MpIDndUserCancelNotifier Implementation

        public event EventHandler OnGlobalEscKeyPressed;

        private void MpAvSearchCriteriaItemView_OnGlobalEscKeyPressed(object sender, EventArgs e) {
            ResetDragOvers();
        }

        #endregion
        private double[] _autoScrollAccumulators;

        private void Db_PointerPressed(object sender, PointerPressedEventArgs e) {
            var dragButton = sender as Control;
            if (dragButton == null) {
                return;
            }
            e.Handled = true;
            dragButton.DragCheckAndStart(
                e,
                CriteriaRowDragButton_Start, CriteriaRowDragButton_Move, CriteriaRowDragButton_End,
                null,
                this);
        }

        private void CriteriaRowDragButton_Start(PointerPressedEventArgs e) {
            e.Pointer.Capture(e.Source as Control);
            DragDrop.DoDragDrop(e, new DataObject(), DragDropEffects.Move).FireAndForgetSafeAsync(null);
            var lb = this.GetVisualAncestor<ListBox>();
            var sv = lb.GetVisualAncestor<ScrollViewer>();

            sv.AutoScroll(
                lb.PointToScreen(e.GetPosition(lb)).ToPortablePoint(lb.VisualPixelDensity()),
                lb,
                ref _autoScrollAccumulators);
        }
        private void CriteriaRowDragButton_Move(PointerEventArgs e) {
            var sclb = this.GetVisualAncestor<ListBox>();
            MpPoint sclb_mp = e.GetClientMousePoint(sclb);
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;

            int drag_idx = BindingContext.SortOrderIdx;
            int drop_idx = GetDropIdx(sclb, sclb_mp);
            MpConsole.WriteLine("DropIdx: " + drop_idx);

            if (drop_idx == scicvm.Items.Count) {
                // tail drop
                if(drag_idx == drop_idx - 1) {
                    // reject same item drop
                    ResetDragOvers();
                    return;
                }
                scicvm.Items.ForEach(x => x.IsDragOverTop = false);
                scicvm.Items.ForEach(x => x.IsDragOverBottom = x.SortOrderIdx == drop_idx - 1);
            } else {
                if(drag_idx == drop_idx) {
                    ResetDragOvers();
                    return;
                }
                scicvm.Items.ForEach(x => x.IsDragOverTop = x.SortOrderIdx == drop_idx);
                scicvm.Items.ForEach(x => x.IsDragOverBottom = false);
            }         
        }

        private void CriteriaRowDragButton_End(PointerReleasedEventArgs e) {
            e.Pointer.Capture(null);

            int drag_idx = BindingContext.SortOrderIdx;
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;

            var resorted_items = scicvm.SortedItems.ToList();

            var drag_over_top_item = scicvm.Items.FirstOrDefault(x => x.IsDragOverTop);
            if(drag_over_top_item != null) {
                resorted_items.Move(drag_idx, drag_over_top_item.SortOrderIdx);
            } else {
                //tail drop
                var drag_over_bottom_item = scicvm.Items.FirstOrDefault(x => x.IsDragOverBottom);
                if(drag_over_bottom_item != null) {
                    resorted_items.Move(drag_idx, drag_over_bottom_item.SortOrderIdx);
                } else {
                    // flag no drop
                    resorted_items = null;
                }
            }
            if(resorted_items != null) {
                resorted_items.ForEach((x, idx) => x.SortOrderIdx = idx);
                scicvm.OnPropertyChanged(nameof(scicvm.SortedItems));
            }
            ResetDragOvers();
        }

        private int GetDropIdx(ListBox lb, MpPoint lb_mp) {
            if (!lb.Bounds.Contains(lb_mp.ToAvPoint())) {
                return -1;
            }
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;

            if (scicvm.Items.Count == 0) {
                // TODO Add logic for pin popout overlay or add binding somewhere when empty
                return 0;
            }
            MpRectSideHitTest closet_side_ht = null;
            int closest_side_lbi_idx = -1;
            for (int i = 0; i < scicvm.Items.Count; i++) {
                var lbi_rect = lb.ItemContainerGenerator.ContainerFromIndex(i).Bounds.ToPortableRect();
                var cur_tup = lbi_rect.GetClosestSideToPoint(lb_mp, "r,l");
                if (closet_side_ht == null || cur_tup.ClosestSideDistance < closet_side_ht.ClosestSideDistance) {
                    closet_side_ht = cur_tup;
                    closest_side_lbi_idx = i;
                }
            }

            if (closet_side_ht.ClosestSideLabel == "b") {
                return closest_side_lbi_idx + 1;
            }
            return closest_side_lbi_idx;
        }

        private void ResetDragOvers() {
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;
            scicvm.Items.ForEach(x => x.IsDragOverTop = false);
            scicvm.Items.ForEach(x => x.IsDragOverBottom = false);
        }


        #endregion

    }
}
