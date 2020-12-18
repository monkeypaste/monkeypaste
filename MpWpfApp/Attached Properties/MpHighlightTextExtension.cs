using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpHighlightTextExtension : DependencyObject {
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
                    var ctvm = (MpClipTileViewModel)((FrameworkElement)s).DataContext;
                    if(ctvm.MainWindowViewModel.IsLoading) {
                        return;
                    }
                    //ctvm.TileVisibility = Visibility.Collapsed;
                    var sttvm = ctvm.MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;

                    var hlt = (string)e.NewValue;
                    if(s.GetType() == typeof(TextBlock)) {
                        var tb = (TextBlock)s; 
                        new TextRange(tb.ContentStart, tb.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                        if (sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem) && tb.Text.ToLower().Contains(hlt.ToLower())) {
                            ctvm.TileVisibility = Visibility.Visible;
                            var tr = MpHelpers.FindStringRangeFromPosition(tb.ContentStart, hlt);
                            if(tr != null) {
                                tr.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString));
                            }
                        }
                    }
                    else if (s.GetType() == typeof(RichTextBox) && ctvm.CopyItemType == MpCopyItemType.RichText) {
                        var rtb = (RichTextBox)s;
                        Dispatcher.CurrentDispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            (Action)(() => {                                
                                rtb.BeginChange();
                                new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                                
                                if (!sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                                    ctvm.TileVisibility = Visibility.Collapsed;
                                    rtb.EndChange();
                                } else if (hlt == null ||
                                            string.IsNullOrEmpty(hlt.Trim()) ||
                                            hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                                    ctvm.TileVisibility = Visibility.Visible;
                                    rtb.EndChange();
                                } else {
                                    TextRange lastSearchTextRange = null;
                                    for (TextPointer position = rtb.Document.ContentStart;
                                     position != null && position.CompareTo(rtb.Document.ContentEnd) <= 0;
                                     position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                                        if (position.CompareTo(rtb.Document.ContentEnd) == 0) {
                                            break;
                                        }
                                        string textRun = string.Empty;
                                        int indexInRun = -1;
                                        if (Properties.Settings.Default.IsSearchCaseSensitive) {
                                            textRun = position.GetTextInRun(LogicalDirection.Forward);
                                            indexInRun = textRun.IndexOf(hlt, StringComparison.CurrentCulture);
                                        } else {
                                            textRun = position.GetTextInRun(LogicalDirection.Forward).ToLower();
                                            indexInRun = textRun.IndexOf(hlt.ToLower(), StringComparison.CurrentCulture);
                                        }
                                        if (indexInRun >= 0) {
                                            position = position.GetPositionAtOffset(indexInRun);
                                            if (position != null) {
                                                TextPointer nextPointer = position.GetPositionAtOffset(hlt.Length);
                                                lastSearchTextRange = new TextRange(position, nextPointer);
                                                lastSearchTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString));
                                            }
                                        }
                                    }

                                    if (lastSearchTextRange != null) {
                                        ctvm.TileVisibility = Visibility.Visible;
                                        rtb.ScrollToHome();
                                        rtb.CaretPosition = rtb.Document.ContentStart;
                                        Rect r = lastSearchTextRange.End.GetCharacterRect(LogicalDirection.Backward);
                                        rtb.ScrollToVerticalOffset(500);// VerticalOffset r.Y - (FontSize * 0.5));
                                                                        //var characterRect = lastTokenPointer.GetCharacterRect(LogicalDirection.Forward);
                                                                        //this.ScrollToHorizontalOffset(this.HorizontalOffset + characterRect.Left - this.ActualWidth / 2d);
                                                                        //this.ScrollToVerticalOffset(this.VerticalOffset + characterRect.Top - this.ActualHeight / 2d);
                                                                        //ScrollToEnd();
                                } else {
                                        ctvm.TileVisibility = Visibility.Collapsed;
                                    }
                                    rtb.EndChange();
                                }
                                rtb.ClearHyperlinks();
                                rtb.CreateHyperlinks(hlt);
                            }));
                    } else if(s.GetType() == typeof(Image) && ctvm.CopyItemType == MpCopyItemType.Image) {
                        if (!sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                            return;
                        } else if (hlt == null ||
                                    string.IsNullOrEmpty(hlt.Trim()) ||
                                    hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                            ctvm.TileVisibility = Visibility.Visible;
                            return;
                        }
                        foreach(var diovm in ctvm.DetectedImageObjectViewModels) {
                            if(diovm.ObjectTypeName.ToLower().Contains(hlt.ToLower())) {
                                ctvm.TileVisibility = Visibility.Visible;
                                return;
                            }
                        }
                        ctvm.TileVisibility = Visibility.Collapsed;
                    } else if (s.GetType() == typeof(ListBox) && ctvm.CopyItemType == MpCopyItemType.FileList) {
                        if (!sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                            return;
                        } else if (hlt == null ||
                                    string.IsNullOrEmpty(hlt.Trim()) ||
                                    hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                            ctvm.TileVisibility = Visibility.Visible;
                            return;
                        }
                        foreach (var fivm in ctvm.FileListViewModels) {
                            if(fivm.ItemPath.ToLower().Contains(hlt.ToLower())) {
                                ctvm.TileVisibility = Visibility.Visible;
                                return;
                            }
                        }
                        ctvm.TileVisibility = Visibility.Collapsed;
                    }
                }
            });
    }
}
