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
            get { return (String)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        private static void OnDataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => {
                    if (e.OldValue != null) {
                        OnDataChangedHelper((RichTextBox)source, (string)e.OldValue, Brushes.Transparent);
                    }
                    OnDataChangedHelper((RichTextBox)source, (string)e.NewValue,Brushes.Yellow);
                })
            );
        }
        private static void OnDataChangedHelper(RichTextBox rtb, string updatedSearchText,SolidColorBrush highlightColor) {
            
            if(highlightColor == Brushes.Transparent) {
                Console.WriteLine("Erase");
            } else {
                Console.WriteLine("Highlight");
            }
            string rtbt = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;

            string regExStr = $@"\b" + updatedSearchText + @"\b";
            MatchCollection mc = Regex.Matches(rtbt, $@"\b{Regex.Escape(updatedSearchText)}\b", RegexOptions.IgnoreCase);

            TextRange lastTokenRange = null;

            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        TextPointer startPoint = lastTokenRange == null ? rtb.Document.ContentStart : lastTokenRange.End;
                        var range = FindStringRangeFromPosition(startPoint, c.Value);
                        range.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                        lastTokenRange = range;
                    }
                }
            }
        }

        public static TextRange FindStringRangeFromPosition(TextPointer position, string str) {
            while (position != null) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text) {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(str);
                    if (indexInRun >= 0) {
                        return new TextRange(position.GetPositionAtOffset(indexInRun), position.GetPositionAtOffset(indexInRun + str.Length));
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
