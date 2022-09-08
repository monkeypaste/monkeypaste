using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Input;
using AvaloniaEdit.Document;
using Avalonia.Controls.Primitives;
using MonkeyPaste.Common.Utils.Extensions;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentViewDropBehavior : MpAvDropBehaviorBase<Control> {
        #region Privates              
        private double _autoScrollMinScrollDist = 15;

        private double _autoScrollAccumulator = 5.0d;
        private double _baseAutoScrollVelocity = 5.0d;
        private double _autoScrollVelocity;

        private bool _isPreBlockDrop = false;
        private bool _isPostBlockDrop = false;

        private bool _isBlockDrop => _isPreBlockDrop || _isPostBlockDrop || _isSplitBlockDrop;
        private bool _isSplitBlockDrop => MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown;
        #endregion

        public override bool IsDropEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Content;

        public override Control RelativeToElement => AssociatedObject;

        public override Control AdornedElement => AssociatedObject;

        public override Orientation AdornerOrientation => Orientation.Vertical;

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        public override void OnLoaded() {
            base.OnLoaded();
            if (AssociatedObject == null) {
                return;
            }

            _dataContext = AssociatedObject.DataContext;

            MpMessenger.Register<MpMessageType>(
                AssociatedObject.DataContext,
                ReceivedAssociateObjectViewModelMessage,
                AssociatedObject.DataContext);

            //IsDebugEnabled = true;
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

        protected override void ReceivedGlobalMessage(MpMessageType msg) {
            base.ReceivedGlobalMessage(msg);

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

        public override async Task<List<MpRect>> GetDropTargetRectsAsync() {
            await Task.Delay(1);

            if (AssociatedObject == null) {
                return new List<MpRect>();
            }
            var rtbRect = new List<MpRect>() {
                new MpRect(0, 0, AssociatedObject.Bounds.Width, AssociatedObject.Bounds.Height)
            };
            return rtbRect;
        }

        public override async Task<int> GetDropTargetRectIdxAsync() {            
            if (AssociatedObject == null) {
                return -1;
            }
            if (!MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                // NOTE only continue if drop isn't executing or debugging drop idx will be off
                return DropIdx;
            }

            var control = AssociatedObject;
            MpAvCefNetWebView wv = null;
            if (control is MpAvCefNetContentWebView cwv) { 
                wv = cwv.GetVisualDescendant<MpAvCefNetWebView>();
            }

            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;

            var mp = VisualExtensions.PointToClient(control, gmp.ToAvPixelPoint()).ToPortablePoint();
            MpRect rtb_rect = new MpRect(0, 0, control.Bounds.Width, control.Bounds.Height);
            if (!rtb_rect.Contains(mp)) {
                Reset();
                //MpConsole.WriteLine("rtb mp (no hit): " + mp);
                return -1;
            }
            
            var this_ctvm = control.DataContext as MpAvClipTileViewModel;
            if (this_ctvm.IsItemDragging) {
                //if dropping onto self
                if (wv != null) {
                    if(wv.Selection.IsEmpty ||
                        (wv.Selection.Start == wv.Document.ContentStart &&
                            wv.Selection.End == wv.Document.ContentEnd)) {
                        //only allow self drop for partial selection
                        return -1;
                    }
                    var rtb_mp = VisualExtensions.PointToClient(control, MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation.ToAvPixelPoint()).ToPortablePoint();
                    bool isPressOnSelection = await wv.Selection.IsPointInRangeAsync(rtb_mp);
                    if (isPressOnSelection) {
                        // do not allow drop onto selection
                        return -1;
                    }
                }
                if(!this_ctvm.IsSubSelectionEnabled) {
                    //implies all is selected
                    return -1;
                }
            }

            var mptp = await wv.Document.GetPosisitionFromPointAsync(mp, true); 
            if(mptp == null) {
                // TODO? maybe to differentiate block drops turn off snap in GetPositionFromPoint and only 
                // snap to find block drop

                // (when not snapping) this means mouse is NOT directly over part of text 
                // either after a line break, in header/footer or before line start
                // snap but only check for block drops


                return -1;
            } 


            var mptp_rect = await mptp.GetCharacterRectAsync(LogicalDirection.Forward);
            var doc_start_rect = await wv.Document.ContentStart.GetCharacterRectAsync(LogicalDirection.Forward);

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
                mptp = wv.Document.ContentStart;
            } else if(this_ctvm.ItemType == MpCopyItemType.FileList) {
                _isPostBlockDrop = true;
            }

            //MpConsole.WriteLine("Pre: " + (_isPreBlockDrop ? "YES" : "NO"));
            //MpConsole.WriteLine("Inline: " + (_isSplitBlockDrop ? "YES" : "NO"));
            //MpConsole.WriteLine("Post: " + (_isPostBlockDrop ? "YES" : "NO"));
            return wv.Document.ContentStart.GetOffsetToPosition(mptp);
        }

        public override async Task<MpShape[]> GetDropTargetAdornerShapeAsync() {
            MpAvCefNetWebView wv = null;
            if (AssociatedObject is MpAvCefNetContentWebView cwv) {
                wv = cwv.GetVisualDescendant<MpAvCefNetWebView>();
            }
            var dt_ll = new List<MpShape>();
            if (wv == null) {
                return dt_ll.ToArray();
            }
            if (DropIdx < 0) {
                return dt_ll.ToArray();
            }
            var dt_tp = wv.Document.ContentStart.GetPositionAtOffset(DropIdx);
            if(dt_tp == null) {
                return dt_ll.ToArray();
            }
            // NOTE since default tile width is usually less than document width the lines will wrap
            // so for block drop use rect at beginning/end of line or it will be at weird spot

            double blockLineOffset = 3;

            var line_start_tp = dt_tp.GetLineStartPosition(0);
            var line_start_rect = await line_start_tp.GetCharacterRectAsync(LogicalDirection.Forward);
            double pre_y = line_start_rect.Top - blockLineOffset;
            var pre_line = new MpLine(0, pre_y, AssociatedObject.Bounds.Width, pre_y);

            var line_end_tp = dt_tp.GetLineEndPosition(0);
            var line_end_rect = await line_end_tp.GetCharacterRectAsync(LogicalDirection.Backward);
            double post_y = line_end_rect.Bottom + blockLineOffset;
            var post_line = new MpLine(0, post_y, AssociatedObject.Bounds.Width, post_y);

            var dltp_rect = await dt_tp.GetCharacterRectAsync(LogicalDirection.Forward);
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
        public override async Task<bool> IsDragDataValidAsync(bool isCopy,object dragData) {
            if (AssociatedObject == null || AssociatedObject == null) {
                return false;
            }
            bool isValidBaseResult = await base.IsDragDataValidAsync(isCopy, dragData);
            if (!isValidBaseResult) {
                return false;
            }
            var rtb = AssociatedObject;
            var drop_ctvm = rtb.DataContext as MpAvClipTileViewModel;
            if (AssociatedObject != null) {
                if(dragData is MpAvClipTileViewModel drag_ctvm) {
                    return drop_ctvm.ItemType == drag_ctvm.ItemType;
                }
                if(dragData is List<MpCopyItem> ddl) {
                    if (!isCopy &&
                        AssociatedObject.DataContext is MpAvClipTileViewModel ctvm &&
                       ddl.Any(x => x.Id == ctvm.CopyItemId)) {
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
            var rtb = AssociatedObject;
            MpAvCefNetWebView wv = null;
            if (AssociatedObject is MpAvCefNetContentWebView cwv) {
                wv = cwv.GetVisualDescendant<MpAvCefNetWebView>();
            }
            if (rtb == null || wv == null) {
                return;
            }
            // paste into selection range 
            if(pasteData is MpPortableDataObject pdo) {
                if (tsr.SelectionLength > 0) {
                    wv.Selection.SetTextAsync(string.Empty).FireAndForgetSafeAsync(AssociatedObject.DataContext as MpAvClipTileViewModel);
                }
                DropIdx = tsr.SelectionStart;
                await DropAsync(false, pdo);
            }
        }

        public override async Task DropAsync(bool isCopy, object dragData) {
            MpAvCefNetWebView wv = null;
            if (AssociatedObject is MpAvCefNetContentWebView cwv) {
                wv = cwv.GetVisualDescendant<MpAvCefNetWebView>();
            }

            if (AssociatedObject == null || wv == null || DropIdx < 0) {
                return;
            }
            var rtb = AssociatedObject;
            var drag_ctvm = dragData as MpAvClipTileViewModel;
            var drop_ctvm = AssociatedObject.DataContext as MpAvClipTileViewModel;
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

            await base.DropAsync(isCopy, dragData);

            // get drop range before altering content (if self drop offset may change if selection is before drop so use pointer which is passive ref
            var dropRange = GetDropRange(rtfDropIdx, pre, post, split);

           // bool deleteDragItem = false;
            //MpRtbContentView dragContentView = null;
            string dropData = string.Empty;
            if (drag_ctvm == null) {
                if (dragData is MpPortableDataObject mpdo) {
                    if(MpAvCefNetWebView.DraggingRtb != null) {
                        //var internalObj = mpdo.GetData(MpPortableDataFormats.InternalContent);
                        await DropAsync(isCopy, MpAvCefNetWebView.DraggingRtb.DataContext);
                        return;
                    }
                    // from external source
                    var tempCopyItem = await MpAvCopyItemBuilder.CreateFromDataObject(mpdo, true);
                    if(tempCopyItem == null) {
                        //empty item ignore (or was a bug and unnecessary check here
                        return;
                    }
                    dropData = tempCopyItem.ItemData.ToPlainText();
                } else {
                    // external data should be pre-processed
                    Debugger.Break();
                }
            } else {
                dropData = drag_ctvm.SelectedPlainText;
            }


            if(dropData.IsStringBase64()) {
                dropRange.LoadImage(dropData, out MpSize dummySize);
            } else if(dropData.IsStringWindowsFileOrPathFormat()) {
                dropRange = dropRange.End.ToTextRange();
                dropRange.LoadItemData(dropData, MpCopyItemType.FileList, out MpSize dummySize);
            } else if (dropData.IsStringRichTextTable()) {
                //string csv = MpCsvToRtfTableConverter.GetCsv(dropData);
                dropRange.LoadTable(dropData);
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
                await dropRange.SetTextAsync(dropData);
            }

            await MpAvCefNetWebViewExtension.SaveTextContentAsync(wv);

            if(!isCopy && drag_ctvm != null) {
                MpAvCefNetWebViewExtension.FinishContentCutAsync(drag_ctvm)
                    .FireAndForgetSafeAsync(drag_ctvm);
            }

            while(MpAvClipTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            // NOTE since drag item was likely deleted this behavior may
            // not be attached to the same content anymore so use id ref to find/select
            // drop tile

            var ctvm_toSelect = MpAvClipTrayViewModel.Instance.GetClipTileViewModelById(dropItemId);
            if(ctvm_toSelect != null) {
                ctvm_toSelect.IsSelected = true;
            }
        }


        private MpAvITextRange GetDropRange(int rtfDropIdx, bool pre, bool post, bool split) {
            MpConsole.WriteLine("DropIdx: " + rtfDropIdx);
            MpConsole.WriteLine("Pre: " + (pre ? "TRUE" : "FALSE"));
            MpConsole.WriteLine("Post: " + (post ? "TRUE" : "FALSE"));
            MpConsole.WriteLine("Split: " + (split ? "TRUE" : "FALSE"));

            MpAvCefNetWebView wv = null;
            if (AssociatedObject is MpAvCefNetContentWebView cwv) {
                wv = cwv.GetVisualDescendant<MpAvCefNetWebView>();
            }
            // isolate insertion point and account for block drop
            MpAvITextPointer dtp_end;
            if (pre) {
                dtp_end = wv.Document.ContentStart.GetInsertionPosition(LogicalDirection.Forward);

            } else if (post) {
                dtp_end = wv.Document.ContentStart.GetPositionAtOffset(rtfDropIdx).GetLineStartPosition(1);
                if(dtp_end == null) {
                    dtp_end = wv.Document.ContentEnd.GetInsertionPosition(LogicalDirection.Backward);
                } else {
                    dtp_end = dtp_end.GetInsertionPosition(LogicalDirection.Forward);
                }

            } else {
                dtp_end = wv.Document.ContentStart
                            .GetPositionAtOffset(rtfDropIdx)
                            .GetInsertionPosition(LogicalDirection.Forward);
            }
            return new MpAvTextRange(dtp_end, dtp_end);
        }

        public override void AutoScrollByMouse() {
            if (AssociatedObject == null || AssociatedObject == null) {
                return;
            }
            
            var sv = AssociatedObject.GetVisualDescendant<ScrollViewer>();
            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = VisualExtensions.PointToClient(AssociatedObject, gmp.ToAvPixelPoint()).ToPortablePoint();

            MpRect rtb_rect = new MpRect(0, 0, AssociatedObject.Bounds.Width, AssociatedObject.Bounds.Height);

            if(sv.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible) {
                //rtb_rect.Height -= sv.GetScrollBar(Orientation.Horizontal).Height;
                rtb_rect.Bottom -= sv.GetScrollBar(Orientation.Horizontal).Height;
            }
            if (sv.VerticalScrollBarVisibility == ScrollBarVisibility.Visible) {
                //rtb_rect.Width -= sv.GetScrollBar(Orientation.Vertical).Width;
                rtb_rect.Right -= sv.GetScrollBar(Orientation.Vertical).Width;
            }

            if (!rtb_rect.Contains(mp)) {
                return;
            }

            double ldist = Math.Abs(mp.X - rtb_rect.Left);
            double rdist = Math.Abs(mp.X - rtb_rect.Right);
            double tdist = Math.Abs(mp.Y - rtb_rect.Top);
            double bdist = Math.Abs(mp.Y - rtb_rect.Bottom);

            MpPoint rtbScrollOffsetDelta = new MpPoint(); 
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
                sv.ScrollByPointDelta(rtbScrollOffsetDelta);
            }

        }

        public override async Task StartDropAsync() {
            await Task.Delay(1);
            if (AssociatedObject == null || AssociatedObject == null) {
                return;
            }
            //var rtb = AssociatedObject;
            //rtb.FitDocToRtb(true);
            _autoScrollVelocity = _baseAutoScrollVelocity;
            if(AssociatedObject.DataContext is MpAvClipTileViewModel ctvm) {
                ctvm.IsCurrentDropTarget = true;
            }
        }

        public override void CancelDrop() {
            base.CancelDrop();
            if (AssociatedObject != null && 
                AssociatedObject.DataContext is MpAvClipTileViewModel ctvm) {
                ctvm.IsCurrentDropTarget = false;
            }
        }

        public override void Reset() {
            base.Reset();

            _autoScrollVelocity = _baseAutoScrollVelocity;
            _isPostBlockDrop = _isPreBlockDrop = false;

            if(AssociatedObject == null || AssociatedObject.DataContext == null) {
                return;
            }

            
            if (!MpAvDragDropManager.IsDragAndDrop || 
               !(AssociatedObject.DataContext as MpAvClipTileViewModel).IsItemDragging) {
                // these checks make sure selection isn't cleared during self drop
                
                var rtb = AssociatedObject;
                if((AssociatedObject.DataContext as MpAvClipTileViewModel).IsContentReadOnly) {
                    AssociatedObject.GetVisualDescendant<ScrollViewer>().ScrollToHome();
                }
                
                //if(rtb.Document == null) {
                //    return;
                //}

                //rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);
                //rtb.FitDocToRtb();
            }
            if (AssociatedObject != null &&
                AssociatedObject.DataContext is MpAvClipTileViewModel ctvm) {
                ctvm.IsCurrentDropTarget = false;
            }
        }
    }
}
