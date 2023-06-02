using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Org.BouncyCastle.Crypto.Fpe;
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

        #region Constructors
        public MpAvTagView() {
            AvaloniaXamlLoader.Load(this);
            this.AttachedToVisualTree += MpAvTagView_AttachedToVisualTree;
            this.AddHandler(PointerPressedEvent, MpAvTagView_PointerPressed, RoutingStrategies.Tunnel);

        }
        #endregion

        #region Public Methods

        public bool IsPinTrayTagView() {
            if (_isPinView == null) {
                _isPinView = this.GetVisualAncestor<MpAvTagTrayView>() != null;
            }
            return _isPinView.IsTrue();
        }
        #endregion

        #region Private Methods

        private void MpAvTagView_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (!e.IsLeftPress(sender as Visual) ||
                (BindingContext != null && !BindingContext.IsTagNameReadOnly)) {
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

                    // HACK since preview8 drag source datacontext becomes null after dnd so storing to finsih up
                    MpAvTagTileViewModel dc = BindingContext;
                    var dragButton = sender as Control;
                    if (dragButton == null) {
                        return;
                    }
                    BindingContext.IsDragging = true;
                    BindingContext.IsPinTagDragging = IsPinTrayTagView();

                    var mpdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT, BindingContext);
                    var result = await DragDrop.DoDragDrop(e, mpdo, DragDropEffects.Link | DragDropEffects.Copy);

                    if (BindingContext == null) {
                        dc.IsDragging = false;
                        dc.IsPinTagDragging = false;
                    } else {

                        BindingContext.IsDragging = false;
                        BindingContext.IsPinTagDragging = false;
                    }
                    MpConsole.WriteLine($"Tag Tile Drop Result: '{result}'");
                },
                move: null,
                end: (end_e) => {

                    //ended = true;
                });
        }

        private void MpAvTagView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            InitDnd();
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

        private void InitDnd() {
            var drop_control = this;
            DragDrop.SetAllowDrop(drop_control, true);
            drop_control.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            drop_control.AddHandler(DragDrop.DragOverEvent, DragOver);
            drop_control.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            drop_control.AddHandler(DragDrop.DropEvent, Drop);
        }
        #region Drop Events
        MpAvTagTileViewModel _drop_dc = null;

        private void DragEnter(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragEnter] TagTile: " + BindingContext);

            _drop_dc = BindingContext;
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
            bool is_drop_valid = IsDropValid(e.Data, is_copy);
            if (BindingContext.IsContentDragOverTag) {
                BindingContext.IsContentDragOverTagValid = is_drop_valid;
            } else {
                BindingContext.IsTagDragValid = is_drop_valid;
            }

            e.DragEffects = GetDndEffects(is_drop_valid, is_copy);
            //MpConsole.WriteLine($"[DragOver] TagTile: '{e.DragEffects}'");

            if (!e.Data.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT)) {
                return;
            }

            if (is_drop_valid) {
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
            bool is_drop_valid = IsDropValid(e.Data, is_copy);
            if (BindingContext.IsContentDragOverTag) {
                BindingContext.IsContentDragOverTagValid = is_drop_valid;
            } else {
                BindingContext.IsTagDragValid = is_drop_valid;
            }

            e.DragEffects = GetDndEffects(is_drop_valid, is_copy);

            if (e.DragEffects == DragDropEffects.None) {
                ResetDrop();
                return;
            }

            BindingContext.IsBusy = true;
            // NOTE need to use processed/output data object, avdo becomes disposed
            // (or weird stuff happens..external drop made invalid copyitemTag w/ the ciid. it was a huge number, not sure why
            // hard to step trace due dnd thread)
            var mpdo = await
                Mp.Services.DataObjectHelperAsync
                .ReadDragDropDataObjectAsync(e.Data) as IDataObject;
            //
            if (mpdo.TryGetDragTagViewModel(out var drag_ttvm)) {
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

                //bool is_internal = mpdo.ContainsContentRef();
                //if (is_internal) {
                //    // Internal Drop
                //    await PerformTileDropAsync(mpdo, is_copy);
                //} else {
                //    // External Drop
                //    await PerformExternalOrPartialDropAsync(mpdo, is_copy);
                //}
                var drop_ci = await mpdo.ToCopyItemAsync(
                    //addAsNewItem: true,
                    is_copy: is_copy
                    );

                if (drop_ci == null || drop_ci.Id == 0) {
                    ResetDrop();
                    BindingContext.IsBusy = false;
                    return;
                }

                BindingContext.LinkCopyItemCommand.Execute(drop_ci.Id);
                if (BindingContext.IsSelected) {
                    // when selected do in-place requery
                    while (!MpAvClipTrayViewModel.Instance.QueryCommand.CanExecute(string.Empty)) {
                        await Task.Delay(100);
                    }
                    MpAvClipTrayViewModel.Instance.QueryCommand.CanExecute(string.Empty);
                }
            }
            ResetDrop();
            BindingContext.IsBusy = false;
        }

        #endregion

        #region Drop Helpers

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
        private bool IsDropValid(IDataObject ido, bool is_copy) {

            if (ido.ContainsSearchCriteria()) {
                // can't sit here!
                return false;
            }

            if (ido.TryGetDragTagViewModel(out MpAvTagTileViewModel drag_ttvm)) {
                bool is_tag_drop_valid = IsTagDropValid(drag_ttvm, is_copy);
                return is_tag_drop_valid;
            }


            bool is_content_or_ext_drop_valid = IsContentOrExternalDropValid(ido, is_copy);
            return is_content_or_ext_drop_valid;
        }

        private bool IsContentOrExternalDropValid(IDataObject ido, bool is_copy) {
            if (!BindingContext.CanLinkContent) {
                // only allow content drop onto link tags
                return false;
            }
            bool is_internal = ido.ContainsContentRef();
            if (!is_copy && is_internal) {
                if (ido.TryGetSourceRefIdBySourceType(MpTransactionSourceType.CopyItem, out int ciid)) {
                    bool is_already_linked = BindingContext.IsCopyItemLinked(ciid);
                    if (is_already_linked) {
                        // invalidate tile drag if tag is already linked to copy item and its not a copy operation
                        return false;
                    }
                }

            }
            return true;
        }
        private bool IsTagDropValid(MpAvTagTileViewModel drag_ttvm, bool is_copy) {
            bool can_move = IsPinTrayTagView() || drag_ttvm.CanTreeMove;
            //bool is_self_drop = drag_ttvm != null && drag_ttvm.TagId == BindingContext.TagId;

            if (!can_move && !is_copy) {
                // reject moving root tags
                return false;
            }
            if (IsPinTrayTagView()) {
                // on pin tray rely on CanPin property
                return drag_ttvm.CanPin;
            }
            if (drag_ttvm.SelfAndAllDescendants.Any(x => x == BindingContext)) {
                // reject self or child drop
                return false;
            }
            var parent_vm = BindingContext.ParentTreeItem;

            if ((BindingContext.IsCollectionsTag || (parent_vm != null && (parent_vm.IsCollectionsTag || parent_vm.IsLinkTag))) && drag_ttvm.IsLinkTag) {
                return true;
            }

            if ((BindingContext.IsFiltersTag || (parent_vm != null && parent_vm.IsGroupTag)) &&
                (drag_ttvm.IsQueryTag || drag_ttvm.IsGroupTag)) {
                return true;
            }
            return false;
        }

        private void ResetDrop() {
            if (BindingContext == null) {
                // HACK preview8 drop bug
                BindingContext = _drop_dc;
            }
            if (BindingContext == null) {
                MpConsole.WriteLine("Couldn't reset drop, no bindingContext. IsPinTrayView: " + IsPinTrayTagView());
                return;
            }
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

        private async Task PerformExternalOrPartialDropAsync(IDataObject avdo, bool is_copy) {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                var drop_ci = await avdo.ToCopyItemAsync(
                    //addAsNewItem: true,
                    is_copy: is_copy
                    );

                if (drop_ci == null || drop_ci.Id == 0) {
                    return;
                }

                BindingContext.LinkCopyItemCommand.Execute(drop_ci.Id);

                // wait for all tags to update before notifiying clip tray
                while (MpAvTagTrayViewModel.Instance.IsAnyBusy) { await Task.Delay(100); }


                //MpAvClipTrayViewModel.Instance.AddNewItemsCommand.Execute(drop_ci);
            });
        }
        private ItemsControl _itemsControl;
        private bool? _isPinView;

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
        #endregion

        #endregion

        #endregion
    }
}
