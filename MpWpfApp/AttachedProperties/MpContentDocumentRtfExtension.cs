//using CefSharp;
//using CefSharp.JavascriptBinding;
//using CefSharp.Wpf;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace MpWpfApp {
    public class MpContentDocumentRtfExtension : DependencyObject {
        #region Private Variables

        
        private static readonly double _EDITOR_DEFAULT_WIDTH = 900;

        private static List<RichTextBox> _isChangeBlockRtbs = new List<RichTextBox>();

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
            typeof(MpContentDocumentRtfExtension),
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
            typeof(MpContentDocumentRtfExtension),
            new FrameworkPropertyMetadata(MpClipTileViewModel.DefaultBorderWidth));

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
            typeof(MpContentDocumentRtfExtension),
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
            }
        }

        private static void DisableReadOnly(FrameworkElement fe) {
            var ctcv = fe.GetVisualAncestor<MpClipTileContainerView>();
            
            if (ctcv != null) {
                SetReadOnlyWidth(fe,ctcv.ActualWidth);
                

                if (ctcv.ActualWidth < _EDITOR_DEFAULT_WIDTH) {
                    ctcv.TileResizeBehavior.ResizeWidth(_EDITOR_DEFAULT_WIDTH);
                }
                //(fe as RichTextBox).FitDocToRtb();
            }
        }

        public static void ExpandContent(MpClipTileViewModel ctvm) {
            var rtb = FindRtbByViewModel(ctvm);
            var ctcv = rtb.GetVisualAncestor<MpClipTileContainerView>();

            if (ctcv != null) {
                SetReadOnlyWidth(rtb, ctcv.ActualWidth);

                if (ctcv.ActualWidth < _EDITOR_DEFAULT_WIDTH) {
                    ctcv.TileResizeBehavior.ResizeWidth(_EDITOR_DEFAULT_WIDTH);
                }
                //rtb.FitDocToRtb();
            }
        }

        public static void UnexpandContent(MpClipTileViewModel ctvm) {
            var rtb = FindRtbByViewModel(ctvm);
            var ctcv = rtb.GetVisualAncestor<MpClipTileContainerView>();
            if (ctcv != null) {
                ctcv.TileResizeBehavior.ResizeWidth(GetReadOnlyWidth(rtb));
            }
        }

        public static async Task SaveTextContent(RichTextBox rtb) {
            if(MpClipTrayViewModel.Instance.IsRequery || MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }

            if(rtb.DataContext is MpClipTileViewModel ctvm) {
                // flags detail info to reload in ctvm propertychanged
                ctvm.CopyItemData = GetEncodedContent(rtb);

                await LoadContent(rtb);
            }
        }

        public static string GetEncodedContent(RichTextBox rtb, bool ignoreSubSelection = true, bool asPlainText = false) {
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            if(ctvm == null) {
                Debugger.Break();
            }            

            switch(ctvm.ItemType) {
                case MpCopyItemType.FileList:
                    if(!ignoreSubSelection) {
                        return string.Join(Environment.NewLine, ctvm.FileItems.Where(x => x.IsSelected).Select(x => x.Path));
                    }
                    return string.Join(Environment.NewLine, ctvm.FileItems.Select(x => x.Path));
                case MpCopyItemType.Image:
                    return rtb.Document.GetAllTextElements()
                                       .Where(x => x is InlineUIContainer)
                                       .Cast<InlineUIContainer>()
                                       .Select(x => x.Child as Image)
                                       .Select(x => (x.Source as BitmapSource).ToBase64String())
                                       .FirstOrDefault();
                case MpCopyItemType.Text:
                    List<TextElement> elementsToEncode = new List<TextElement>();
                    if (!ignoreSubSelection && !rtb.Selection.IsEmpty) {
                        //if nothing is selected treat full content as selection
                        bool hasSelectedTemplates = MpTextSelectionRangeExtension.IsSelectionContainTemplate(ctvm);
                        if(hasSelectedTemplates) {
                            elementsToEncode = rtb.Selection.GetAllTextElements()
                                            .Where(x => x is InlineUIContainer && x.Tag is MpTextTemplate)
                                            .ToList();
                        }
                    } else if(ctvm.HasTemplates) {
                        elementsToEncode = rtb.Document.GetAllTextElements()
                                            .Where(x => x is InlineUIContainer && x.Tag is MpTextTemplate)
                                            .ToList();
                    }
                    
                    foreach (InlineUIContainer thl in elementsToEncode) {
                        var cit = thl.Tag as MpTextTemplate;
                        var span = new Span(thl.ContentStart, thl.ContentEnd);
                        span.Inlines.Clear();
                        span.Inlines.Add(cit.EncodedTemplate);
                    }

                    if(ignoreSubSelection || rtb.Selection.IsEmpty) {
                        if(asPlainText) {
                            return rtb.Document.ToPlainText();
                        }
                        return rtb.Document.ToRichText();
                    }
                    if (asPlainText) {
                        return rtb.Selection.ToRichText().ToPlainText();
                    }

                    return rtb.Selection.ToRichText();
            }
            MpConsole.WriteTraceLine("Unknown item type " + ctvm);
            return null;            
        }

        public static string ExchangeDragDataWithDropTarget(MpClipTileViewModel drag_ctvm, bool isCopy, bool isSelfDrop) {            
            string dropData = null;
            var drag_rtb = FindRtbByViewModel(drag_ctvm);

            switch (drag_ctvm.ItemType) {
                case MpCopyItemType.Text:
                    if(MpTextSelectionRangeExtension.IsSelectionContainTemplate(drag_ctvm)) {
                        dropData = GetEncodedContent(drag_rtb, false);
                    } else {
                        dropData = drag_rtb.Selection.Text;
                    }                    

                    if(!isCopy) {
                        //when drag selection is not copy delete selection from source
                        drag_rtb.Selection.Text = string.Empty;                        
                    }
                    break;
                case MpCopyItemType.FileList:
                    if(drag_ctvm.FileItems.All(x=>x.IsSelected == false)) {
                        drag_ctvm.FileItems.ForEach(x => x.IsSelected = true);
                    }
                    dropData = string.Join(Environment.NewLine, drag_ctvm.FileItems.Where(x => x.IsSelected).Select(x => x.Path));

                    if (!isCopy) {
                        //when drag selection is not copy delete selection from source
                        var fileItemsToRemove = drag_ctvm.FileItems.Where(x => x.IsSelected).ToList();
                        for(int i = 0; i < fileItemsToRemove.Count;i++) {
                            drag_ctvm.FileItems.Remove(fileItemsToRemove[i]);

                            
                        }
                        var paragraphsToRemove = drag_rtb.Document.GetAllTextElements()
                           .Where(x => x is MpFileItemParagraph).Cast<MpFileItemParagraph>()
                               .Where(x => fileItemsToRemove.Any(y => y == x.DataContext));

                        paragraphsToRemove.ForEach(x => drag_rtb.Document.Blocks.Remove(x));
                    }
                    break;
                case MpCopyItemType.Image:
                    // shouldn't happen for images
                    Debugger.Break();
                    break;
            }

            return dropData;
        }

        public static async Task CleanupDragItemAfterDrop(MpClipTileViewModel drag_ctvm, bool isSelfDrop) {
            var drag_rtb = FindRtbByViewModel(drag_ctvm);
            if(drag_rtb == null) {
                return;
            }
            bool delete_item = false;
            if(drag_ctvm.ItemType == MpCopyItemType.Text) {
                string dpt = drag_rtb.Document.ToPlainText().Trim().Replace(Environment.NewLine, string.Empty);
                if (string.IsNullOrWhiteSpace(dpt) && !isSelfDrop) {
                    delete_item = true;
                }
            } else if(drag_ctvm.ItemType == MpCopyItemType.FileList) {
                if(drag_ctvm.FileItems.Count == 0) {
                    delete_item = true;
                }
            } else {
                return;
            }
            
            if(delete_item) {
                await drag_ctvm.CopyItem.DeleteFromDatabaseAsync();
            } else {
                await SaveTextContent(drag_rtb);
            }            
        }

        #endregion

        #region HeadContentItemViewModel

        public static object GetCopyItem(DependencyObject obj) {
            return obj.GetValue(CopyItemProperty);
        }
        public static void SetCopyItem(DependencyObject obj, object value) {
            obj.SetValue(CopyItemProperty, value);
        }

        public static readonly DependencyProperty CopyItemProperty =
          DependencyProperty.RegisterAttached(
            "CopyItem",
            typeof(object),
            typeof(MpContentDocumentRtfExtension),
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

        private static void Rtb_Unloaded(object sender, RoutedEventArgs e) {
            if(sender is RichTextBox rtb) {
                UnloadContent(rtb);
            }
        }

        private static void UnloadContent(RichTextBox rtb) {
            rtb.Loaded -= Rtb_Loaded;
            rtb.Unloaded -= Rtb_Unloaded;
            rtb.TextChanged -= Rtb_TextChanged;
        }

        public static async Task LoadContent(RichTextBox rtb) {
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

            MpCopyItem ci = ctvm.CopyItem;
            new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)
                .LoadItemData(ci.ItemData, ci.ItemType, out Size rawDimensions);
            ctvm.UnformattedContentSize = rawDimensions;

            if (ctvm == null) {
                return;
            }

            //LoadHyperlinks(rtb);

            await LoadTemplates(rtb);

            ctvm.IsBusy = false;

            switch (ctvm.ItemType) {
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

            rtb.TextChanged += Rtb_TextChanged;

            var rtb_a = rtb.GetVisualAncestor<AdornerLayer>();
            if (rtb_a != null) {
                rtb_a.Update();
            }
        }



        public static async Task LoadTemplates(RichTextBox rtb) {
            var ctvm = rtb.DataContext as MpClipTileViewModel;

            var tcvm = ctvm.TemplateCollection;
            // this should only occur in root document once all children are added to avoid different document error
            var templateRanges = GetEncodedRanges(rtb.Document, "{t{", "}t}");
            var templateGuids = templateRanges.Select(x => x.Text.Replace("{t{", string.Empty).Replace("}t}", string.Empty)).ToList();
            var templateItems = await MpDataModelProvider.GetTextTemplatesByGuids(templateGuids);
            
            if(rtb.IsReadOnly && tcvm.Items.Count > templateGuids.Distinct().Count()) {
                //all instances of at least one template were deleted
                Debugger.Break();
                
            }
            for (int i = 0; i < templateGuids.Count; i++) {
                string templateGuid = templateGuids[i];
                MpTextTemplate templateItem = null;
                if (templateItems.All(x => x.Guid != templateGuid)) {
                    //Debugger.Break();
                    // when template is encoded in document but not referenced in MpTextTemplate
                    var missingItem = await MpDataModelProvider.GetTextTemplateByGuid(templateGuid);
                    if(missingItem == null) {
                        ctvm.CopyItemData = ctvm.CopyItemData.Replace(@"\{t\{" + templateGuid + @"\{t\{", string.Empty);
                        templateRanges[i].Text = string.Empty;
                        MpConsole.WriteLine($"CopyItem {ctvm} item's data had ref to {templateGuid} which is not in the db, is now removed from item data");

                        var tvm = tcvm.Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid);
                        if(tvm != null) {
                            tcvm.Items.Remove(tvm);
                            MpConsole.WriteLine($"Template collection also had ref to item {templateGuid}, which is also now removed");
                        }
                        continue;
                    }
                    templateItem = missingItem;
                } else {
                    templateItem = templateItems.FirstOrDefault(x => x.Guid == templateGuid);
                }
                if(templateItem == null) {
                    Debugger.Break();
                } else if(!tcvm.Items.Any(x=>x.TextTemplateGuid == templateGuid)) {
                    // only add one distinct tvm to tcvm
                    var tvm = await tcvm.CreateTemplateViewModel(templateItem);
                    tcvm.Items.Add(tvm);

                }
                var templateRange = templateRanges.FirstOrDefault(x => x.Text == "{t{"+templateGuid+"}t}");
                if(templateRange == null) {
                    Debugger.Break();
                }
                if(templateItem == null) {
                    Debugger.Break();
                }
                templateRange.LoadTextTemplate(templateItem);
            }

            tcvm.OnPropertyChanged(nameof(tcvm.Items));

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
                tp = encodedRangeCloseTag.End.GetNextInsertionPosition(LogicalDirection.Forward);
            }

            return encodedRange.ToArray();
        }

        private static void Rtb_TextChanged(object sender, TextChangedEventArgs e) {
            //return;
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

            if (ctvm.IsTextItem) {
                
                rtb.Document.ConfigureLineHeight();
                ctvm.ResetExpensiveDetails();
                
            }
        }


        public static List<TextRange> FindContent(MpClipTileViewModel ctvm, string matchText, bool isCaseSensitive = false, bool matchWholeWord = false, bool useRegEx = false) {
            var rtb = FindRtbByViewModel(ctvm);

            return rtb.Document.FindText(
                matchText,
                isCaseSensitive,
                matchWholeWord,
                useRegEx);
        }

        public static RichTextBox FindRtbByViewModel(MpClipTileViewModel ctvm) {
            var cvl = Application.Current.MainWindow.GetVisualDescendents<MpRtbContentView>();
            if (cvl == null) {
                Debugger.Break();
            }
            var cv = cvl.FirstOrDefault(x => x.DataContext == ctvm);
            if (cv == null) {
                Debugger.Break();
            }
            if(cv.Rtb == null) {
                Debugger.Break();
            }
            return cv.Rtb;
        }

        private static void LoadHyperlinks(RichTextBox rtb) {
            var ctvm = rtb.DataContext as MpClipTileViewModel;

            //select system created hyperlinks

            var hll = rtb.Document.GetAllTextElements()
                                  .Where(x => x is Hyperlink)
                                  .Cast<Hyperlink>()
                                  .Where(x => x.NavigateUri.AbsoluteUri.HasGuid());

            foreach(var hl in hll) {
                RequestNavigateEventHandler hl_Click_handler = (s, e) => {
                    string presetGuid = hl.NavigateUri.AbsoluteUri.ParseGuid();
                    var presetVm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelByGuid(presetGuid);
                    presetVm.Parent.ExecuteAnalysisCommand.Execute(new object[] { presetVm, hl.ContentRange().Text });
                };
                RoutedEventHandler hl_Unload_handler = null;
                hl_Unload_handler = (s, e) => {
                    hl.RequestNavigate -= hl_Click_handler;
                    hl.Unloaded -= hl_Unload_handler;
                };

                hl.RequestNavigate += hl_Click_handler;
                hl.Unloaded += hl_Unload_handler;
            }
        }

        public static Tuple<int, int> GetLineAndCharCount(MpClipTileViewModel ctvm) {
            var rtb = FindRtbByViewModel(ctvm);
            if(rtb == null) {
                return new Tuple<int, int>(0, 0);
            }
        
            string pt = rtb.Document.ToPlainText();
            return new Tuple<int, int>(
                pt.IndexListOfAll(Environment.NewLine).Count + 1,
                pt.Length);
        }

        #endregion
    }
}