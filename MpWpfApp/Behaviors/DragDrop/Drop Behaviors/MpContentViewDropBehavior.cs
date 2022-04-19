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
        #region Private Variables

        private bool _isBlockDrop => _isPreBlockDrop || _isPostBlockDrop;

        private bool _isPreBlockDrop = false;
        private bool _isPostBlockDrop = false;

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

        public void Rtb_Drop(object sender, DragEventArgs e) {
            if (e.Handled) {
                return;
            }
            if (e.Data.GetDataPresent(MpDataObject.InternalContentFormat)) {
                
            }
        }

        public void Rtb_DragOver(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.None;

            bool isValid = true;
            if(MpDragDropManager.DragData == null) {
                isValid = MpDragDropManager.PrepareDropDataFromExternalSource(e.Data);
            }
            
            if (isValid) {
                if (e.KeyStates == DragDropKeyStates.ControlKey ||
                   e.KeyStates == DragDropKeyStates.AltKey ||
                   e.KeyStates == DragDropKeyStates.ShiftKey) {
                    e.Effects = DragDropEffects.Copy;
                } else {
                    e.Effects = DragDropEffects.Move;
                }

                if(!MpDragDropManager.IsCheckingForDrag) {
                    MpDragDropManager.StartDragCheck(MpDragDropManager.DragData);
                }
            }
            e.Handled = true;
        }

        public void Rtb_DragLeave(object sender, DragEventArgs e) {
            Reset();
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
                case MpMessageType.ContentListItemsChanged:
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
            var rtb = AssociatedObject.Rtb;
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = Application.Current.MainWindow.TranslatePoint(gmp, rtb);
            Rect rtb_rect = new Rect(0, 0, rtb.ActualWidth, rtb.ActualHeight);
            if (!rtb_rect.Contains(mp)) {
                //Reset();
                MpConsole.WriteLine("rtb mp (no hit): " + mp);
                return -1;
            }
            MpConsole.WriteLine("rtb mp: " + mp);
            //MpIsFocusedExtension.SetIsFocused(rtb, true);
            var mptp = rtb.GetPositionFromPoint(mp, true);
            var mptp_rect = mptp.GetCharacterRect(LogicalDirection.Forward);
            double blockThreshold = 0.1;
            if(Math.Abs(mp.Y - mptp_rect.Top) < blockThreshold || mp.Y < mptp_rect.Top) {
                _isPreBlockDrop = true;
            } else if (Math.Abs(mp.Y - mptp_rect.Bottom) < blockThreshold || mp.Y > mptp_rect.Bottom) {
                _isPostBlockDrop = true;
            } else {
                _isPostBlockDrop = _isPreBlockDrop = false;
            }
            //rtb.CaretPosition = mptp;
            return rtb.Document.ContentStart.GetOffsetToPosition(mptp);
        }

        public override MpShape GetDropTargetAdornerShape() {
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return null;
            }
            if (DropIdx < 0) {
                return null;
            }
            var dltp = AssociatedObject.Rtb.Document.ContentStart.GetPositionAtOffset(DropIdx);
            var dltp_rect = dltp.GetCharacterRect(LogicalDirection.Forward);
            if (_isBlockDrop) {
                double blockLineOffset = 3;
                double y = 0;
                if(_isPreBlockDrop) {
                    y = dltp_rect.Top - blockLineOffset;
                } else {
                    y = dltp_rect.Bottom + blockLineOffset;
                }
                return new MpLine(0, y, AssociatedObject.Rtb.ActualWidth, y);
            }
            return new MpLine(dltp_rect.Left, dltp_rect.Top, dltp_rect.Left, dltp_rect.Bottom);
        }
        public override bool IsDragDataValid(bool isCopy,object dragData) {
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return false;
            }
            if (!base.IsDragDataValid(isCopy,dragData)) {
                return false;
            }
            if(AssociatedObject != null) {
                if(dragData is MpClipTileViewModel ctvm) {
                    if(ctvm == AssociatedObject.Rtb.DataContext) {
                        //if dropping onto self
                        if(AssociatedObject.Rtb.Selection.IsEmpty ||
                           (AssociatedObject.Rtb.Selection.Start == AssociatedObject.Rtb.Document.ContentStart &&
                            AssociatedObject.Rtb.Selection.End == AssociatedObject.Rtb.Document.ContentEnd)) {
                            //only allow self drop for partial selection
                            return false;
                        }
                        var rtb_mp = Application.Current.MainWindow.TranslatePoint(MpShortcutCollectionViewModel.Instance.GlobalMouseLocation, AssociatedObject.Rtb);
                        return !AssociatedObject.Rtb.Selection.IsPointInRange(rtb_mp);
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

        public override async Task Drop(bool isCopy, object dragData) {
            if (AssociatedObject == null) {
                return;
            }
            // BUG storing dropIdx because somehow it got lost while step tracing
            int dropIdx = DropIdx;
            
            await base.Drop(isCopy, dragData);

            var dctvm = dragData as MpClipTileViewModel;
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;

            if(dctvm == null) {
                if(dragData is MpDataObject mpdo) {
                    // from external source\
                    var exci = await MpCopyItemBuilder.CreateFromDataObject(mpdo);
                    dctvm = ctvm;
                    //ensure external drop will be treated like new content
                    isCopy = false;
                    dctvm.IsAllSelected = false;
                } else {
                    ;
                }
            }
            var rtb = AssociatedObject.Rtb;
            var drtb = Application.Current.MainWindow.GetVisualDescendents<MpContentView>()
                            .FirstOrDefault(x => x.DataContext is MpClipTileViewModel temp && temp.HeadItem.CopyItemId == dctvm.HeadItem.CopyItemId).Rtb;

            string rootGuid = ctvm.HeadItem.CopyItemGuid;

            //string preText = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentStart.GetPositionAtOffset(dropIdx)).Text;
            //bool isNewRoot = string.IsNullOrWhiteSpace(preText.Trim().Replace(Environment.NewLine,string.Empty));
            //if (isNewRoot) {
            //    rootGuid = null;
            //}
            bool isNewRoot = false;

            var dragTextElements = drtb.Selection.GetAllTextElements().OrderBy(x=>drtb.Document.ContentStart.GetOffsetToPosition(x.ContentStart));
            var dragContentRefs = dragTextElements
                                        .Where(x => x.Tag is MpCopyItemReference)
                                        .Select(x => x.Tag as MpCopyItemReference)
                                        .Distinct()
                                        .ToList();
            var dragCopyItems = await MpDataModelProvider.GetCopyItemsByGuids(dragContentRefs.Select(x => x.CopyItemGuid).ToArray());
            List<MpCopyItem> dropCopyItems = new List<MpCopyItem>();

            bool willSourceBeRemoved = dctvm.Count == dragCopyItems.Count && !isCopy;

            //var drtf_fd = drtb.Selection.ToRichText().ToFlowDocument();
            if (!dctvm.IsAllSelected || isCopy) {
                //retarget partial drag refs
                foreach(var dcr in dragContentRefs) {

                    string dcr_guid = dcr.CopyItemGuid;                    

                    var thisSourceElements = dragTextElements.Where(x => x.Tag is MpCopyItemReference cir && cir.CopyItemGuid == dcr_guid);

                    var thisSourceStart = thisSourceElements.Aggregate((a, b) => drtb.Document.ContentStart.GetOffsetToPosition(a.ContentStart) < drtb.Document.ContentStart.GetOffsetToPosition(b.ContentStart) ? a : b).ElementStart;
                    var thisSourceEnd = thisSourceElements.Aggregate((a, b) => drtb.Document.ContentStart.GetOffsetToPosition(a.ContentStart) > drtb.Document.ContentStart.GetOffsetToPosition(b.ContentStart) ? a : b).ElementEnd;

                    var thisSourceRange = new TextRange(thisSourceStart, thisSourceEnd);

                    var thisSourceRtf = thisSourceRange.ToRichText();

                    var thisSourceCi = dragCopyItems.FirstOrDefault(x => x.Guid == dcr_guid);

                    MpCopyItem thisSourceNewCi = null;
                    MpCopyItemReference retargeted_dcr = null;

                    string fullSourcePlainText = thisSourceCi.ItemData.ToPlainText().Trim();
                    string dragSourcePlainText = thisSourceRange.Text.Trim();
                    if (fullSourcePlainText == dragSourcePlainText && !isCopy) {
                        // NOTE this means this entire item is moving so don't retarget
                        // may need to trim line breaks...
                        retargeted_dcr = new MpCopyItemReference() { CopyItemGuid = thisSourceCi.Guid, CopyItemSourceGuid = thisSourceCi.CopyItemSourceGuid };
                        thisSourceNewCi = thisSourceCi;
                    } else {
                        retargeted_dcr = new MpCopyItemReference();
                        if (string.IsNullOrEmpty(dcr.CopyItemSourceGuid)) {
                            //when source is direct content make retargeted source the direct content
                            retargeted_dcr.CopyItemSourceGuid = dcr_guid;
                        } else {
                            //otherwise carry original content source
                            retargeted_dcr.CopyItemSourceGuid = dcr.CopyItemSourceGuid;
                        }
                        retargeted_dcr.CopyItemGuid = System.Guid.NewGuid().ToString();

                        thisSourceNewCi = await thisSourceCi.Clone(false) as MpCopyItem;
                        thisSourceNewCi.Id = 0;
                        thisSourceNewCi.Guid = retargeted_dcr.CopyItemGuid;
                        thisSourceNewCi.CopyItemSourceGuid = retargeted_dcr.CopyItemSourceGuid;
                        thisSourceNewCi.ItemData = thisSourceRtf;
                    }                    
                    
                    if(string.IsNullOrEmpty(rootGuid)) {
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
                if(string.IsNullOrEmpty(rootGuid)) {
                    rootGuid = dropCopyItems[0].Guid;
                }
            }
            // isolate insertion point and account for block drop
            var dtp = rtb.Document.ContentStart.GetPositionAtOffset(dropIdx).GetNextInsertionPosition(LogicalDirection.Backward);
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

            //insert encoded items
            foreach(var dci in dropCopyItems) {
                var dci_cir = new MpCopyItemReference() { CopyItemGuid = dci.Guid, CopyItemSourceGuid = dci.CopyItemSourceGuid };
                var dr = new Run("{c{" + dci.CopyItemGuid + "}c}") {
                    Tag = dci_cir
                };
                var span = new Span(dr, dtp) {
                    Tag = dci_cir
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
            allTargetItems.AddRange(ctvm.ItemViewModels.Select(x => x.CopyItem));
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
       
        public override void AutoScrollByMouse() {
            if (AssociatedObject == null || AssociatedObject.Rtb == null) {
                return;
            }
            if (DropIdx == 0) {
                AssociatedObject.BindingContext.HeadItem.ScrollToHomeCommand.Execute(null);
            } else if(DropIdx == 1) {
                AssociatedObject.BindingContext.HeadItem.ScrollToEndCommand.Execute(null);
            }
        }

        public override async Task StartDrop() {
            await Task.Delay(1);
        }

        public override void Reset() {
            base.Reset();
            _isPostBlockDrop = _isPreBlockDrop = false;

            if(AssociatedObject == null || AssociatedObject.Rtb == null || AssociatedObject.DataContext == null) {
                return;
            }
            if(!MpDragDropManager.IsDragAndDrop || 
               !(AssociatedObject.DataContext as MpClipTileViewModel).IsAnyItemDragging) {
                // these checks make sure selection isn't cleared during self drop

                AssociatedObject.Rtb.ScrollToHome();
                AssociatedObject.Rtb.Selection.Select(AssociatedObject.Rtb.Document.ContentStart, AssociatedObject.Rtb.Document.ContentStart);
            }
        }
    }

}
