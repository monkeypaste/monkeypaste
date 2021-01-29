using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;
using Alturos.Yolo;
using Newtonsoft.Json;
using QRCoder;
using static MpWpfApp.MpShellEx;
using static QRCoder.PayloadGenerator;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using CsvHelper;

namespace MpWpfApp {
    public class MpHelpers {
        private static readonly Lazy<MpHelpers> _Lazy = new Lazy<MpHelpers>(() => new MpHelpers());
        public static MpHelpers Instance { get { return _Lazy.Value; } }
        
        public void Init() {
            //empty to init singleton
        }

        #region Documents
        public Hyperlink CreateTemplateHyperlink(MpTemplateHyperlinkViewModel thlvm, TextRange tr) {
            var ctvm = thlvm.ClipTileViewModel;
                        
            //if the range for the template contains a sub-selection of a hyperlink the hyperlink(s)
            //needs to be broken into their text before the template hyperlink can be created
            var trSHl = (Hyperlink)MpHelpers.Instance.FindParentOfType(tr.Start.Parent, typeof(Hyperlink));            
            var trEHl = (Hyperlink)MpHelpers.Instance.FindParentOfType(tr.End.Parent, typeof(Hyperlink));
            var trText = tr.Text;

            if (trSHl != null) {
                var linkText = new TextRange(trSHl.ElementStart, trSHl.ElementEnd).Text;
                trSHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trSHl.ElementStart);
                tr = FindStringRangeFromPosition(span.ContentStart, trText, true);
            }
            if (trEHl != null && trEHl != trSHl) {
                var linkText = new TextRange(trEHl.ElementStart, trEHl.ElementEnd).Text;
                trEHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trEHl.ElementStart);
                tr = FindStringRangeFromPosition(span.ContentStart, trText, true);
            }
            thlvm.TemplateTextRange = tr;
            //var r = new Run();
            //r.Loaded += thlvm.TemplateHyperLinkRun_Loaded;

            var tb = new TextBlock();
            tb.DataContext = thlvm;
            tb.Loaded += thlvm.TemplateHyperLinkRun_Loaded;

