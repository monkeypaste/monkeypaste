using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public enum MpTreeLinkOpType {
        None = 0,
        PrevSibling,
        NextSibling,
        Child
    }

    [DoNotNotify]
    public partial class MpAvTagView : MpAvUserControl<MpAvTagTileViewModel> {
        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public MpAvTagView() {
            InitializeComponent();
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
#if MOBILE
            return;
#else
            if (!e.IsLeftPress(sender as Visual) ||
                (BindingContext != null && !BindingContext.IsTagNameReadOnly)) {
                return;
            }

            this.DragCheckAndStart(e,
                start: async (start_e) => {

                    // HACK since preview8 drag source datacontext becomes null after dnd so storing to finsih up
                    MpAvTagTileViewModel dc = BindingContext;
                    var dragButton = sender as Control;
                    if (dragButton == null) {
                        return;
                    }
                    BindingContext.IsDragging = true;
                    BindingContext.IsPinTagDragging = IsPinTrayTagView();

                    var avdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT, BindingContext);
                    var result = await MpAvDoDragDropWrapper.DoDragDropAsync(dragButton, e, avdo, DragDropEffects.Link | DragDropEffects.Copy);

                    if (BindingContext == null) {
                        dc.IsDragging = false;
                        dc.IsPinTagDragging = false;
                    } else {

                        BindingContext.IsDragging = false;
                        BindingContext.IsPinTagDragging = false;
                    }
                    //MpConsole.WriteLine($"Tag Tile Drop Result: '{result}'");
                },
                move: (move_e) => {

                },
                end: (end_e) => {
                    MpDebug.BreakAll();
                    if (end_e == null) {
                        // release was handled in pointer release

                        return;
                    }
                    if (end_e.Source is Control c && c.DataContext is MpAvTagTileViewModel dc) {
                        if (dc.IsDragging) {
                            end_e.Handled = true;
                        }
                    }
                    //ended = true;
                },
                MIN_DISTANCE: 20);

#endif
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
            var link_type = GetTreeLinkType(e.GetPosition(this));
            bool is_drop_valid = IsDropValid(e.Data, is_copy, link_type);

            if (BindingContext.IsContentDragOverTag) {
                BindingContext.IsContentDragOverTagValid = is_drop_valid;
            } else {
                BindingContext.IsTagDragValid = is_drop_valid;
            }

            e.DragEffects = GetDndEffects(is_drop_valid, is_copy, BindingContext.IsContentDragOverTag);
            MpConsole.WriteLine($"[DragOver] TagTile: '{e.DragEffects}' Link Type: '{link_type}'");

            if (!e.Data.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT)) {
                // content drop is done
                return;
            }

            if (is_drop_valid) {
                BindingContext.IsTagDragOverCopy = is_copy;
            } else {
                BindingContext.IsTagDragOverCopy = false;
            }
            // NOTE!! for  child dropline style to work BOTH leaf and bottom need to be true
            BindingContext.IsTagDragOverBottom = link_type != MpTreeLinkOpType.PrevSibling;
            BindingContext.IsTagDragLeafChildDrop = link_type == MpTreeLinkOpType.Child;
            BindingContext.IsTagDragOverTop = link_type == MpTreeLinkOpType.PrevSibling;
        }

        private async void Drop(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[Drop] TagTile: " + BindingContext + " IsCopy: " + is_copy + " IsValid: " + BindingContext.IsContentDragOverTagValid);

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            var link_type = GetTreeLinkType(e.GetPosition(this));
            bool is_drop_valid = IsDropValid(e.Data, is_copy, link_type);

            if (BindingContext.IsContentDragOverTag) {
                BindingContext.IsContentDragOverTagValid = is_drop_valid;
            } else {
                BindingContext.IsTagDragValid = is_drop_valid;
            }

            e.DragEffects = GetDndEffects(is_drop_valid, is_copy, BindingContext.IsContentDragOverTag);

            bool is_tag_drop = e.Data.TryGetDragTagViewModel(out var drag_ttvm);
            IDataObject processed_drop_data =
                is_tag_drop ?
                null :
                 await Mp.Services.DataObjectTools.ReadDragDropDataObjectAsync(e.Data) as IDataObject;

            Dispatcher.UIThread.Post(async () => {
                if (e.DragEffects == DragDropEffects.None) {
                    ResetDrop();
                    return;
                }
                if (is_tag_drop && drag_ttvm != null) {

                    // SORT (NOTE! don't mark as busy, let db events handle)
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
                } else if (processed_drop_data is MpAvDataObject mpdo) {
                    // CONTENT DROP

                    BindingContext.IsBusy = true;
                    var drop_ci = await mpdo.ToCopyItemAsync(is_copy);

                    if (drop_ci == null || drop_ci.Id <= 0) {
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
                    BindingContext.IsBusy = false;
                }
                ResetDrop();
            });
        }

        #endregion

        #region Drop Helpers

        private MpTreeLinkOpType GetTreeLinkType(Point mp) {
            if (BindingContext.IsContentDragOverTag) {
                return MpTreeLinkOpType.None;
            }

            bool is_after;
            bool is_leaf_child_drop = false;
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
                //MpConsole.WriteLine($"MP: {mp} BOUNDS: {this.Bounds}");
            }

            if (is_leaf_child_drop && !BindingContext.ValidChildDropTagTypes.Contains(MpTagType.None)) {
                return MpTreeLinkOpType.Child;
            }
            if (is_after) {
                return MpTreeLinkOpType.NextSibling;
            }
            return MpTreeLinkOpType.PrevSibling;
        }
        private DragDropEffects GetDndEffects(bool is_drop_valid, bool is_copy, bool is_content) {
            DragDropEffects dde = DragDropEffects.None;

            if (!is_drop_valid) {
                return dde;
            }
            //if (is_copy) {
            //    dde |= DragDropEffects.Copy;
            //} else if (!is_content) {
            //    //dde |= DragDropEffects.Move;
            //    dde |= DragDropEffects.Link;
            //}
            //if (is_content) {
            //    dde |= DragDropEffects.Link;
            //}

            //return dde;
            return DragDropEffects.Copy;
        }
        private bool IsDropValid(IDataObject ido, bool is_copy, MpTreeLinkOpType linkType) {

            if (ido.ContainsSearchCriteria()) {
                // can't sit here!
                return false;
            }

            if (ido.TryGetDragTagViewModel(out MpAvTagTileViewModel drag_ttvm)) {
                bool is_tag_drop_valid = IsTagDropValid(drag_ttvm, is_copy, linkType);
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
        private bool IsTagDropValid(MpAvTagTileViewModel drag_ttvm, bool is_copy, MpTreeLinkOpType linkType) {
            bool can_move = IsPinTrayTagView() || drag_ttvm.CanTreeMove;
            //bool is_self_drop = drag_ttvm != null && drag_ttvm.TagId == BindingContext.TagId;

            if (!can_move && !is_copy) {
                // reject moving root tags
                MpConsole.WriteLine($"TagDrop rejected. can_move: {can_move} is_copy: {is_copy}");
                return false;
            }
            if (IsPinTrayTagView()) {
                // on pin tray rely on CanPin property
                return drag_ttvm.CanPin;
            }
            if (!is_copy && drag_ttvm.SelfAndAllDescendants.Cast<MpAvTagTileViewModel>().Any(x => x.TagId == BindingContext.TagId)) {
                // reject self or child drop
                MpConsole.WriteLine($"TagDrop rejected. Internal tree drop for source '{drag_ttvm.TagName}' and target '{BindingContext.TagName}' ");
                return false;
            }


            MpAvTagTileViewModel parent_to_check_ttvm =
                linkType == MpTreeLinkOpType.Child ?
                    BindingContext :
                    BindingContext.ParentTreeItem;

            if (parent_to_check_ttvm != null &&
                !drag_ttvm.ValidParentDragTagTypes.Contains(parent_to_check_ttvm.TagType)) {
                // mainly to reject all tag from becoming child of another link tag
                return false;
            }

            if (linkType == MpTreeLinkOpType.Child) {
                return BindingContext.ValidChildDropTagTypes.Contains(drag_ttvm.TagType);
            }
            return BindingContext.ValidSiblingDropTagTypes.Contains(drag_ttvm.TagType);
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
