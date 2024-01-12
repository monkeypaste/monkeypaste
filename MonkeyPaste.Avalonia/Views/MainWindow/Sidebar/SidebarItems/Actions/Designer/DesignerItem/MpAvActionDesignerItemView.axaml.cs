
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common.Plugin;
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
            InitShortcuts();
            InitDnd();
        }

        private void InitShortcuts() {

            var dicc = this.FindControl<ContentControl>("DesignerItemContentControl");
            dicc.AddHandler(ContentControl.KeyUpEvent, Dicc_KeyUp, RoutingStrategies.Tunnel);
        }

        private void Dicc_KeyUp(object sender, global::Avalonia.Input.KeyEventArgs e) {
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
                    tagType: ttvm.TagType,
                    ciids_to_omit: Mp.Services.ContentQueryTools.GetOmittedContentIds(),
                    ignore_descendants: false);
                drag_ttvm = ttvm;
            } else {
                // external drop
                var ext_ci = await Mp.Services.ContentBuilder.BuildFromDataObjectAsync(e.Data, false, MpDataObjectSourceType.ActionDrop);
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
            Dispatcher.UIThread.Post(async () => {
                var sw = Stopwatch.StartNew();

                int pre_completed_count = 0, pre_total_count = 0, goal_completed_count = 0;
                if (drag_ttvm != null) {
                    pre_completed_count = drag_ttvm.CompletedAnalysisCount;
                    pre_total_count = drag_ttvm.TotalAnalysisCount;

                    drag_ttvm.TotalAnalysisCount += drop_cil.Count;
                    goal_completed_count = drag_ttvm.TotalAnalysisCount;
                }
                while (pre_completed_count < pre_total_count) {
                    // waiting for earlier drop
                    await Task.Delay(100);
                }

                async void OnActionComplete(object sender, object e) {
                    if (drag_ttvm == null) {
                        return;
                    }
                    // TODO maybe overkill but may need to verify the completed output is part of the drop
                    drag_ttvm.CompletedAnalysisCount = drag_ttvm.CompletedAnalysisCount + 1;
                    drag_ttvm.OnPropertyChanged(nameof(drag_ttvm.PercentLoaded));

                    if (drag_ttvm.CompletedAnalysisCount == goal_completed_count) {
                        // this drop is done
                        BindingContext.OnActionComplete -= OnActionComplete;
                    }
                    if (drag_ttvm.CompletedAnalysisCount == drag_ttvm.TotalAnalysisCount) {
                        // all drops are done, reset
                        MpConsole.WriteLine($"Batch process complete of {drag_ttvm.TotalAnalysisCount} items in {sw.ElapsedMilliseconds}ms");
                        drag_ttvm.TotalAnalysisCount = 0;
                        drag_ttvm.CompletedAnalysisCount = 0;
                        drag_ttvm.OnPropertyChanged(nameof(drag_ttvm.TotalAnalysisCount));

                        drag_ttvm.IsBusy = true;
                        while (BindingContext.IsSelfOrAnyDescendantPerformingAction) {
                            await Task.Delay(100);
                        }
                        drag_ttvm.IsBusy = false;
                    }
                }

                BindingContext.OnActionComplete += OnActionComplete;
                foreach (var drop_ci in drop_cil) {
                    BindingContext.InvokeThisActionCommand.Execute(drop_ci);
                }
            }, DispatcherPriority.Render);
        }

        #endregion

        #region Drop Helpers

        private bool IsDropValid(IDataObject avdo) {
            if (BindingContext == null) {
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
