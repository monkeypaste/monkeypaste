using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
        #endregion

        #region Properties
        #endregion

        public MpAvTagView() {
            AvaloniaXamlLoader.Load(this);
            this.AttachedToVisualTree += MpAvTagView_AttachedToVisualTree;
            this.AddHandler(PointerPressedEvent, MpAvTagView_PointerPressed, RoutingStrategies.Tunnel);

        }

        private void MpAvTagView_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (!e.IsLeftPress(sender as Visual)) {
                return;
            }
            //if (!BindingContext.IsSelected) {
            // ignore drag check when not selected, 
            // to pass input to select tag
            // }
            //bool ended = false;
            this.DragCheckAndStart(e,
                start: async (start_e) => {
                    //while (Mp.Services.Query.IsQuerying) {
                    //    // wait for tag selection to finish 
                    //    if (ended) {
                    //        // was just a click
                    //        return;
                    //    }
                    //    await Task.Delay(50);
                    //}
                    var dragButton = sender as Control;
                    if (dragButton == null) {
                        return;
                    }
                    BindingContext.IsDragging = true;
                    BindingContext.IsPinTagDragging = IsPinTrayTagView();

                    var mpdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT, BindingContext);
                    var result = await DragDrop.DoDragDrop(e, mpdo, DragDropEffects.Move | DragDropEffects.Copy);

                    BindingContext.IsDragging = false;
                    BindingContext.IsPinTagDragging = false;
                    MpConsole.WriteLine($"Tag Tile Drop Result: '{result}'");
                },
                move: null,
                end: (end_e) => {

                    //ended = true;
                });
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
            BindingContext.IsExpanded = true;
            BindingContext.IsContentDragOverTag = !e.Data.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT);

        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            //MpConsole.WriteLine("[DragLeave] TagTile: " + BindingContext);
            ResetDrop();
        }

        private void DragOver(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragOver] TagTile: " + BindingContext);
            //e.Handled = true;
            GetItemsControl().AutoScrollItemsControl(e);

            BindingContext.Parent.IsPinTrayDragOver = IsPinTrayTagView();

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_valid = IsDropValid(e.Data, is_copy);
            if (BindingContext.IsContentDragOverTag) {
                BindingContext.IsContentDragOverTagValid = is_valid;
            } else {
                BindingContext.IsTagDragValid = is_valid;
            }

            e.DragEffects =
                is_copy ? DragDropEffects.Copy :
                is_valid ?
                    DragDropEffects.Move : DragDropEffects.None;
            MpConsole.WriteLine($"[DragOver] TagTile: '{e.DragEffects}'");

            if (!e.Data.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT)) {
                return;
            }

            if (is_valid) {
                BindingContext.IsTagDragOverCopy = is_copy;
            } else {
                BindingContext.IsTagDragOverCopy = false;
            }

            bool is_after;
            bool is_leaf_child_drop = false;
            var mp = e.GetPosition(this);
            if (IsPinTrayTagView()) {
                is_after = mp.X > this.Bounds.Width / 2;
                is_leaf_child_drop = false;
            } else {
                is_after = mp.Y > this.Bounds.Height / 2;
                // for tree drop check special case if leaf
                // to decide if drag is a child or next sibling by x offset
                if (BindingContext.IsLeaf && is_after) {
                    is_leaf_child_drop = mp.X > this.Bounds.Width / 4;
                }
            }
            BindingContext.IsTagDragOverBottom = is_after;
            BindingContext.IsTagDragOverTop = !is_after;
            BindingContext.IsTagDragLeafChildDrop = is_leaf_child_drop;
        }

        private async void Drop(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[Drop] TagTile: " + BindingContext + " IsCopy: " + is_copy + " IsValid: " + BindingContext.IsContentDragOverTagValid);

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_valid = IsDropValid(e.Data, is_copy);
            if (BindingContext.IsContentDragOverTag) {
                BindingContext.IsContentDragOverTagValid = is_valid;
            } else {
                BindingContext.IsTagDragValid = is_valid;
            }

            e.DragEffects =
                is_copy ? DragDropEffects.Copy :
                is_valid ?
                    DragDropEffects.Move : DragDropEffects.None;

            if (e.DragEffects == DragDropEffects.None) {
                ResetDrop();
                return;
            }

            //
            if (e.Data.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) &&
                e.Data.Get(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) is MpAvTagTileViewModel drag_ttvm) {
                // SORT
                int this_sort_idx = IsPinTrayTagView() ?
                    BindingContext.PinSortIdx : BindingContext.TreeSortIdx;

                int drop_idx = BindingContext.IsTagDragOverTop ?
                    this_sort_idx : this_sort_idx + 1;

                bool is_pinning = IsPinTrayTagView();

                int target_parent_id = BindingContext.IsTagDragLeafChildDrop ?
                    BindingContext.TagId : BindingContext.ParentTagId;

                await drag_ttvm.MoveOrCopyThisTagCommand.ExecuteAsync(
                    new object[] {
                            target_parent_id,
                            drop_idx,
                            is_pinning,
                            is_copy});
            } else {
                // CONTENT DROP

                BindingContext.IsBusy = true;
                bool is_internal = e.Data.ContainsInternalContentItem();
                if (is_internal) {
                    // Internal Drop
                    await PerformTileDropAsync(e.Data, is_copy);
                } else {
                    // External Drop
                    await PerformExternalOrPartialDropAsync(e.Data);
                }
                BindingContext.IsBusy = false;
            }
            ResetDrop();
        }

        #endregion

        #region Drop Helpers

        private bool IsDropValid(IDataObject avdo, bool is_copy) {
            if (avdo.Contains(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT)) {
                // can't sit here!
                return false;
            }

            //MpConsole.WriteLine($"DragOverTag: " + BindingContext + " IsCopy: " + is_copy);
            if (avdo.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) &&
                avdo.Get(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) is MpAvTagTileViewModel drag_ttvm) {
                bool can_move = IsPinTrayTagView() || drag_ttvm.CanTreeMove;
                if (!can_move && !is_copy) {
                    // reject moving root tags
                    return false;
                }
                if (IsPinTrayTagView()) {
                    // on pin tray rely on CanPin property
                    return drag_ttvm.CanPin;
                }
                if (drag_ttvm.AllDescendants.Any(x => x == BindingContext)) {
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
                    bool is_already_linked = BindingContext.IsCopyItemLinked(ctvm.CopyItemId);
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
            BindingContext.IsTagDragLeafChildDrop = false;

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
                drop_ci = await drop_ctvm.CopyItem.CloneDbModelAsync(deepClone: true);
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
            bool from_ext = !avdo.ContainsInternalContentItem();
            MpPortableDataObject mpdo = await Mp.Services.DataObjectHelperAsync.ReadDragDropDataObjectAsync(avdo) as MpPortableDataObject;

            //int drag_ciid = -1;
            string drag_ctvm_pub_handle = mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            if (!string.IsNullOrEmpty(drag_ctvm_pub_handle)) {
                var drag_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drag_ctvm_pub_handle);
                if (drag_ctvm != null) {
                    // tile sub-selection drop

                    mpdo.SetData(MpPortableDataFormats.LinuxUriList, new string[] { Mp.Services.SourceRefBuilder.ConvertToRefUrl(drag_ctvm.CopyItem) });
                }
            }

            MpCopyItem drop_ci = await Mp.Services.CopyItemBuilder.BuildAsync(
                mpdo,
                transType: MpTransactionType.Created,
                force_ext_sources: from_ext);

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

        private ItemsControl _itemsControl;
        private bool? _isPinView;
        private bool IsPinTrayTagView() {
            if (_isPinView == null) {
                _isPinView = this.GetVisualAncestor<MpAvTagTrayView>() != null;
            }
            return _isPinView.IsTrue();
        }

        private ItemsControl GetItemsControl() {
            if (_itemsControl == null) {
                if (this.GetVisualAncestor<TreeView>() is TreeView tv) {
                    _itemsControl = tv;
                } else if (this.GetVisualAncestor<ListBox>() is ListBox lb) {
                    _itemsControl = lb;
                }
            }
            return _itemsControl;
        }
    }
}
