using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Avalonia.Behaviors._Factory;
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
        private int _dropIdx { get; set; } = -1;

        bool MpAvIDropHost.IsDropValid(IDataObject avdo) {
            return true;
        }
        void MpAvIDropHost.DragOver(MpPoint ptr_lb_mp, IDataObject avdo, DragDropEffects dragEffects) {
            _dropIdx = -1;

            if(BindingContext.PinnedItems.Count == 0) {
                // TODO Add logic for pin popout overlay or add binding somewhere when empty
                _dropIdx = 0;
                return;
            }
            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");

            MpRectSideHitTest closet_side_ht = null;
            int closest_side_lbi_idx = -1;
            for (int i = 0; i < BindingContext.PinnedItems.Count; i++) {
                var lbi_rect = ptr_lb.ItemContainerGenerator.ContainerFromIndex(i).Bounds.ToPortableRect();
                var cur_tup = lbi_rect.GetClosestSideToPoint(ptr_lb_mp,"t,b");
                if (closet_side_ht == null || cur_tup.ClosestSideDistance < closet_side_ht.ClosestSideDistance) {
                    closet_side_ht = cur_tup;
                    closest_side_lbi_idx = i;
                }
            }

            if (closet_side_ht.ClosestSideLabel == "r") {
                _dropIdx = closest_side_lbi_idx + 1;
            } else {
                _dropIdx = closest_side_lbi_idx;
            }

            // BUG Have adjust pad for vert/horiz I think its from pin tray padding maybe...
            double pad = BindingContext.PinnedItems.First().OuterSpacing;
            if(BindingContext.ListOrientation == Orientation.Vertical) {
                pad *= 0.75;
            } else {
                pad *= 2;
            }
            MpLine drop_line;
            if(_dropIdx < BindingContext.PinnedItems.Count) {
                drop_line = ptr_lb.ItemContainerGenerator.ContainerFromIndex(_dropIdx).Bounds.ToPortableRect().GetSideByLabel("l");
            } else {
                drop_line = ptr_lb.ItemContainerGenerator.ContainerFromIndex(BindingContext.PinnedItems.Count - 1).Bounds.ToPortableRect().GetSideByLabel("r");
            }
            drop_line.P1.X += pad;
            drop_line.P2.X += pad;

            double vert_pad = pad * 2;
            drop_line.P1.Y += vert_pad;
            drop_line.P2.Y -= vert_pad;

            MpConsole.WriteLine("DropIdx: " + _dropIdx );

            var dropAdorner = MpAvDropExtension.GetDropAdorner(this);
            if(dropAdorner == null) {
                Debugger.Break();
                MpConsole.WriteLine("Error Drop adorner missing! Ignoring rendering drop adorners");
                return;
            }

            
            drop_line.StrokeOctColor = MpSystemColors.White;
            drop_line.StrokeThickness = 2;
            drop_line.StrokeDashStyle = new double[] { 5, 5 };

            MpAvDropExtension.GetDropAdorner(this).DrawDropAdorner(new MpShape[] { drop_line });
        }

        void MpAvIDropHost.DragEnter() {
            MpAvDropExtension.GetDropAdorner(this).IsVisible = true;
        }

        void MpAvIDropHost.DragLeave() {
            _dropIdx = -1;
            MpAvDropExtension.GetDropAdorner(this).IsVisible = false;
            MpAvDropExtension.GetDropAdorner(this).DrawDropAdorner(null);
        }

        async Task<DragDropEffects> MpAvIDropHost.DropDataObjectAsync(IDataObject avdo, DragDropEffects effects) {
            // NOTE only pin tray allows drop

            MpAvDropExtension.GetDropAdorner(this).DrawDropAdorner(null);

            DragDropEffects dropEffects = DragDropEffects.None;
            MpAvClipTileViewModel drop_ctvm;

            if (avdo.GetDataFormats().Contains(MpAvDataObjectHelper.CLIP_TILE_DATA_FORMAT)) {
                // Internal DragDrop
                drop_ctvm = avdo.Get(MpAvDataObjectHelper.CLIP_TILE_DATA_FORMAT) as MpAvClipTileViewModel;
                if (drop_ctvm == null) {
                    Debugger.Break();
                }
                bool isCopy = effects.HasFlag(DragDropEffects.Copy);

                if (isCopy) {
                    //  duplicate
                    var dup_ci = (MpCopyItem)await BindingContext.SelectedItem.CopyItem.Clone(true);

                    await dup_ci.WriteToDatabaseAsync();


                    drop_ctvm = await BindingContext.CreateClipTileViewModel(dup_ci, -1);
                    BindingContext.PinTileCommand.Execute(drop_ctvm);
                    dropEffects |= DragDropEffects.Copy;
                } else {
                    // move

                    if (drop_ctvm.IsPinned) {
                        int drop_ctvm_idx = BindingContext.PinnedItems.IndexOf(drop_ctvm);
                        if(_dropIdx > drop_ctvm_idx) {
                            _dropIdx -= 1;
                        }

                        BindingContext.PinnedItems.Move(drop_ctvm_idx, _dropIdx);
                    } else {
                        BindingContext.ToggleTileIsPinnedCommand.Execute(drop_ctvm);
                    }
                    dropEffects |= DragDropEffects.Move;
                }                
            }

            return dropEffects;
        }

        #endregion

        public MpAvPinTrayView() {
            InitializeComponent();
            PinTrayListBox = this.FindControl<ListBox>("PinTrayListBox");
        }

        private void MpAvClipTileTitleView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            MpAvViewBehaviorFactory.BuildAllViewBehaviors(this, this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
