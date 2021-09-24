using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RtfTest {
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window {
        public ObservableCollection<RichTextBox> Rtfs { get; set; }

        public MainWindow2() {
            InitializeComponent();
            new TextRange(rtf1.Document.ContentStart, rtf1.Document.ContentEnd).Text = "HELLO!";
        }

        public FlowDocument ConvertFromRtf(string rtf) {
            using (var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(rtf))) {
                try {
                    var flowDocument = new FlowDocument();
                    var range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    range.Load(stream, System.Windows.DataFormats.Rtf);

                    var tr = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                    if (rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
                        //ignore to r
                    } else if ((TextAlignment)rtbAlignment == TextAlignment.Justify) {
                        tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                    }
                    //var ps = flowDocument.GetDocumentSize();
                    //flowDocument.PageWidth = ps.Width;
                    //flowDocument.PageHeight = ps.Height;
                    return flowDocument;
                }
                catch (Exception ex) {
                    Console.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...");
                    Console.WriteLine("Exception Details: " + ex);
                    return null;//.ToPlainText().ToFlowDocument();
                }
            }
        }

        public string ConvertFromFlowDocument(FlowDocument fd) {
            string rtf = string.Empty;
            using (var ms = new MemoryStream()) {
                var range2 = new TextRange(fd.ContentStart, fd.ContentEnd);
                range2.Save(ms, System.Windows.DataFormats.Rtf);
                ms.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(ms)) {
                    rtf = sr.ReadToEnd();
                }
            }
            return rtf;
        }


        public string Diff(string str1, string str2) {
            if (str1 == null) {
                return str2;
            }
            if (str2 == null) {
                return str1;
            }

            List<string> set1 = str1.Split(' ').Distinct().ToList();
            List<string> set2 = str2.Split(' ').Distinct().ToList();

            var diff = set2.Count() > set1.Count() ? set2.Except(set1).ToList() : set1.Except(set2).ToList();

            return string.Join("", diff);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var cdo = Clipboard.GetDataObject();
            if (cdo.GetFormats().Contains(DataFormats.Rtf)) {
                var cbot = cdo.GetData(DataFormats.Rtf) as string;
                new TextRange(rtf1.Document.ContentStart, rtf1.Document.ContentEnd).Text = cbot;

                string old_rtf = ConvertFromFlowDocument(rtf2.Document);
                rtf2.Document = ConvertFromRtf(cbot);

                string new_rtf = ConvertFromFlowDocument(rtf2.Document);

                var rtb = new RichTextBox();
                rtb.IsReadOnly = true;
                new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text = new_rtf;//Diff(old_rtf, new_rtf);
                rtb.Document.PagePadding = new Thickness(0);
                rtb.Width = Width / 3 - 10;
                rtb.Document.PageWidth = Width / 3 - 20;
                rtb.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                rtf3_lb.Items.Insert(0,rtb);
                rtf3_lb.Height += rtb.Document.PageHeight;

                rtb.PreviewMouseDown += Rtb_MouseUp;
            }
        }

        private void Rtb_MouseUp(object sender, MouseButtonEventArgs e) {
            var rtb = sender as RichTextBox;
            rtb.SelectAll();
            Clipboard.SetData(DataFormats.Rtf, ConvertFromFlowDocument(rtb.Document));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            rtf3_lb.Items.Clear();
        }
    }

    public static class Ext {
        public static Size GetDocumentSize(this FlowDocument doc) {
            var ft = doc.GetFormattedText();
            return new Size(ft.Width, ft.Height);
            //var tp = doc.ContentStart;
            //Rect lastRect = tp.GetCharacterRect(LogicalDirection.Forward);
            //double w = lastRect.Width, h = lastRect.Height, y = lastRect.Y;
            //while(tp != null && tp.CompareTo(doc.ContentEnd) < 0) {
            //    tp = tp.GetPositionAtOffset(1, LogicalDirection.Forward);
            //    Rect newRect = tp.GetCharacterRect(LogicalDirection.Forward);
            //    if(newRect.Y > lastRect.Y) {
            //        //w = Math.Max(w, newRect.X);
            //        h += newRect.Y;// Math.Max(h, newRect.Y);
            //    }
            //    lastRect = newRect;
            //}
            //return new Size(w, h);
        }

        public static FormattedText GetFormattedText(this FlowDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            var output = new FormattedText(
              GetText(doc),
              CultureInfo.CurrentCulture,
              doc.FlowDirection,
              new Typeface(doc.FontFamily, doc.FontStyle, doc.FontWeight, doc.FontStretch),
              doc.FontSize,
              doc.Foreground,
              new NumberSubstitution(),
              VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

            int offset = 0;
            var runsAndParagraphsList = doc.GetRunsAndParagraphs().ToList();
            for (int i = 0; i < runsAndParagraphsList.Count; i++) {
                TextElement el = runsAndParagraphsList[i];
                Run run = el as Run;

                if (run != null) {
                    int count = run.Text.Length;

                    output.SetFontFamily(run.FontFamily, offset, count);
                    output.SetFontStyle(run.FontStyle, offset, count);
                    output.SetFontWeight(run.FontWeight, offset, count);
                    output.SetFontSize(run.FontSize, offset, count);
                    output.SetForegroundBrush(run.Foreground, offset, count);
                    output.SetFontStretch(run.FontStretch, offset, count);
                    output.SetTextDecorations(run.TextDecorations, offset, count);

                    offset += count;
                } else {
                    offset += Environment.NewLine.Length;
                }
            }

            return output;
        }

        public static string GetText(FlowDocument doc) {
            StringBuilder sb = new StringBuilder();

            foreach (TextElement el in GetRunsAndParagraphs(doc)) {
                Run run = el as Run;
                sb.Append(run == null ? Environment.NewLine : run.Text);
            }
            return sb.ToString();
        }

        public static IEnumerable<TextElement> GetRunsAndParagraphs(this FlowDocument doc) {
            for (TextPointer position = doc.ContentStart;
              position != null && position.CompareTo(doc.ContentEnd) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    Run run = position.Parent as Run;

                    if (run != null) {
                        yield return run;
                    } else {
                        Paragraph para = position.Parent as Paragraph;

                        if (para != null) {
                            yield return para;
                        }
                    }
                }
            }
        }
    }
}
