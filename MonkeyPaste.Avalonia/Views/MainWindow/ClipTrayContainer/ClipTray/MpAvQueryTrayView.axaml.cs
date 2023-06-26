using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvQueryTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        #region Private Variables
        private DispatcherTimer _autoScrollTimer;
        private double[] _autoScrollAccumulators;
        #endregion

        #region Statics

        public static MpAvQueryTrayView Instance { get; private set; }

        #endregion

        public MpAvQueryTrayView() {
            if (Instance != null) {
                // ensure singleton
                Debugger.Break();
                return;
            }
            Instance = this;

            AvaloniaXamlLoader.Load(this);

            this.AttachedToVisualTree += MpAvQueryTrayView_AttachedToVisualTree;
            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            //var advSearchSplitter = this.FindControl<GridSplitter>("AdvancedSearchSplitter");
            //advSearchSplitter.DragDelta += AdvSearchSplitter_DragDelta;
            //advSearchSplitter.AddHandler(GridSplitter.DragDeltaEvent, AdvSearchSplitter_DragDelta, RoutingStrategies.Tunnel);
        }


        private void MpAvQueryTrayView_AttachedToVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
            //InitDnd();
        }

        #region Drop

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragEnter] PinTrayListBox: ");
            BindingContext.IsDragOverQueryTray = true;
        }

        private void DragOver(object sender, DragEventArgs e) {
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, is_copy);
            e.DragEffects = GetDndEffects(is_drop_valid, is_copy);
        }
        private void DragLeave(object sender, DragEventArgs e) {
            // MpConsole.WriteLine("[DragLeave] PinTrayListBox: ");
            ResetDrop();
        }

        private async void Drop(object sender, DragEventArgs e) {
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, is_copy);
            // MpConsole.WriteLine("[Drop] PinTrayListBox DropIdx: " + drop_idx + " IsCopy: " + is_copy + " IsValid: " + is_drop_valid);

            e.DragEffects = GetDndEffects(is_drop_valid, is_copy);
            if (!is_drop_valid) {
                ResetDrop();
                return;
            }

            var cur_ttvm = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem;
            // NOTE need to use processed/output data object, avdo becomes disposed
            var mpdo = await
                Mp.Services.DataObjectHelperAsync
                .ReadDragDropDataObjectAsync(e.Data) as MpAvDataObject;

            var drop_ci = await mpdo.ToCopyItemAsync(is_copy: is_copy);
            if (drop_ci == null || drop_ci.Id == 0) {
                ResetDrop();
                return;
            }

            cur_ttvm.LinkCopyItemCommand.Execute(drop_ci.Id);
            if (cur_ttvm.IsSelected) {
                // when selected do in-place requery
                while (!BindingContext.QueryCommand.CanExecute(string.Empty)) {
                    await Task.Delay(100);
                }
                BindingContext.QueryCommand.CanExecute(string.Empty);
            }

            ResetDrop();
        }

        #endregion

        #region Drop Helpers

        private void InitDnd() {
            var ctrlb = this.FindControl<ListBox>("ClipTrayListBox");
            DragDrop.SetAllowDrop(ctrlb, true);
            ctrlb.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            ctrlb.AddHandler(DragDrop.DragOverEvent, DragOver);
            ctrlb.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            ctrlb.AddHandler(DragDrop.DropEvent, Drop);
        }
        private DragDropEffects GetDndEffects(bool is_drop_valid, bool is_copy) {
            DragDropEffects dde = DragDropEffects.None;

            if (!is_drop_valid) {
                return dde;
            }
            dde |= DragDropEffects.Link;
            if (is_copy) {
                dde |= DragDropEffects.Copy;
            }

            return dde;
        }
        private bool IsDropValid(IDataObject avdo, bool is_copy) {
            if (avdo.ContainsSearchCriteria()) {
                return false;
            }
            if (avdo.ContainsTagItem()) {
                return false;
            }
            var cur_ttvm = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem;
            if (!cur_ttvm.CanLinkContent) {
                // only allow drop onto link tags
                return false;
            }
            if (is_copy) {
                return true;
            }
            if (avdo.TryGetSourceRefIdBySourceType(MpTransactionSourceType.CopyItem, out int ciid)) {
                bool is_already_linked = cur_ttvm.IsCopyItemLinked(ciid);
                if (is_already_linked) {
                    // invalidate tile drag if tag is already linked to copy item and its not a copy operation
                    return false;
                }
            }
            return true;
        }

        private void ResetDrop() {
            BindingContext.IsDragOverQueryTray = false;
        }

        private async Task PerformTileDropAsync(int drop_idx, IDataObject avdo, bool isCopy) {
            string drop_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            var drop_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drop_ctvm_pub_handle);
            if (drop_ctvm == null) {
                Debugger.Break();
            }

            if (isCopy) {
                //  duplicate
                var dup_ci = await BindingContext.SelectedItem.CopyItem.CloneDbModelAsync(deepClone: true);
                await dup_ci.WriteToDatabaseAsync();
                drop_ctvm = await BindingContext.CreateClipTileViewModelAsync(dup_ci, -1);
            }
            BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });
            MpConsole.WriteLine($"Tile '{drop_ctvm}' dropped onto pintray idx: {drop_idx}");
        }

        private async Task PerformExternalOrPartialDropAsync(int drop_idx, IDataObject avdo) {
            // NOTE external or partial drop never needs to copy since result is always new content

            //var avdo_ci = await Mp.Services.CopyItemBuilder.BuildAsync(
            //    pdo: mpdo,
            //    force_ext_sources: from_ext,
            //    transType: MpTransactionType.Created);
            //bool from_ext = !avdo.ContainsContentRef();
            var avdo_ci = await avdo.ToCopyItemAsync();

            var drop_ctvm = await BindingContext.CreateClipTileViewModelAsync(avdo_ci, -1);
            BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });

            MpConsole.WriteLine($"PARTIAL Tile '{drop_ctvm}' dropped onto pintray idx: {drop_idx}");
        }

        #endregion

        #endregion

        private void AdvSearchSplitter_DragDelta(object sender, VectorEventArgs e) {
            var gs = sender as GridSplitter;
            var pg = gs.Parent as Grid;
            var pg_r0_def = pg.RowDefinitions[0];
            var sc_sv = this.FindControl<MpAvSearchCriteriaListBoxView>("SearchCriteriaView").FindControl<ScrollViewer>("SearchCriteriaContainerScrollViewer");
            if (pg_r0_def.ActualHeight >= sc_sv.Extent.Height) {
                e.Handled = true;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.MainWindowOrientationChangeBegin:
                case MpMessageType.TrayLayoutChanged:
                    //this.InvalidateMeasure();
                    break;
                case MpMessageType.DropOverTraysBegin:
                    StartAutoScroll();
                    break;
                case MpMessageType.DropOverTraysEnd:
                    StopAutoScroll();
                    break;
            }
        }

        #region Auto Scroll

        private void StartAutoScroll() {
            if (_autoScrollTimer == null) {
                _autoScrollTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
                _autoScrollTimer.Tick += _autoScrollTimer_Tick;
            }
            _autoScrollTimer.Start();
        }

        private void StopAutoScroll() {
            if (_autoScrollTimer == null) {
                return;
            }
            _autoScrollTimer.Stop();
            _autoScrollAccumulators.ForEach(x => x = 0);

        }

        private void _autoScrollTimer_Tick(object sender, EventArgs e) {
            if (!MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                // wait till end of dnd to stop timer
                // otherwise it goes on/off a lot
                BindingContext.NotifyDragOverTrays(false);
                return;
            }
            if (BindingContext.IsBusy) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                var sv = this.FindControl<ScrollViewer>("ClipTrayScrollViewer");
                var lb = this.FindControl<ListBox>("ClipTrayListBox");
                var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
                if (MpAvPagingListBoxExtension.CheckAndDoAutoScrollJump(sv, lb, gmp)) {
                    // drag is over a tray track and is thumb dragging
                    // until outside track, then busy for load more
                    return;
                }
                var scroll_delta = sv.AutoScroll(
                    gmp,
                    sv,
                    ref _autoScrollAccumulators,
                    false);

                BindingContext.ScrollOffset += scroll_delta;
            });
        }

        #endregion
    }
}
