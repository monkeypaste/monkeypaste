using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvTagView : MpAvUserControl<MpAvTagTileViewModel> {
        #region Private Variables

        private static double[] _autoScrollAccumulators;
        #endregion

        #region Properties
        #endregion

        public MpAvTagView() {
            AvaloniaXamlLoader.Load(this);
            this.AttachedToVisualTree += MpAvTagView_AttachedToVisualTree;

            this.AddHandler(PointerPressedEvent, MpAvTagView_PointerPressed, RoutingStrategies.Tunnel);
        }

        private async void MpAvTagView_PointerPressed(object sender, PointerPressedEventArgs e) {
            var dragButton = sender as Control;
            if (dragButton == null) {
                return;
            }
            BindingContext.IsDragging = true;
            _autoScrollAccumulators = null;

            var mpdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT, BindingContext);
            var result = await DragDrop.DoDragDrop(e, mpdo, DragDropEffects.Move | DragDropEffects.Copy);

            BindingContext.IsDragging = false;
            MpConsole.WriteLine($"Tag Tile Drop Result: '{result}'");
        }

        private void MpAvTagView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var drop_control = this;
            DragDrop.SetAllowDrop(drop_control, true);
            drop_control.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            drop_control.AddHandler(DragDrop.DragOverEvent, DragOver);
            drop_control.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            drop_control.AddHandler(DragDrop.DropEvent, Drop);
        }

        private void TagNameTextBox_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                BindingContext.FinishRenameTagCommand.Execute(null);

            } else if (e.Key == Key.Escape) {
                e.Handled = true;
                BindingContext.CancelRenameTagCommand.Execute(null);
            }
        }


        private void TagNameBorder_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
                return;
            }
            if (e.ClickCount > 1) {
                BindingContext.RenameTagCommand.Execute(null);
            } else if (BindingContext.IsSelected) {
                //MpPlatform.Services.QueryInfo.NotifyQueryChanged();
            }
            //MpAvDragDropManager.StartDragCheck(BindingContext);
        }

        #region Drop

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragEnter] TagTile: " + BindingContext);
            BindingContext.IsContentDragOverTag = !e.Data.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT);
        }

        private async void DragOver(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragOver] TagTile: " + BindingContext);
            // e.DragEffects = DragDropEffects.Default;

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_valid = await IsDropValidAsync(e.Data, is_copy);
            if (BindingContext.IsContentDragOverTag) {
                BindingContext.IsContentDragOverTagValid = is_valid;
            } else {
                BindingContext.IsTagDragValid = is_valid;
            }

            //MpConsole.WriteLine("[DragOver] TagTile: " + BindingContext + " IsCopy: " + is_copy + " IsValid: " + BindingContext.IsContentDragOverTagValid);
            e.DragEffects =
                is_copy ? DragDropEffects.Copy :
                is_valid ?
                    DragDropEffects.Move : DragDropEffects.None;

            var drag_tag_vm = e.Data.Get(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) as MpAvTagTileViewModel;
            if (drag_tag_vm == null) {
                return;
            }

            BindingContext.IsExpanded = true;

            if (is_valid) {
                BindingContext.IsTagDragOverCopy = is_copy;
            } else {
                BindingContext.IsTagDragOverCopy = false;
            }

            bool is_after;
            var mp = e.GetPosition(this);
            if (IsTreeTagView()) {
                is_after = mp.Y > this.Bounds.Height / 2;
            } else {
                is_after = mp.X > this.Bounds.Width / 2;
            }
            BindingContext.IsTagDragOverBottom = is_after;
            BindingContext.IsTagDragOverTop = !is_after;


        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            //MpConsole.WriteLine("[DragLeave] TagTile: " + BindingContext);
            ResetDrop();
        }

        private async void Drop(object sender, DragEventArgs e) {
            // NOTE only pin tray allows drop not clip tray

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            BindingContext.IsContentDragOverTagValid = await IsDropValidAsync(e.Data, is_copy);
            //MpConsole.WriteLine("[Drop] TagTile: " + BindingContext + " IsCopy: " + is_copy + " IsValid: " + BindingContext.IsContentDragOverTagValid);

            if (!BindingContext.IsContentDragOverTagValid) {
                ResetDrop();
                return;
            }
            e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;
            if (e.Data.Get(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) is MpAvTagTileViewModel drag_ttvm) {
                // TODO handle tile move/copy here
            } else {
                bool is_internal = e.Data.ContainsInternalContentItem();
                if (is_internal) {
                    // Internal Drop
                    await PerformTileDropAsync(e.Data, is_copy);
                } else {
                    // External Drop
                    await PerformExternalOrPartialDropAsync(e.Data);
                }
            }
            ResetDrop();
        }

        #endregion

        #region Drop Helpers

        private async Task<bool> IsDropValidAsync(IDataObject avdo, bool is_copy) {
            if (avdo.Contains(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT)) {
                // can't sit here!
                return false;
            }

            //MpConsole.WriteLine($"DragOverTag: " + BindingContext + " IsCopy: " + is_copy);
            if (avdo.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) &&
                avdo.Get(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) is MpAvTagTileViewModel drag_ttvm) {
                if (!IsTreeTagView()) {
                    // on pin board hierarchy is meaningless
                    return drag_ttvm.CanPin;
                }
                if (drag_ttvm.SelfAndAllDescendants.Any(x => x == BindingContext)) {
                    // reject self or child drop
                    return false;
                }
                var parent_vm = BindingContext.ParentTreeItem;

                if ((BindingContext.IsAllTag || (parent_vm != null && parent_vm.IsLinkTag)) && drag_ttvm.IsLinkTag) {
                    return true;
                }

                if ((BindingContext.IsRootGroupTag || (parent_vm != null && parent_vm.IsGroupTag)) &&
                    (drag_ttvm.IsQueryTag || drag_ttvm.IsGroupTag)) {
                    return true;
                }
                return false;
            }

            bool is_internal = avdo.ContainsInternalContentItem();
            if (!is_copy && is_internal) {
                // invalidate tile drag if tag is already linked to copy item and its not a copy operation
                string drop_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
                var ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drop_ctvm_pub_handle);
                if (ctvm != null) {
                    bool is_already_linked = await BindingContext.IsCopyItemLinkedAsync(ctvm.CopyItemId);
                    if (is_already_linked) {
                        return false;
                    }
                }
            }
            return true;
        }

        private void ResetDrop() {
            BindingContext.IsContentDragOverTag = false;
            BindingContext.IsContentDragOverTagValid = false;

            BindingContext.IsTagDragOverTop = false;
            BindingContext.IsTagDragOverBottom = false;
            BindingContext.IsTagDragOverCopy = false;
        }

        private async Task PerformTileDropAsync(IDataObject avdo, bool isCopy) {
            string drop_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            var drop_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drop_ctvm_pub_handle);
            if (drop_ctvm == null) {
                Debugger.Break();
            }
            MpCopyItem drop_ci;

            if (isCopy) {
                //  duplicate
                drop_ci = (MpCopyItem)await drop_ctvm.CopyItem.Clone(true);
                await drop_ci.WriteToDatabaseAsync();
            } else {
                // move
                drop_ci = drop_ctvm.CopyItem;
            }
            if (drop_ci == null) {
                return;
            }

            BindingContext.LinkCopyItemCommand.Execute(drop_ci.Id);
        }

        private async Task PerformExternalOrPartialDropAsync(IDataObject avdo) {
            MpPortableDataObject mpdo = await MpPlatform.Services.DataObjectHelperAsync.ReadDragDropDataObjectAsync(avdo) as MpPortableDataObject;

            //int drag_ciid = -1;
            string drag_ctvm_pub_handle = mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            if (!string.IsNullOrEmpty(drag_ctvm_pub_handle)) {
                var drag_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drag_ctvm_pub_handle);
                if (drag_ctvm != null) {
                    // tile sub-selection drop
                    //drag_ciid = drag_ctvm.CopyItemId;

                    mpdo.SetData(MpPortableDataFormats.LinuxUriList, new string[] { MpPlatform.Services.SourceRefBuilder.ConvertToRefUrl(drag_ctvm.CopyItem) });
                }
            }

            MpCopyItem drop_ci = await MpPlatform.Services.CopyItemBuilder.BuildAsync(mpdo);//, drag_ciid);

            if (drop_ci == null) {
                return;
            }

            BindingContext.LinkCopyItemCommand.Execute(drop_ci.Id);

            // wait for all tags to update before notifiying clip tray
            while (MpAvTagTrayViewModel.Instance.IsAnyBusy) { await Task.Delay(100); }

            //push new item onto new item list so it shows in pin regardless if tile is selected
            // NOTE avoiding altering query tray without clicking the thing as consistent behavior.
            // (scrolloffset or sort may not make it visible)          

            MpAvClipTrayViewModel.Instance.AddNewItemsCommand.Execute(drop_ci);
        }

        #endregion

        #endregion

        private bool IsTreeTagView() {
            return this.GetVisualAncestor<MpAvTagTrayView>() == null;
        }
    }
}
