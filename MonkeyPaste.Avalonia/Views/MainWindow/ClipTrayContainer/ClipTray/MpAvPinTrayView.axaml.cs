using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Utils.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPinTrayView : MpAvUserControl<MpAvClipTrayViewModel>, MpAvIDropHost {

        #region MpAvIDropHost Implementation
        bool MpAvIDropHost.IsDropValid(IDataObject avdo, MpPoint host_mp, DragDropEffects dragEffects) {
            if(dragEffects == DragDropEffects.None) {
                return false;
            }
            // called in DropExtension DragOver 
            int drop_idx = GetDropIdx(host_mp);

            MpConsole.WriteLine("IsDropValid DropIdx: " + drop_idx);
            if (drop_idx < 0) {
                return false;
            }
            var drag_pctvm = BindingContext.PinnedItems.FirstOrDefault(x => x.IsItemDragging);
            if(drag_pctvm == null) {
                // Tile drop is always valid
                return true;
            }
            int drag_pctvm_idx = BindingContext.PinnedItems.IndexOf(drag_pctvm);
            bool is_copy = dragEffects.HasFlag(DragDropEffects.Copy);
            bool is_drop_onto_same_idx = drop_idx == drag_pctvm_idx || drop_idx == drag_pctvm_idx + 1;
            if(!is_copy && is_drop_onto_same_idx) {
                // don't allow moving item if it'll be at same position
                return false;
            }
            // TODO Check data here (which shouldn't matter for internal but would be general app-level check from external)

            avdo.GetDataFormats().ForEach(x => MpConsole.WriteLine($"FORMAT: '{x}' Data: '{avdo.Get(x)}'"));


            return true;
        }

        void MpAvIDropHost.DragOver(MpPoint host_mp, IDataObject avdo, DragDropEffects dragEffects) {
            int drop_idx = GetDropIdx(host_mp);
            MpConsole.WriteLine("DragOver DropIdx: " + drop_idx);

            if (dragEffects == DragDropEffects.None || drop_idx < 0) {
                MpConsole.WriteLine("DragOver invalidated: Effects is none");
                ClearAdorner();
                return;
            }

            MpLine dropLine = CreateDropLine(drop_idx,dragEffects);
            DrawAdorner(dropLine);
        }

        //void MpAvIDropHost.DragEnter() {
        //    DrawAdorner(null);
        //}

        void MpAvIDropHost.DragLeave() {
            ClearAdorner();
        }

        async Task<DragDropEffects> MpAvIDropHost.DropDataObjectAsync(IDataObject avdo, MpPoint host_mp, DragDropEffects dragEffects) {
            // NOTE only pin tray allows drop not clip tray

            ClearAdorner();

            int drop_idx = GetDropIdx(host_mp);
            MpConsole.WriteLine("DropDataObjectAsync DropIdx: " + drop_idx);

            if (dragEffects == DragDropEffects.None || drop_idx < 0) {
                return DragDropEffects.None;
            }

            DragDropEffects dropEffects = DragDropEffects.None;

            if (avdo.GetDataFormats().Contains(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT)) {
                // Internal DragDrop
                dropEffects = await PerformInternalDropAsync(drop_idx, avdo, dragEffects);
            } else {
                dropEffects = await PerformExternalDropAsync(drop_idx, avdo, dragEffects);
            }

            return dropEffects;
        }

        private async Task<DragDropEffects> PerformInternalDropAsync(int drop_idx, IDataObject avdo, DragDropEffects effects) {
            var drop_ctvm = avdo.Get(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT) as MpAvClipTileViewModel;
            if (drop_ctvm == null) {
                Debugger.Break();
            }
            bool isCopy = effects.HasFlag(DragDropEffects.Copy);

            if (isCopy) {
                //  duplicate
                var dup_ci = (MpCopyItem)await BindingContext.SelectedItem.CopyItem.Clone(true);

                await dup_ci.WriteToDatabaseAsync();


                drop_ctvm = await BindingContext.CreateClipTileViewModel(dup_ci, -1);
                BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });
            } else {
                // move

                if (drop_ctvm.IsPinned) {
                    int drop_ctvm_idx = BindingContext.PinnedItems.IndexOf(drop_ctvm);
                    if (drop_idx > drop_ctvm_idx) {
                        drop_idx -= 1;
                    }

                    BindingContext.PinnedItems.Move(drop_ctvm_idx, drop_idx);
                } else {
                    BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });
                }
            }
            return effects;
        }

        private async Task<DragDropEffects> PerformExternalDropAsync(int drop_idx, IDataObject avdo, DragDropEffects effects) {
            MpPortableDataObject mpdo = MpPlatformWrapper.Services.DataObjectHelper.ConvertToSupportedPortableFormats(avdo);

            var avdo_ci = await MpPlatformWrapper.Services.CopyItemBuilder.CreateAsync(mpdo);

            var drop_ctvm = await BindingContext.CreateClipTileViewModel(avdo_ci, -1);
            BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });

            return effects;
        }

        private void ClearAdorner() {
            MpAvDropExtension.GetDropAdorner(this).IsVisible = false;
            MpAvDropExtension.GetDropAdorner(this).DrawDropAdorner(null);
        }
        private void DrawAdorner(MpLine dropLine) {
            MpAvDropExtension.GetDropAdorner(this).IsVisible = true;
            if(dropLine == null) {
                return;
            }
            MpAvDropExtension.GetDropAdorner(this).DrawDropAdorner(new MpShape[] {dropLine});
        }
        private MpLine CreateDropLine(int drop_idx, DragDropEffects dragEffects) {
            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            int total_count = BindingContext.PinnedItems.Count;
            if (drop_idx > total_count) {
                return null;
            }
            MpLine drop_line;
            int ref_lbi_idx = Math.Clamp(drop_idx, 0, total_count - 1);
            var ref_lbi = ptr_lb.ItemContainerGenerator.ContainerFromIndex(ref_lbi_idx) as ListBoxItem;
            var ref_lbi_rect = ref_lbi.Bounds.ToPortableRect();

            if (drop_idx < total_count) {
                drop_line = ref_lbi_rect.GetSideByLabel("l");
            } else {
                drop_line = ref_lbi_rect.GetSideByLabel("r");
            }
            // NOTE this cleans up the line...for some reason container bounds ignores lb padding 
            var ptr_lb_wp = ptr_lb.GetVisualDescendant<WrapPanel>();

            // lbi bounds need to be relative to items panel not lb, lb doesn't account for box model calc
            var items_panel_offset = ptr_lb_wp.TranslatePoint(new Point(0, 0), ptr_lb).Value.ToPortablePoint();
            drop_line.P1 += items_panel_offset;
            drop_line.P2 += items_panel_offset;

            // get child to adjust height
            var ctv = ref_lbi.GetVisualDescendant<MpAvClipTileView>();
            // NOTE get inner child because of border thickness
            var lbi_child = ctv.FindControl<MpAvClipBorder>("ClipTileContainerBorder").Child;
            var lbi_child_tl = lbi_child.TranslatePoint(new Point(0, 0), ptr_lb_wp);
            drop_line.P1.Y = lbi_child_tl.Value.Y;

            var lbi_child_bl = lbi_child.TranslatePoint(new Point(0, lbi_child.Bounds.Height), ptr_lb_wp);
            drop_line.P2.Y = lbi_child_bl.Value.Y;

            drop_line.StrokeOctColor = dragEffects.HasFlag(DragDropEffects.Copy) ? MpSystemColors.limegreen : MpSystemColors.White;
            drop_line.StrokeThickness = 2;
            drop_line.StrokeDashStyle = new double[] { 5, 2 };

            return drop_line;
        }

        private int GetDropIdx(MpPoint host_mp) {
            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            var ptr_lb_mp = this.TranslatePoint(host_mp.ToAvPoint(), ptr_lb).Value.ToPortablePoint();

            if (!ptr_lb.Bounds.Contains(ptr_lb_mp.ToAvPoint())) {
                return -1;
            }

            if (BindingContext.PinnedItems.Count == 0) {
                // TODO Add logic for pin popout overlay or add binding somewhere when empty
                return 0;
            }

            MpRectSideHitTest closet_side_ht = null;
            int closest_side_lbi_idx = -1;
            for (int i = 0; i < BindingContext.PinnedItems.Count; i++) {
                var lbi_rect = ptr_lb.ItemContainerGenerator.ContainerFromIndex(i).Bounds.ToPortableRect();
                var cur_tup = lbi_rect.GetClosestSideToPoint(ptr_lb_mp, "t,b");
                if (closet_side_ht == null || cur_tup.ClosestSideDistance < closet_side_ht.ClosestSideDistance) {
                    closet_side_ht = cur_tup;
                    closest_side_lbi_idx = i;
                }
            }

            if (closet_side_ht.ClosestSideLabel == "r") {
                return closest_side_lbi_idx + 1;
            }
            return closest_side_lbi_idx;
        }

        #endregion

        public MpAvPinTrayView() {
            InitializeComponent();
            //PinTrayListBox = this.FindControl<ListBox>("PinTrayListBox");
        }

        private void MpAvClipTileTitleView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            //MpAvViewBehaviorFactory.BuildAllViewBehaviors(this, this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
