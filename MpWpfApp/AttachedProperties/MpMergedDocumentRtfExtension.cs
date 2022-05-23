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
            if(MpClipTrayViewModel.Instance.IsRequery || MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }
            //await EncodeContent(rtb);
            //return;

            if(rtb.DataContext is MpClipTileViewModel ctvm) {
                var contentLookup = new Dictionary<string, List<TextElement>>();
                var allTemplateHyperlinks = rtb.Document.GetAllTextElements()
                    .Where(x=>x is InlineUIContainer && x.Tag is MpTextTemplate)                            
                    .ToList();

                foreach (InlineUIContainer thl in allTemplateHyperlinks) {
                    var cit = thl.Tag as MpTextTemplate;
                    var span = new Span(thl.ContentStart, thl.ContentEnd);
                    span.Inlines.Clear();
                    span.Inlines.Add(cit.EncodedTemplate);
                }

                //var hl_test = rtb.Document.GetAllTextElements()
                //    .Where(x => x is Hyperlink)
                //    .ToList();

                ctvm.HeadItem.CopyItemData = rtb.Document.ToRichText();

                while(ctvm.IsAnyBusy) {
                    await Task.Delay(100);
                }
                LoadContent(rtb).FireAndForgetSafeAsync(ctvm);
                //var allTextElements = rtb.Document.GetAllTextElements().ToList();
                //foreach(var te in allTextElements) {
                //    // fill dictionary w/ all text elements per copy item in doc order
                //    if(te.Tag == null) {
                //        // this should only happen when tile is initially loading which means it doesn't need to be saved
                //        return;
                //        //Debugger.Break();
                //        //throw new Exception("Error all text elements should have a model as their tag (either MpCopyItem or MpTextTemplate)");
                //    }
                //    if(te.Tag is MpCopyItem ci) {                        
                //        if(!contentLookup.ContainsKey(ci.Guid)) {
                //            contentLookup.Add(ci.Guid, new List<TextElement>());
                //        }
                //        contentLookup[ci.Guid].Add(te);
                //        contentLookup[ci.Guid].Sort((a, b) => {
                //            return a.ContentStart.CompareTo(b.ContentStart);
                //        });
                //    }
                //    // TODO should add template reference obj and check here probably
                //}

                //foreach(var ckvp in contentLookup) {
                //    var start = ckvp.Value.Aggregate((a, b) =>
                //        rtb.Document.ContentStart.GetOffsetToPosition(a.ElementStart) < rtb.Document.ContentStart.GetOffsetToPosition(b.ElementStart) ? a : b).ElementStart;
                //    var end = ckvp.Value.Aggregate((a, b) =>
                //        rtb.Document.ContentStart.GetOffsetToPosition(a.ElementEnd) > rtb.Document.ContentStart.GetOffsetToPosition(b.ElementEnd) ? a : b).ElementEnd;

                //    string itemRtf = new TextRange(start, end).ToRichText();
                //    var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelByGuid(ckvp.Key);
                //    if(civm == null) {
                //        // NOTE this should proibably not happen
                //        var ci = await MpDataModelProvider.GetCopyItemByGuid(ckvp.Key);
                //        if (ci == null) {
                //            //a new item

                //        } else {
                //            ci.ItemData = itemRtf;
                //            await ci.WriteToDatabaseAsync();
                //        }
                //    } else {
                //        civm.CopyItemData = itemRtf;
                //    }

                //}
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

            MpCopyItem ci = ctvm.HeadItem.CopyItem;
            new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).LoadItemData(ci.ItemData, ci.ItemType, ci.IconId);

            if (ctvm == null) {
                return;
            }
            if (ctvm.HeadItem == null) {
                return;
            }

            ctvm.HeadItem.UnformattedContentSize = rtb.Document.GetDocumentSize();

            await LoadTemplates(rtb);

            ctvm.IsBusy = false;

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

            rtb.TextChanged += Rtb_TextChanged;

            var rtb_a = rtb.GetVisualAncestor<AdornerLayer>();
            if (rtb_a != null) {
                rtb_a.Update();
            }

            //if(ctvm.HasTemplates) {
            //    foreach(var tvm in ctvm.HeadItem.TemplateCollection.Items) {
            //        var templateRanges = rtb.Document.FindText(tvm.TemplateName);
            //        Debugger.Break();
            //    }
            //}
        }

        public static async Task LoadTemplates(RichTextBox rtb) {
            var ctvm = rtb.DataContext as MpClipTileViewModel;
            var civm = ctvm.HeadItem;
            var tcvm = civm.TemplateCollection;
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
                    Debugger.Break();
                    // when template is encoded in document but not referenced in MpTextTemplate
                    var missingItem = await MpDataModelProvider.GetTextTemplateByGuid(templateGuid);
                    if(missingItem == null) {
                        civm.CopyItemData = civm.CopyItemData.Replace(@"\{t\{" + templateGuid + @"\{t\{", string.Empty);
                        templateRanges[i].Text = string.Empty;
                        MpConsole.WriteLine($"CopyItem {civm} item's data had ref to {templateGuid} which is not in the db, is now removed from item data");

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
                tp = encodedRangeCloseTag.End.GetNextContextPosition(LogicalDirection.Forward);
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
            }
        }

        #endregion
    }
}