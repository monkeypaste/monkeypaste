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

        private static double _readOnlyWidth;

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
                PropertyChangedCallback = async (s, e) => {
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

        private static void EnableReadOnly(FrameworkElement fe) {
            var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
            if (ctcv != null) {
                ctcv.TileResizeBehvior.Resize(_readOnlyWidth - ctcv.ActualWidth, 0);
            }
            SaveTextContent(fe as RichTextBox).FireAndForgetSafeAsync(fe.DataContext as MpClipTileViewModel);
        }

        private static void DisableReadOnly(FrameworkElement fe) {
            var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
            
            if (ctcv != null) {
                _readOnlyWidth = ctcv.ActualWidth;
                ctcv.GetVisualDescendent<MpRtbEditToolbarView>().SetActiveRtb(fe as RichTextBox);

                if (ctcv.ActualWidth < _EDITOR_DEFAULT_WIDTH) {
                    ctcv.TileResizeBehvior.Resize(_EDITOR_DEFAULT_WIDTH - ctcv.ActualWidth, 0);
                }
                (fe as RichTextBox).FitDocToRtb();
                MpIsFocusedExtension.SetIsFocused(fe, true);
            }

        }

        private static async Task SaveTextContent(RichTextBox rtb) {
            if(rtb.DataContext is MpClipTileViewModel ctvm) {
                var contentLookup = new Dictionary<string, List<TextElement>>();

                var allTextElements = rtb.Document.GetAllTextElements();
                foreach(var te in allTextElements) {
                    if(te.Tag == null) {
                        throw new Exception("Error all text elements should have a model as their tag (either MpCopyItem or MpTextTemplate)");
                    }
                    if(te.Tag is MpDbModelBase dbo) {
                        if(dbo is MpTextTemplate) {
                            continue;
                        }
                        if(!contentLookup.ContainsKey(dbo.Guid)) {
                            contentLookup.Add(dbo.Guid, new List<TextElement>());
                        }
                        contentLookup[dbo.Guid].Add(te);
                        contentLookup[dbo.Guid].Sort((a, b) => {
                            return a.ContentStart.CompareTo(b.ContentStart);
                        });
                    }
                }

                foreach(var ckvp in contentLookup) {
                    var start = ckvp.Value.Aggregate((a, b) =>
                        rtb.Document.ContentStart.GetOffsetToPosition(a.ElementStart) < rtb.Document.ContentStart.GetOffsetToPosition(b.ElementStart) ? a : b).ElementStart;
                    var end = ckvp.Value.Aggregate((a, b) =>
                        rtb.Document.ContentStart.GetOffsetToPosition(a.ElementEnd) > rtb.Document.ContentStart.GetOffsetToPosition(b.ElementEnd) ? a : b).ElementEnd;

                    var cil = await MpDataModelProvider.GetCopyItemsByGuids(new string[] { ckvp.Key });
                    if(cil == null || cil.Count == 0) {
                        //a new item

                    } else {
                        string itemRtf = new TextRange(start, end).ToRichText();
                        cil[0].ItemData = itemRtf;
                        await cil[0].WriteToDatabaseAsync();
                    }
                }
            }
        }

        #endregion

        #region HeadContentItemViewModel

        public static object GetHeadContentItemViewModel(DependencyObject obj) {
            return (object)obj.GetValue(HeadContentItemViewModelProperty);
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
                    if (rtb.IsLoaded) {
                        Rtb_Loaded(rtb, null);
                    } else {
                        rtb.Loaded += Rtb_Loaded;
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
            var encodedItems = new List<MpCopyItem>();

            var allTextElements = rtb.Document.GetAllTextElements().OrderBy(x => rtb.Document.ContentStart.GetOffsetToPosition(x.ContentStart)).ToList();
            string rootGuid = (allTextElements[0].Tag as MpCopyItemReference).CopyItemGuid;
            var ctp_start = rtb.Document.ContentStart;
            string encodedContentStr = string.Empty;
            while (ctp_start != null && ctp_start != rtb.Document.ContentEnd) {
                string curGuid = null;
                if (ctp_start.Parent is FlowDocument fd) {
                    curGuid = (fd.Tag as MpCopyItemReference).CopyItemGuid;
                } else if (ctp_start.Parent is TextElement te) {
                    curGuid = (te.Tag as MpCopyItemReference).CopyItemGuid;
                } else {
                    Debugger.Break();
                } 
                var ctp_end = allTextElements
                                .Where(x => x.Tag is MpCopyItemReference cir && cir.CopyItemGuid == curGuid)
                                .Aggregate((a, b) => rtb.Document.ContentStart.GetOffsetToPosition(a.ContentEnd) > rtb.Document.ContentStart.GetOffsetToPosition(b.ContentEnd) ? a : b).ContentEnd;
                var ctp_range = new TextRange(ctp_start, ctp_end);
                if (curGuid == rootGuid) {
                    encodedContentStr += ctp_range.ToRichText();
                } else {
                    var cur_ci = await MpDataModelProvider.GetCopyItemByGuid(curGuid);
                    if (cur_ci == null) {
                        Debugger.Break();
                    }
                    cur_ci.ItemData = ctp_range.ToRichText();
                    await cur_ci.WriteToDatabaseAsync();
                    encodedItems.Add(cur_ci);
                    encodedContentStr += "{c{" + curGuid + "}c}";
                }
                ctp_start = ctp_end.GetNextInsertionPosition(LogicalDirection.Forward);
            }

            var root_ci = await MpDataModelProvider.GetCopyItemByGuid(rootGuid);
            root_ci.ItemData = encodedContentStr;
            await root_ci.WriteToDatabaseAsync();
            encodedItems.Insert(0, root_ci);
            return encodedItems;
        }

        private static async Task LoadContent(RichTextBox rtb) {
            if(rtb == null) {
                return;
            }
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            if(ctvm == null) {
                return;
            }
            while(ctvm.IsBusy) {
                // wait till ctvm finishes initializing 
                await Task.Delay(100);
            }
            if(ctvm.IsPlaceholder) {
                return;
            }
            rtb.Document = await DecodeContentItem(ctvm.HeadItem.CopyItemGuid,ctvm.ItemViewModels.Select(x=>x.CopyItem).ToList(), true);
            
            switch (ctvm.HeadItem.CopyItemType) {
                case MpCopyItemType.Text:
                case MpCopyItemType.FileList:
                    rtb.FitDocToRtb();
                    break;
                case MpCopyItemType.Image:
                    rtb.Document.PageWidth = rtb.ActualWidth;
                    rtb.Document.PageHeight = rtb.ActualHeight;
                    rtb.HorizontalAlignment = HorizontalAlignment.Center;
                    rtb.VerticalAlignment = VerticalAlignment.Center;

                    var p = rtb.Document.Blocks.FirstBlock as Paragraph;
                    var iuic = p.Inlines.FirstInline as InlineUIContainer;
                    
                    var img = iuic.Child as System.Windows.Controls.Image;
                    iuic.Child = null;
                    //p.Inlines.Clear();

                    iuic.Child = new Viewbox() {
                        VerticalAlignment = VerticalAlignment.Top,
                        Stretch = Stretch.Uniform,
                        Width = rtb.ActualWidth,
                        Height = rtb.ActualHeight,
                        Margin = new Thickness(5),
                        Child = img
                    };

                    //p.Inlines.Add(new InlineUIContainer(vb));

                    rtb.Document.LineStackingStrategy = LineStackingStrategy.MaxHeight;
                    rtb.Document.ConfigureLineHeight();

                    new TextRange(p.ContentStart, p.ContentEnd).ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Center);

                    rtb.VerticalAlignment = VerticalAlignment.Top;
                    rtb.VerticalContentAlignment = VerticalAlignment.Top;
                    rtb.GetVisualAncestor<MpContentListView>().VerticalAlignment = VerticalAlignment.Top;
                    rtb.GetVisualAncestor<MpContentListView>().VerticalContentAlignment = VerticalAlignment.Top;
                    break;
            }

            //wait till full doc is loaded to hook textChanged
            rtb.TextChanged += Rtb_TextChanged;
        }

        private static async Task<FlowDocument> DecodeContentItem(string itemGuid, List<MpCopyItem> items, bool decodeTemplates = false) {
            MpCopyItem ci = items.FirstOrDefault(x=>x.Guid == itemGuid);

            if (ci == default) {
                MpConsole.WriteLine("error fetching copy item: " + itemGuid);
                ci = await MpDataModelProvider.GetCopyItemByGuid(itemGuid);
            }

            FlowDocument fd = ci.ItemData.ToFlowDocument(ci.IconId);
            var childRanges = GetEncodedRanges(fd,"{c{","}c}");
            var childGuids = childRanges.Select(x => x.Text.Replace("{c{", string.Empty).Replace("}c}", string.Empty)).ToArray();
            var childItems = items.Where(x => childGuids.Contains(x.Guid)).ToList();
            if(childItems.Count != childGuids.Length) {
                var missingGuids = childGuids.Where(x => childItems.All(y => y.Guid != x));
                var missingItems = await MpDataModelProvider.GetCopyItemsByGuids(missingGuids.ToArray());
                childItems.AddRange(missingItems);
            }
            
            for (int i = 0; i < childGuids.Length; i++) {
                var insertRange = childRanges[i];
                string childGuid = childGuids[i];
                if(childItems.All(x=>x.Guid != childGuid)) {
                    MpConsole.WriteTraceLine("Missing content child detected, replacing w/ empty string " + childGuid);
                    insertRange.Text = string.Empty;
                    continue;
                }
                var cfd = await DecodeContentItem(childGuid,items);

                //fd.Combine(cfd, childRange.Start);
                using (MemoryStream stream = new MemoryStream()) {
                    var rangeFrom = new TextRange(cfd.ContentStart, cfd.ContentEnd);
                    if (insertRange.Start.Parent is Inline insertInline) { 
                        if(false) {//insertInline.PreviousInline == null) {
                            //insert is at beginning of paragraph so no need to alter child doc
                        } else {
                            rangeFrom = new TextRange(
                                cfd.ContentStart.GetInsertionPosition(LogicalDirection.Forward),
                                cfd.ContentEnd);
                            //remove line ending from text? (alt. change ContentEnd point maybe)
                            rangeFrom.Text = rangeFrom.Text.Replace(Environment.NewLine, string.Empty);
                        }
                    }
                    XamlWriter.Save(rangeFrom, stream);
                    rangeFrom.Save(stream, DataFormats.XamlPackage);
                    var rangeTo = new TextRange(insertRange.Start, insertRange.End);
                    rangeTo.Load(stream, DataFormats.XamlPackage);

                    if(decodeTemplates) {
                        //only register events on root document
                        var allRangeElements = rangeTo.GetAllTextElements();
                        allRangeElements.ForEach(x => x.Tag = new MpCopyItemReference() { CopyItemGuid = childGuid });
                        foreach (var te in allRangeElements) {
                            var origBrush = te.Background;
                            te.MouseEnter += (s, e) => {
                                te.Background = Brushes.Yellow;
                                //var civm = MpClipTrayViewModel.Instance.Items.FirstOrDefault(x => x.ItemViewModels.Any(y => y.CopyItemGuid == childGuid)).ItemViewModels.FirstOrDefault(x => x.CopyItemGuid == childGuid);
                                //civm.IsHovering = true;
                            };
                            te.MouseLeave += (s, e) => {
                                te.Background = origBrush;
                                //var civm = MpClipTrayViewModel.Instance.Items.FirstOrDefault(x => x.ItemViewModels.Any(y => y.CopyItemGuid == childGuid)).ItemViewModels.FirstOrDefault(x => x.CopyItemGuid == childGuid);
                                //civm.IsHovering = false;
                            };
                        }
                    }
                }
            }

            if(decodeTemplates) {
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
            }

            var allTextElements = fd.GetAllTextElements();
            var cir = new MpCopyItemReference() { CopyItemGuid = ci.Guid, CopyItemSourceGuid = ci.CopyItemSourceGuid };
            fd.Tag = cir;
            allTextElements.Where(x => x.Tag == null).ForEach(x => x.Tag = cir);

            MpConsole.WriteLine("FlowDoc w/ Tags Xaml:");
            MpConsole.WriteLine(fd.ToXamlPackage());
            return fd;
        }

        private static TextRange[] GetEncodedRanges(FlowDocument fd, string rangeStartText, string rangeEndText) {
            List<TextRange> encodedRange = new List<TextRange>();

            var tp = fd.ContentStart;
            while (true) {
                var encodedRangeOpenTag = tp.FindText(fd.ContentEnd, rangeStartText);
                if (encodedRangeOpenTag == null) {
                    break;
                }
                var encodedRangeCloseTag = encodedRangeOpenTag.End.FindText(fd.ContentEnd, rangeEndText);
                if (encodedRangeCloseTag == null) {
                    MpConsole.WriteLine(@"Corrupt text content, missing ending range tag. Item xaml: ");
                    MpConsole.WriteLine(fd.ToXamlPackage());
                    throw new Exception("Corrupt text content see console");
                }
                encodedRange.Add(new TextRange(encodedRangeOpenTag.Start, encodedRangeCloseTag.End));
                tp = encodedRangeCloseTag.End.GetNextContextPosition(LogicalDirection.Forward);
            }

            return encodedRange.ToArray();
        }

        private static void Rtb_TextChanged(object sender, TextChangedEventArgs e) {
            if(MpDragDropManager.IsDragAndDrop) {
                // NOTE during drop rtb will be reinitialized
                return;
            }
            var rtb = sender as RichTextBox;
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            if(ctvm.IsPlaceholder) {
                // BUG I think this event gets called when a tile is dropped and its turned into placeholder
                return;
            }
            MpConsole.WriteLines("Tile " + ctvm.HeadItem.CopyItemTitle + " text changed:");
            MpConsole.WriteLine(rtb.Document.ToXamlPackage());

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
                int curOffset = fd.ContentStart.GetOffsetToPosition(ctp);
                if(curOffset < 0) {
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

        #endregion
    }
}