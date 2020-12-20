using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
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
                //PropertyChangedCallback = (s, e) => {
                //    var cb = (MpClipBorder)s;
                //    var rtb = (RichTextBox)cb.FindName("ClipTileRichTextBox");
                //    if(rtb == null || rtb.Visibility == Visibility.Collapsed) {
                //        return;
                //    }
                //    var hlt = (string)e.NewValue;
                //    Dispatcher.CurrentDispatcher.BeginInvoke(
                //        DispatcherPriority.Background,
                //        (Action)(() => {
                //            //var cb = (MpClipBorder)rtb.GetVisualAncestor<MpClipBorder>();
                //            //if (cb == null) {
                //            //    Console.WriteLine("TokenizedRichTextBox error, cannot find clipborder");
                //            //    return;
                //            //}
                //            //if (cb.DataContext.GetType() != typeof(MpClipTileViewModel)) {
                //            //    return;
                //            //}

                //            var ctvm = (MpClipTileViewModel)cb.DataContext;
                //            //if (ctvm == null) {
                //            //    Console.WriteLine("TokenizedRichTextBox error, cannot find cliptile viewmodel");
                //            //    return;
                //            //}
                //            //if(ctvm.MainWindowViewModel.IsLoading) {
                //            //    return;
                //            //}
                //            var sttvm = ctvm.MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;

                //            rtb.BeginChange();
                //            new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                //            ctvm.TileVisibility = Visibility.Collapsed;
                //            if (!sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                //                ctvm.TileVisibility = Visibility.Collapsed;
                //                rtb.EndChange();
                //                //return;
                //            } else if (hlt == null ||
                //                        string.IsNullOrEmpty(hlt.Trim()) ||
                //                        hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                //                ctvm.TileVisibility = Visibility.Visible;
                //                rtb.EndChange();
                //                //return;
                //            } else {
                //                TextRange lastSearchTextRange = null;
                //                for (TextPointer position = rtb.Document.ContentStart;
                //                 position != null && position.CompareTo(rtb.Document.ContentEnd) <= 0;
                //                 position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                //                    if (position.CompareTo(rtb.Document.ContentEnd) == 0) {
                //                        break;
                //                    }
                //                    string textRun = string.Empty;
                //                    int indexInRun = -1;
                //                    if (Properties.Settings.Default.IsSearchCaseSensitive) {
                //                        textRun = position.GetTextInRun(LogicalDirection.Forward);
                //                        indexInRun = textRun.IndexOf(hlt, StringComparison.CurrentCulture);
                //                    } else {
                //                        textRun = position.GetTextInRun(LogicalDirection.Forward).ToLower();
                //                        indexInRun = textRun.IndexOf(hlt.ToLower(), StringComparison.CurrentCulture);
                //                    }
                //                    if (indexInRun >= 0) {
                //                        position = position.GetPositionAtOffset(indexInRun);
                //                        if (position != null) {
                //                            TextPointer nextPointer = position.GetPositionAtOffset(hlt.Length);
                //                            lastSearchTextRange = new TextRange(position, nextPointer);
                //                            lastSearchTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString));
                //                        }
                //                    }
                //                }

                //                if (lastSearchTextRange != null) {
                //                    ctvm.TileVisibility = Visibility.Visible;
                //                    rtb.ScrollToHome();
                //                    rtb.CaretPosition = rtb.Document.ContentStart;
                //                    Rect r = lastSearchTextRange.End.GetCharacterRect(LogicalDirection.Backward);
                //                    rtb.ScrollToVerticalOffset(500);// VerticalOffset r.Y - (FontSize * 0.5));
                //                                                    //var characterRect = lastTokenPointer.GetCharacterRect(LogicalDirection.Forward);
                //                                                    //this.ScrollToHorizontalOffset(this.HorizontalOffset + characterRect.Left - this.ActualWidth / 2d);
                //                                                    //this.ScrollToVerticalOffset(this.VerticalOffset + characterRect.Top - this.ActualHeight / 2d);
                //                                                    //ScrollToEnd();
                //                } else {
                //                    ctvm.TileVisibility = Visibility.Collapsed;
                //                }
                //                rtb.EndChange();
                //            }
                //            rtb.ClearHyperlinks();
                //            rtb.CreateHyperlinks(hlt);
                //        }));
                //}
                PropertyChangedCallback = (s, e) => {
                    var cb = (MpClipBorder)s;
                    var ctvm = (MpClipTileViewModel)cb.DataContext;
                    if (ctvm.MainWindowViewModel.IsLoading) {
                        return;
                    }
                    
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        (Action)(() => {
                            var sttvm = ctvm.MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;
                            var hlt = (string)e.NewValue;

                            var ttb = (TextBlock)cb.FindName("ClipTileTitleTextBlock");
                            var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
                            var ctbb = Brushes.Transparent;// (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.ClipTileBackgroundColor);

                            Console.WriteLine("Beginning highlight clip with title: " + ctvm.CopyItemTitle + " with highlight text: "+(string)e.NewValue);
                            if (!sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                                Console.WriteLine("Clip tile w/ title " + ctvm.CopyItemTitle + " is not linked with current tag");
                                ctvm.TileVisibility = Visibility.Collapsed;
                                return;
                            }
                            bool isInTitle = ttb.Text.ContainsByCaseSetting(hlt);
                            bool isInContent = ctvm.ToString().ContainsByCaseSetting(hlt);
                            bool isSearchBlank = string.IsNullOrEmpty(hlt.Trim()) || hlt == Properties.Settings.Default.SearchPlaceHolderText;
                            ctvm.TileVisibility = isInTitle || isInContent || isSearchBlank ? Visibility.Visible : Visibility.Collapsed;
                            return;

                            ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, ctbb);
                            ctvm.LastTitleHighlightRangeList.Clear();

                            ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, ctbb);
                            ctvm.LastContentHighlightRangeList.Clear();

                            if (string.IsNullOrEmpty(hlt.Trim()) ||
                                hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                                //if search text is empty clear any highlights and show clip (if associated w/ current tag)
                                ctvm.TileVisibility = Visibility.Visible;
                                return;
                            }

                            //highlight title 
                            if (ttb.Text.ContainsByCaseSetting(hlt)) {
                                var ttr = MpHelpers.FindStringRangeFromPosition(ttb.ContentStart, hlt);
                                if (ttr != null) {
                                    ttr.ApplyPropertyValue(TextElement.BackgroundProperty, hb);
                                    ctvm.LastTitleHighlightRangeList.Add(ttr);
                                }
                            }
                            switch (ctvm.CopyItemType) {
                                case MpCopyItemType.RichText:
                                    var rtb = (RichTextBox)cb.FindName("ClipTileRichTextBox");
                                    //rtb.BeginChange();
                                    ctvm.LastContentHighlightRangeList = MpHelpers.FindAllStringRangesFromPosition(new TextRange(rtb.Document.ContentStart,rtb.Document.ContentEnd), hlt, Properties.Settings.Default.IsSearchCaseSensitive);
                                    if(ctvm.LastContentHighlightRangeList.Count > 0){
                                        ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, hb);
                                        rtb.CaretPosition = ctvm.LastContentHighlightRangeList[0].Start;
                                    } else if (ctvm.LastTitleHighlightRangeList.Count == 0) {
                                        ctvm.TileVisibility = Visibility.Collapsed;
                                    } 
                                    //rtb.EndChange();
                                    break;
                                case MpCopyItemType.Image:
                                    foreach (var diovm in ctvm.DetectedImageObjectViewModels) {
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

        private static void ApplyBackgroundBrushToRangeList(List<TextRange> rangeList, Brush bgBrush) {
            if(rangeList == null || rangeList.Count == 0) {
                return;
            }
            foreach(var range in rangeList) {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, bgBrush);
            }
        }
    }

}
