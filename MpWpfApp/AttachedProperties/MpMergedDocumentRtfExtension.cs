//using CefSharp;
//using CefSharp.JavascriptBinding;
//using CefSharp.Wpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpMergedDocumentRtfExtension : DependencyObject {
        #region Private Variables

        
        private static readonly double _EDITOR_DEFAULT_WIDTH = 900;

        private static List<RichTextBox> _isChangeBlockRtbs = new List<RichTextBox>();
        //private static double _readOnlyWidth;

        #endregion

        #region IsSelected

        public static bool GetIsSelected(DependencyObject obj) {
            return (bool)obj.GetValue(IsSelectedProperty);
        }
        public static void SetIsSelected(DependencyObject obj, bool value) {
            obj.SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(MpMergedDocumentRtfExtension),
            new FrameworkPropertyMetadata(false));

        #endregion

        #region ReadOnlyWidth

        public static double GetReadOnlyWidth(DependencyObject obj) {
            return (double)obj.GetValue(ReadOnlyWidthProperty);
        }
        public static void SetReadOnlyWidth(DependencyObject obj, double value) {
            obj.SetValue(ReadOnlyWidthProperty, value);
        }
        public static readonly DependencyProperty ReadOnlyWidthProperty =
          DependencyProperty.RegisterAttached(
            "ReadOnlyWidth",
            typeof(double),
            typeof(MpMergedDocumentRtfExtension),
            new FrameworkPropertyMetadata(default));

        #endregion

        #region IsContentReadOnly

        public static bool GetIsContentReadOnly(DependencyObject obj) {
            return (bool)obj.GetValue(IsContentReadOnlyProperty);
        }
        public static void SetIsContentReadOnly(DependencyObject obj, bool value) {
            obj.SetValue(IsContentReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsContentReadOnlyProperty =
          DependencyProperty.RegisterAttached(
            "IsContentReadOnly",
            typeof(bool),
            typeof(MpMergedDocumentRtfExtension),
            new FrameworkPropertyMetadata() {
                PropertyChangedCallback = (s, e) => {
                    if (e.NewValue == null) {
                        return;
                    }
                    var fe = s as FrameworkElement;
                    if(!fe.IsLoaded) {
                        //ignore readOnly = true on load
                        return;
                    }
                    bool isReadOnly = (bool)e.NewValue;
                    if (isReadOnly) {
                        EnableReadOnly(fe);
                    } else {
                        DisableReadOnly(fe);
                    }
                }
            });

        private static async void EnableReadOnly(FrameworkElement fe) {
            await SaveTextContent(fe as RichTextBox);
            var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
            if (ctcv != null) {
                ctcv.TileResizeBehavior.ResizeWidth(GetReadOnlyWidth(fe));
            }//.FireAndForgetSafeAsync(fe.DataContext as MpClipTileViewModel);
        }

        private static void DisableReadOnly(FrameworkElement fe) {
            // when this works:
            // rtb.IsReadOnly = true
            // ctvm.IsContentReadOnly = false

            //
            var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
            
            if (ctcv != null) {
                SetReadOnlyWidth(fe,ctcv.ActualWidth);
                ctcv.GetVisualDescendent<MpRtbEditToolbarView>().SetActiveRtb(fe as RichTextBox);

                if (ctcv.ActualWidth < _EDITOR_DEFAULT_WIDTH) {
                    ctcv.TileResizeBehavior.ResizeWidth(_EDITOR_DEFAULT_WIDTH);
                }
                (fe as RichTextBox).FitDocToRtb();
                MpIsFocusedExtension.SetIsFocused(fe, true);
            }

        }

        public static async Task SaveTextContent(RichTextBox rtb) {
            if(rtb.DataContext is MpClipTileViewModel ctvm) {
                var contentLookup = new Dictionary<string, List<TextElement>>();

                var allTextElements = rtb.Document.GetAllTextElements().ToList();
                foreach(var te in allTextElements) {
                    // fill dictionary w/ all text elements per copy item in doc order
                    if(te.Tag == null) {
                        // this should only happen when tile is initially loading which means it doesn't need to be saved
                        return;
                        //Debugger.Break();
                        //throw new Exception("Error all text elements should have a model as their tag (either MpCopyItem or MpTextTemplate)");
                    }
                    if(te.Tag is MpCopyItem ci) {                        
                        if(!contentLookup.ContainsKey(ci.Guid)) {
                            contentLookup.Add(ci.Guid, new List<TextElement>());
                        }
                        contentLookup[ci.Guid].Add(te);
                        contentLookup[ci.Guid].Sort((a, b) => {
                            return a.ContentStart.CompareTo(b.ContentStart);
                        });
                    }
                    // TODO should add template reference obj and check here probably
                }

                foreach(var ckvp in contentLookup) {
                    var start = ckvp.Value.Aggregate((a, b) =>
                        rtb.Document.ContentStart.GetOffsetToPosition(a.ElementStart) < rtb.Document.ContentStart.GetOffsetToPosition(b.ElementStart) ? a : b).ElementStart;
                    var end = ckvp.Value.Aggregate((a, b) =>
                        rtb.Document.ContentStart.GetOffsetToPosition(a.ElementEnd) > rtb.Document.ContentStart.GetOffsetToPosition(b.ElementEnd) ? a : b).ElementEnd;

                    string itemRtf = new TextRange(start, end).ToRichText();
                    var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelByGuid(ckvp.Key);
                    if(civm == null) {
                        // NOTE this should proibably not happen
                        var ci = await MpDataModelProvider.GetCopyItemByGuid(ckvp.Key);
                        if (ci == null) {
                            //a new item

                        } else {
                            ci.ItemData = itemRtf;
                            await ci.WriteToDatabaseAsync();
                        }
                    } else {
                        civm.CopyItemData = itemRtf;
                    }
                    
                }
            }
        }

        #endregion

        #region HeadContentItemViewModel

        public static object GetHeadContentItemViewModel(DependencyObject obj) {
            return obj.GetValue(HeadContentItemViewModelProperty);
        }
        public static void SetHeadContentItemViewModel(DependencyObject obj, object value) {
            obj.SetValue(HeadContentItemViewModelProperty, value);
        }

        public static readonly DependencyProperty HeadContentItemViewModelProperty =
          DependencyProperty.RegisterAttached(
            "HeadContentItemViewModel",
            typeof(object),
            typeof(MpMergedDocumentRtfExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (s, e) => {
                    if(e.NewValue == null) {
                        //occurs when tile is placeholder
                        return;
                    }
                    var rtb = s as RichTextBox;
                    if (rtb == null) {
                        return;
                    }
                    rtb.Loaded += Rtb_Loaded;
                    rtb.Unloaded += Rtb_Unloaded;
                    if (rtb.IsLoaded) {
                        Rtb_Loaded(rtb, null);
                    } else {
                        
                    }
                }
            });


        private static void Rtb_Loaded(object sender, RoutedEventArgs e) {
            var rtb = sender as RichTextBox;
            if(rtb == null) {
                return;
            }

            LoadContent(rtb).FireAndForgetSafeAsync(rtb.DataContext as MpClipTileViewModel);
        }

        public static async Task<List<MpCopyItem>> EncodeContent(RichTextBox rtb) {
            var ctvm = rtb.DataContext as MpClipTileViewModel;

            var allTextElements = rtb.Document
                                        .GetAllTextElements()
                                        .OrderBy(x => rtb.Document.ContentStart.GetOffsetToPosition(x.ContentStart)).ToList();

            var allStrayElements = allTextElements.Where(x => x.Tag == null).ToList();
            if(allStrayElements.Count > 0) {
                // shouldn't happen, somethings isn't tagged in drop
                Debugger.Break();
            }
            var tagGroups = allTextElements
                                .GroupBy(x => x.Tag as MpCopyItem)
                                .ToDictionary(t => t.Key, te => te.ToList());
            string rootGuid = ctvm.HeadItem.CopyItemGuid;
            string origRootGuid = rootGuid;

            foreach (var kvp in tagGroups) {
                // find doc start item and update root guid
                var tagElements = kvp.Value;
                if (tagElements.Select(x => x.ContentStart).Any(x => x == rtb.Document.ContentStart)) {
                    rootGuid = kvp.Key.Guid;
                }
            }
            if(!_isChangeBlockRtbs.Contains(rtb)) {
                rtb.BeginChange();
            }
            
            
            foreach (var kvp in tagGroups.Where(x => x.Key.Guid != rootGuid)) {
                // loop through all non-root ranges and encode content (substitute range with encoded guid)

                //find min/max text pointers for this content
                var itemRangeStart = kvp.Value
                                            .Aggregate((a, b) =>
                                                rtb.Document.ContentStart.GetOffsetToPosition(a.ContentStart) <
                                                rtb.Document.ContentStart.GetOffsetToPosition(b.ContentStart) ? a : b).ElementStart;
                var itemRangeEnd = kvp.Value
                                            .Aggregate((a, b) =>
                                                rtb.Document.ContentStart.GetOffsetToPosition(a.ContentEnd) >
                                                rtb.Document.ContentStart.GetOffsetToPosition(b.ContentEnd) ? a : b).ElementEnd;
                var fullItemRange = new TextRange(itemRangeStart, itemRangeEnd);

                if (kvp.Key.Guid != rootGuid) {
                    // NOTE only write root after all sub-content is encoded
                    // store non root's rtf before encoding it
                    if(kvp.Key.ItemType == MpCopyItemType.Text) {
                        kvp.Key.ItemData = fullItemRange.ToRichText();
                    }
                    
                }
                fullItemRange.Text = "{c{" + kvp.Key.Guid + "}c}";
            }


            // NOTE since sortOrder isn't really used just ensure composites are unique and not zero
            int dummyIdx = 1;
            foreach (var tg in tagGroups) {
                var ci = tg.Key;
                if (ci.Guid == rootGuid) {
                    ci.CompositeParentCopyItemId = 0;
                    ci.CompositeSortOrderIdx = 0;
                    ci.RootCopyItemGuid = string.Empty;

                    //store root rtf
                    if(ci.ItemType == MpCopyItemType.Text) {
                        ci.ItemData = rtb.Document.ToRichText();
                    }
                } else {
                    var rci = tagGroups.FirstOrDefault(x => x.Key.Guid == rootGuid).Key;
                    ci.CompositeParentCopyItemId = rci.Id;
                    ci.CompositeSortOrderIdx = dummyIdx;
                    ci.RootCopyItemGuid = rootGuid;
                    dummyIdx++;
                }

                await ci.WriteToDatabaseAsync();
            }

            // NOTE Not sure if swap interferes w/ pinned/persisted items
            if (origRootGuid != rootGuid) {
                int rootId = tagGroups.FirstOrDefault(x => x.Key.Guid == rootGuid).Key.Id;
                MpDataModelProvider.SwapQueryItem(ctvm.QueryOffsetIdx, rootId);
            }

            var orderedItems = tagGroups.Select(x => x.Key).OrderBy(x => x.CompositeSortOrderIdx).ToList();
            
            return orderedItems;
        }

        

        private static void Rtb_Unloaded(object sender, RoutedEventArgs e) {
            if(sender is RichTextBox rtb) {
                UnloadContent(rtb);
            }
        }

        private static void UnloadContent(RichTextBox rtb) {
            rtb.Loaded -= Rtb_Loaded;
            rtb.Unloaded -= Rtb_Unloaded;
            rtb.TextChanged -= Rtb_TextChanged;
            rtb.Document.GetAllTextElements().ForEach(x => UnregisterTextElement(x));
        }

        private static async Task LoadContent(RichTextBox rtb) {
            if (rtb == null) {
                return;
            }
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            if (ctvm == null) {
                return;
            }

            while (ctvm.IsAnyBusy) {
                // wait till ctvm finishes initializing 
                await Task.Delay(100);
            }
            if (ctvm.IsPlaceholder && !ctvm.IsPinned) {
                return;
            }
            ctvm.IsBusy = true;
            

            rtb.Document.Blocks.Clear();

            await LoadContentItem(
                rtb: rtb,
                itemGuid: ctvm.HeadItem.CopyItemGuid,
                itemRange: new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd),
                items: ctvm.Items.Select(x => x.CopyItem).ToList(),
                decodeAsRootDocument: true);

            if (ctvm == null) {
                return;
            }
            ctvm.IsBusy = false;
            if (ctvm.HeadItem == null) {
                return;
            }


            switch (ctvm.HeadItem.CopyItemType) {
                case MpCopyItemType.Text:
                case MpCopyItemType.FileList:
                    rtb.HorizontalAlignment = HorizontalAlignment.Stretch;
                    rtb.VerticalAlignment = VerticalAlignment.Stretch;
                    rtb.FitDocToRtb();
                    break;
                case MpCopyItemType.Image:
                    rtb.FitDocToRtb();
                    break;

            }

            //wait till full doc is loaded to hook textChanged
            rtb.TextChanged += Rtb_TextChanged;

            var rtb_a = rtb.GetVisualAncestor<AdornerLayer>();
            if (rtb_a != null) {
                rtb_a.Update();
            }

            // NOTE this EndChange() is only relevant after EncodeContent was called and tile was re-initialized

            if (_isChangeBlockRtbs.Contains(rtb)) {
                _isChangeBlockRtbs.Remove(rtb);
                rtb.EndChange();
            }
        }

        private static async Task LoadContentItem(
            RichTextBox rtb,
            string itemGuid, 
            TextRange itemRange,
            List<MpCopyItem> items, 
            bool decodeAsRootDocument = false) {

            var fd = rtb.Document;
            MpCopyItem ci = items.FirstOrDefault(x=>x.Guid == itemGuid);

            if (ci == default) {
                MpConsole.WriteLine("error fetching copy item: " + itemGuid);
                ci = await MpDataModelProvider.GetCopyItemByGuid(itemGuid);
                if(ci == default) {
                    MpConsole.WriteLine("and " + itemGuid + " was not found");
                } else {
                    MpConsole.WriteLine("but " + itemGuid + " was found");
                }
            }

            // convert itemData to flow document and gather child fragment guid ranges and their content
            //FlowDocument fd = ci.ItemData.ToFlowDocument(ci.IconId); //
            itemRange.LoadRtf(ci.ItemData, ci.IconId);

            TextRange[] childRanges = null;
            string[] childGuids = null;
            List<MpCopyItem> childCopyItems = null;

            if(ci.ItemType == MpCopyItemType.Text) {
                childRanges = GetEncodedRanges(itemRange, "{c{", "}c}");
                childGuids = childRanges.Select(x => x.Text.Replace("{c{", string.Empty).Replace("}c}", string.Empty)).ToArray();
                childCopyItems = items.Where(x => x.CompositeParentCopyItemId == items.FirstOrDefault(y=>y.Guid == itemGuid).Id).ToList();

                if (childCopyItems.Count != childGuids.Length) {
                    var missingGuids = childGuids.Where(x => childCopyItems.All(y => y.Guid != x));
                    var missingItems = await MpDataModelProvider.GetCopyItemsByGuids(missingGuids.ToArray());
                    MpConsole.WriteLine($"{missingGuids.Count()} child items were missing for item {itemGuid}");
                    missingGuids.ForEach(x => 
                        MpConsole.WriteLine(x + " : " + (missingItems.Any(y => y.Guid == x) ? "FOUND" : "NOT FOUND")));
                    childCopyItems.AddRange(missingItems);
                }

                for (int i = 0; i < childGuids.Length; i++) {
                    var insertRange = childRanges[i];
                    string childGuid = childGuids[i];
                    var childItem = childCopyItems.FirstOrDefault(x => x.Guid == childGuid);
                    if (childItem == null) {
                        MpConsole.WriteTraceLine("Missing content child detected, replacing w/ empty string " + childGuid);
                        insertRange.Text = string.Empty;
                        continue;
                    }
                    await LoadContentItem(rtb, childGuid, insertRange, items);

                    //fd.Combine(cfd, childRange.Start);
                    //using (MemoryStream stream = new MemoryStream()) {
                    //    var rangeFrom = new TextRange(cfd.ContentStart, cfd.ContentEnd);
                    //    if (insertRange.Start.Parent is Inline insertInline) {
                    //        if (false) {//insertInline.PreviousInline == null) {
                    //            //insert is at beginning of paragraph so no need to alter child doc
                    //        } else {
                    //            rangeFrom = new TextRange(
                    //                cfd.ContentStart.GetInsertionPosition(LogicalDirection.Forward),
                    //                cfd.ContentEnd);
                    //            //remove line ending from text? (alt. change ContentEnd point maybe)
                    //            rangeFrom.Text = rangeFrom.Text.Replace(Environment.NewLine, string.Empty);
                    //        }
                    //    }
                    //    XamlWriter.Save(rangeFrom, stream);
                    //    rangeFrom.Save(stream, DataFormats.XamlPackage);
                    //    var rangeTo = new TextRange(insertRange.Start, insertRange.End);
                    //    rangeTo.Load(stream, DataFormats.XamlPackage);

                    //    if (decodeAsRootDocument) {
                    //        var ctel = rangeTo.GetAllTextElements();
                    //        ctel.Where(x => x is Run).ForEach(x => x.Tag = childItem);
                    //    }
                    //}
                }

            } else if(ci.ItemType == MpCopyItemType.FileList) {
                if(decodeAsRootDocument) {
                    var children_start_tp = itemRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                    foreach (var cci in items.Where(x => x.Guid != itemGuid && x.CompositeParentCopyItemId == ci.Id).OrderBy(x => x.CompositeSortOrderIdx)) {
                        //var p = new Paragraph();
                        //fd.Blocks.Add(p);
                        var tp = fd.Blocks.Last().ContentEnd.InsertParagraphBreak();
                        var p = tp.Parent as Paragraph;
                        await LoadContentItem(rtb, cci.Guid, p.ContentRange(), items);
                    }
                } else {
                    // NOTE by convention file lists are only 1 level deep so root children have no children
                    childRanges = new TextRange[0];
                    childGuids = new string[0];
                    childCopyItems = new List<MpCopyItem>();
                }                
            }
            
            //if(childCopyItems.Count != childGuids.Length) {
            //    var missingGuids = childGuids.Where(x => childCopyItems.All(y => y.Guid != x));
            //    var missingItems = await MpDataModelProvider.GetCopyItemsByGuids(missingGuids.ToArray());
            //    childCopyItems.AddRange(missingItems);
            //}
            
            // decode fragment and replace its guid range with fragment content
            //for (int i = 0; i < childGuids.Length; i++) {
            //    var insertRange = childRanges[i];
            //    string childGuid = childGuids[i];
            //    var childItem = childCopyItems.FirstOrDefault(x => x.Guid == childGuid);
            //    if(childItem == null) {
            //        MpConsole.WriteTraceLine("Missing content child detected, replacing w/ empty string " + childGuid);
            //        insertRange.Text = string.Empty;
            //        continue;
            //    }
            //    var cfd = await DecodeContentItem(rtb,childGuid,itemRange,items,rootDocument);

            //    //fd.Combine(cfd, childRange.Start);
            //    using (MemoryStream stream = new MemoryStream()) {
            //        var rangeFrom = new TextRange(cfd.ContentStart, cfd.ContentEnd);
            //        if (insertRange.Start.Parent is Inline insertInline) { 
            //            if(false) {//insertInline.PreviousInline == null) {
            //                //insert is at beginning of paragraph so no need to alter child doc
            //            } else {
            //                rangeFrom = new TextRange(
            //                    cfd.ContentStart.GetInsertionPosition(LogicalDirection.Forward),
            //                    cfd.ContentEnd);
            //                //remove line ending from text? (alt. change ContentEnd point maybe)
            //                rangeFrom.Text = rangeFrom.Text.Replace(Environment.NewLine, string.Empty);
            //            }
            //        }
            //        XamlWriter.Save(rangeFrom, stream);
            //        rangeFrom.Save(stream, DataFormats.XamlPackage);
            //        var rangeTo = new TextRange(insertRange.Start, insertRange.End);
            //        rangeTo.Load(stream, DataFormats.XamlPackage);

            //        if(decodeAsRootDocument) {
            //            var ctel = rangeTo.GetAllTextElements();
            //            ctel.Where(x => x is Run).ForEach(x => x.Tag = childItem);
            //        }
            //    }
            //}

            if(decodeAsRootDocument) {
                // this should only occur in root document once all children are added to avoid different document error
                var templateRanges = GetEncodedRanges(fd, "{t{", "}t}");
                var templateGuids = templateRanges.Select(x => x.Text.Replace("{c{", string.Empty).Replace("}c}", string.Empty)).ToList();
                var templateItems = await MpDataModelProvider.GetTextTemplatesByGuids(templateGuids);
                for (int i = 0; i < templateGuids.Count; i++) {
                    string templateGuid = templateGuids[i];
                    if (templateItems.All(x => x.Guid != templateGuid)) {
                        MpConsole.WriteTraceLine("Missing content child detected, replacing w/ empty string " + templateGuid);
                        templateRanges[i].Text = string.Empty;
                        continue;
                    }
                    MpTemplateHyperlink.Create(templateRanges[i], templateItems[i]);
                }

                var ctvm = rtb.DataContext as MpClipTileViewModel;
                if(ctvm == null) {
                    return;
                }
                var rcivm = ctvm.HeadItem;
                if(rcivm == null) {
                    return;
                }
                rcivm.UnformattedContentSize = fd.GetDocumentSize();
                //if(rcivm.CopyItemTitle == "Untitled1942") {
                //Debugger.Break();
                //}
                fd.Tag = ci;
            }

            var allTextElements = itemRange.GetAllTextElements();
            allTextElements.Where(x => x.Tag == null).ForEach(x => x.Tag = ci);


            if (decodeAsRootDocument) {
                //only register events on actual text in root document
                // or containers may supercede precedence
                //allTextElements.Where(x=>x is Run).ForEach(x => RegisterTextElement(x));
            }
            return;
        }

        private static TextRange[] GetEncodedRanges(FlowDocument fd, string rangeStartText, string rangeEndText) {
            return GetEncodedRanges(new TextRange(fd.ContentStart, fd.ContentEnd), rangeStartText, rangeEndText);
        }

        private static TextRange[] GetEncodedRanges(TextRange tr, string rangeStartText, string rangeEndText) {
            List<TextRange> encodedRange = new List<TextRange>();

            var tp = tr.Start;
            while (true) {
                var encodedRangeOpenTag = tp.FindText(tr.End, rangeStartText);
                if (encodedRangeOpenTag == null) {
                    break;
                }
                var encodedRangeCloseTag = encodedRangeOpenTag.End.FindText(tr.End, rangeEndText);
                if (encodedRangeCloseTag == null) {
                    MpConsole.WriteLine(@"Corrupt text content, missing ending range tag. Item xaml: ");
                    MpConsole.WriteLine(tr.Start.Parent.FindParentOfType<FlowDocument>().ToXamlPackage());
                    throw new Exception("Corrupt text content see console");
                }
                encodedRange.Add(new TextRange(encodedRangeOpenTag.Start, encodedRangeCloseTag.End));
                tp = encodedRangeCloseTag.End.GetNextContextPosition(LogicalDirection.Forward);
            }

            return encodedRange.ToArray();
        }

        private static void Rtb_TextChanged(object sender, TextChangedEventArgs e) {
            return;
            var rtb = sender as RichTextBox;

            var rtb_a = rtb.GetVisualAncestor<AdornerLayer>();
            if(rtb_a != null) {
                rtb_a.Update();
            }

            if (MpDragDropManager.IsDragAndDrop) {
                // NOTE during drop rtb will be reinitialized
                return;
            }
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            if(ctvm.IsPlaceholder) {
                // BUG I think this event gets called when a tile is dropped and its turned into placeholder
                return;
            }
            //MpConsole.WriteLines("Tile " + ctvm.HeadItem.CopyItemTitle + " text changed:");
            //MpConsole.WriteLine(rtb.Document.ToXamlPackage());

            foreach(var tc in e.Changes) {
                if(tc.AddedLength == 0) {
                    continue;
                }
                for (int i = 0; i < tc.AddedLength; i++) {
                    var tcp = rtb.Document.ContentStart.GetPositionAtOffset(tc.Offset + i, LogicalDirection.Forward);
                    var tcte = tcp.Parent.FindParentOfType<FrameworkContentElement>();
                    if(tcte == null) {
                        int preOffset = tc.Offset;
                        while(tcte == null) {
                            preOffset--;
                            if(preOffset < 0) {
                                MpConsole.WriteLine(rtb.Document.ToXamlPackage());
                                Debugger.Break();
                            }
                            tcp = rtb.Document.ContentStart.GetPositionAtOffset(preOffset + i, LogicalDirection.Backward);
                            tcte = tcp.Parent.FindParentOfType<FrameworkContentElement>();
                        }
                    }
                    if(tcte.Tag == null) {
                        tcte.Tag = FindNearestContentReference(tcte);
                    }
                }
            }
            if (ctvm.IsTextItem) {
                rtb.Document.ConfigureLineHeight();
            }
        }

        private static object FindNearestContentReference(FrameworkContentElement fce) {
            var fd = fce.Parent.FindParentOfType<FlowDocument>();
            if(fd == null) {
                return null;
            }
            TextPointer ctp;
            if(fce is FlowDocument fce_fd) {
                return fce_fd.Tag;
            } else if(fce is TextElement te) {
                ctp = te.ContentStart;
            } else {
                return null;
            }
            //always search backwards first for first reference
            
            while(true) {
                if(ctp == null) {
                    break;
                }
                int curOffset = fd.ContentStart.GetOffsetToPosition(ctp);
                if(curOffset <= 1) {
                    break;
                }
                var pte = ctp.Parent.FindParentOfType<FrameworkContentElement>();
                if (pte != null && pte.Tag != null) {
                    return pte.Tag;
                }
                ctp = ctp.GetPositionAtOffset(curOffset - 1);                
            }
            //now search forward
            ctp = (fce as TextElement).ContentStart;
            int maxOffset = fd.ContentStart.GetOffsetToPosition(fd.ContentEnd);
            while (true) {
                int curOffset = fd.ContentStart.GetOffsetToPosition(ctp);
                if (curOffset > maxOffset) {
                    break;
                }
                ctp = ctp.GetPositionAtOffset(curOffset + 1);
                var pte = ctp.Parent.FindParentOfType<TextElement>();
                if (pte != null && pte.Tag != null) {
                    return pte.Tag;
                }
            }

            // TODO need to create new element here, return root or throw an error depending on how changes handled
            return null;
        }

        private static void RegisterTextElement(TextElement te) {
            if(te == null) {
                return;
            }
            te.MouseEnter += Te_MouseEnter;
            te.MouseLeave += Te_MouseLeave;
            te.PreviewMouseLeftButtonDown += Te_PreviewMouseLeftButtonDown;

            var ci = te.Tag as MpCopyItem;
            if (ci == null) {
                return;
            }
            var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(ci.Id);
            if (civm == null) {
                return;
            }

            //civm.ItemEditorBackgroundHexColor = te.Background == null ? MpSystemColors.Transparent : te.Background.ToHex();

            //MpHelpers.CreateBinding(
            //   civm,
            //   new PropertyPath(
            //       nameof(civm.ItemBackgroundHexColor)),
            //       te, 
            //       TextElement.BackgroundProperty);
        }

        private static void UnregisterTextElement(TextElement te) {
            if (te == null) {
                return;
            }
            te.MouseEnter -= Te_MouseEnter;
            te.MouseLeave -= Te_MouseLeave;
            te.PreviewMouseLeftButtonDown -= Te_PreviewMouseLeftButtonDown;
        }


        private static void Te_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        }

        private static void Te_MouseLeave(object sender, MouseEventArgs e) {
            if(MpDragDropManager.IsDragAndDrop) {
                return;
            }
            var te = sender as TextElement;
            if (te == null) {
                return;
            }
            var ci = te.Tag as MpCopyItem;
            if (ci == null) {
                return;
            }
            var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(ci.Id);
            if (civm == null) {
                return;
            }

            civm.IsHovering = false;
            //civm.Parent.Items.ForEach(x => x.IsHovering = x.CopyItemId == civm.CopyItemId);
            UpdateHoverHighlight(te.Parent.FindParentOfType<RichTextBox>());
        }

        private static void Te_MouseEnter(object sender, MouseEventArgs e) {
            if (MpDragDropManager.IsDragAndDrop) {
                return;
            }
            var te = sender as TextElement;
            if(te == null) {
                return;
            }
            var ci = te.Tag as MpCopyItem;
            if(ci == null) {
                return;
            }
            var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(ci.Id);
            if(civm == null) {
                return;
            }

            MpConsole.WriteLine("Hover Item Id: " + civm.CopyItemId);

            civm.Parent.Items.ForEach(x => x.IsHovering = x.CopyItemId == civm.CopyItemId);

            UpdateHoverHighlight(te.Parent.FindParentOfType<RichTextBox>());

            if(civm.CompositeParentCopyItemId > 0) {
                return;
            }
        }

        private static void UpdateHoverHighlight(RichTextBox rtb) {
            if (rtb == null) {
                return;
            }
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            if (ctvm.Items.Count <= 1 ||
                !ctvm.IsContentReadOnly) {
                //return;
            }
            var fd = rtb.Document;
            var allRuns = fd.GetAllTextElements().Where(x => x is Run);

            allRuns.ForEach(x =>
                    new TextRange(x.ContentStart, x.ContentEnd)
                    .ApplyPropertyValue(
                        TextElement.BackgroundProperty,
                        x.Tag != null && ctvm.HoverItem != null && (x.Tag as MpCopyItem).Id == ctvm.HoverItem.CopyItemId ?
                            Brushes.Yellow :
                            Brushes.Transparent));
        }

        #endregion
    }
}