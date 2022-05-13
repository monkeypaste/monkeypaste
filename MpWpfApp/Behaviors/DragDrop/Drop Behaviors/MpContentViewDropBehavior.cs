using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpContentViewDropBehavior : MpDropBehaviorBase<MpContentView> {
        #region Privates              
        private double _autoScrollMinScrollDist = 15;

        private double _autoScrollAccumulator = 5.0d;
        private double _baseAutoScrollVelocity = 5.0d;
        private double _autoScrollVelocity;

        private bool _isPreBlockDrop = false;
        private bool _isPostBlockDrop = false;

        private bool _isBlockDrop => _isPreBlockDrop || _isPostBlockDrop || _isSplitBlockDrop;
        private bool _isSplitBlockDrop => MpShortcutCollectionViewModel.Instance.GlobalIsAltDown;
        #endregion

        public override bool IsDropEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Content;

        public override UIElement RelativeToElement => AssociatedObject.Rtb;

        public override FrameworkElement AdornedElement => AssociatedObject.Rtb;

        public override Orientation AdornerOrientation => Orientation.Vertical;

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        public override void OnLoaded() {
            base.OnLoaded();
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return;
            }

            _dataContext = AssociatedObject.DataContext;

            MpMessenger.Register<MpMessageType>(
                AssociatedObject.DataContext,
                ReceivedAssociateObjectViewModelMessage,
                AssociatedObject.DataContext);

            //IsDebugEnabled = true;
            //AssociatedObject.Rtb.DragOver += Rtb_DragOver;
            //AssociatedObject.Rtb.Drop += Rtb_Drop;
        }

        public override void OnUnloaded() {
            base.OnUnloaded();

            MpMessenger.Unregister<MpMessageType>(
                _dataContext,
                ReceivedAssociateObjectViewModelMessage,
                _dataContext);
        }

        private void ReceivedAssociateObjectViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ContentItemsChanged:
                case MpMessageType.ContentListScrollChanged:
                    RefreshDropRects();
                    break;
            }
        }

        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingMainWindowComplete:
                    if (IsDropEnabled) {
                        RefreshDropRects();
                    }
                    IsDropEnabled = true;
                    UpdateAdorner();
                    break;
            }
        }

        public override List<Rect> GetDropTargetRects() {
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return new List<Rect>();
            }
            var rtbRect = new List<Rect>() {
                new Rect(0, 0, AssociatedObject.Rtb.ActualWidth, AssociatedObject.Rtb.ActualHeight)
            };
            return rtbRect;
        }

        public override int GetDropTargetRectIdx() {            
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return -1;
            }
            if (!MpShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                // NOTE only continue if drop isn't executing or debugging drop idx will be off
                return DropIdx;
            }

            var rtb = AssociatedObject.Rtb;
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;

            var mp = Application.Current.MainWindow.TranslatePoint(gmp, rtb);
            Rect rtb_rect = new Rect(0, 0, rtb.ActualWidth, rtb.ActualHeight);
            if (!rtb_rect.Contains(mp)) {
                Reset();
                //MpConsole.WriteLine("rtb mp (no hit): " + mp);
                return -1;
            }

            var this_ctvm = rtb.DataContext as MpClipTileViewModel;
            if (this_ctvm.IsAnyItemDragging) {
                //if dropping onto self
                if (rtb.Selection.IsEmpty ||
                   (rtb.Selection.Start == rtb.Document.ContentStart &&
                    rtb.Selection.End == rtb.Document.ContentEnd)) {
                    //only allow self drop for partial selection
                    return -1;
                }
                var rtb_mp = Application.Current.MainWindow.TranslatePoint(MpShortcutCollectionViewModel.Instance.GlobalMouseLocation, rtb);
                
                if(rtb.Selection.IsPointInRange(rtb_mp)) {
                    // do not allow drop onto selection
                    return -1;
                }
            }

            var mptp = rtb.GetPositionFromPoint(mp, true); 
            if(mptp == null) {
                // TODO? maybe to differentiate block drops turn off snap in GetPositionFromPoint and only 
                // snap to find block drop

                // (when not snapping) this means mouse is NOT directly over part of text 
                // either after a line break, in header/footer or before line start
                // snap but only check for block drops


                return -1;
            }

            var mptp_rect = mptp.GetCharacterRect(LogicalDirection.Forward);
            var doc_start_rect = rtb.Document.ContentStart.GetCharacterRect(LogicalDirection.Forward);

            double blockThreshold = Math.Max(2, mptp_rect.Height / 4);
            // NOTE to avoid conflicts between each line as pre/post drop only use pre for first
            // line of content then only check post for others
            _isPreBlockDrop = Math.Abs(mp.Y - doc_start_rect.Top) < blockThreshold || mp.Y < doc_start_rect.Top;
            _isPostBlockDrop = Math.Abs(mp.Y - mptp_rect.Bottom) < blockThreshold || mp.Y > mptp_rect.Bottom;

            if(_isSplitBlockDrop && this_ctvm.ItemType != MpCopyItemType.FileList) {
                // inline takes priority if alt is down so pre/post is ignored
                _isPreBlockDrop = _isPostBlockDrop = false;
            }
            if(_isPreBlockDrop) {
                mptp = rtb.Document.ContentStart;
            } else if(this_ctvm.ItemType == MpCopyItemType.FileList) {
                _isPostBlockDrop = true;
            }

            //MpConsole.WriteLine("Pre: " + (_isPreBlockDrop ? "YES" : "NO"));
            //MpConsole.WriteLine("Inline: " + (_isSplitBlockDrop ? "YES" : "NO"));
            //MpConsole.WriteLine("Post: " + (_isPostBlockDrop ? "YES" : "NO"));
            return rtb.Document.ContentStart.GetOffsetToPosition(mptp);
        }

        public override MpShape[] GetDropTargetAdornerShape() {
            var dt_ll = new List<MpShape>();
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return dt_ll.ToArray();
            }
            if (DropIdx < 0) {
                return dt_ll.ToArray();
            }
            var dt_tp = AssociatedObject.Rtb.Document.ContentStart.GetPositionAtOffset(DropIdx);
            if(dt_tp == null) {
                return dt_ll.ToArray();
            }
            // NOTE since default tile width is usually less than document width the lines will wrap
            // so for block drop use rect at beginning/end of line or it will be at weird spot

            double blockLineOffset = 3;

            var line_start_tp = dt_tp.GetLineStartPosition(0);
            var line_start_rect = line_start_tp.GetCharacterRect(LogicalDirection.Forward);
            double pre_y = line_start_rect.Top - blockLineOffset;
            var pre_line = new MpLine(0, pre_y, AssociatedObject.Rtb.ActualWidth, pre_y);

            var line_end_tp = dt_tp.GetLineEndPosition(0);
            var line_end_rect = line_end_tp.GetCharacterRect(LogicalDirection.Backward);
            double post_y = line_end_rect.Bottom + blockLineOffset;
            var post_line = new MpLine(0, post_y, AssociatedObject.Rtb.ActualWidth, post_y);

            var dltp_rect = dt_tp.GetCharacterRect(LogicalDirection.Forward);
            var caret_line = new MpLine(dltp_rect.Left, dltp_rect.Top, dltp_rect.Left, dltp_rect.Bottom);

            if (_isSplitBlockDrop) {
                /*                
                                                                     ------------------------ (pre)
                        this is a line of text and the caret is here | and the line continues
                   (post)----------------------------------------------

                */

                var pre_split_line = pre_line;
                pre_split_line.P1.X = caret_line.P1.X;

                var post_split_line = post_line;
                post_split_line.P2.X = caret_line.P1.X;

                dt_ll.AddRange(new MpShape[] { pre_split_line, caret_line, post_split_line });
            } else if (_isPreBlockDrop) {
                dt_ll.Add(pre_line);
            } else if (_isPostBlockDrop) {
                dt_ll.Add(post_line);
            } else {
                dt_ll.Add(caret_line);
            }

            return dt_ll.ToArray();
        }
        public override bool IsDragDataValid(bool isCopy,object dragData) {
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return false;
            }
            if (!base.IsDragDataValid(isCopy,dragData)) {
                return false;
            }
            var rtb = AssociatedObject.Rtb;
            var drop_ctvm = rtb.DataContext as MpClipTileViewModel;
            if (AssociatedObject != null) {
                if(dragData is MpClipTileViewModel drag_ctvm) {
                    return drop_ctvm.ItemType == drag_ctvm.ItemType;
                }
                if(dragData is List<MpCopyItem> ddl) {
                    if (!isCopy &&
                       ddl.Any(x => x.Id == AssociatedObject.BindingContext.HeadItem.CopyItemId)) {
                        return false;
                    }

                    if (ddl[0].ItemType != MpCopyItemType.Text) {
                        return false;
                    }
                }
                
            }
            return dragData != null;
        }

        public async Task Paste(MpITextSelectionRange tsr, object pasteData) {
            if(AssociatedObject == null) {
                return;
            }
            var rtb = AssociatedObject.Rtb;
            if(rtb == null) {
                return;
            }
            // paste into selection range 
            if(pasteData is MpPortableDataObject pdo) {
                if (tsr.SelectionLength > 0) {
                    rtb.Selection.Text = string.Empty;
                }
                DropIdx = tsr.SelectionStart;
                await Drop(false, pdo);
            }
        }

        public override async Task Drop(bool isCopy, object dragData) {
            if (AssociatedObject == null || DropIdx < 0) {
                return;
            }
            var rtb = AssociatedObject.Rtb;
            var drag_ctvm = dragData as MpClipTileViewModel;
            var drop_ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            MpConsole.WriteLine("Dropping onto " + drop_ctvm + " at idx: " + DropIdx);
            if (drop_ctvm.HeadItem == null) {
                return;
            }

            // BUG storing dropIdx because somehow it gets lost after calling base
            int rtfDropIdx = DropIdx;

            bool pre = _isPreBlockDrop;
            bool post = _isPostBlockDrop;
            bool split = _isSplitBlockDrop;

            string rootGuid = drop_ctvm.HeadItem.CopyItemGuid;
            bool isNewRoot = rtfDropIdx <= 0;

            await base.Drop(isCopy, dragData);

            // get drop range before altering content (if self drop offset may change if selection is before drop so use pointer which is passive ref
            var dropRange = GetDropRange(rtfDropIdx, pre, post, split);

            List<MpCopyItem> dropItems = new List<MpCopyItem>();
            List<MpCopyItem> itemsToDelete = new List<MpCopyItem>();
            if (drag_ctvm == null) {
                if (dragData is MpPortableDataObject mpdo) {
                    // from external source
                    var extDropItem = await MpCopyItemBuilder.CreateFromDataObject(mpdo);
                    dropItems.Add(extDropItem);
                } else {
                    // external data should be pre-processed
                    Debugger.Break();
                }
            } else {
                // from internal content

                //find drag content view
                var dctv = Application.Current.MainWindow
                                .GetVisualDescendents<MpContentView>()
                                .FirstOrDefault(x =>
                                    x.DataContext is MpClipTileViewModel tctvm &&
                                    tctvm.HeadCopyItemId == drag_ctvm.HeadCopyItemId);

                if (dctv == null) {
                    Debugger.Break();
                }
                string dragPlainText = drag_ctvm.SelectedPlainText;
                List<TextElement> dragSelectionElements = dctv.Rtb.Selection.GetAllTextElements().ToList();
                var dragSelectionItems = dragSelectionElements.Select(x => x.Tag as MpCopyItem).Distinct().ToList();
                int selectionOffset = 0;

                foreach(var drag_item in dragSelectionItems.OrderBy(x=>x.CompositeSortOrderIdx)) {
                    MpCopyItem dropItem = null;
                    if(drag_item.ItemType == MpCopyItemType.Text) {

                    }
                    var drag_item_elements = dragSelectionElements.Where(x => (x.Tag as MpCopyItem).Id == drag_item.Id);
                    var drag_item_range = drag_item_elements.Where(x=>dragSelectionElements.Contains(x)).ToTextRange();
                    
                    string drag_item_data = string.Empty;
                    switch (drag_item.ItemType) {
                        case MpCopyItemType.Text:
                            string remaingSelectionPlainText = dragPlainText.Substring(selectionOffset);
                            if(drag_item_range.Text.Contains(remaingSelectionPlainText)) {
                                drag_item_data = remaingSelectionPlainText;
                            } else if(remaingSelectionPlainText.StartsWith(drag_item_range.Text)) {
                                drag_item_data = drag_item_range.Text;
                                selectionOffset = drag_item_data.Length;
                            } else {
                                continue;
                                //Debugger.Break();
                                //drag_item_data = remaingSelectionPlainText;
                            }
                            
                            break;
                        case MpCopyItemType.FileList:
                            drag_item_data = drag_item.ItemData;
                            break;
                        case MpCopyItemType.Image:
                            drag_item_data = drag_item.ItemData;
                            break;
                    }

                    MpCopyItem drop_item = null;
                    if (isCopy) {
                        drop_item = await drag_item.Clone(true) as MpCopyItem;
                        drop_item.ItemData = drag_item_data;
                        drop_item.CopyItemSourceGuid = string.IsNullOrEmpty(drag_item.CopyItemSourceGuid) ? drag_item.Guid : drag_item.CopyItemSourceGuid;                        
                    } else {
                        drop_item = drag_item;
                    }
                    dropItems.Add(drop_item);
                    if (!isCopy && drop_item.ItemType != MpCopyItemType.FileList) {
                        string drag_item_pt = drag_item.ItemData.ToPlainText().Trim().Replace(Environment.NewLine, string.Empty);
                        string drop_item_pt = drop_item.ItemData.ToPlainText().Trim().Replace(Environment.NewLine, string.Empty);
                        if (drop_item_pt == drag_item_pt) {
                            itemsToDelete.Add(drag_item);
                        }
                    }                    
                }
                               
                if(!isCopy) {
                    dctv.Rtb.Selection.Text = string.Empty;

                    string dpt = dctv.Rtb.Document.ToPlainText().Trim().Replace(Environment.NewLine, string.Empty);
                    if (string.IsNullOrWhiteSpace(dpt)) {
                        //when all content is being dropped flag drag source for delete
                        //deleteDragItem = true;
                        itemsToDelete = dragSelectionItems;
                    } else {
                        MpMergedDocumentRtfExtension.SaveTextContent(dctv.Rtb).FireAndForgetSafeAsync(drag_ctvm);
                    }
                }

                //if(isCopy) {
                //    var mpdo = new MpPortableDataObject();
                //    if (dropItems.Count == 0) {
                //        Debugger.Break();
                //    } else if (dropItems[0].ItemType == MpCopyItemType.Text) {
                //        mpdo.DataFormatLookup.Add(MpClipboardFormatType.Text, dragPlainText);
                //    } else if (dropItems[0].ItemType == MpCopyItemType.FileList) {
                //        mpdo.DataFormatLookup.Add(
                //            MpClipboardFormatType.FileDrop,
                //            string.Join(Environment.NewLine, dropItems.Select(x => x.ItemData)));
                //    } else {
                //        Debugger.Break();
                //    }
                //    var internalDropItem = await MpCopyItemBuilder.CreateFromDataObject(mpdo);
                //    if (dropItems.Count > 0) {
                //        internalDropItem.CopyItemSourceGuid = dropItems[0].Guid;
                //    }
                //    dropItems.Add(internalDropItem);

                //} else {
                //    //when drag selection is not copy delete selection from source
                //    dctv.Rtb.Selection.Text = string.Empty;

                //    string dpt = dctv.Rtb.Document.ToPlainText().Trim().Replace(Environment.NewLine, string.Empty);
                //    if (string.IsNullOrWhiteSpace(dpt)) {
                //        //when all content is being dropped flag drag source for delete
                //        //deleteDragItem = true;
                //    } else {
                //        MpMergedDocumentRtfExtension.SaveTextContent(dctv.Rtb).FireAndForgetSafeAsync(drag_ctvm);
                //    }

                //}
                
            }

            foreach(var dropItem in dropItems) {
                switch (dropItem.ItemType) {
                    case MpCopyItemType.Text:
                        if (dropItem.ItemData.IsStringRichTextTable()) {
                            string csv = MpCsvToRtfTableConverter.GetCsv(dropItem.ItemData);
                            dropRange.LoadTable(csv);
                        } else {
                            string pt = dropItem.ItemData.ToPlainText();
                            MpConsole.WriteLine("Drop Plain Text: " + pt);
                            pt = pt.TrimTrailingLineEndings();

                            if (pre) {
                                pt = pt + Environment.NewLine;
                            } else if (post) {
                                pt = Environment.NewLine + pt;
                                var post_tp = dropRange.End.GetLineStartPosition(1);
                                if (post_tp != null) {
                                    pt = pt + Environment.NewLine;
                                }
                            } else if (split) {
                                pt = Environment.NewLine + pt + Environment.NewLine;
                            }

                            if (true) {//pre || post || split) {
                                var preElements = dropRange.GetAllTextElements();
                                dropRange.Text = pt;
                                var postElements = dropRange.GetAllTextElements();
                            } else {
                                dropRange.Start.InsertTextInRun(pt);
                            }

                        }
                        break;
                    case MpCopyItemType.Image:
                        dropRange.LoadImage(dropItem.ItemData);
                        break;
                    case MpCopyItemType.FileList:
                        dropRange.LoadFileItem(dropItem.ItemData.Replace(Environment.NewLine, string.Empty), dropItem.IconId);
                        break;
                }

                // instead of handling all added text elements uniquely
                // find all with null tag (which will be added content) and set tag to added content

                var allTextElements = rtb.Document
                                            .GetAllTextElements()
                                            .OrderBy(x => rtb.Document.ContentStart.GetOffsetToPosition(x.ContentStart)).ToList();
                var allStrayElements = allTextElements.Where(x => x.Tag == null).ToList();
                if (allStrayElements.Count > 0) {
                    allStrayElements.ForEach(x => x.Tag = dropItem);
                }

                dropRange = new TextRange(dropRange.End, dropRange.End);
            }

            var encodedItems = await MpMergedDocumentRtfExtension.EncodeContent(rtb);

            foreach (var dri in itemsToDelete) {
                await dri.DeleteFromDatabaseAsync();
            }

            while (MpClipTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            if (drop_ctvm.IsPinned) {
                await drop_ctvm.InitializeAsync(encodedItems);
            } else {
                MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
            }
            

            while(MpClipTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelByGuid(rootGuid);
            if(civm != null) {
                civm.IsSelected = true;
            }
        }


        private TextRange GetDropRange(int rtfDropIdx, bool pre, bool post, bool split) {
            MpConsole.WriteLine("DropIdx: " + rtfDropIdx);
            MpConsole.WriteLine("Pre: " + (pre ? "TRUE" : "FALSE"));
            MpConsole.WriteLine("Post: " + (post ? "TRUE" : "FALSE"));
            MpConsole.WriteLine("Split: " + (split ? "TRUE" : "FALSE"));

            var rtb = AssociatedObject.Rtb;

            // isolate insertion point and account for block drop
            TextPointer dtp_end;
            if (pre) {
                dtp_end = rtb.Document.ContentStart.GetInsertionPosition(LogicalDirection.Forward);

            } else if (post) {
                dtp_end = rtb.Document.ContentStart.GetPositionAtOffset(rtfDropIdx).GetLineStartPosition(1);
                if(dtp_end == null) {
                    dtp_end = rtb.Document.ContentEnd.GetInsertionPosition(LogicalDirection.Backward);
                } else {
                    dtp_end = dtp_end.GetInsertionPosition(LogicalDirection.Forward);
                }

            } else {
                dtp_end = rtb.Document.ContentStart
                            .GetPositionAtOffset(rtfDropIdx)
                            .GetInsertionPosition(LogicalDirection.Forward);
            }
            return new TextRange(dtp_end, dtp_end);
        }

        public override void AutoScrollByMouse() {
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return;
            }
            
            var rtb = AssociatedObject.Rtb;
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = Application.Current.MainWindow.TranslatePoint(gmp, rtb);

            Rect rtb_rect = new Rect(0, 0, rtb.ActualWidth,rtb.ActualHeight);

            if(rtb.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible) {
                rtb_rect.Height -= rtb.GetVisualDescendent<ScrollViewer>().GetScrollBar(Orientation.Horizontal).Height;
            }
            if (rtb.VerticalScrollBarVisibility == ScrollBarVisibility.Visible) {
                rtb_rect.Width -= rtb.GetVisualDescendent<ScrollViewer>().GetScrollBar(Orientation.Vertical).Width;
            }

            if (!rtb_rect.Contains(mp)) {
                return;
            }

            double ldist = Math.Abs(mp.X - rtb_rect.Left);
            double rdist = Math.Abs(mp.X - rtb_rect.Right);
            double tdist = Math.Abs(mp.Y - rtb_rect.Top);
            double bdist = Math.Abs(mp.Y - rtb_rect.Bottom);

            Point rtbScrollOffsetDelta = new Point(); 
            if(ldist <= _autoScrollMinScrollDist) {
                rtbScrollOffsetDelta.X = -_autoScrollVelocity;
            } else if (rdist <= _autoScrollMinScrollDist) {
                rtbScrollOffsetDelta.X = _autoScrollVelocity;
            }

            if (tdist <= _autoScrollMinScrollDist) {
                rtbScrollOffsetDelta.Y = -_autoScrollVelocity;
            } else if (bdist <= _autoScrollMinScrollDist) {
                rtbScrollOffsetDelta.Y = _autoScrollVelocity;
            }

            //MpConsole.WriteLine(string.Format(@"L {0} R {1} T {2} B {3} V {4}",ldist, rdist, tdist, bdist, _autoScrollVelocity));

            if(rtbScrollOffsetDelta.X != 0 || rtbScrollOffsetDelta.Y != 0) {
                _autoScrollVelocity += _autoScrollAccumulator;
                var cv = rtb.GetVisualAncestor<MpContentView>();
                cv.ScrollByPointDelta(rtbScrollOffsetDelta);
            }

        }

        public override async Task StartDrop() {
            await Task.Delay(1);
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return;
            }
            var rtb = AssociatedObject.Rtb;
            rtb.FitDocToRtb();
            _autoScrollVelocity = _baseAutoScrollVelocity;
            if(rtb.DataContext is MpClipTileViewModel ctvm) {
                ctvm.IsCurrentDropTarget = true;
            }
        }

        public override void CancelDrop() {
            base.CancelDrop();
            if (AssociatedObject != null && 
                AssociatedObject.DataContext is MpClipTileViewModel ctvm) {
                ctvm.IsCurrentDropTarget = false;
            }
        }

        public override void Reset() {
            base.Reset();

            _autoScrollVelocity = _baseAutoScrollVelocity;
            _isPostBlockDrop = _isPreBlockDrop = false;

            if(AssociatedObject == null || AssociatedObject.Rtb == null || AssociatedObject.DataContext == null) {
                return;
            }

            
            if (!MpDragDropManager.IsDragAndDrop || 
               !(AssociatedObject.DataContext as MpClipTileViewModel).IsAnyItemDragging) {
                // these checks make sure selection isn't cleared during self drop
                //rtb.FitDocToRtb();
                var rtb = AssociatedObject.Rtb;
                rtb.ScrollToHome();
                if(rtb.Document == null) {
                    return;
                }

                //rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);
                rtb.FitDocToRtb();
            }
            if (AssociatedObject != null &&
                AssociatedObject.DataContext is MpClipTileViewModel ctvm) {
                ctvm.IsCurrentDropTarget = false;
            }
        }


        private async Task<TextRange> RetargetRange(TextRange foreignTextRange, bool isCopy, string rootGuid) {
            var foreign_rtb = foreignTextRange.Start.Parent.FindParentOfType<RichTextBox>();
            var foreign_ctvm = foreign_rtb.DataContext as MpClipTileViewModel;
            var dtel = foreignTextRange.GetAllTextElements();
            var dragTextElements = dtel.OrderBy(x => foreign_rtb.Document.ContentStart.GetOffsetToPosition(x.ContentStart));
            var dragCopyItemRefs = dragTextElements
                                        .Where(x => x.Tag is MpICopyItemReference)
                                        .Select(x => x.Tag as MpICopyItemReference)
                                        .Distinct()
                                        .ToList();
            var dragCopyItems = await MpDataModelProvider.GetCopyItemsByGuids(dragCopyItemRefs.Select(x => x.Guid).ToArray());

            List<MpCopyItem> dropCopyItems = new List<MpCopyItem>();

            bool willSourceBeRemoved = foreign_ctvm.Count == dragCopyItems.Count && !isCopy;

            //var drtf_fd = drtb.Selection.ToRichText().ToFlowDocument();
            if (!foreign_ctvm.IsAllSelected || isCopy) {
                //retarget partial drag refs
                foreach (var dcr in dragCopyItemRefs) {

                    string dcr_guid = dcr.Guid;

                    var curDragTextElements = dragTextElements.Where(x => x.Tag is MpICopyItemReference cir && cir.Guid == dcr_guid);

                    var curDragSourceRangeStart = curDragTextElements.Aggregate((a, b) => foreign_rtb.Document.ContentStart.GetOffsetToPosition(a.ContentStart) < foreign_rtb.Document.ContentStart.GetOffsetToPosition(b.ContentStart) ? a : b).ElementStart;
                    var curDragSourceRangeEnd = curDragTextElements.Aggregate((a, b) => foreign_rtb.Document.ContentStart.GetOffsetToPosition(a.ContentStart) > foreign_rtb.Document.ContentStart.GetOffsetToPosition(b.ContentStart) ? a : b).ElementEnd;

                    var curDragSourceRange = new TextRange(curDragSourceRangeStart, curDragSourceRangeEnd);
                    var thisSourceRtf = curDragSourceRange.ToRichText();
                    var thisSourceCi = dragCopyItems.FirstOrDefault(x => x.Guid == dcr_guid);

                    MpCopyItem thisSourceNewCi = null;

                    string fullSourcePlainText = thisSourceCi.ItemData.ToPlainText().Trim();
                    string dragSourcePlainText = curDragSourceRange.Text.Trim();
                    if (fullSourcePlainText == dragSourcePlainText && !isCopy) {
                        // NOTE this means this entire item is moving so don't retarget
                        // may need to trim line breaks...
                        thisSourceNewCi = thisSourceCi;
                    } else {
                        if (string.IsNullOrEmpty(dcr.CopyItemSourceGuid)) {
                            //when source is direct content make retargeted source the direct content
                            thisSourceNewCi.CopyItemSourceGuid = dcr_guid;
                        } else {
                            //otherwise carry original content source
                            thisSourceNewCi.CopyItemSourceGuid = dcr.CopyItemSourceGuid;
                        }
                        thisSourceNewCi.Guid = System.Guid.NewGuid().ToString();

                        thisSourceNewCi = await thisSourceCi.Clone(false) as MpCopyItem;
                        thisSourceNewCi.Id = 0;
                        thisSourceNewCi.ItemData = thisSourceRtf;
                    }

                    if (string.IsNullOrEmpty(rootGuid)) {
                        // NOTE this is only null if drop is at beginning of target and this the first drop item
                        rootGuid = thisSourceNewCi.Guid;
                    } else {
                        thisSourceNewCi.RootCopyItemGuid = rootGuid;
                    }
                    await thisSourceNewCi.WriteToDatabaseAsync();

                    dropCopyItems.Add(thisSourceNewCi);
                    // NOTE retain sort from source
                    dropCopyItems = dropCopyItems.OrderBy(x => dragCopyItems.IndexOf(dragCopyItems.FirstOrDefault(y => y.Guid == dcr_guid))).ToList();
                }
            } else {
                // NOTE sort is carried from drag
                dropCopyItems = dragCopyItems;
                if (string.IsNullOrEmpty(rootGuid)) {
                    rootGuid = dropCopyItems[0].Guid;
                }
            }

            return null;
        }

    }

}
