﻿using Microsoft.Xaml.Behaviors;
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
                // MpConsole.WriteLine("rtb mp (no hit): " + mp);
                return -1;
            }
            var mptp = rtb.GetPositionFromPoint(mp, true);
            var mptp_rect = mptp.GetCharacterRect(LogicalDirection.Forward);

            double blockThreshold = 1;
            _isPreBlockDrop = _isPostBlockDrop = false;

            if(_isSplitBlockDrop) {
                // inline takes priority if alt is down so pre/post is ignored
            } else if(Math.Abs(mp.Y - mptp_rect.Top) < blockThreshold || mp.Y < mptp_rect.Top) {
                _isPreBlockDrop = true;
            } else if (Math.Abs(mp.Y - mptp_rect.Bottom) < blockThreshold || mp.Y > mptp_rect.Bottom) {
                _isPostBlockDrop = true;
            }

            //MpConsole.WriteLine("Pre: " + (_isPreBlockDrop ? "YES" : "NO"));
            //MpConsole.WriteLine("Inline: " + (_isSplitBlockDrop ? "YES" : "NO"));
            //MpConsole.WriteLine("Post: " + (_isPostBlockDrop ? "YES" : "NO"));
            return rtb.Document.ContentStart.GetOffsetToPosition(mptp);
        }

        public override MpShape[] GetDropTargetAdornerShape() {
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return null;
            }
            if (DropIdx < 0) {
                return null;
            }
            var dt_ll = new List<MpShape>();
            var dt_tp = AssociatedObject.Rtb.Document.ContentStart.GetPositionAtOffset(DropIdx);

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
            if (AssociatedObject != null) {
                if(dragData is MpClipTileViewModel ctvm) {
                    if(ctvm == rtb.DataContext) {
                        //if dropping onto self
                        if(rtb.Selection.IsEmpty ||
                           (rtb.Selection.Start == rtb.Document.ContentStart &&
                            rtb.Selection.End == rtb.Document.ContentEnd)) {
                            //only allow self drop for partial selection
                            return false;
                        }
                        var rtb_mp = Application.Current.MainWindow.TranslatePoint(MpShortcutCollectionViewModel.Instance.GlobalMouseLocation, rtb);
                        return !rtb.Selection.IsPointInRange(rtb_mp);
                    }
                    return true;
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
            if (AssociatedObject == null) {
                return;
            }
            var dctvm = dragData as MpClipTileViewModel;
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            // BUG storing dropIdx because somehow it gets lost after calling base
            int rtfDropIdx = DropIdx;

            if(ctvm.HeadItem == null) {
                return;
            }
            string rootGuid = ctvm.HeadItem.CopyItemGuid;

            bool isNewRoot = rtfDropIdx <= 0;

            await base.Drop(isCopy, dragData);

            if(dctvm == null) {
                if(dragData is MpPortableDataObject mpdo) {
                    // from external source
                    var exci = await MpCopyItemBuilder.CreateFromDataObject(mpdo);
                    if(isNewRoot) {
                        rootGuid = exci.Guid;
                    }
                    await DropFromExternal(exci, rtfDropIdx);
                    return;
                } 
            }

            var rtb = AssociatedObject.Rtb;
            var drtb = Application.Current.MainWindow.GetVisualDescendents<MpContentView>()
                            .FirstOrDefault(x => x.DataContext is MpClipTileViewModel temp && temp.HeadItem.CopyItemId == dctvm.HeadItem.CopyItemId).Rtb;


            
            // isolate insertion point and account for block drop
            var dtp = rtb.Document.ContentStart.GetPositionAtOffset(rtfDropIdx).GetNextInsertionPosition(LogicalDirection.Backward);
            if (_isPreBlockDrop) {
                dtp = dtp.GetLineStartPosition(0).InsertParagraphBreak();
            } else if (_isPostBlockDrop) {
                if(dtp == null || dtp.GetLineStartPosition(1) == null) {
                    //at end of document
                    dtp = rtb.Document.ContentEnd;
                } else {
                    dtp = dtp.GetLineStartPosition(1);
                }
                dtp = dtp.InsertParagraphBreak();
            }

            var dropCopyItems = new List<MpCopyItem>();
            //insert encoded items
            foreach(var dci in dropCopyItems) {
                var dr = new Run("{c{" + dci.Guid + "}c}") {
                    Tag = dci
                };
                var span = new Span(dr, dtp) {
                    Tag = dci
                };
                dtp = span.ElementEnd.GetNextInsertionPosition(LogicalDirection.Forward);
            }

            if (_isBlockDrop) {
                //add trailing line break
                if (dtp == null || dtp.GetLineStartPosition(1) == null) {
                    //at end of document
                    dtp = rtb.Document.ContentEnd;
                } else {
                    dtp = dtp.GetLineStartPosition(1);
                }
                dtp = dtp.InsertParagraphBreak();
            } 

            var allTargetItems = dropCopyItems;
            allTargetItems.AddRange(ctvm.Items.Select(x => x.CopyItem));
            allTargetItems = allTargetItems.Distinct().ToList();
            var rootItem = allTargetItems.FirstOrDefault(x => x.Guid == rootGuid);
            rootItem.RootCopyItemGuid = string.Empty;
            rootItem.CompositeSortOrderIdx = 0;
            rootItem.CompositeParentCopyItemId = 0;
            await rootItem.WriteToDatabaseAsync();

            allTargetItems.Remove(rootItem);
            allTargetItems.Insert(0, rootItem);
            foreach (var cci in allTargetItems.Where(x => x.Id != rootItem.Id)) {
                cci.RootCopyItemGuid = rootGuid;
                cci.CompositeParentCopyItemId = rootItem.Id;
                cci.CompositeSortOrderIdx = allTargetItems.IndexOf(cci);
                // BUG this just ensures sub-items don't seem like the root in instances where sortOrder = 0 is used to decide
                cci.CompositeSortOrderIdx = Math.Max(1, cci.CompositeSortOrderIdx);
                await cci.WriteToDatabaseAsync();
            }
            

            var encodedItems = await MpMergedDocumentRtfExtension.EncodeContent(rtb);

            //await ctvm.InitializeAsync(encodedItems);
            MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);

            while(MpClipTrayViewModel.Instance.IsBusy) {
                await Task.Delay(100);
            }

            MpClipTrayViewModel.Instance.GetContentItemViewModelByGuid(rootGuid).IsSelected = true;
        }

        public async Task DropFromExternal(MpCopyItem dropItem, int rtfDropIdx) {
            await Task.Delay(1);
            var rtb = AssociatedObject.Rtb;

            var dropRange = GetDropRange(rtfDropIdx);
            
            switch(dropItem.ItemType) {
                case MpCopyItemType.Text:
                    if(dropItem.ItemData.IsStringRichTextTable()) {
                        string csv = MpCsvToRtfTableConverter.GetCsv(dropItem.ItemData);
                        dropRange.LoadTable(csv);
                    } else {
                        string pt = dropItem.ItemData.ToPlainText();
                        MpConsole.WriteLine("Drop Plain Text: " + pt);
                        pt = pt.TrimTrailingLineEndings();
                        MpConsole.WriteLine("Trimmed Plain Text: " + pt);
                        dropRange.Text = pt; 
                    }                    
                    break;
                case MpCopyItemType.Image:
                    dropRange.LoadImage(dropItem.ItemData);
                    break;
                case MpCopyItemType.FileList:
                    dropRange.LoadFileItem(dropItem.ItemData.Replace(Environment.NewLine, string.Empty),dropItem.IconId);
                    break;
            }


            if (_isBlockDrop) {
                //var tdtp = dtp;
                ////add trailing line break
                //if (tdtp == null || tdtp.GetLineStartPosition(1) == null) {
                //    //at end of document
                //    tdtp = rtb.Document.ContentEnd;
                //} else {
                //    tdtp = tdtp.GetLineStartPosition(1);
                //}
                //tdtp.InsertParagraphBreak();
                dropRange.End.InsertLineBreak();
            }

            dropRange.GetAllTextElements().ForEach(x => x.Tag = dropItem);

            await MpMergedDocumentRtfExtension.EncodeContent(rtb);

            if (rtb.DataContext is MpClipTileViewModel ctvm && !ctvm.IsSelected) {
                ctvm.IsSelected = true;
            }
        }

        private TextRange GetDropRange(int rtfDropIdx) {
            var rtb = AssociatedObject.Rtb;

            // isolate insertion point and account for block drop
            var dtp = rtb.Document.ContentStart
                            .GetPositionAtOffset(rtfDropIdx)
                            .GetNextInsertionPosition(LogicalDirection.Backward);

            if (_isPreBlockDrop) {
                if(dtp.GetLineStartPosition(0) == null) {
                    MpConsole.WriteLine(@"Pre block doc start detected");
                    //at start of doc
                    dtp = rtb.Document.ContentStart;
                    //rtb.Document.ContentStart.InsertLineBreak();
                } else {
                    dtp = dtp.GetLineStartPosition(0);
                    //dtp.InsertLineBreak();
                }
                
            } else if (_isPostBlockDrop) {
                if (dtp == null || dtp.GetLineStartPosition(1) == null) {
                    //at end of document
                    dtp = rtb.Document.ContentEnd;
                } else {
                    dtp = dtp.GetLineStartPosition(1);
                }
                dtp = dtp.InsertLineBreak();
            } else if(_isSplitBlockDrop) {
                dtp = dtp.InsertLineBreak();
            }

            return new TextRange(dtp,dtp);
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
        }


        public override void Reset() {
            base.Reset();

            _autoScrollVelocity = _baseAutoScrollVelocity;
            _isPostBlockDrop = _isPreBlockDrop = false;

            if(AssociatedObject == null || AssociatedObject.Rtb == null || AssociatedObject.DataContext == null) {
                return;
            }

            var rtb = AssociatedObject.Rtb;
            rtb.FitDocToRtb();

            //if(rtb.IsReadOnly ) {
            //    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            //}

            if (!MpDragDropManager.IsDragAndDrop || 
               !(AssociatedObject.DataContext as MpClipTileViewModel).IsAnyItemDragging) {
                // these checks make sure selection isn't cleared during self drop

                rtb.ScrollToHome();
                rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);
            }
        }
    }

}
