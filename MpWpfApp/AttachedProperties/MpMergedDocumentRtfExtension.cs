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

            //rtb.FitDocToRtb();

            //wait till full doc is loaded to hook textChanged
            rtb.TextChanged += Rtb_TextChanged;
        }

        private static async Task<FlowDocument> DecodeContentItem(string itemGuid, List<MpCopyItem> items, bool decodeTemplates = false) {
            MpCopyItem ci = items.FirstOrDefault(x=>x.Guid == itemGuid);

            if (ci == default) {
                MpConsole.WriteLine("error fetching copy item: " + itemGuid);
            }

            FlowDocument fd = ci.ItemData.ToFlowDocument(ci.IconId);
            var childRanges = GetEncodedRanges(fd,"{c{","}c}");
            var childGuids = childRanges.Select(x => x.Text.Replace("{c{", string.Empty).Replace("}c}", string.Empty)).ToArray();
            var childItems = await MpDataModelProvider.GetCopyItemsByGuids(childGuids.ToArray());
            
            for (int i = 0; i < childGuids.Length; i++) {
                string childGuid = childGuids[i];
                if(childItems.All(x=>x.Guid != childGuid)) {
                    MpConsole.WriteTraceLine("Missing content child detected, replacing w/ empty string " + childGuid);
                    childRanges[i].Text = string.Empty;
                    continue;
                }
                var cfd = await DecodeContentItem(childGuid,items);
                fd.InsertFlowDocument(cfd, childRanges[i]);
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
            fd.Tag = (MpICopyItemReference)ci;
            allTextElements.Where(x => x.Tag == null).ForEach(x => x.Tag = (MpICopyItemReference)ci);
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
            var rtb = sender as RichTextBox;
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            
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
                    tcte.Tag = FindNearestContentReference(tcte);
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
                ctp = ctp.GetPositionAtOffset(curOffset - 1);
                var pte = ctp.Parent.FindParentOfType<FrameworkContentElement>();
                if(pte != null && pte.Tag is MpDbModelBase dbo) {
                    return dbo;
                }
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
                if (pte != null && pte.Tag is MpDbModelBase dbo) {
                    return dbo;
                }
            }

            // TODO need to create new element here, return root or throw an error depending on how changes handled
            return null;
        }

        #endregion
    }
}