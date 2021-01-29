using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                    if(e.OldValue == null) {
                        //occurs when cliptile created
                        return;
                    }
                    var cb = (MpClipBorder)s;
                    var ctvm = (MpClipTileViewModel)cb.DataContext; 
                    var hlt = (string)e.NewValue;
                    if (ctvm.MainWindowViewModel.IsLoading || ctvm.IsEditingTile || ctvm.IsLoading) {
                        ctvm.TileVisibility = Visibility.Visible;
                        return;
                    }
                    var mc1 = Regex.Matches(ctvm.CopyItemPlainText, hlt, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                    var mc2 = Regex.Matches(ctvm.CopyItemTitle, hlt, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                    if (mc1.Count == 0 && mc2.Count == 0) {
                        ctvm.TileVisibility = Visibility.Collapsed;
                        return;
                    }
                    PerformHighlight(cb, ctvm, hlt);
                }
            });


        private static async Task PerformHighlight(MpClipBorder cb, MpClipTileViewModel ctvm, string hlt) {
            ctvm.MainWindowViewModel.SearchBoxViewModel.IsSearching = true;
            await Dispatcher.CurrentDispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            (Action)(() => {
                                var sttvm = ctvm.MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;

                                var ttb = (TextBlock)cb.FindName("ClipTileTitleTextBlock");
                                var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
                                var hfb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightFocusedHexColorString);
                                var ctbb = Brushes.Transparent;

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
                            Console.WriteLine("Beginning highlight clip with title: " + ctvm.CopyItemTitle + " with highlight text: " + hlt);


                                ctvm.TileVisibility = Visibility.Visible;

                                MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, ctbb);
                                ctvm.LastTitleHighlightRangeList.Clear();

                                MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, ctbb);
                                ctvm.LastContentHighlightRangeList.Clear();

                                if (string.IsNullOrEmpty(hlt.Trim()) ||
                                    hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                                //if search text is empty clear any highlights and show clip (if associated w/ current tag)
                                ctvm.TileVisibility = Visibility.Visible;
                                    return;
                                }

                            //highlight title 
                            if (ttb.Text.ContainsByCaseSetting(hlt)) {
                                    foreach (var mr in MpHelpers.Instance.FindStringRangesFromPosition(ttb.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive)) {
                                        ctvm.LastTitleHighlightRangeList.Add(mr);
                                    }
                                    MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, hb);
                                }
                                switch (ctvm.CopyItemType) {
                                    case MpCopyItemType.RichText:
                                        var rtb = (RichTextBox)cb.FindName("ClipTileRichTextBox");                                       

                                        rtb.BeginChange();
                                        foreach (var mr in MpHelpers.Instance.FindStringRangesFromPosition(rtb.Document.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive)) {
                                            ctvm.LastContentHighlightRangeList.Add(mr);
                                        }
                                        if (ctvm.LastContentHighlightRangeList.Count > 0) {
                                            MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, hb);
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
                                                        var hlr = MpHelpers.Instance.FindStringRangeFromPosition(fitb.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive);
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
            ctvm.MainWindowViewModel.SearchBoxViewModel.IsSearching = false;
        }
    }


}
