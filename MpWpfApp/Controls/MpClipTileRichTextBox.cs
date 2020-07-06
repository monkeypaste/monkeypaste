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

        private static void OnSearchTextChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background, 
                new Action(() => { })//OnDataChangedHelper((RichTextBox)source,(string)e.NewValue))
            );
        }
        private static void OnDataChangedHelper(RichTextBox rtb, string updatedSearchText) {
            //if(rtb.Visibility == Visibility.Collapsed) {
            //    return;
            //}
            string rtbt = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;

            if (rtbt.Length == 0 || updatedSearchText.Length == 0) {
                return;
            }
            string regExStr = @"\b" + Regex.Escape(updatedSearchText) + @"\b";
            MatchCollection mc = Regex.Matches(rtbt, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            TextRange lastTokenRange = null;
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        TextPointer startPoint = lastTokenRange == null ? rtb.Document.ContentStart : lastTokenRange.End;
                        var range = FindStringRangeFromPosition(startPoint, c.Value);
                        range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Aquamarine));
                        lastTokenRange = range;
                    }
                }
            }
            //string textUpper = rtbt.ToUpper();
            //string toFind = updatedSearchText.ToUpper();
            //int firstIndex = textUpper.IndexOf(toFind);
            //if (firstIndex < 0) {
            //    return;
            //}
            //string firstStr = rtbt.Substring(0, firstIndex);
            //string foundStr = rtbt.Substring(firstIndex, toFind.Length);
            //string endStr = rtbt.Substring(firstIndex + toFind.Length, rtbt.Length - (firstIndex + toFind.Length));

            //rtb.Inlines.Clear();
            //var run = new Run();
            //run.Text = firstStr;
            //rtb.Inlines.Add(run);
            //run = new Run();
            //run.Background = Brushes.Yellow;
            //run.Text = foundStr;
            //rtb.Inlines.Add(run);
            //run = new Run();
            //run.Text = endStr;

            //rtb.Inlines.Add(run);

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
                                        new FrameworkPropertyMetadata(null,OnSearchTextChanged));
    }
}