            var hl = new Hyperlink(tr.Start, tr.End);
            hl.DataContext = thlvm;
            hl.Inlines.Clear();
            hl.Inlines.Add(tb);
            thlvm.TemplateHyperlink = hl;
            return hl;
        }

        public void ApplyBackgroundBrushToRangeList(ObservableCollection<TextRange> rangeList, Brush bgBrush) {
            if (rangeList == null || rangeList.Count == 0) {
                return;
            }
            foreach (var range in rangeList) {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, bgBrush);
            }
        }

        public DependencyObject FindParentOfType(DependencyObject dpo, Type type) {
            if (dpo == null) {
                return null;
            }
            if (dpo.GetType() == type) {
                return dpo;
            }
            if(dpo.GetType().IsSubclassOf(typeof(FrameworkContentElement))) {
                return FindParentOfType(((FrameworkContentElement)dpo).Parent, type);
            } else if (dpo.GetType().IsSubclassOf(typeof(FrameworkElement))) {
                return FindParentOfType(((FrameworkElement)dpo).Parent, type);
            } else {
                return null;
            }
        }
        //public List<TextRange> FindStringRangesFromPosition(TextPointer position, string matchStr, bool isCaseSensitive = false) {
        //    var matchRangeList = new List<TextRange>();
        //    TextPointer lastDocPosition = null; 
        //    int curIdx = 0;
        //    TextPointer rootPosition = position;
        //    TextPointer lastMatchParentEndPosition = null;
        //    TextPointer startPointer = null;
        //    StringComparison stringComparison = isCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        //    while (position != null) {
        //        while(position != null && 
        //              position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text) {
        //            position = position.GetPositionAtOffset(1, LogicalDirection.Forward);
        //        }
        //        var runStr = position.GetTextInRun(LogicalDirection.Forward);
        //        if (string.IsNullOrEmpty(runStr)) {
        //            position = position.GetPositionAtOffset(1, LogicalDirection.Forward);
        //            continue;
        //        }
        //        //only concerned with current character of match string
        //        int runIdx = runStr.IndexOf(matchStr[curIdx].ToString(), stringComparison);
        //        if (runIdx == -1) {
        //            //if no match found reset search
        //            curIdx = 0;
        //            if (startPointer == null) {
        //                position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
        //            } else {
        //                //when no match somewhere after first character reset search to the position AFTER beginning of last partial match
        //                position = startPointer.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
        //            }
        //            continue;
        //        }
        //        if (curIdx == 0) {
        //            //beginning of range found at runIdx
        //            startPointer = position.GetPositionAtOffset(runIdx, LogicalDirection.Forward);
        //        }
        //        if (curIdx == matchStr.Length - 1) {
        //            //each character has been matched
        //            var endPointer = position.GetPositionAtOffset(runIdx, LogicalDirection.Forward);
        //            //for edge cases of repeating characters these loops ensure start is not early and last character isn't lost 
        //            if (isCaseSensitive) {
        //                while (endPointer != null && !new TextRange(startPointer, endPointer).Text.Contains(matchStr)) {
        //                    endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
        //                }
        //            } else {
        //                while (endPointer != null && !new TextRange(startPointer, endPointer).Text.ToLower().Contains(matchStr.ToLower())) {
        //                    endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
        //                }
        //            }
        //            if (endPointer == null) {
        //                curIdx = 0;
        //                startPointer = null;
        //                position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
        //                continue;
        //            }
        //            while (startPointer != null && new TextRange(startPointer, endPointer).Text.Length > matchStr.Length) {
        //                startPointer = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
        //            }
        //            if (startPointer == null) {
        //                curIdx = 0;
        //                startPointer = null;
        //                position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
        //                continue;
        //            }
        //            matchRangeList.Add(new TextRange(startPointer, endPointer));
        //            position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
        //        } else {
        //            //prepare loop for next match character
        //            curIdx++;
        //            //iterate position one offset AFTER match offset
        //            if (rootPosition.IsInSameDocument(position)) {
        //                position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
        //            } else {
        //                var nextPosition = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
        //                if (nextPosition == null) {
        //                    //this occurs when a uielement match was found at the end of its textblock and
        //                    //position needs to iterate passed the uielement
        //                    position = ((Hyperlink)FindParentOfType(position.Parent, typeof(Hyperlink))).ElementEnd;
        //                } else {
        //                    position = nextPosition;
        //                }
        //            }
        //        }
        //        var hlr = FindStringRangeFromPosition(position, matchStr, isCaseSensitive);
        //        if (hlr == null) {
        //            //this occurs when no more matchStr's are found AND
        //            //more importantly if the last match was within a inlineuicontainer textblock
        //            if (lastDocPosition != null && matchRangeList.Count > 0) {
        //                //this occurs when the match is outside the document
        //                var lastMatch = matchRangeList[matchRangeList.Count - 1];
        //                //find inlineuielement parent to return to document
        //                var phl = FindParentOfType(lastMatch.End.Parent, typeof(Hyperlink));
        //                if (phl != null) {
        //                    //if parent found move position to its big pretty tail
        //                    position = ((Hyperlink)phl).ElementEnd;
        //                    //clear lastdocposition to terminate matching 
        //                    lastDocPosition = null;
        //                    continue;
        //                }
        //            }
        //            break;
        //        } else {
        //            matchRangeList.Add(hlr);
        //            if (!position.IsInSameDocument(hlr.End)) {
        //                lastDocPosition = position;
        //            }
        //            position = hlr.End;

        //        }
        //    }
        //    return matchRangeList;
        //}

        public List<TextRange> FindStringRangesFromPosition(TextPointer position, string matchStr, bool isCaseSensitive = false) {
            var orgPosition = position;
            TextPointer nextDocPosition = null;
            var matchRangeList = new List<TextRange>();
            while (position != null) {
                var hlr = FindStringRangeFromPosition(position, matchStr, isCaseSensitive);
                if (hlr == null) {
                    if(nextDocPosition != null) {
                        position = nextDocPosition;
                        nextDocPosition = null;
                        continue;
                    }
                    break;
                } else {
                    matchRangeList.Add(hlr);
                    if(!hlr.End.IsInSameDocument(orgPosition)) {
                        var phl = (Hyperlink)FindParentOfType(hlr.End.Parent, typeof(Hyperlink));
                        nextDocPosition = phl.ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                    }
                    position = hlr.End;
                }
            }
            return matchRangeList;
        }

        public TextRange FindStringRangeFromPosition(TextPointer position, string matchStr, bool isCaseSensitive = false) {
            int curIdx = 0;
            TextPointer postOfUiElement = null;
            TextPointer startPointer = null;
            StringComparison stringComparison = isCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            while (position != null || postOfUiElement != null) {
                if(position == null) {
                    position = postOfUiElement;
                    postOfUiElement = null;
                }
                if (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text) {
                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement) {
                        var tb = (position.Parent as InlineUIContainer).Child as TextBlock;
                        postOfUiElement = ((position.Parent as InlineUIContainer).Parent as Hyperlink).ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                        position = tb.ContentStart;
                        continue;
                        //var highlightRange = MpHelpers.Instance.FindStringRangeFromPosition(tb.ContentStart, matchStr);
                        //if (highlightRange != null) {
                        //    return highlightRange;
                        //}
                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                    continue;
                }
                var runStr = position.GetTextInRun(LogicalDirection.Forward);
                if (string.IsNullOrEmpty(runStr)) {
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                    continue;
                }
                //only concerned with current character of match string
                int runIdx = runStr.IndexOf(matchStr[curIdx].ToString(), stringComparison);
                if (runIdx == -1) {
                    //if no match found reset search
                    curIdx = 0;
                    if (startPointer == null) {
                        position = position.GetNextContextPosition(LogicalDirection.Forward);
                    } else {
                        //when no match somewhere after first character reset search to the position AFTER beginning of last partial match
                        position = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        startPointer = null;
                    }
                    continue;
                }
                if (curIdx == 0) {
                    //beginning of range found at runIdx
                    startPointer = position.GetPositionAtOffset(runIdx, LogicalDirection.Forward);
                }
                if (curIdx == matchStr.Length - 1) {
                    //each character has been matched
                    var endPointer = position.GetPositionAtOffset(runIdx, LogicalDirection.Forward);
                    //for edge cases of repeating characters these loops ensure start is not early and last character isn't lost 
                    if (isCaseSensitive) {
                        while (endPointer != null && !new TextRange(startPointer, endPointer).Text.Contains(matchStr)) {
                            endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        }
                    } else {
                        while (endPointer != null && !new TextRange(startPointer, endPointer).Text.ToLower().Contains(matchStr.ToLower())) {
                            endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        }
                    }
                    if (endPointer == null) {
                        return null;
                    }
                    while (startPointer != null && new TextRange(startPointer, endPointer).Text.Length > matchStr.Length) {
                        startPointer = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                    }
                    if (startPointer == null) {
                        return null;
                    }
                    return new TextRange(startPointer, endPointer);
                } else {
                    //prepare loop for next match character
                    curIdx++;
                    //iterate position one offset AFTER match offset
                    position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
                }
            }
            return null;
        }
        /* The idea is to find the offset of the first character (IndexOf) and 
         * then to find the TextPointer at this index (but by counting only text characters).
         * 
         * Good solution, but there is a minor problem. GetTextRunLength does not consider \r and \n characters. 
         * If you have those in searchRange.Text then the resulting TextRange will be ahead of the correct 
         * position by the number of new line characters*/

        public TextRange FindTextInRange(TextRange searchRange, string searchText) {
            int offset = searchRange.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (offset < 0)
                return null;  // Not found

            var start = GetTextPositionAtOffset(searchRange.Start, offset);
            TextRange result = new TextRange(start, GetTextPositionAtOffset(start, searchText.Length));

            return result;
        }

        public TextPointer GetTextPositionAtOffset(TextPointer position, int characterCount) {
            while (position != null) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text) {
                    int count = position.GetTextRunLength(LogicalDirection.Forward);
                    if (characterCount <= count) {
                        return position.GetPositionAtOffset(characterCount);
                    }

                    characterCount -= count;
                }

                TextPointer nextContextPosition = position.GetNextContextPosition(LogicalDirection.Forward);
                if (nextContextPosition == null)
                    return position;

                position = nextContextPosition;
            }

            return position;
        }

        //public TextRange FindStringRangeFromPosition(TextPointer position, string str, bool isCaseSensitive = false) {
        //    while (position != null) {
        //        var dir = LogicalDirection.Forward;
        //        if (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text) {
        //            dir = LogicalDirection.Backward;
        //        }
        //        string textRun = isCaseSensitive ? position.GetTextInRun(dir) : position.GetTextInRun(dir).ToLower();

        //        // Find the starting index of any substring that matches "str".
        //        int indexInRun = textRun.IndexOf(isCaseSensitive ? str : str.ToLower());
        //        if (indexInRun >= 0) {
        //            if (dir == LogicalDirection.Forward) {
        //                return new TextRange(position.GetPositionAtOffset(indexInRun), position.GetPositionAtOffset(indexInRun + str.Length));
        //            } else {
        //                return new TextRange(position.GetPositionAtOffset(indexInRun), position.GetPositionAtOffset(indexInRun - str.Length));
        //            }
        //        }
        //        position = position.GetNextContextPosition(dir);
        //    }
        //    // position will be null if "word" is not found.
        //    return null;
        //}

        public TextRange FindStringRangeFromPosition2(TextPointer position, string str, bool isCaseSensitive = false)             {
            for (;
             position != null;
             position = position.GetNextContextPosition(LogicalDirection.Forward)) {                
                string textRun = string.Empty;
                int indexInRun = -1;
                if (isCaseSensitive) {
                    textRun = position.GetTextInRun(LogicalDirection.Forward);
                    indexInRun = textRun.IndexOf(str, StringComparison.CurrentCulture);
                } else {
                    textRun = position.GetTextInRun(LogicalDirection.Forward).ToLower();
                    indexInRun = textRun.IndexOf(str.ToLower(), StringComparison.CurrentCulture);
                }
                if (indexInRun >= 0) {
                    position = position.GetPositionAtOffset(indexInRun);
                    if (position != null) {
                        TextPointer nextPointer = position.GetPositionAtOffset(str.Length);
                        return new TextRange(position, nextPointer);
                        //lastSearchTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString));
                    }
                }
            }
            return null;
        }

        public string PlainTextToRtf2(string input) {
            //first take care of special RTF chars
            StringBuilder backslashed = new StringBuilder(input);
            backslashed.Replace(@"\", @"\\");
            backslashed.Replace(@"{", @"\{");
            backslashed.Replace(@"}", @"\}");

            // then convert the string char by char
            StringBuilder sb = new StringBuilder();
            foreach (char character in backslashed.ToString()) {
                if (character <= 0x7f) {
                    sb.Append(character);
                } else {
                    sb.Append("\\u" + Convert.ToUInt32(character) + "?");
                }
            }
            return sb.ToString();
        }

        public bool IsStringRichText(string text) {
            if(string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"{\rtf");
        }

        public bool IsStringXaml(string text) {
            return text.StartsWith(@"<Section xmlns=") || text.StartsWith(@"<Span xmlns=");
        }

        public bool IsStringSpan(string text) {
            return text.StartsWith(@"<Span xmlns=");
        }

        public bool IsStringSection(string text) {
            return text.StartsWith(@"<Section xmlns=");
        }

        public System.Drawing.Font GetRichTextFont(string rt) {
            using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                rtb.Rtf = rt;
                return rtb.Font;
            }
        }

        public System.Drawing.Size GetRichTextFontSize(string rt) {
            string pt = ConvertRichTextToPlainText(rt);
            using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                rtb.Rtf = rt;
                rtb.SelectAll();
                return System.Windows.Forms.TextRenderer.MeasureText(pt, rtb.SelectionFont);
            }
        }

        public string CombineRichText(string rt1, string rt2,bool insertNewLine = false) {
            using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                rtb.Rtf = rt1;
                if(insertNewLine) {
                    rtb.Text += Environment.NewLine;
                }
                rtb.Select(rtb.TextLength, 0);
                rtb.SelectedRtf = rt2;
                return rtb.Rtf;
            }
        }        

        public CurrencyType GetCurrencyTypeFromString(string moneyStr) {
            if (moneyStr == null || moneyStr.Length == 0) {
                return CurrencyType.USD;
            }
            char currencyLet = moneyStr[0];
            foreach(var c in MpCurrencyConverter.Instance.CurrencyList) {
                 if(c.CurrencySymbol == currencyLet.ToString()) {
                    Enum.TryParse(c.Id, out CurrencyType ct);
                    return ct;
                }
            }
            return CurrencyType.USD;
        }

        public double GetCurrencyValueFromString(string moneyStr) {
            if(string.IsNullOrEmpty(moneyStr) || moneyStr.Length < 2) {
                return 0;
            }
            moneyStr = moneyStr.Remove(0, 1);
            try {
                return Math.Round(Convert.ToDouble(moneyStr), 2);
            }
            catch (Exception ex) {
                Console.WriteLine("MpHelper exception cannot convert moneyStr '" + moneyStr + "' to a value, returning 0");
                return 0;
            }
        }

        public int GetColCount(string text) {
            int maxCols = int.MinValue;
            foreach (string row in text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                if (row.Length > maxCols) {
                    maxCols = row.Length;
                }
            }
            return maxCols;
        }

        public int GetRowCount(string text) {
            if(string.IsNullOrEmpty(text)) {
                return 0;
            }
            if(IsStringRichText(text)) {
                int nlCount = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                int parCount = text.Split(new string[] { @"\par" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                if(nlCount + parCount == 0) {
                    return 1;
                }
                return nlCount + parCount;
            }
            return text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length;
        }

        public string GetRandomString(int charsPerLine = 32, int lines = 1) {
            StringBuilder str_build = new StringBuilder();

            for (int i = 0; i < lines; i++) {
                for (int j = 0; j < charsPerLine; j++) {
                    double flt = Rand.NextDouble();
                    int shift = Convert.ToInt32(Math.Floor(25 * flt));
                    char letter = Convert.ToChar(shift + 65);
                    str_build.Append(letter);
                }
                if (i + 1 < lines) {
                    str_build.Append('\n');
                }
            }
            return str_build.ToString();
        }

        public string RemoveSpecialCharacters(string str) {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", string.Empty, RegexOptions.Compiled);
        }

        //public string CombineRichText(string from, string to) {
        //    return ConvertFlowDocumentToRichText(
        //        CombineFlowDocuments(
        //            ConvertRichTextToFlowDocument(from),
        //            ConvertRichTextToFlowDocument(to)
        //        )
        //    );
        //}

        public MpEventEnabledFlowDocument CombineFlowDocuments(MpEventEnabledFlowDocument from, MpEventEnabledFlowDocument to) {
            using (MemoryStream stream = new MemoryStream()) {
                TextRange range = new TextRange(from.ContentStart, from.ContentEnd);

                System.Windows.Markup.XamlWriter.Save(range, stream);
                range.Save(stream, DataFormats.XamlPackage);

                //below removed so meerging follows items characters, no extra formatting
                //LineBreak lb = new LineBreak();
                //Paragraph p = (Paragraph)to.Blocks.LastBlock;
                //p.LineHeight = 1;
                //p.Inlines.Add(lb);
                TextRange range2 = new TextRange(to.ContentEnd, to.ContentEnd);
                range2.Load(stream, DataFormats.XamlPackage);

                return to;
            }            
        }

        public string CurrencyConvert(decimal amount, string fromCurrency, string toCurrency) {
            try {
                //Grab your values and build your Web Request to the API
                string apiURL = String.Format("https://www.google.com/finance/converter?a={0}&from={1}&to={2}&meta={3}", amount, fromCurrency, toCurrency, Guid.NewGuid().ToString());

                //Make your Web Request and grab the results
                var request = WebRequest.Create(apiURL);

                //Get the Response
                var streamReader = new StreamReader(request.GetResponse().GetResponseStream(), System.Text.Encoding.ASCII);

                //Grab your converted value (ie 2.45 USD)
                var result = Regex.Matches(streamReader.ReadToEnd(), "<span class=\"?bld\"?>([^<]+)</span>")[0].Groups[1].Value;

                //Get the Result
                return result;
            }
            catch (Exception ex) {
                Console.WriteLine("MpHelpers Currency Conversion exception: " + ex.ToString());
                return string.Empty;
            }
        }

        public void AppendBitmapSourceToFlowDocument(FlowDocument flowDocument, BitmapSource bitmapSource) {
            Image image = new Image() {
                Source = bitmapSource,
                Width = 300,
                Height = 300,
                Stretch = Stretch.Fill
            };
            Paragraph para = new Paragraph();
            para.Inlines.Add(image);
            flowDocument.Blocks.Add(para);
        }

        #endregion

        #region System
        public double ConvertBytesToMegabytes(long bytes, int precision = 2) {
            return Math.Round((bytes / 1024f) / 1024f,precision);
        }

        public void CreateBinding(
            object source, 
            PropertyPath sourceProperty, 
            DependencyObject target, 
            DependencyProperty targetProperty, 
            BindingMode mode = BindingMode.OneWay) {
            Binding b = new Binding();
            b.Source = source;
            b.Path = sourceProperty;
            b.Mode = mode;
            if(b.Mode == BindingMode.TwoWay) {
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }
            BindingOperations.SetBinding(target, targetProperty, b);
        }

        public Random Rand { get; set; } = new Random();

        public bool IsInDesignMode {
            get {
                return DesignerProperties.GetIsInDesignMode(new DependencyObject());
            }
        }

        public bool ApplicationIsActivated() {
            var activatedHandle = WinApi.GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) {
                return false;       // No window is currently activated
            }
            var procId = Process.GetCurrentProcess().Id;
            WinApi.GetWindowThreadProcessId(activatedHandle, out uint activeProcId);

            return (int)activeProcId == procId;
        }

        public void OpenUrl(string url, bool openInNewWindow = true) {
            if(url.StartsWith(@"http") && !openInNewWindow) {
                //WinApi.SetActiveWindow()
            } else {
                Process.Start(url);
            }
        }
        public string CreateEmail(string fromAddress, string subject, object body, string attachmentPath = "") {
            //this returns the .eml file that will need to be deleted
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromAddress);
            mailMessage.Subject = "Your subject here";
            if(body.GetType() == typeof(BitmapSource)) {
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = string.Format("<img src='{0}'>", MpHelpers.Instance.WriteBitmapSourceToFile(Path.GetTempPath(),(BitmapSource)body), true);
            } else {
                mailMessage.Body = (string)body;
            }            

            if(!string.IsNullOrEmpty(attachmentPath)) {
                mailMessage.Attachments.Add(new Attachment(attachmentPath));
            }

            var filename = Path.GetTempPath() + "mymessage.eml";

            //save the MailMessage to the filesystem
            mailMessage.Save(filename);

            //Open the file with the default associated application registered on the local machine
            Process.Start(filename);
            return filename;
        }
        
        public long FileListSize(string[] paths) {
            long total = 0;
            foreach (string path in paths) {
                if (Directory.Exists(path)) {
                    total += CalcDirSize(path, true);
                } else if (File.Exists(path)) {
                    total += new FileInfo(path).Length;
                }
            }
            return total;
        }

        public string GetUniqueFileName(string fullPath) {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath)) {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }

        public string ReadTextFromFile(string filePath) {
            using (StreamReader f = new StreamReader(filePath)) {
                string outStr = string.Empty;
                outStr = f.ReadToEnd();
                f.Close();
                return outStr;
            }
        }

        public string WriteTextToFile(string filePath, string text, bool isTemporary = false) {
            using (StreamWriter of = new StreamWriter(filePath)) {
                of.Write(text);
                of.Close();
                return filePath;
            }
        }

        public string WriteBitmapSourceToFile(string filePath, BitmapSource bmpSrc, bool isTemporary = false) {
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(MpHelpers.Instance.ConvertBitmapSourceToBitmap(bmpSrc))) {
                bmp.Save(filePath, ImageFormat.Png);
                return filePath;
            }
        }

        public string WriteStringListToCsvFile(string filePath, IList<string> strList, bool isTemporary = false) {
            var textList = new List<string>();
            foreach (var str in strList) {
                if (!string.IsNullOrEmpty(str.Trim())) {
                    textList.Add(str);
                }
            }
            using (var writer = new StreamWriter(filePath)) {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
                    csv.WriteRecords(textList);
                }
            }
            return filePath;
            //using (var stream = File.OpenRead(filePath)) {
            //    using (var reader = new StreamReader(stream)) {
            //        return reader.ReadToEnd();
            //    }
            //}
        }

        /* public long DirSize(string sourceDir,bool recurse) {
             long size = 0;
             string[] fileEntries = Directory.GetFiles(sourceDir);

             foreach(string fileName in fileEntries) {
                 Interlocked.Add(ref size,(new FileInfo(fileName)).Length);
             }

             if(recurse) {
                 string[] subdirEntries = Directory.GetDirectories(sourceDir);

                 Parallel.For<long>(0,subdirEntries.Length,() => 0,(i,loop,subtotal) =>
                 {
                     if((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint) {
                         subtotal += DirSize(subdirEntries[i],true);
                         return subtotal;
                     }
                     return 0;
                 },
                     (x) => Interlocked.Add(ref size,x)
                 );
             }
             return size;
         }*/

        /*public string GeneratePassword() {
            var generator = new MpPasswordGenerator(minimumLengthPassword: 8,
                                      maximumLengthPassword: 12,
                                      minimumUpperCaseChars: 2,
                                      minimumSpecialChars: 2);
            return generator.Generate();
        }*/

        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files) {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public string GetCPUInfo() {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc) {
                if (string.IsNullOrEmpty(cpuInfo)) {
                    //Get only the first CPU's ID
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return cpuInfo;
        }

        public string GetShortcutTargetPath(string file) {
            try {
                if (System.IO.Path.GetExtension(file).ToLower() != ".lnk") {
                    throw new Exception("Supplied file must be a .LNK file");
                }

                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream)) {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1) {                      // Bit 1 set means we have to
                                                                 // skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                                                                 // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                                                               // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                                                                                        // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                                                                                                        // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    var link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0");
                    if (begin > -1) {
                        int end = link.IndexOf("\\\\", begin + 2) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    } else {
                        return link;
                    }
                }
            }
            catch {
                return string.Empty;
            }
        }

        //exampe RunWithString(@"C:\windows\system32\cmd.exe",
        public void RunInShell(string args, bool asAdministrator, bool isSilent, IntPtr forceHandle) {
            System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo();
            processInfo.FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe"; //Sets the FileName property of myProcessInfo to %SystemRoot%\System32\cmd.exe where %SystemRoot% is a system variable which is expanded using Environment.ExpandEnvironmentVariables
            processInfo.Arguments = "/K " + args;
            processInfo.WindowStyle = isSilent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal; //Sets the WindowStyle of myProcessInfo which indicates the window state to use when the process is started to Hidden
            processInfo.Verb = asAdministrator ? "runas" : string.Empty; //The process should start with elevated permissions
            System.Diagnostics.Process.Start(processInfo); //Starts the process based on myProcessInfo
        }
        
        public string GetApplicationDirectory() {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public string GetProcessApplicationName(IntPtr hWnd) {
            string mwt = GetProcessMainWindowTitle(hWnd);
            if (string.IsNullOrEmpty(mwt)) {
                return string.Empty;
            }
            var mwta = mwt.Split('-');
            if (mwta.Length == 1) {
                return "Explorer";
            }
            return mwta[mwta.Length - 1].Trim();
        }

        public string GetProcessPath(IntPtr hwnd) { 
            try {
                if (hwnd == null || hwnd == IntPtr.Zero) {
                    return string.Empty;
                    //return GetProcessPath(((MpClipTrayViewModel)((MpMainWindowViewModel)((MpMainWindow)App.Current.MainWindow).DataContext).ClipTrayViewModel).ClipboardManager.LastWindowWatcher.ThisAppHandle);
                }
                //if (hwnd == IntPtr.Zero) {
                //    return GetProcessPath(((MpClipTrayViewModel)((MpMainWindowViewModel)((MpMainWindow)App.Current.MainWindow).DataContext).ClipTrayViewModel).ClipboardManager.LastWindowWatcher.ThisAppHandle);
                //}
                WinApi.GetWindowThreadProcessId(hwnd, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    //Process mainProc = Process.GetProcessById(Process.GetProcessById(proc.Handle.ToInt32()).MainWindowHandle.ToInt32());
                    return proc.MainModule.FileName.ToString();
                }
            }
            catch (Exception e) {
                Console.WriteLine("MpHelpers.Instance.GetProcessPath error (likely) cannot find process path: " + e.ToString());
                return GetProcessPath(((MpClipTrayViewModel)((MpMainWindowViewModel)((MpMainWindow)App.Current.MainWindow).DataContext).ClipTrayViewModel).ClipboardManager.LastWindowWatcher.ThisAppHandle);
            }
        }
        public string GetProcessMainWindowTitle(IntPtr hWnd) {
            if (hWnd == null) {
                throw new Exception("MpHelpers error hWnd is null");
            }
            if (hWnd == IntPtr.Zero) {
                return string.Empty;
            }
            uint processId;
            WinApi.GetWindowThreadProcessId(hWnd, out processId);
            using (Process proc = Process.GetProcessById((int)processId)) {
                return proc.MainWindowTitle;
            }
        }
        public Point GetMousePosition() {
            WinApi.Win32Point w32Mouse = new WinApi.Win32Point();
            WinApi.GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public string GetMainModuleFilepath(int processId) {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString)) {
                using (var results = searcher.Get()) {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null) {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }

        public IPAddress GetCurrentIPAddress() {
            Ping ping = new Ping();
            var replay = ping.Send(Dns.GetHostName());

            if (replay.Status == IPStatus.Success) {
                return replay.Address;
            }
            return null;
        }

        public bool CheckForInternetConnection() {
            try {
                using (var client = new WebClient())
                using (client.OpenRead("http://www.google.com/")) {
                    return true;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public bool IsPathDirectory(string str) {
            // get the file attributes for file or directory
            return File.GetAttributes(str).HasFlag(FileAttributes.Directory);
        }
        #endregion

        #region Visual
        private List<List<Brush>> _ContentColors = new List<List<Brush>> {
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(248, 160, 174)),
                    new SolidColorBrush(Color.FromRgb(243, 69, 68)),
                    new SolidColorBrush(Color.FromRgb(229, 116, 102)),
                    new SolidColorBrush(Color.FromRgb(211, 159, 161)),
                    new SolidColorBrush(Color.FromRgb(191, 53, 50))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(252, 168, 69)),
                    new SolidColorBrush(Color.FromRgb(251, 108, 40)),
                    new SolidColorBrush(Color.FromRgb(253, 170, 130)),
                    new SolidColorBrush(Color.FromRgb(189, 141, 103)),
                    new SolidColorBrush(Color.FromRgb(177, 86, 55))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(215, 157, 60)),
                    new SolidColorBrush(Color.FromRgb(168, 123, 82)),
                    new SolidColorBrush(Color.FromRgb(214, 182, 133)),
                    new SolidColorBrush(Color.FromRgb(162, 144, 122)),
                    new SolidColorBrush(Color.FromRgb(123, 85, 72))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(247, 245, 144)),
                    new SolidColorBrush(Color.FromRgb(252, 240, 78)),
                    new SolidColorBrush(Color.FromRgb(239, 254, 185)),
                    new SolidColorBrush(Color.FromRgb(198, 193, 127)),
                    new SolidColorBrush(Color.FromRgb(224, 200, 42))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(189, 254, 40)),
                    new SolidColorBrush(Color.FromRgb(143, 254, 115)),
                    new SolidColorBrush(Color.FromRgb(217, 231, 170)),
                    new SolidColorBrush(Color.FromRgb(172, 183, 38)),
                    new SolidColorBrush(Color.FromRgb(140, 157, 45))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(50, 255, 76)),
                    new SolidColorBrush(Color.FromRgb(68, 199, 33)),
                    new SolidColorBrush(Color.FromRgb(193, 214, 135)),
                    new SolidColorBrush(Color.FromRgb(127, 182, 99)),
                    new SolidColorBrush(Color.FromRgb(92, 170, 58))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(54, 255, 173)),
                    new SolidColorBrush(Color.FromRgb(32, 195, 178)),
                    new SolidColorBrush(Color.FromRgb(170, 206, 160)),
                    new SolidColorBrush(Color.FromRgb(160, 201, 197)),
                    new SolidColorBrush(Color.FromRgb(32, 159, 148))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(96, 255, 227)),
                    new SolidColorBrush(Color.FromRgb(46, 238, 249)),
                    new SolidColorBrush(Color.FromRgb(218, 253, 233)),
                    new SolidColorBrush(Color.FromRgb(174, 193, 208)),
                    new SolidColorBrush(Color.FromRgb(40, 103, 146))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(149, 204, 243)),
                    new SolidColorBrush(Color.FromRgb(43, 167, 237)),
                    new SolidColorBrush(Color.FromRgb(215, 244, 248)),
                    new SolidColorBrush(Color.FromRgb(153, 178, 198)),
                    new SolidColorBrush(Color.FromRgb(30, 51, 160))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(99, 141, 227)),
                    new SolidColorBrush(Color.FromRgb(22, 127, 193)),
                    new SolidColorBrush(Color.FromRgb(201, 207, 233)),
                    new SolidColorBrush(Color.FromRgb(150, 163, 208)),
                    new SolidColorBrush(Color.FromRgb(52, 89, 170))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(157, 176, 255)),
                    new SolidColorBrush(Color.FromRgb(148, 127, 220)),
                    new SolidColorBrush(Color.FromRgb(216, 203, 233)),
                    new SolidColorBrush(Color.FromRgb(180, 168, 192)),
                    new SolidColorBrush(Color.FromRgb(109, 90, 179))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(221, 126, 230)),
                    new SolidColorBrush(Color.FromRgb(186, 141, 200)),
                    new SolidColorBrush(Color.FromRgb(185, 169, 231)),
                    new SolidColorBrush(Color.FromRgb(203, 178, 200)),
                    new SolidColorBrush(Color.FromRgb(170, 90, 179))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(225, 103, 164)),
                    new SolidColorBrush(Color.FromRgb(252, 74, 210)),
                    new SolidColorBrush(Color.FromRgb(238, 233, 237)),
                    new SolidColorBrush(Color.FromRgb(195, 132, 163)),
                    new SolidColorBrush(Color.FromRgb(205, 60, 117))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(223, 223, 223)),
                    new SolidColorBrush(Color.FromRgb(187, 187, 187)),
                    new SolidColorBrush(Color.FromRgb(137, 137, 137)),
                    new SolidColorBrush(Color.FromRgb(65, 65, 65))
                }
            };

        public Brush GetContentColor(int c, int r) {
            return _ContentColors[c][r];
        }

        public void SetColorChooserMenuItem(
            ContextMenu cm,
            MenuItem cmi,
            MouseButtonEventHandler selectedEventHandler, 
            int defX, 
            int defY) {
            var cmic = new Canvas();
            double s = 15;
            double pad = 2.5;
            double w = (_ContentColors.Count * (s + pad)) + pad;
            double h = (_ContentColors[0].Count * (s + pad)) + pad;
            for (int x = 0; x < _ContentColors.Count; x++) {
                for (int y = 0; y < _ContentColors[0].Count; y++) {
                    Border b = new Border();
                    b.Background = _ContentColors[x][y];
                    b.BorderThickness = new Thickness(1.5);
                    b.BorderBrush = Brushes.DarkGray;
                    b.CornerRadius = new CornerRadius(2);
                    b.Width = b.Height = s;

                    b.MouseEnter += (s1, e1) => {
                        b.BorderBrush = Brushes.DimGray;
                    };

                    b.MouseLeave += (s1, e1) => {
                        b.BorderBrush = Brushes.DarkGray;
                    };

                    b.MouseLeftButtonUp += selectedEventHandler;

                    cm.Closed += (s1, e) => {
                        b.MouseLeftButtonUp -= selectedEventHandler;
                    };

                    if (x == defX && y == defY) {
                        b.Focus();
                    }

                    cmic.Children.Add(b);

                    Canvas.SetLeft(b, (x * (s + pad)) + pad);
                    Canvas.SetTop(b, (y * (s + pad)) + pad);
                }
            }
            cmic.Background = Brushes.Transparent;
            cmi.Header = cmic;
            cmi.Height = h;
            cmi.Style = (Style)Application.Current.MainWindow.FindResource("MenuItemStyle");
            cm.Width = 300;
        }
        
        public int GetColorColumn(Brush scb) {
            for (int c = 0; c < _ContentColors.Count; c++) {
                for (int r = 0; r < _ContentColors[0].Count; r++) {
                    if(scb == _ContentColors[c][r]) {
                        return c;
                    }
                }
            }
            return 0;
        }

        public int GetColorRow(Brush scb) {
            for (int c = 0; c < _ContentColors.Count; c++) {
                for (int r = 0; r < _ContentColors[0].Count; r++) {
                    if (scb == _ContentColors[c][r]) {
                        return r;
                    }
                }
            }
            return 0;
        }

        private double sign(Point p1, Point p2, Point p3) {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public bool IsPointInTriangle(Point pt, Point v1, Point v2, Point v3) {
            double d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            
            return !(has_neg && has_pos);
        }

        public double DistanceBetweenPoints(Point a, Point b) {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        public DoubleAnimation AnimateDoubleProperty(double from, double to, double dt, object obj, DependencyProperty property, EventHandler onCompleted) {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(dt));

            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            animation.EasingFunction = easing;
            
            if(onCompleted != null) {
                animation.Completed += onCompleted;
            }
            if(obj.GetType() == typeof(List<FrameworkElement>)) {
                foreach(var fe in (List<FrameworkElement>)obj) {
                    fe.BeginAnimation(property, animation);
                }
            } else {
                ((FrameworkElement)obj).BeginAnimation(property, animation);
            }

            return animation;
        }

        public Size MeasureText(string text, Typeface typeface, double fontSize) {
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

            return new Size(formattedText.Width, formattedText.Height);
        }

        public Brush ShowColorDialog(Brush currentBrush) {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowHelp = true;
            cd.Color = MpHelpers.Instance.ConvertSolidColorBrushToWinFormsColor((SolidColorBrush)currentBrush);
            cd.CustomColors = Properties.Settings.Default.UserCustomColorIdxArray;

            var mw = (MpMainWindow)Application.Current.MainWindow;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = true;
            // Update the text box color if the user clicks OK 
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                return MpHelpers.Instance.ConvertWinFormsColorToSolidColorBrush(cd.Color);
            }
            return null;
        }

        public List<MpDetectedImageObject> DetectObjects(byte[] image, double confidence = 0.0) {
            var detectedObjectList = new List<MpDetectedImageObject>();
            using (var yoloWrapper = new YoloWrapper(new ConfigurationDetector().Detect())) {
                var items = yoloWrapper.Detect(image);
                foreach(var item in items) {
                    if(item.Confidence >= confidence) {
                        detectedObjectList.Add(new MpDetectedImageObject(
                            0,
                            0,
                            item.Confidence,
                            item.X,
                            item.Y,
                            item.Width,
                            item.Height,
                            item.Type));
                    }                    
                }
                //items[0].Type -> "Person , Car, ..."
                //items[0].Confidence -> 0.0 (low) -> 1.0 (high)
                //items[0].X -> bounding box
                //items[0].Y -> bounding box
                //items[0].Width -> bounding box
                //items[0].Height -> bounding box
                return detectedObjectList;
            }
        }
        
        public BitmapSource TintBitmapSource(BitmapSource bmpSrc, Color tint) {
            var bmp = new WriteableBitmap(bmpSrc);
            var pixels = GetPixels(bmp);
            var pixelColor = new PixelColor[1, 1];
            pixelColor[0, 0] = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    PixelColor c = pixels[x, y];
                    //Color gotColor = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
                    if (c.Alpha > 0) {
                        PutPixels(bmp, pixelColor, x, y);
                    }
                }
            }
            return bmp;
        }

        public Color ConvertHexToColor(string hexString) {
            if (hexString.IndexOf('#') != -1) {
                hexString = hexString.Replace("#", string.Empty);
            }

            byte r = byte.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            byte g = byte.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            byte b = byte.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);

            return Color.FromArgb(255, r, g, b);
        }

        public BitmapSource GetIconImage(string sourcePath) {
            if (!File.Exists(sourcePath)) {
                if (!Directory.Exists(sourcePath)) {                    
                    //return (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/monkey (2).png"));
                    return ConvertBitmapToBitmapSource(System.Drawing.SystemIcons.Question.ToBitmap());
                } else {
                    return GetBitmapFromFolderPath(sourcePath, IconSizeEnum.MediumIcon32);
                }

            }
            return GetBitmapFromFilePath(sourcePath, IconSizeEnum.MediumIcon32);
        }

        public BitmapSource ResizeBitmapSource(BitmapSource bmpSrc, Size newSize) {
            
            using (System.Drawing.Bitmap result = new System.Drawing.Bitmap((int)newSize.Width, (int)newSize.Height)) {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage((System.Drawing.Image)result)) {
                    //The interpolation mode produces high quality images
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(ConvertBitmapSourceToBitmap(bmpSrc), 0, 0, (int)newSize.Width, (int)newSize.Height);
                    g.Dispose();
                    return ConvertBitmapToBitmapSource(result);
                }
            }
        }

        public bool ByteArrayCompare(byte[] b1, byte[] b2) {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && WinApi.memcmp(b1, b2, b1.Length) == 0;
        }

        public BitmapSource ReadImageFromFile(string filePath) {
            return new BitmapImage(new Uri(filePath));
        }

        public System.Drawing.Color GetDominantColor(System.Drawing.Bitmap bmp) {
            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    System.Drawing.Color clr = bmp.GetPixel(x, y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return System.Drawing.Color.FromArgb((byte)r, (byte)g, (byte)b);
        }

        public void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public System.Drawing.Color ColorFromHSV(double hue, double saturation, double value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = (hue / 60) - Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - (f * saturation)));
            int t = Convert.ToInt32(value * (1 - ((1 - f) * saturation)));

            if (hi == 0) {
                return System.Drawing.Color.FromArgb(255, (byte)v, (byte)t, (byte)p);
            } else if (hi == 1) {
                return System.Drawing.Color.FromArgb(255, (byte)q, (byte)v, (byte)p);
            } else if (hi == 2) {
                return System.Drawing.Color.FromArgb(255, (byte)p, (byte)v, (byte)t);
            } else if (hi == 3) {
                return System.Drawing.Color.FromArgb(255, (byte)p, (byte)q, (byte)v);
            } else if (hi == 4) {
                return System.Drawing.Color.FromArgb(255, (byte)t, (byte)p, (byte)v);
            } else {
                return System.Drawing.Color.FromArgb(255, (byte)v, (byte)p, (byte)q);
            }
        }

        public System.Drawing.Color GetInvertedColor(System.Drawing.Color c) {
            ColorToHSV(c, out double h, out double s, out double v);
            h = (h + 180) % 360;
            return ColorFromHSV(h, s, v);
        }

        public bool IsBright(Color c, int brightThreshold = 130) {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114) > brightThreshold;
        }

        public SolidColorBrush ChangeBrushAlpha(SolidColorBrush solidColorBrush, byte alpha) {
            var c = solidColorBrush.Color;
            c.A = alpha;
            solidColorBrush.Color = c;
            return solidColorBrush;
        }

        public SolidColorBrush ChangeBrushBrightness(SolidColorBrush b, double correctionFactor) {
            if (correctionFactor == 0.0f) {
                return b.Clone();
            }
            double red = (double)b.Color.R;
            double green = (double)b.Color.G;
            double blue = (double)b.Color.B;

            if (correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            } else {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return new SolidColorBrush(Color.FromArgb(b.Color.A, (byte)red, (byte)green, (byte)blue));
        }

        public Brush GetDarkerBrush(Brush b) {
            return ChangeBrushBrightness((SolidColorBrush)b, 0.5);
        }

        public Brush GetLighterBrush(Brush b) {
            return ChangeBrushBrightness((SolidColorBrush)b, 0.5);
        }

        public Color GetRandomColor(byte alpha = 255) {
            //if (alpha == 255) {
            //    return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            //}
            //return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            int x = Rand.Next(0, _ContentColors.Count);
            int y = Rand.Next(0, _ContentColors[0].Count);
            return ((SolidColorBrush)GetContentColor(x, y)).Color;
        }

        public Brush GetRandomBrushColor(byte alpha = 255) {
            return (Brush)new SolidColorBrush() { Color = GetRandomColor(alpha) };
        }

        public System.Drawing.Icon GetIconFromBitmap(System.Drawing.Bitmap bmp) {
            IntPtr hIcon = bmp.GetHicon();
            return System.Drawing.Icon.FromHandle(hIcon);
        }

        public string GetColorString(Color c) {
            return (int)c.A + "," + (int)c.R + "," + (int)c.G + "," + (int)c.B;
        }

        public System.Drawing.Color GetColorFromString(string colorStr) {
            if (string.IsNullOrEmpty(colorStr)) {
                colorStr = GetColorString(GetRandomColor());
            }

            int[] c = new int[colorStr.Split(',').Length];
            for (int i = 0; i < c.Length; i++) {
                c[i] = Convert.ToInt32(colorStr.Split(',')[i]);
            }

            if (c.Length == 3) {
                return System.Drawing.Color.FromArgb(255/*c[3]*/, c[0], c[1], c[2]);
            }

            return System.Drawing.Color.FromArgb(c[3], c[0], c[1], c[2]);
        }

        public BitmapSource MergeImages(IList<BitmapSource> bmpSrcList) {
            int width = 0;
            int height = 0;
            int dpiX = 0;
            int dpiY = 0;
            // Get max width and height of the image
            foreach (var image in bmpSrcList) {
                width = Math.Max(image.PixelWidth, width);
                height = Math.Max(image.PixelHeight, height);
                dpiX = Math.Max((int)image.DpiX, dpiX);
                dpiY = Math.Max((int)image.DpiY, dpiY);
            }
            var renderTargetBitmap = new RenderTargetBitmap(width, height, dpiX, dpiY, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen()) {
                foreach (var image in bmpSrcList) {
                    drawingContext.DrawImage(image, new Rect(0, 0, width, height));
                }
            }
            renderTargetBitmap.Render(drawingVisual);

            return ConvertRenderTargetBitmapToBitmapSource(renderTargetBitmap);
        }

        public BitmapSource ConvertRenderTargetBitmapToBitmapSource(RenderTargetBitmap rtb) {
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));
            using (var stream = new MemoryStream()) {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public BitmapSource CombineBitmap(IList<BitmapSource> bmpSrcList, bool tileHorizontally = true) {
            if (bmpSrcList.Count == 1) {
                return bmpSrcList[0];
            }
            //read all images into memory
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;

            try {
                int width = 0;
                int height = 0;

                foreach (var bmpSrc in bmpSrcList) {
                    //create a Bitmap from the file and add it to the list
                    System.Drawing.Bitmap bitmap = ConvertBitmapSourceToBitmap(bmpSrc);

                    //update the size of the final bitmap
                    if (tileHorizontally) {
                        width += bitmap.Width;
                        height = Math.Max(bitmap.Height, height);
                    } else {
                        width = Math.Max(bitmap.Width, width);
                        height += bitmap.Height;
                    }
                    images.Add(bitmap);
                }

                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage)) {
                    //set background color
                    g.Clear(System.Drawing.Color.Transparent);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images) {
                        g.DrawImage(image, new System.Drawing.Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                    g.Dispose();
                }
                return ConvertBitmapToBitmapSource(finalImage);
            }
            catch (Exception ex) {
                if (finalImage != null) {
                    finalImage.Dispose();
                }
                throw ex;
            } finally {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images) {
                    image.Dispose();
                }
            }
        }

        #endregion

        #region Converters
        public async Task<string> ConvertBitmapSourceToPlainText(string image) {
            var engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en-US")); 
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(image);
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) {
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                var ocrResult = await engine.RecognizeAsync(softwareBitmap);

                Console.WriteLine(ocrResult.Text);

                return ocrResult.Text;
            }            
        }

        public string ConvertFlowDocumentToXaml(MpEventEnabledFlowDocument fd) {
            TextRange range = new TextRange(fd.ContentStart, fd.ContentEnd);
            using (MemoryStream stream = new MemoryStream()) {
                range.Save(stream, DataFormats.XamlPackage);
                //return ASCIIEncoding.Default.GetString(stream.ToArray());
                return UTF8Encoding.Default.GetString(stream.ToArray());
            }
        }

        public MpEventEnabledFlowDocument ConvertXamlToFlowDocument(string xaml) {
            using (var stringReader = new StringReader(xaml)) {
                var xmlReader = XmlReader.Create(stringReader);
                //if (!IsStringFlowSection(xaml)) {
                //    return (MpEventEnabledFlowDocument)XamlReader.Load(xmlReader);
                //}
                var doc = new MpEventEnabledFlowDocument();
                var data = XamlReader.Load(xmlReader);
                if (data.GetType() == typeof(Span)) {
                    Span span = (Span)data;
                    while (span.Inlines.Count > 0) {
                        //doc.Blocks.Add(sec.Blocks.FirstBlock);
                        var inline = span.Inlines.FirstInline;
                        span.Inlines.Remove(inline);
                        doc.Blocks.Add(new Paragraph(inline));
                    }
                } else if (data.GetType() == typeof(Section)) {
                    Section sec = (Section)data;
                    while (sec.Blocks.Count > 0) {
                        //doc.Blocks.Add(sec.Blocks.FirstBlock);
                        var block = sec.Blocks.FirstBlock;
                        sec.Blocks.Remove(block);
                        doc.Blocks.Add(block);
                    }
                } else {
                    doc = (MpEventEnabledFlowDocument)data;
                }

                return doc;
            }
        }

        public string ConvertPlainTextToRichText(string plainText) {
            System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox();
            rtb.Text = plainText;
            rtb.Font = new System.Drawing.Font(Properties.Settings.Default.DefaultFontFamily,(float)Properties.Settings.Default.DefaultFontSize);
            return rtb.Rtf;
        }
        public string ConvertPlainTextToRichText2(string plainText) {
            string escapedPlainText = plainText.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}");
            string rtf = @"{\rtf1\ansi{\fonttbl\f0\fswiss Helvetica;}\f0\pard ";
            rtf += escapedPlainText.Replace(Environment.NewLine, @" \par ");
            rtf += " }";
            return rtf;
        }
        public string ConvertRichTextToPlainText(string richText) {
            if (IsStringRichText(richText)) {
                RichTextBox rtb = new RichTextBox();
                rtb.SetRtf(richText);
                var pt = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
                int rtcount = GetRowCount(richText);
                int ptcount = GetRowCount(pt);
                if(rtcount != ptcount) {
                    return pt.Trim(new char[] { '\r', '\n' });
                }
                return pt;
            } else {
                return richText;
            }
        }

        public string ConvertXamlToRichText(string xaml) {
            //return string.Empty;
            var richTextBox = new System.Windows.Controls.RichTextBox();
            if (string.IsNullOrEmpty(xaml)) {
                return string.Empty;
            }

            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

            using (var xamlMemoryStream = new MemoryStream()) {
                using (var xamlStreamWriter = new StreamWriter(xamlMemoryStream)) {
                    xamlStreamWriter.Write(xaml);
                    xamlStreamWriter.Flush();
                    xamlMemoryStream.Seek(0, SeekOrigin.Begin);

                    textRange.Load(xamlMemoryStream, DataFormats.Xaml);
                }
            }

            using (var rtfMemoryStream = new MemoryStream()) {
                textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                textRange.Save(rtfMemoryStream, DataFormats.Rtf);
                rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                using (var rtfStreamReader = new StreamReader(rtfMemoryStream)) {
                    return rtfStreamReader.ReadToEnd();
                }
            }
        }

        public string ConvertXamlToPlainText(string xaml) {
            var fd = ConvertXamlToFlowDocument(xaml);
            return new TextRange(fd.ContentStart, fd.ContentEnd).Text;
        }

        public string ConvertPlainTextToXaml(string plainText) {
            return ConvertRichTextToXaml(ConvertPlainTextToRichText(plainText));
        }

        public MpEventEnabledFlowDocument ConvertRichTextToFlowDocument(string rtf) {
            if(IsStringRichText(rtf)) {
                //using (var stream = new MemoryStream(Encoding.Default.GetBytes(rtf))) {
                using (var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(rtf))) {
                    var flowDocument = new MpEventEnabledFlowDocument();
                    var range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    range.Load(stream, System.Windows.DataFormats.Rtf);
                    return flowDocument;
                }
            }
            return ConvertXamlToFlowDocument(rtf);
        }

        public string ConvertRichTextToXaml(string rt) {
            var assembly = Assembly.GetAssembly(typeof(System.Windows.FrameworkElement));
            var xamlRtfConverterType = assembly.GetType("System.Windows.Documents.XamlRtfConverter");
            var xamlRtfConverter = Activator.CreateInstance(xamlRtfConverterType, true);
            var convertRtfToXaml = xamlRtfConverterType.GetMethod("ConvertRtfToXaml");
            var xamlContent = (string)convertRtfToXaml.Invoke(xamlRtfConverter, new object[] { rt });
            return xamlContent;
        }

        public string ConvertFlowDocumentToRichText(FlowDocument fd) {
            var rtb = (RichTextBox)fd.Parent;
            //if(rtb.DataContext.GetType() == typeof(MpClipTileViewModel)) {
            //    //rtb.ClearTemplates();
            //}
            Console.WriteLine("Flow Doc Contents:");
            Console.WriteLine(new TextRange(fd.ContentStart, fd.ContentEnd).Text);
            string rtf = string.Empty;
            using (var ms = new MemoryStream()) {
                var range2 = new TextRange(fd.ContentStart, fd.ContentEnd);
                range2.Save(ms, System.Windows.DataFormats.Rtf);
                ms.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(ms)) {
                    rtf = sr.ReadToEnd();
                }
            }
            //if (rtb.DataContext.GetType() == typeof(MpClipTileViewModel)) {
            //    //rtb.CreateHyperlinks();
            //}
            return rtf;
        }

        public string ConvertBitmapSourceToPlainText(BitmapSource bmpSource) {
            string[] asciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
            using (System.Drawing.Bitmap image = ConvertBitmapSourceToBitmap(ResizeBitmapSource(bmpSource, new Size(MpMeasurements.Instance.ClipTileBorderSize, MpMeasurements.Instance.ClipTileContentHeight)))) {
                string outStr = string.Empty;
                for (int h = 0; h < image.Height; h++) {
                    for (int w = 0; w < image.Width; w++) {
                        System.Drawing.Color pixelColor = image.GetPixel(w, h);
                        //Average out the RGB components to find the Gray Color
                        int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                        int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                        int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                        System.Drawing.Color grayColor = System.Drawing.Color.FromArgb(red, green, blue);
                        int index = (grayColor.R * 10) / 255;
                        outStr += asciiChars[index];
                    }
                    outStr += Environment.NewLine;
                }
                return outStr;
            }
        }

        public byte[] ConvertBitmapSourceToByteArray(BitmapSource bs) {            
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            using (MemoryStream stream = new MemoryStream()) {
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bs));
                encoder.Save(stream);
                byte[] bit = stream.ToArray();
                stream.Close();
                return bit;
            }
        }

        public BitmapSource ConvertByteArrayToBitmapSource(byte[] bytes) {
            return (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);
        }

        public BitmapSource ConvertBitmapToBitmapSource(System.Drawing.Bitmap bitmap) {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width,
                bitmapData.Height,
                bitmap.HorizontalResolution,
                bitmap.VerticalResolution,
                PixelFormats.Bgra32,
                null,
                bitmapData.Scan0,
                bitmapData.Stride * bitmapData.Height,
                bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public System.Drawing.Bitmap ConvertBitmapSourceToBitmap(BitmapSource bitmapsource) {
            using (MemoryStream outStream = new MemoryStream()) {
                System.Windows.Media.Imaging.BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                return new System.Drawing.Bitmap(outStream);
            }
        }

        public System.Drawing.Color ConvertSolidColorBrushToWinFormsColor(SolidColorBrush scb) {
            return System.Drawing.Color.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);
        }

        public SolidColorBrush ConvertWinFormsColorToSolidColorBrush(System.Drawing.Color c) {
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public BitmapSource ConvertRichTextToBitmapSource(string rt) {
            System.Drawing.Size ts = GetRichTextFontSize(rt);
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ts.Width, ts.Height)) {
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp)) {
                    graphics.Clear(System.Drawing.Color.White);
                    graphics.DrawRtfText(rt, new System.Drawing.RectangleF(0, 0, bmp.Width, bmp.Height), 1f);
                    graphics.Flush();
                    graphics.Dispose();
                }
                return ConvertBitmapToBitmapSource(bmp);
            }
        }        
        #endregion

        #region Http
        public async Task<string> ShortenUrl(string url) {
            string bitlyToken = @"f6035b9ed05ac82b42d4853c984e34a4f1ba05d8";
            using (HttpClient client = new HttpClient()) {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api-ssl.bitly.com/v4/shorten")) {
                    request.Content = new StringContent($"{{\"long_url\":\"{url}\"}}", Encoding.UTF8, "application/json");
                    try {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bitlyToken);
                        using (var response = await client.SendAsync(request).ConfigureAwait(false)) {
                            if (!response.IsSuccessStatusCode) {
                                Console.WriteLine("Minify error: " + response.Content.ToString());
                                return string.Empty;
                            }

                            var responsestr = await response.Content.ReadAsStringAsync();

                            dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);
                            return jsonResponse["link"];
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine("Minify exception: " + ex.ToString());
                        return string.Empty;
                    }
                }
            }
        }

        public BitmapSource ConvertUrlToQrCode(string url) {
            Url generator = new Url(url);
            string payload = generator.ToString();

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator()) {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q)) {
                    using (QRCode qrCode = new QRCode(qrCodeData)) {
                        var qrCodeAsBitmap = qrCode.GetGraphic(20);
                        return MpHelpers.Instance.ConvertBitmapToBitmapSource(qrCodeAsBitmap);
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private PixelColor[,] GetPixels(BitmapSource source) {
            if (source.Format != PixelFormats.Bgra32) {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixels(result, width * 4, 0, false);
            return result;
        }

        private void PutPixels(WriteableBitmap bitmap, PixelColor[,] pixels, int x, int y) {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, x, y);
        }

        private long CalcDirSize(string sourceDir, bool recurse = true) {
            return CalcDirSizeHelper(new DirectoryInfo(sourceDir), recurse);
        }

        private long CalcDirSizeHelper(DirectoryInfo di, bool recurse = true) {
            long size = 0;
            FileInfo[] fiEntries = di.GetFiles();
            foreach (var fiEntry in fiEntries) {
                Interlocked.Add(ref size, fiEntry.Length);
            }

            if (recurse) {
                DirectoryInfo[] diEntries = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
                System.Threading.Tasks.Parallel.For<long>(
                    0,
                    diEntries.Length,
                    () => 0,
                    (i, loop, subtotal) => {
                        if ((diEntries[i].Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) {
                            return 0;
                        }
                        subtotal += CalcDirSizeHelper(diEntries[i], true);
                        return subtotal;
                    },
                    (x) => Interlocked.Add(ref size, x));
            }
            return size;
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }
}
