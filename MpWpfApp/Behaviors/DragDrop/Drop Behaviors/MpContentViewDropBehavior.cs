using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Documents;

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
            if (this_ctvm.IsItemDragging) {
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
                       ddl.Any(x => x.Id == AssociatedObject.BindingContext.CopyItemId)) {
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
            int dropItemId = drop_ctvm.CopyItemId;

            bool isSelfDrop = drag_ctvm == drop_ctvm;

            MpConsole.WriteLine("Dropping onto " + drop_ctvm + " at idx: " + DropIdx);
            if (drop_ctvm == null) {
                return;
            }

            // BUG storing dropIdx because somehow it gets lost after calling base
            int rtfDropIdx = DropIdx;

            bool pre = _isPreBlockDrop;
            bool post = _isPostBlockDrop;
            bool split = _isSplitBlockDrop;

            await base.Drop(isCopy, dragData);

            // get drop range before altering content (if self drop offset may change if selection is before drop so use pointer which is passive ref
            var dropRange = GetDropRange(rtfDropIdx, pre, post, split);

            bool deleteDragItem = false;
            MpContentView dragContentView = null;
            string dropData = string.Empty;
            if (drag_ctvm == null) {
                if (dragData is MpPortableDataObject mpdo) {
                    // from external source
                    var tempCopyItem = await MpCopyItemBuilder.CreateFromDataObject(mpdo, true);
                    dropData = tempCopyItem.ItemData.ToPlainText();
                } else {
                    // external data should be pre-processed
                    Debugger.Break();
                }
            } else {
                // from internal content

                //find drag content view
                dragContentView = Application.Current.MainWindow
                                .GetVisualDescendents<MpContentView>()
                                .FirstOrDefault(x =>
                                    x.DataContext is MpClipTileViewModel tctvm &&
                                    tctvm.CopyItemId == drag_ctvm.CopyItemId);

                switch(drag_ctvm.ItemType) {
                    case MpCopyItemType.Text:
                        dropData = dragContentView.Rtb.Selection.Text;
                        break;
                    case MpCopyItemType.FileList:
                        dropData = dragContentView.Rtb.Document
                                                             .GetAllTextElements()
                                                             .Where(x => x is Hyperlink && x.IsEnabled)
                                                             .Cast<Hyperlink>()
                                                             .Select(x => x.NavigateUri.LocalPath)
                                                             .FirstOrDefault();
                        if(dropData == null) {
                            //when dragging from below file list and not hovering over any item, select last
                            var fil = drag_ctvm.CopyItemData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            if(fil.Count == 0) {
                                Debugger.Break();
                            }
                            dropData = fil[fil.Count - 1];
                        }
                        break;
                    default:
                        return;
                }

                if (!isCopy) {
                    //when drag selection is not copy delete selection from source
                    dragContentView.Rtb.Selection.Text = string.Empty;

                    string dpt = dragContentView.Rtb.Document.ToPlainText().Trim().Replace(Environment.NewLine, string.Empty);
                    if (string.IsNullOrWhiteSpace(dpt) && !isSelfDrop) {
                        //when all content is being dropped flag drag source for delete
                        deleteDragItem = true;
                    }
                }
            }
            if(dropData.IsStringBase64()) {
                dropRange.LoadImage(dropData);
            } else if(dropData.IsStringFileOrPathFormat()) {
                dropRange = dropRange.End.ToTextRange();
                dropRange.LoadItemData(dropData, MpCopyItemType.FileList);
            } else if (dropData.IsStringRichTextTable()) {
                string csv = MpCsvToRtfTableConverter.GetCsv(dropData);
                dropRange.LoadTable(csv);
            } else {
                MpConsole.WriteLine("Drop Plain Text: " + dropData);
                dropData = dropData.TrimTrailingLineEndings();

                if (pre) {
                    dropData = dropData + Environment.NewLine;
                } else if (post) {
                    dropData = Environment.NewLine + dropData;
                    var post_tp = dropRange.End.GetLineStartPosition(1);
                    if (post_tp != null) {
                        dropData = dropData + Environment.NewLine;
                    }
                } else if (split) {
                    dropData = Environment.NewLine + dropData + Environment.NewLine;
                }
                dropRange.Text = dropData;
            }

            await MpContentDocumentRtfExtension.SaveTextContent(rtb);

            if(deleteDragItem) {
                await drag_ctvm.CopyItem.DeleteFromDatabaseAsync();
            } else if(dragContentView != null){
                await MpContentDocumentRtfExtension.SaveTextContent(dragContentView.Rtb);
            }

            while (MpClipTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            if (drop_ctvm.IsPinned) {
                await drop_ctvm.InitializeAsync(drop_ctvm.CopyItem);
            } else {
                MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
            }
            

            while(MpClipTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            // NOTE since drag item was likely deleted this behavior may
            // not be attached to the same content anymore so use id ref to find/select
            // drop tile

            var ctvm_toSelect = MpClipTrayViewModel.Instance.GetClipTileViewModelById(dropItemId);
            if(ctvm_toSelect != null) {
                ctvm_toSelect.IsSelected = true;
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
               !(AssociatedObject.DataContext as MpClipTileViewModel).IsItemDragging) {
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
    }
}
