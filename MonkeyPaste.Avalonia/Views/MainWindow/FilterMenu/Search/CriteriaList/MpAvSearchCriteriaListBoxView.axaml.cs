using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpSearchDetailView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaListBoxView :
        MpAvUserControl<MpAvSearchCriteriaItemCollectionViewModel> {
        #region Private Variables

        private double[] _autoScrollAccumulators;
        #endregion

        private static MpAvSearchCriteriaListBoxView _instance;
        public static MpAvSearchCriteriaListBoxView Instance => _instance;
        public MpAvSearchCriteriaListBoxView() {
            _instance = this;
            InitializeComponent();
            var sv = this.FindControl<ScrollViewer>("SearchCriteriaContainerScrollViewer");
            sv.AddHandler(PointerWheelChangedEvent, Sclb_PointerWheelChanged, RoutingStrategies.Tunnel);

            InitDragDrop();
        }

        #region Drag Drop
        private void InitDragDrop() {
            var clb = this.FindControl<ListBox>("SearchCriteriaListBox");
            clb.AddHandler(DragDrop.DragEnterEvent, SearchCriteriaListBox_DragEnter, RoutingStrategies.Direct);
            clb.AddHandler(DragDrop.DragOverEvent, SearchCriteriaListBox_DragOver, RoutingStrategies.Direct);
            clb.AddHandler(DragDrop.DragLeaveEvent, SearchCriteriaListBox_DragLeave, RoutingStrategies.Direct);
            clb.AddHandler(DragDrop.DropEvent, SearchCriteriaListBox_Drop, RoutingStrategies.Direct);
        }

        private void SearchCriteriaListBox_DragEnter(object sender, DragEventArgs e) {
            MpConsole.WriteLine($"Drag Enter");

            var sv = this.FindControl<ScrollViewer>("SearchCriteriaContainerScrollViewer");
            var lb = this.FindControl<ListBox>("SearchCriteriaListBox");
            sv.AutoScroll(
                lb.PointToScreen(e.GetPosition(lb)).ToPortablePoint(lb.VisualPixelDensity()),
                lb,
                ref _autoScrollAccumulators);
        }
        private void SearchCriteriaListBox_DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine($"Drag Over");

            e.Handled = true;
            e.DragEffects = DragDropEffects.None;
            var drag_vm = e.Data.Get(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT) as MpAvSearchCriteriaItemViewModel;
            if (drag_vm == null) {
                return;
            }
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;

            var sclb = this.FindControl<ListBox>("SearchCriteriaListBox");
            MpPoint sclb_mp = e.GetPosition(sclb).ToPortablePoint();
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;

            int drag_idx = drag_vm.SortOrderIdx;
            int drop_idx = GetDropIdx(sclb, sclb_mp);
            MpConsole.WriteLine("(DragOver) DropIdx: " + drop_idx);

            if (drop_idx == scicvm.Items.Count) {
                // tail drop
                if (drag_idx == drop_idx - 1) {
                    // reject same item drop
                    e.DragEffects = DragDropEffects.None;
                    ResetDragOvers();
                    return;
                }
                scicvm.Items.ForEach(x => x.IsDragOverTop = false);
                scicvm.Items.ForEach(x => x.IsDragOverBottom = x.SortOrderIdx == drop_idx - 1);
                scicvm.Items.ForEach(x => x.IsDragOverCopy = x.SortOrderIdx == drop_idx - 1 && is_copy);
            } else {
                if (drag_idx == drop_idx) {
                    // reject same item drop
                    e.DragEffects = DragDropEffects.None;
                    ResetDragOvers();
                    return;
                }
                scicvm.Items.ForEach(x => x.IsDragOverTop = x.SortOrderIdx == drop_idx);
                scicvm.Items.ForEach(x => x.IsDragOverCopy = x.SortOrderIdx == drop_idx && is_copy);
                scicvm.Items.ForEach(x => x.IsDragOverBottom = false);
            }
        }
        private void SearchCriteriaListBox_DragLeave(object sender, DragEventArgs e) {
            MpConsole.WriteLine($"Drag Leave");
            ResetDragOvers();
        }
        private async void SearchCriteriaListBox_Drop(object sender, DragEventArgs e) {
            MpConsole.WriteLine($"Drag Drop");

            e.Handled = true;
            if (e.DragEffects == DragDropEffects.None) {
                ResetDragOvers();
                return;
            }
            bool is_copy = e.DragEffects.HasFlag(DragDropEffects.Copy);

            var drag_vm = e.Data.Get(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT) as MpAvSearchCriteriaItemViewModel;
            if (drag_vm == null) {
                return;
            }
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;
            int drag_idx = drag_vm.SortOrderIdx;
            var resorted_items = scicvm.SortedItems.ToList();

            if (is_copy) {
                var clone_sci = await drag_vm.SearchCriteriaItem.CloneDbModelAsync();
                var clone_scivm = await BindingContext.CreateCriteriaItemViewModelAsync(clone_sci);

                int drop_idx = -1;
                if (BindingContext.Items.FirstOrDefault(x => x.IsDragOverTop) is MpAvSearchCriteriaItemViewModel top_drop_vm) {
                    drop_idx = top_drop_vm.SortOrderIdx;
                } else if (BindingContext.Items.FirstOrDefault(x => x.IsDragOverBottom) is MpAvSearchCriteriaItemViewModel bottom_drop_vm) {
                    drop_idx = bottom_drop_vm.SortOrderIdx + 1;
                }
                if (drop_idx < 0) {
                    ResetDragOvers();
                    return;
                }
                BindingContext.Items.Add(clone_scivm);
                resorted_items.Insert(drop_idx, clone_scivm);
            } else {

                var drag_over_top_item = scicvm.Items.FirstOrDefault(x => x.IsDragOverTop);
                if (drag_over_top_item != null) {
                    resorted_items.Move(drag_idx, drag_over_top_item.SortOrderIdx);
                } else {
                    //tail drop
                    var drag_over_bottom_item = scicvm.Items.FirstOrDefault(x => x.IsDragOverBottom);
                    if (drag_over_bottom_item != null) {
                        resorted_items.Move(drag_idx, drag_over_bottom_item.SortOrderIdx);
                    } else {
                        // flag no drop
                        resorted_items = null;
                    }
                }

            }
            if (resorted_items != null) {
                resorted_items.ForEach((x, idx) => x.SortOrderIdx = idx);
                scicvm.OnPropertyChanged(nameof(scicvm.SortedItems));
            }

            ResetDragOvers();
        }

        #region Dnd Helpers
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
                var lbi_rect = lb.ContainerFromIndex(i).Bounds.ToPortableRect();
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
            scicvm.Items.ForEach(x => x.IsDragOverCopy = false);
        }
        #endregion

        #endregion
        private void Sclb_PointerWheelChanged(object sender, global::Avalonia.Input.PointerWheelEventArgs e) {
            var sv = sender as ScrollViewer;

            double dir = e.Delta.Y < 0 ? 1 : -1;
            double amt = 30;
            sv.ScrollToVerticalOffset(sv.Offset.Y + (amt * dir));
            //sv.ScrollByPointDelta(new MpPoint(0, amt * dir));
            e.Handled = true;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
