using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
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

        #endregion

        private static MpAvSearchCriteriaListBoxView _instance;
        public static MpAvSearchCriteriaListBoxView Instance => _instance;
        public MpAvSearchCriteriaListBoxView() {
            _instance = this;
            AvaloniaXamlLoader.Load(this);
            //var sv = this.FindControl<ScrollViewer>("SearchCriteriaContainerScrollViewer");
            //sv.AddHandler(PointerWheelChangedEvent, Sclb_PointerWheelChanged, RoutingStrategies.Tunnel);

            InitDragDrop();
        }

        #region Drag Drop
        private void InitDragDrop() {
            var clb = this.FindControl<ListBox>("SearchCriteriaListBox");
            clb.EnableItemsControlAutoScroll();

            DragDrop.SetAllowDrop(clb, true);
            clb.AddHandler(DragDrop.DragEnterEvent, SearchCriteriaListBox_DragEnter);
            clb.AddHandler(DragDrop.DragOverEvent, SearchCriteriaListBox_DragOver);
            clb.AddHandler(DragDrop.DragLeaveEvent, SearchCriteriaListBox_DragLeave);
            clb.AddHandler(DragDrop.DropEvent, SearchCriteriaListBox_Drop);
        }

        private void SearchCriteriaListBox_DragEnter(object sender, DragEventArgs e) {
            MpConsole.WriteLine($"Drag Enter");

        }
        private void SearchCriteriaListBox_DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine($"Drag Over");

            e.Handled = true;
            if (!e.Data.Contains(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT)) {
                e.DragEffects = DragDropEffects.None;
                ResetDragOvers();
                return;
            }
            MpAvSearchCriteriaItemViewModel drag_vm = e.Data.Get(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT) as MpAvSearchCriteriaItemViewModel;
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            e.DragEffects = DragDropEffects.Copy;// is_copy ? DragDropEffects.Copy : DragDropEffects.Move;

            var sclb = this.FindControl<ListBox>("SearchCriteriaListBox");
            sclb.AutoScrollItemsControl(e);

            MpPoint sclb_mp = e.GetPosition(sclb).ToPortablePoint();
            var gmp = sclb.PointToScreen(sclb_mp.ToAvPoint()).ToPortablePoint(sclb.VisualPixelDensity());
            MpConsole.WriteLine("Mp: " + sclb_mp);
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;

            int drag_idx = drag_vm.SortOrderIdx;
            int drop_idx = -1;
            for (int i = 0; i < sclb.ItemCount; i++) {
                // NOTE maybe weird this uses screen points but wasn't working i think header is screwing stuff up
                if (sclb.ContainerFromIndex(i) is ListBoxItem lbi) {
                    var go = lbi.PointToScreen(new Point()).ToPortablePoint(lbi.VisualPixelDensity());
                    var gb = lbi.Bounds.ToPortableRect();
                    gb.Move(go);
                    if (gb.Contains(gmp)) {
                        drop_idx = i;
                        if (gmp.Y > gb.Y + gb.Height / 2) {
                            drop_idx++;
                        }
                        break;
                    }

                }
            }
            MpConsole.WriteLine("(DragOver) DropIdx: " + drop_idx);

            if (drop_idx == scicvm.Items.Count) {
                scicvm.Items.ForEach(x => x.IsDragOverTop = false);
                scicvm.Items.ForEach(x => x.IsDragOverBottom = x.SortOrderIdx == drop_idx - 1);
                scicvm.Items.ForEach(x => x.IsDragOverCopy = x.SortOrderIdx == drop_idx - 1 && is_copy);
            } else {
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
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);

            var drag_vm = e.Data.Get(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT) as MpAvSearchCriteriaItemViewModel;
            if (drag_vm == null) {
                return;
            }
            int drag_idx = drag_vm.SortOrderIdx;
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;
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
    }
}
