using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpHighlightTextExtension : DependencyObject {
        //public List<TextRange> LastHighlightRangeList { get; set; } = new List<TextRange>();

        public static string GetHighlightText(DependencyObject obj) {
            return (string)obj.GetValue(HighlightTextProperty);
        }
        public static void SetHighlightText(DependencyObject obj, string value) {
            obj.SetValue(HighlightTextProperty, value);
        }
        public static readonly DependencyProperty HighlightTextProperty =
          DependencyProperty.RegisterAttached(
            "HighlightText",
            typeof(string),
            typeof(MpHighlightTextExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (s, e) => {
                    var cb = (MpClipBorder)s;
                    var ctvm = (MpClipTileViewModel)cb.DataContext;
                    if (ctvm.MainWindowViewModel.IsLoading || ctvm.IsEditingTile || ctvm.IsLoading) {
                        ctvm.TileVisibility = Visibility.Visible;
                        return;
                    }                    
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var sttvm = ctvm.MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;
                            var hlt = (string)e.NewValue;

                            var ttb = (TextBlock)cb.FindName("ClipTileTitleTextBlock");
                            var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
                            var hfb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightFocusedHexColorString);
                            var ctbb = Brushes.Transparent;// (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.ClipTileBackgroundColor);

                            if (!sttvm.IsLinkedWithClipTile(ctvm)) {
                                Console.WriteLine("Clip tile w/ title " + ctvm.CopyItemTitle + " is not linked with current tag");
                                ctvm.TileVisibility = Visibility.Collapsed;
                                return;
                            }
                            //bool isInTitle = ttb.Text.ContainsByCaseSetting(hlt);
                            //bool isInContent = ctvm.ToString().ContainsByCaseSetting(hlt);
                            //bool isSearchBlank = string.IsNullOrEmpty(hlt.Trim()) || hlt == Properties.Settings.Default.SearchPlaceHolderText;
                            //ctvm.TileVisibility = isInTitle || isInContent || isSearchBlank ? Visibility.Visible : Visibility.Collapsed;
                            //return;
                            Console.WriteLine("Beginning highlight clip with title: " + ctvm.CopyItemTitle + " with highlight text: " + (string)e.NewValue);

                            ctvm.TileVisibility = Visibility.Visible;

                            MpHelpers.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, ctbb);
                            ctvm.LastTitleHighlightRangeList.Clear();

                            MpHelpers.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, ctbb);
                            ctvm.LastContentHighlightRangeList.Clear();

                            if (string.IsNullOrEmpty(hlt.Trim()) ||
                                hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                                //if search text is empty clear any highlights and show clip (if associated w/ current tag)
                                ctvm.TileVisibility = Visibility.Visible;
                                return;
                            }

                            //highlight title 
                            if (ttb.Text.ContainsByCaseSetting(hlt)) {
                                foreach(var mr in MpHelpers.FindStringRangesFromPosition(ttb.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive)) {
                                    ctvm.LastTitleHighlightRangeList.Add(mr);
                                }
                                MpHelpers.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, hb);
                            }
                            switch (ctvm.CopyItemType) {
                                case MpCopyItemType.RichText:
                                    var rtb = (RichTextBox)cb.FindName("ClipTileRichTextBox");
                                    rtb.BeginChange();
                                    foreach (var mr in MpHelpers.FindStringRangesFromPosition(rtb.Document.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive)) {
                                        ctvm.LastContentHighlightRangeList.Add(mr);
                                    }
                                    if (ctvm.LastContentHighlightRangeList.Count > 0){
                                        MpHelpers.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, hb);
                                        //rtb.CaretPosition = ctvm.LastContentHighlightRangeList[0].Start;
                                    } else if (ctvm.LastTitleHighlightRangeList.Count == 0) {
                                        ctvm.TileVisibility = Visibility.Collapsed;
                                    }
                                    if (ctvm.LastContentHighlightRangeList.Count > 0 || ctvm.LastTitleHighlightRangeList.Count > 0) {
                                        ctvm.CurrentHighlightMatchIdx = 0;
                                    }
                                    rtb.EndChange();
                                    break;
                                case MpCopyItemType.Image:
                                    foreach (var diovm in ctvm.DetectedImageObjectCollectionViewModel) {
                                        if (diovm.ObjectTypeName.ContainsByCaseSetting(hlt)) {
                                            ctvm.TileVisibility = Visibility.Visible;
                                            return;
                                        }
                                    }
                                    if (ctvm.LastContentHighlightRangeList.Count == 0) {
                                        ctvm.TileVisibility = Visibility.Collapsed;
                                    }
                                    break;
                                case MpCopyItemType.FileList:
                                    var flb = (ListBox)cb.FindName("ClipTileFileListBox");
                                    if (ctvm.LastContentHighlightRangeList != null) {
                                        foreach (var lhr in ctvm.LastContentHighlightRangeList) {
                                            lhr.ApplyPropertyValue(TextElement.BackgroundProperty, ctbb);
                                        }
                                    }
                                    foreach (var fivm in ctvm.FileListViewModels) {
                                        if (fivm.ItemPath.ContainsByCaseSetting(hlt)) {
                                            var container = flb.ItemContainerGenerator.ContainerFromItem(fivm) as FrameworkElement;
                                            if (container != null) {
                                                var fitb = (TextBlock)container.FindName("FileListItemTextBlock");
                                                if (fitb != null) {
                                                    var hlr = MpHelpers.FindStringRangeFromPosition(fitb.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive);
                                                    if (hlr != null) {
                                                        hlr.ApplyPropertyValue(TextBlock.BackgroundProperty, hb);
                                                        ctvm.LastContentHighlightRangeList.Add(hlr);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (ctvm.LastContentHighlightRangeList.Count == 0) {
                                        ctvm.TileVisibility = Visibility.Collapsed;
                                    } else {
                                        ctvm.TileVisibility = Visibility.Visible;
                                    }
                                    break;
                            }
                            Console.WriteLine("Ending highlighting clip with title: " + ctvm.CopyItemTitle);
                        }));
                }
            });
        
    }

}
