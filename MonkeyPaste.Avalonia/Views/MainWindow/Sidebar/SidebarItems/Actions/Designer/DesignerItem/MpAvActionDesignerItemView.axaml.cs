
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpActionDesignerItemView.xaml
    /// </summary>
    public partial class MpAvActionDesignerItemView : MpAvUserControl<MpAvActionViewModelBase> {
        public MpAvActionDesignerItemView() {
            InitializeComponent();

            var dicc = this.FindControl<ContentControl>("DesignerItemContentControl");
            dicc.AddHandler(ContentControl.KeyDownEvent, Dicc_KeyDown, RoutingStrategies.Tunnel);
            InitDnd();
        }
        private void Dicc_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                if (BindingContext.IsSelected) {
                    e.Handled = true;
                    BindingContext.DeleteThisActionCommand.Execute(null);
                }
            }
        }

        #region Drop

        private void InitDnd() {
            var drop_control = this;
            DragDrop.SetAllowDrop(drop_control, true);
            drop_control.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            drop_control.AddHandler(DragDrop.DragOverEvent, DragOver);
            drop_control.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            drop_control.AddHandler(DragDrop.DropEvent, Drop);
        }
        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            BindingContext.IsDragOver = true;

        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            ResetDrop();
        }

        private void DragOver(object sender, DragEventArgs e) {
            bool is_valid = IsDropValid(e.Data);
            e.DragEffects =
                is_valid ?
                    /*DragDropEffects.Move*/DragDropEffects.Copy : DragDropEffects.None;
            //MpConsole.WriteLine($"[DragOver] TagTile: '{e.DragEffects}'");

        }

        private async void Drop(object sender, DragEventArgs e) {
            bool is_valid = IsDropValid(e.Data);

            e.DragEffects =
                is_valid ?
                    /*DragDropEffects.Move*/DragDropEffects.Copy : DragDropEffects.None;

            if (e.DragEffects == DragDropEffects.None) {
                ResetDrop();
                return;
            }
            MpAvTagTileViewModel drag_ttvm = null;
            List<MpCopyItem> drop_cil = new List<MpCopyItem>();
            if (e.Data.TryGetSourceRefIdBySourceType(MpTransactionSourceType.CopyItem, out int ciid) &&
                MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == ciid) is MpAvClipTileViewModel drop_ctvm) {
                // tile drop
                drop_cil.Add(drop_ctvm.CopyItem);
            } else if (e.Data.TryGetDragTagViewModel(out MpAvTagTileViewModel ttvm)) {
                // tag drop
                drop_cil = await MpDataModelProvider.GetCopyItemsByTagIdAsync(
                    tid: ttvm.TagId,
                    ignore_descendants: false);
                ttvm.TotalAnalysisCount = drop_cil.Count;
                drag_ttvm = ttvm;
            } else {
                // external drop
                var ext_ci = await e.Data.ToCopyItemAsync();
                if (ext_ci != null && ext_ci.Id > 0) {
                    drop_cil.Add(ext_ci);
                }
            }
            drop_cil = drop_cil.DistinctBy(x => x.Id).ToList();
            if (!drop_cil.Any()) {
                e.DragEffects = DragDropEffects.None;
                ResetDrop();
                return;
            }


            ResetDrop();
            if (drag_ttvm == null) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var sw = Stopwatch.StartNew();
                // BUG parallel throws a com exception running img ann for an access violation
                // not sure if thats a me bug or not but probably best to avoid since that can be problematic
                // TODO? if decide to use parallel maybe do parallel w/ ctrl mod key down or something i dunno
                // if it works you probably always want to do it

                bool invoke_sequential = !e.KeyModifiers.HasFlag(KeyModifiers.Control);
                MpConsole.WriteLine($"Batch drop started. Mode: {(invoke_sequential ? "SEQUENTIAL" : "PARALLEL")}");
                // Run drop w/ background priority since it may be long running
                if (invoke_sequential) {
                    foreach (var ci in drop_cil) {
                        BindingContext.InvokeThisActionCommand.Execute(ci);
                        // slight delay for state change
                        await Task.Delay(20);
                        //while (BindingContext.IsSelfOrAnyDescendantPerformingAction) {
                        while (BindingContext.IsSelfOrDescendentProcessingItemById(ci.Id)) {
                            await Task.Delay(100);
                        }
                        drag_ttvm.CompletedAnalysisCount++;
                        drag_ttvm.OnPropertyChanged(nameof(drag_ttvm.PercentLoaded));
                    }
                } else {
                    // invoke in parallel
                    drop_cil.ForEach(x => BindingContext.InvokeThisActionCommand.Execute(x));

                    // create list of all ciid's for tag to be processed (to avoid using remaininCount == 0 as terminator)
                    var remaining_item_ids = drop_cil.Select(x => x.Id).ToList();
                    while (true) {
                        if (remaining_item_ids.Count == 0) {
                            break;
                        }
                        var to_remove = new List<int>();
                        for (int i = 0; i < remaining_item_ids.Count; i++) {
                            if (BindingContext.IsSelfOrDescendentProcessingItemById(remaining_item_ids[i])) {
                                continue;
                            }
                            to_remove.Add(remaining_item_ids[i]);
                        }
                        drag_ttvm.CompletedAnalysisCount += to_remove.Count;
                        drag_ttvm.OnPropertyChanged(nameof(drag_ttvm.PercentLoaded));
                        MpDebug.Assert(drag_ttvm.CompletedAnalysisCount >= 0, $"Analyze count mismatch for tag '{drag_ttvm}' droppe onto action '{BindingContext}'");
                        for (int i = 0; i < to_remove.Count; i++) {
                            remaining_item_ids.Remove(to_remove[i]);
                        }
                        await Task.Delay(500);
                    }
                }
                MpConsole.WriteLine($"Batch process complete of {drag_ttvm.TotalAnalysisCount} items in {sw.ElapsedMilliseconds}ms");
                // reset progress
                drag_ttvm.TotalAnalysisCount = 0;
                drag_ttvm.CompletedAnalysisCount = 0;

                drag_ttvm.OnPropertyChanged(nameof(drag_ttvm.TotalAnalysisCount));

            }, DispatcherPriority.Render);
        }

        #endregion

        #region Drop Helpers

        private bool IsDropValid(IDataObject avdo) {
            if (BindingContext == null //||
                                       //!BindingContext.RootTriggerActionViewModel.IsEnabled.IsTrue()
                ) {
                return false;
            }
            return true;
        }

        private void ResetDrop() {
            if (BindingContext == null) {
                return;
            }
            BindingContext.IsDragOver = false;

        }

        #endregion

        #endregion
    }
}
