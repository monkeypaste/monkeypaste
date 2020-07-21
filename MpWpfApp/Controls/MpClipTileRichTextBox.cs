using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpClipTileRichTextBox : RichTextBox {
        public MpClipTileRichTextBox() : base() { }

        public String SearchText {
            get { 
                return (String)GetValue(SearchTextProperty); 
            }
            set { 
                SetValue(SearchTextProperty, value); 
            }
        }

        private static void OnDataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => {
                    if (e.OldValue != null) {
                        //OnDataChangedHelper((RichTextBox)source, (string)e.OldValue, Brushes.Transparent);
                    }
                    OnDataChangedHelper((RichTextBox)source, (string)e.NewValue,Brushes.Yellow);
                })
            );
        }
        private static void OnDataChangedHelper(RichTextBox rtb, string updatedSearchText,SolidColorBrush highlightColor) {
            //clear highlights
            //foreach(Block b in rtb.Document.Blocks) {
            //    Paragraph p = (Paragraph)b;
            //    p.Inlines.Clear();
            //}
            var fullDocRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            fullDocRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);

            
            if(updatedSearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                return;
            }
            string rtbt = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.ToLower();
            updatedSearchText = updatedSearchText.ToLower();
            //string regExStr = $@"\b" + updatedSearchText + @"\b";
            //MatchCollection mc = Regex.Matches(rtbt, $@"\b"+updatedSearchText+"\b", RegexOptions.IgnoreCase);
            var tokenIdxList = rtbt.AllIndexesOf(updatedSearchText);
            TextRange lastTokenRange = null;
            rtb.CaretPosition = rtb.Document.ContentStart;
            foreach (int idx in tokenIdxList) {
                TextPointer startPoint = lastTokenRange == null ? rtb.Document.ContentStart : lastTokenRange.End;
                var range = FindStringRangeFromPosition(startPoint, updatedSearchText);
                if(range == null) {
                    //i don't know why range is coming back null but to avoid the exception just returtn
                    Console.WriteLine("Can't find range for highlight: " + rtbt);
                    return;
                }
                range.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                lastTokenRange = range;
            }
        }

        public static TextRange FindStringRangeFromPosition(TextPointer position, string lowerCaseStr) {
            while (position != null) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text) {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward).ToLower();

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(lowerCaseStr);
                    if (indexInRun >= 0) {
                        return new TextRange(position.GetPositionAtOffset(indexInRun), position.GetPositionAtOffset(indexInRun + lowerCaseStr.Length));
                    }
                } 
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
            // position will be null if "word" is not found.
            return null;
        }
        public static readonly DependencyProperty SearchTextProperty = 
            DependencyProperty.Register("SearchText",
                                        typeof(string),
                                        typeof(MpClipTileRichTextBox),
                                        new FrameworkPropertyMetadata(null,OnDataChanged));
    }
}
