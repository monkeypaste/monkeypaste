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
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls.Primitives;

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
                if(GetReadOnlyWidth(fe) < MpClipTileViewModel.DefaultBorderWidth) {
                    SetReadOnlyWidth(fe, MpClipTileViewModel.DefaultBorderWidth);
                }
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
                if (GetReadOnlyWidth(rtb) < MpClipTileViewModel.DefaultBorderWidth) {
                    SetReadOnlyWidth(rtb, MpClipTileViewModel.DefaultBorderWidth);
                }
                ctcv.TileResizeBehavior.ResizeWidth(GetReadOnlyWidth(rtb));
            }
        }

        public static async Task SaveTextContent(RichTextBox rtb) {
            if(MpClipTrayViewModel.Instance.IsRequery || 
                MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }

            if(rtb.DataContext is MpClipTileViewModel ctvm) {
                // flags detail info to reload in ctvm propertychanged
                ctvm.CopyItemData = GetEncodedContent(rtb);

                await LoadContent(rtb);
            }
        }

        public static string GetEncodedContent(
            RichTextBox rtb, 
            bool ignoreSubSelection = true, 
            bool asPlainText = false) {
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
                    TextRange tr = null;
                    if(ignoreSubSelection) {
                        tr = rtb.Document.ContentRange();
                    } else {
                        tr = rtb.Selection;
                    }
                    //ignoreSubSelection ? rtb.Document.ContentRange() : rtb.Selection;
                    return asPlainText ? tr.ToEncodedPlainText() : tr.ToEncodedRichText();
            }
            MpConsole.WriteTraceLine("Unknown item type " + ctvm);
            return null;            
        }

        public static async Task FinishContentCut(MpClipTileViewModel drag_ctvm) {
            var rtb = FindRtbByViewModel(drag_ctvm);
            if(rtb == null) {
                return;
            }
            bool delete_item = false;
            if(drag_ctvm.ItemType == MpCopyItemType.Text) {
                rtb.Selection.Text = string.Empty;

                string dpt = rtb.Document.ToPlainText().Trim().Replace(Environment.NewLine, string.Empty);
                if (string.IsNullOrWhiteSpace(dpt)) {
                    delete_item = true;
                }
            } else if(drag_ctvm.ItemType == MpCopyItemType.FileList) {
                if(drag_ctvm.FileItems.Count == 0) {
                    delete_item = true;
                } else {
                    var fileItemsToRemove = drag_ctvm.FileItems.Where(x => x.IsSelected).ToList();
                    for (int i = 0; i < fileItemsToRemove.Count; i++) {
                        drag_ctvm.FileItems.Remove(fileItemsToRemove[i]);
                    }
                    var paragraphsToRemove = rtb.Document.GetAllTextElements()
                       .Where(x => x is MpFileItemParagraph).Cast<MpFileItemParagraph>()
                           .Where(x => fileItemsToRemove.Any(y => y == x.DataContext));

                    paragraphsToRemove.ForEach(x => rtb.Document.Blocks.Remove(x));
                }
            } else {
                return;
            }
            
            if(delete_item) {
                await drag_ctvm.CopyItem.DeleteFromDatabaseAsync();
            } else {
                await SaveTextContent(rtb);
            }            
        }

        #endregion

        #region CopyItem

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
                    if(e.NewValue != null) {
                        var rtb = s as RichTextBox;
                        if (rtb == null) {
                            return;
                        }
                        if (rtb.IsLoaded) {
                            Rtb_Loaded(rtb, null);
                        } else {
                            rtb.Loaded += Rtb_Loaded;
                        }                        
                    } else {
                        Rtb_Unloaded(s, null);
                    }
                    
                }
            });

        #endregion

        #region TextSelectionRange DependencyProperty

        public static MpIRtfSelectionRange GetTextSelectionRange(DependencyObject obj) {
            return (MpIRtfSelectionRange)obj.GetValue(TextSelectionRangeProperty);
        }

        public static void SetTextSelectionRange(DependencyObject obj, MpIRtfSelectionRange value) {
            obj.SetValue(TextSelectionRangeProperty, value);
        }

        public static readonly DependencyProperty TextSelectionRangeProperty =
            DependencyProperty.RegisterAttached(
                "TextSelectionRange",
                typeof(MpIRtfSelectionRange),
                typeof(MpContentDocumentRtfExtension),
                new FrameworkPropertyMetadata(null));

        public static MpRichTextFormatInfoFormat GetSelectionFormat(MpIRtfSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is RichTextBox rtb) {
                var rtfFormat = new MpRichTextFormatInfoFormat() {
                    inlineFormat = GetSelectedInlineFormat(rtb),
                    blockFormat = GetSelectedBlockFormat(rtb)
                };
                return rtfFormat;
            }
            return null;
        }

        public static MpInlineTextFormatInfoFormat GetSelectedInlineFormat(RichTextBox rtb) {
            if (rtb.Selection.Start.Parent is TextElement te) {
                if (te is Inline inline) {
                    var inlineFormat = new MpInlineTextFormatInfoFormat() {
                        background = inline.Background.ToHex(),
                        color = inline.Foreground.ToHex(),
                        bold = inline is Bold ||
                                rtb.Selection.Start.Parent.FindParentOfType<Bold>() != null,
                        italic = inline is Italic || inline.FontStyle == FontStyles.Italic ||
                                rtb.Selection.Start.Parent.FindParentOfType<Italic>() != null,
                        strike = inline.TextDecorations == TextDecorations.Strikethrough,
                        underline = inline is Underline ||
                                    rtb.Selection.Start.Parent.FindParentOfType<Underline>() != null,
                        font = inline.FontFamily.Source,
                        size = inline.FontSize,
                        script = inline.BaselineAlignment.ToString()
                    };
                    return inlineFormat;
                }
            }
            return null;
        }

        public static MpBlockTextFormatInfoFormat GetSelectedBlockFormat(RichTextBox rtb) {
            return null;
        }

        public static string GetSelectedPlainText(MpIRtfSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is RichTextBox rtb) {
                return GetEncodedContent(rtb, false, true);
            }
            return null;
        }

        public static string GetSelectedRichText(MpIRtfSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is RichTextBox rtb) {
                return GetEncodedContent(rtb, false, false);
            }
            return null;
        }

        public static int GetSelectionStart(MpIRtfSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is RichTextBox rtb) {
                TextRange start = new TextRange(rtb.Document.ContentStart, rtb.Selection.Start);
                return start.Text.Length;
            }
            return 0;
        }

        public static int GetSelectionLength(MpIRtfSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is RichTextBox rtb) {
                return rtb.Selection.Text.Length;
            }
            return 0;
        }


        public static void SetTextSelection(MpIRtfSelectionRange tsr, TextRange tr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is RichTextBox rtb) {
                if (!rtb.Document.ContentStart.IsInSameDocument(tr.Start) ||
                !rtb.Document.ContentStart.IsInSameDocument(tr.End)) {
                    return;
                }
                rtb.Selection.Select(tr.Start, tr.End);

                if (tr.Start.Parent is FrameworkContentElement fce) {
                    fce.BringIntoView();
                }
            }
        }

        public static void SetSelectionText(MpIRtfSelectionRange tsr, string text) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is RichTextBox rtb) {
                rtb.Selection.Text = text;
            }
        }

        public static void SelectAll(MpIRtfSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb == null) {
                Debugger.Break();
            }
            if (!tbb.IsFocused) {
                tbb.Focus();
            }
            if (!tbb.IsFocused) {
                Debugger.Break();
            }
            tbb.SelectAll();
        }

        public static bool IsAllSelected(MpIRtfSelectionRange tsr) {
            var tbb = FindTextBoxBase(tsr);
            if (tbb is RichTextBox rtb) {
                //return rtb.Document.ContentStart == rtb.Selection.Start &&
                //       rtb.Document.ContentEnd == rtb.Selection.End;
                return rtb.Document.ContentRange().Text.Length ==
                       rtb.Selection.Text.Length;
            }
            return false;
        }

        private static TextBoxBase FindTextBoxBase(MpIRtfSelectionRange tsr) {
            return FindRtbByViewModel(tsr as MpClipTileViewModel);
        }

        #endregion

        #region Rtb Event Handlers

        private static void Rtb_Loaded(object sender, RoutedEventArgs e) {
            var rtb = sender as RichTextBox;
            if(rtb == null) {
                return;
            }
            if(e == null) {
                rtb.Loaded += Rtb_Loaded;
            }

            rtb.Unloaded += Rtb_Unloaded;

            LoadContent(rtb).FireAndForgetSafeAsync(rtb.DataContext as MpClipTileViewModel);
        }        

        private static void Rtb_Unloaded(object sender, RoutedEventArgs e) {
            if(sender is RichTextBox rtb) {
                UnloadContent(rtb);
            }
        }
        private static void Rtb_TextChanged(object sender, TextChangedEventArgs e) {
            //return;
            var rtb = sender as RichTextBox;

            var rtb_a = rtb.GetVisualAncestor<AdornerLayer>();
            if (rtb_a != null) {
                rtb_a.Update();
            }

            if (MpDragDropManager.IsDragAndDrop) {
                // NOTE during drop rtb will be reinitialized
                return;
            }
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            if (ctvm.IsPlaceholder) {
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

        #endregion

        #region Public Methods

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

            //rtb.IsUndoEnabled = false;
            //rtb.Document.Blocks.Clear();
            //rtb.IsUndoEnabled = true;

            MpCopyItem ci = ctvm.CopyItem;
            //new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd)
            //    .LoadItemData(ci.ItemData, ci.ItemType, out Size rawDimensions);
            rtb.Document = MpRtbContentExtensions.LoadContent(ctvm, ci.ItemData, ci.ItemType, out Size rawDimensions);

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
            tcvm.IsBusy = true;

            // get ranges of templates present in realtime document 
            var loadedTemplateElements = rtb.Document.GetAllTextElements().Where(x => x is MpTextTemplateInlineUIContainer);
            var loadedTemplateGuids = loadedTemplateElements.Select(x => (x.Tag as MpTextTemplate).Guid).Distinct();

            // verify template loaded in document exists, if does add to collection if not present on remove from document 
            var loadedTemplateItems = await MpDataModelProvider.GetTextTemplatesByGuids(loadedTemplateGuids.ToList());
            var loadedTemplateGuids_toRemove = loadedTemplateGuids.Where(x => loadedTemplateItems.All(y => y.Guid != x));
            foreach (var templateGuid_toRemove in loadedTemplateGuids_toRemove) {
                var templateElements_toRemove = loadedTemplateElements.Where(x => (x.Tag as MpTextTemplate).Guid == templateGuid_toRemove);
                foreach (var templateElement_toRemove in templateElements_toRemove) {
                    var ttr = templateElement_toRemove.ContentRange();
                    ttr.Text = string.Empty;
                }

                var templateViewModel_toRemove = tcvm.Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid_toRemove);
                if (templateViewModel_toRemove != null) {
                    tcvm.Items.Remove(templateViewModel_toRemove);
                }
            }

            // get ranges of templates encoded from db (will be present on initial load, after saving content or when new template is added/created)
            var templateEncodedRanges = GetEncodedRanges(rtb.Document, MpTextTemplate.TextTemplateOpenToken, MpTextTemplate.TextTemplateCloseToken);

            var templateEncodedGuids = templateEncodedRanges.Select(x => x.Text.Replace(MpTextTemplate.TextTemplateOpenToken, string.Empty).Replace(MpTextTemplate.TextTemplateCloseToken, string.Empty)).ToList();
            var templateItems = await MpDataModelProvider.GetTextTemplatesByGuids(templateEncodedGuids);

            for (int i = 0; i < templateEncodedGuids.Count; i++) {
                string templateGuid = templateEncodedGuids[i];
                MpTextTemplate templateItem = null;
                if (templateItems.All(x => x.Guid != templateGuid)) {
                    //Debugger.Break();
                    // when template is encoded in document but not referenced in MpTextTemplate
                    var missingItem = await MpDataModelProvider.GetTextTemplateByGuid(templateGuid);
                    if (missingItem == null) {
                        ctvm.CopyItemData = ctvm.CopyItemData.Replace(MpTextTemplate.TextTemplateOpenTokenRtf + templateGuid + MpTextTemplate.TextTemplateCloseTokenRtf, string.Empty);
                        templateEncodedRanges[i].Text = string.Empty;
                        MpConsole.WriteLine($"CopyItem {ctvm} item's data had ref to {templateGuid} which is not in the db, is now removed from item data");

                        var tvm_ToRemove = tcvm.Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid);
                        if (tvm_ToRemove != null) {
                            tcvm.Items.Remove(tvm_ToRemove);
                            MpConsole.WriteLine($"Template collection also had ref to item {templateGuid}, which is also now removed");
                        }
                        continue;
                    }
                    templateItem = missingItem;
                } else {
                    templateItem = templateItems.FirstOrDefault(x => x.Guid == templateGuid);
                }


                if (templateItem == null) {
                    Debugger.Break();
                }
                MpTextTemplateViewModelBase tvm = tcvm.Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid);
                if (tvm == null) {
                    // only add one distinct tvm to tcvm
                    tvm = await tcvm.CreateTemplateViewModel(templateItem);
                    tcvm.Items.Add(tvm);
                }
                var templateRange = templateEncodedRanges.FirstOrDefault(x => x.Text == "{t{" + templateGuid + "}t}");
                if (templateRange == null) {
                    Debugger.Break();
                }
                if (templateItem == null) {
                    Debugger.Break();
                }

                //templateRange.LoadTextTemplate(templateItem);
                MpTextTemplateInlineUIContainer.Create(templateRange, tvm);
            }

            tcvm.OnPropertyChanged(nameof(tcvm.Items));
            tcvm.IsBusy = false;
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
            var cv = Application.Current.MainWindow
                                 .GetVisualDescendents<MpRtbContentView>()
                                 .FirstOrDefault(x => x.DataContext == ctvm);
            if (cv == null) {
                Debugger.Break();
            }
            if (cv.Rtb == null) {
                Debugger.Break();
            }
            return cv.Rtb;
        }

        public static Tuple<int, int> GetLineAndCharCount(MpClipTileViewModel ctvm) {
            var rtb = FindRtbByViewModel(ctvm);
            if (rtb == null) {
                return new Tuple<int, int>(0, 0);
            }

            string pt = rtb.Document.ToPlainText();
            return new Tuple<int, int>(
                pt.IndexListOfAll(Environment.NewLine).Count + 1,
                pt.Length);
        }

        #endregion

        #region Private Methods
        private static void UnloadContent(RichTextBox rtb) {
            rtb.Loaded -= Rtb_Loaded;
            rtb.Unloaded -= Rtb_Unloaded;
            rtb.TextChanged -= Rtb_TextChanged;
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


        #endregion
    }
}