using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Avalonia.Behaviors._Factory;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPinTrayView : MpAvUserControl<MpAvClipTrayViewModel>, MpAvIDropHost {
        public MpAvPinTrayDropBehavior PinTrayDropBehavior { 
            get {
                if (this.Resources.TryGetResource("PinTrayDropBehavior", out object value)) {
                    return value as MpAvPinTrayDropBehavior;
                }
                return null;
            }
        }

        #region MpAvIDropHost Implementation
        private int _dropIdx = -1;

        bool MpAvIDropHost.IsDropValid(IDataObject avdo) {
            return true;
        }
        void MpAvIDropHost.DragOver(MpPoint ptr_lb_mp, IDataObject avdo, DragDropEffects dragEffects) {
            if(BindingContext.PinnedItems.Count == 0) {
                // TODO Add logic for pin popout overlay or add binding somewhere when empty
                _dropIdx = 0;
                return;
            }

            MpAvClipTileViewModel closest_ctvm = null;
            Tuple<double, string> closet_tup = null;
            for (int i = 0; i < BindingContext.PinnedItems.Count; i++) {
                var cur_ctvm = BindingContext.PinnedItems[i];
                if (cur_ctvm.ObservedBounds == null) {
                    continue;
                }
                var cur_tup = cur_ctvm.ObservedBounds.GetClosestSideToPoint(ptr_lb_mp);
                if (closet_tup == null || cur_tup.Item1 < closet_tup.Item1) {
                    closest_ctvm = cur_ctvm;
                    closet_tup = cur_tup;
                }
            }
            if(closest_ctvm == null) {
                Debugger.Break();
            }
            _dropIdx = BindingContext.PinnedItems.IndexOf(closest_ctvm);
            MpConsole.WriteLine("DropIdx: " + _dropIdx);

            var dropAdorner = MpAvMousePressCommand.GetDropAdorner(this);
            if(dropAdorner == null) {
                Debugger.Break();
                MpConsole.WriteLine("Error Drop adorner missing! Ignoring rendering drop adorners");
                return;
            }

            MpLine line = closest_ctvm.ObservedBounds.GetSideByLabel(closet_tup.Item2);
            if(line == null) {
                return;
            }
            line.FillOctColor = MpSystemColors.White;
            dropAdorner.DrawDropAdorner(new MpShape[] { line });
            //return dragEffects;
        }

        void MpAvIDropHost.DragLeave() {
            _dropIdx = -1;

            var dropAdorner = MpAvMousePressCommand.GetDropAdorner(this);
            if (dropAdorner == null) {
                Debugger.Break();
                MpConsole.WriteLine("Error Drop adorner missing! Ignoring rendering drop adorners");
                return;
            }
            dropAdorner.DrawDropAdorner(null);
        }

        async Task<DragDropEffects> MpAvIDropHost.DropDataObjectAsync(IDataObject avdo, DragDropEffects effects) {
            // NOTE only pin tray allows drop

            if (avdo.GetDataFormats().Contains(MpAvDataObjectHelper.CLIP_TILE_DATA_FORMAT)) {
                var drop_ctvm = avdo.Get(MpAvDataObjectHelper.CLIP_TILE_DATA_FORMAT) as MpAvClipTileViewModel;
                if (drop_ctvm == null) {
                    Debugger.Break();
                }
                bool isCopy = effects.HasFlag(DragDropEffects.Copy);

                if (isCopy) {
                    // perform duplicate
                    var dup_ci = (MpCopyItem)await BindingContext.SelectedItem.CopyItem.Clone(true);

                    await dup_ci.WriteToDatabaseAsync();

                    drop_ctvm = await BindingContext.CreateClipTileViewModel(dup_ci, -1);
                    BindingContext.PinTileCommand.Execute(drop_ctvm);

                    BindingContext.PinnedItems.Move(BindingContext.PinnedItems.Count - 1, _dropIdx);
                } else {
                    if (drop_ctvm.IsPinned) {
                        // perform move

                    } else {
                        BindingContext.ToggleTileIsPinnedCommand.Execute(drop_ctvm);
                        return DragDropEffects.Move;
                    }
                }

            }
            return DragDropEffects.None;
        }

        #endregion

        public MpAvPinTrayView() {
            InitializeComponent();
            PinTrayListBox = this.FindControl<ListBox>("PinTrayListBox");
            //MpAvViewBehaviorFactory.BuildAllViewBehaviors(this, this);

            //this.AttachedToVisualTree += MpAvClipTileTitleView_AttachedToVisualTree;
        }

        private void MpAvClipTileTitleView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            MpAvViewBehaviorFactory.BuildAllViewBehaviors(this, this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
