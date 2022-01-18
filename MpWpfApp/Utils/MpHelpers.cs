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
using System.Net.Mail;
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
using QRCoder;
using static MpWpfApp.MpShellEx;
//using Windows.Graphics.Imaging;
//using Windows.Media.Ocr;
//using CsvHelper;
using System.Windows.Threading;
using System.Security.Principal;
using System.Speech.Synthesis;
using WindowsInput;
using Microsoft.Win32;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpHelpers : MpSingleton2<MpHelpers> {
        //public RichTextBox SharedRtb { get; set; }
        private InputSimulator sim = new InputSimulator();
        private BitmapSource _defaultFavIcon = null;

        public MpHelpers() {
            Rand = new Random((int)DateTime.Now.Ticks);
            // SharedRtb = new RichTextBox();
            //yoloWrapper = new YoloWrapper(new ConfigurationDetector().Detect());
            _defaultFavIcon = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/defaultfavicon.png"));
        }

        #region Documents    

        public bool HasTable(RichTextBox rtb) {
            return rtb.Document.Blocks.Any(x => x is Table);
        }

        public Hyperlink CreateAccessibleHyperlink(string uri) {
            uri = MonkeyPaste.MpHelpers.Instance.GetFullyFormattedUrl(uri);
            if (Uri.IsWellFormedUriString(uri,UriKind.Absolute)) {
                var h = new Hyperlink();
                h.NavigateUri = new Uri(uri);
                h.Inlines.Add(uri);
                var click = (MouseButtonEventHandler)((s4, e4) => {
                    if (h.NavigateUri != null) {
                        MpHelpers.Instance.OpenUrl(h.NavigateUri.ToString());
                    }
                });
                h.IsEnabled = true;
                h.MouseLeftButtonDown += click;

                h.Unloaded += (s, e) => {
                    h.MouseLeftButtonDown -= click;
                };
            }
            return null;
        }
        public List<int> IndexListOfAll(string text, string matchStr) {
            var idxList = new List<int>();
            int curIdx = text.IndexOf(matchStr);
            int offset = 0;
            while(curIdx >= 0 && curIdx < text.Length) {
                idxList.Add(curIdx + offset);
                if(curIdx + matchStr.Length + 1 >= text.Length) {
                    break;
                }
                text = text.Substring(curIdx + matchStr.Length + 1);
                offset = curIdx + 1;
                curIdx = text.IndexOf(matchStr);
            }
            return idxList;
        }

        public void ApplyBackgroundBrushToRangeList(ObservableCollection<ObservableCollection<TextRange>> rangeList, Brush bgBrush, CancellationToken ct) {
            if (rangeList == null || rangeList.Count == 0) {
                return;
            }
            foreach (var range in rangeList) {
                ApplyBackgroundBrushToRangeList(range, bgBrush, ct);
            }
        }

        public void ApplyBackgroundBrushToRangeList(ObservableCollection<TextRange> rangeList, Brush bgBrush, CancellationToken ct) {
            if (rangeList == null || rangeList.Count == 0) {
                return;
            }
            foreach (var range in rangeList) {
                if(ct.IsCancellationRequested) {
                    //throw new OperationCanceledException();
                    MonkeyPaste.MpConsole.WriteLine("Bg highlighting canceled");
                    return;
                }
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

        public List<TextRange> FindStringRangesFromPosition(TextPointer position, string matchStr, bool isCaseSensitive = false) {
            if (string.IsNullOrEmpty(matchStr)) {
                return null;
            }
            var orgPosition = position;
            TextPointer nextDocPosition = null;
            var matchRangeList = new List<TextRange>();
            TextSelection rtbSelection = null;
            var rtb = (RichTextBox)FindParentOfType(position.Parent, typeof(RichTextBox));
            if (rtb != null) {
                rtbSelection = rtb.Selection;
            }
            while (position != null) {
                var hlr = FindStringRangeFromPosition(position, matchStr, isCaseSensitive);
                if (hlr == null) {
                    if (nextDocPosition != null) {
                        position = nextDocPosition;
                        nextDocPosition = null;
                        continue;
                    }
                    break;
                } else {
                    matchRangeList.Add(hlr);
                    if (!hlr.End.IsInSameDocument(orgPosition)) {
                        var phl = (Hyperlink)FindParentOfType(hlr.End.Parent, typeof(Hyperlink));
                        if(phl == null) {
                            phl = (MpTemplateHyperlink)FindParentOfType(hlr.End.Parent, typeof(MpTemplateHyperlink));
                        }
                        nextDocPosition = phl.ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                    }
                    position = hlr.End;
                }
            }
            if (rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return matchRangeList;
        }

        public TextRange FindStringRangeFromPosition(TextPointer position, string matchStr,  bool isCaseSensitive = false) {
            if (string.IsNullOrEmpty(matchStr)) {
                return null;
            }
            int curIdx = 0;
            //TextSelection rtbSelection = null;
            //var rtb = (RichTextBox)FindParentOfType(position.Parent, typeof(RichTextBox));
            //if (rtb != null) {
            //    rtbSelection = rtb.Selection;
            //}
            TextPointer postOfUiElement = null;
            TextPointer startPointer = null;
            StringComparison stringComparison = isCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            while (position != null || postOfUiElement != null) {
                if (position == null) {
                    position = postOfUiElement;
                    postOfUiElement = null;
                }
                if (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text) {
                    if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement) {
                        var iuc = (InlineUIContainer)FindParentOfType(position.Parent, typeof(InlineUIContainer));
                        var hl = (Hyperlink)iuc.Parent;
                        var tb = (iuc.Child as Border).Child as TextBlock;
                        postOfUiElement = hl.ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                        position = tb.ContentStart;
                        continue;
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
                    if (!startPointer.IsInSameDocument(endPointer)) {
                        endPointer = ((Hyperlink)FindParentOfType(endPointer.Parent, typeof(Hyperlink))).ElementEnd;
                    }
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
                        break;
                        //return null;
                    }
                    while (startPointer != null && new TextRange(startPointer, endPointer).Text.Length > matchStr.Length) {
                        startPointer = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                    }
                    if (startPointer == null) {
                        break;
                        //return null;
                    }
                    return new TextRange(startPointer, endPointer);
                } else {
                    //prepare loop for next match character
                    curIdx++;
                    //iterate position one offset AFTER match offset
                    position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
                }
            }
            //if (rtbSelection != null) {
            //    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            //}
            return null;
        }

        public async Task<List<TextRange>> FindStringRangesFromPositionAsync(TextPointer position, string matchStr, CancellationToken ct, DispatcherPriority dp = DispatcherPriority.Normal, bool isCaseSensitive = false) {
            if (string.IsNullOrEmpty(matchStr)) {
                return null;
            }
            var orgPosition = position;
            TextPointer nextDocPosition = null;
            var matchRangeList = new List<TextRange>();
            TextSelection rtbSelection = null;
            var rtb = (RichTextBox)FindParentOfType(position.Parent, typeof(RichTextBox));
            if (rtb != null) {
                rtbSelection = rtb.Selection;
            }
            await MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                while (position != null && !ct.IsCancellationRequested) {
                    var hlr = await FindStringRangeFromPositionAsync(position, matchStr, ct, dp, isCaseSensitive);
                    if (hlr == null) {
                        if (nextDocPosition != null) {
                            position = nextDocPosition;
                            nextDocPosition = null;
                            continue;
                        }
                        break;
                    } else {
                        matchRangeList.Add(hlr);
                        if (!hlr.End.IsInSameDocument(orgPosition)) {
                            var phl = (Hyperlink)FindParentOfType(hlr.End.Parent, typeof(Hyperlink));
                            if (phl == null) {
                                phl = (MpTemplateHyperlink)FindParentOfType(hlr.End.Parent, typeof(MpTemplateHyperlink));
                            }
                            nextDocPosition = phl.ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                        }
                        position = hlr.End;
                    }
                }
            },dp);
            if (rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return matchRangeList;
        }

        public async Task<TextRange> FindStringRangeFromPositionAsync(TextPointer position, string matchStr, CancellationToken ct, DispatcherPriority dp = DispatcherPriority.Normal, bool isCaseSensitive = false) {
            if (string.IsNullOrEmpty(matchStr)) {
                return null;
            }
            int curIdx = 0;
            //TextSelection rtbSelection = null;
            //var rtb = (RichTextBox)FindParentOfType(position.Parent, typeof(RichTextBox));
            //if (rtb != null) {
            //    rtbSelection = rtb.Selection;
            //}
            TextPointer postOfUiElement = null;
            TextPointer startPointer = null;
            TextRange matchRange = null;
            StringComparison stringComparison = isCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                while (matchRange == null && (position != null || postOfUiElement != null)) {
                    if (ct.IsCancellationRequested) {
                        break;
                    }
                    if (position == null) {
                        position = postOfUiElement;
                        postOfUiElement = null;
                    }
                    if (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text) {
                        if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement) {
                            var iuc = (InlineUIContainer)FindParentOfType(position.Parent, typeof(InlineUIContainer));
                            var hl = (Hyperlink)iuc.Parent;
                            var tb = (iuc.Child as Border).Child as TextBlock;
                            postOfUiElement = hl.ElementEnd.GetNextContextPosition(LogicalDirection.Forward);
                            position = tb.ContentStart;
                            continue;
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
                        if (!startPointer.IsInSameDocument(endPointer)) {
                            endPointer = ((Hyperlink)FindParentOfType(endPointer.Parent, typeof(Hyperlink))).ElementEnd;
                        }
                        //for edge cases of repeating characters these loops ensure start is not early and last character isn't lost 
                        if (isCaseSensitive) {
                            while (endPointer != null && !new TextRange(startPointer, endPointer).Text.Contains(matchStr)) {
                                if (ct.IsCancellationRequested) {
                                    //trigger break out of parent loop
                                    endPointer = null;
                                    break;
                                }
                                endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                            }
                        } else {
                            while (endPointer != null && !new TextRange(startPointer, endPointer).Text.ToLower().Contains(matchStr.ToLower())) {
                                if (ct.IsCancellationRequested) {
                                    //trigger break out of parent loop
                                    endPointer = null;
                                    break;
                                }
                                endPointer = endPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                            }
                        }
                        if (endPointer == null) {
                            break;
                            //return null;
                        }
                        while (startPointer != null && new TextRange(startPointer, endPointer).Text.Length > matchStr.Length) {
                            startPointer = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        }
                        if (startPointer == null) {
                            break;
                            //return null;
                        }
                        matchRange = new TextRange(startPointer, endPointer);
                    } else {
                        //prepare loop for next match character
                        curIdx++;
                        //iterate position one offset AFTER match offset
                        position = position.GetPositionAtOffset(runIdx + 1, LogicalDirection.Forward);
                    }
                }
            },dp);
            //if (rtbSelection != null) {
            //    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            //}
            return matchRange;
        }

        public bool IsStringQuillText(string str) {
            if(string.IsNullOrEmpty(str)) {
                return false;
            }
            str = str.ToLower();
            foreach (var quillTag in _quillTags) {
                if (str.Contains($"</{quillTag}>")) {
                    return true;
                }
            }
            return false;
        }

        public bool IsStringCsv(string text) {
            if(string.IsNullOrEmpty(text) || IsStringRichText(text)) {
                return false;
            }
            return text.Contains(",");
        }

        public bool IsStringRichText(string text) {
            if(string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"{\rtf");
        }

        public bool IsStringXaml(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=") || text.StartsWith(@"<Span xmlns=");
        }

        public bool IsStringSpan(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Span xmlns=");
        }

        public bool IsStringSection(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=");
        }         

        public bool IsStringPlainText(string text) {
            //returns true for csv
            if(text == null) {
                return false;
            }
            if(text == string.Empty) {
                return true;
            }
            if(IsStringRichText(text) || IsStringSection(text) || IsStringSpan(text) || IsStringXaml(text)) {
                return false;
            }
            return true;
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
                MonkeyPaste.MpConsole.WriteLine(
                    "MpHelper exception cannot convert moneyStr '" + moneyStr + "' to a value, returning 0");
                MonkeyPaste.MpConsole.WriteLine("Exception Details: " + ex);
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

        public string CombineRichText2(string rt1, string rt2, bool insertNewLine = false) {
            using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                rtb.Rtf = rt1;
                if (insertNewLine) {
                    rtb.Text += Environment.NewLine;
                }
                rtb.Select(rtb.TextLength, 0);
                rtb.SelectedRtf = rt2;
                return rtb.Rtf;
            }
        }

        public string CombineRichText(string from, string to, bool insertNewLine = false) {
            return ConvertFlowDocumentToRichText(
                CombineFlowDocuments(
                    ConvertRichTextToFlowDocument(from),
                    ConvertRichTextToFlowDocument(to),
                    insertNewLine
                )
            );
        }

        public async Task<string> CombineRichTextAsync(string from, string to, bool insertNewLine = false, DispatcherPriority priority = DispatcherPriority.Background) {
            return await ConvertFlowDocumentToRichTextAsync(
                await CombineFlowDocumentsAsync(
                    await ConvertRichTextToFlowDocumentAsync(from,priority),
                    await ConvertRichTextToFlowDocumentAsync(to,priority),
                    insertNewLine,
                    priority
                )
                , priority);
        }

        public MpEventEnabledFlowDocument CombineFlowDocuments(MpEventEnabledFlowDocument from, MpEventEnabledFlowDocument to, bool insertNewLine = false) {
            RichTextBox fromRtb = null, toRtb = null;
            TextSelection fromSelection = null, toSelection = null;
            if(from.Parent != null && from.Parent.GetType() == typeof(RichTextBox)) {
                fromRtb = (RichTextBox)from.Parent;
                fromSelection = fromRtb.Selection;
            }
            if (to.Parent != null && to.Parent.GetType() == typeof(RichTextBox)) {
                toRtb = (RichTextBox)to.Parent;
                toSelection = toRtb.Selection;
            }
            using (MemoryStream stream = new MemoryStream()) {
                var rangeFrom = new TextRange(from.ContentStart, from.ContentEnd);

                System.Windows.Markup.XamlWriter.Save(rangeFrom, stream);
                rangeFrom.Save(stream, DataFormats.XamlPackage);

                //if(insertNewLine) {
                //    var lb = new LineBreak();
                //    var p = (Paragraph)to.Blocks.LastBlock;
                //    p.LineHeight = 1;
                //    p.Inlines.Add(lb);
                //}

                var rangeTo = new TextRange(to.ContentEnd, to.ContentEnd);
                rangeTo.Load(stream, DataFormats.XamlPackage);

                if(fromRtb != null && fromSelection != null) {
                    fromRtb.Selection.Select(fromSelection.Start, fromSelection.End);
                }
                if (toRtb != null && toSelection != null) {
                    toRtb.Selection.Select(toSelection.Start, toSelection.End);
                }

                var tr = new TextRange(to.ContentStart, to.ContentEnd);
                var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                if (rtbAlignment == null ||
                    rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}" ||
                    (TextAlignment)rtbAlignment == TextAlignment.Justify) {
                    tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                }

                var ps = to.GetDocumentSize();
                to.PageWidth = ps.Width;
                to.PageHeight = ps.Height;
                return to;
            }            
        }

        public async Task<MpEventEnabledFlowDocument> CombineFlowDocumentsAsync(MpEventEnabledFlowDocument from, MpEventEnabledFlowDocument to, bool insertNewLine = false, DispatcherPriority priority = DispatcherPriority.Background) {
            MpEventEnabledFlowDocument fd = null;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                using (MemoryStream stream = new MemoryStream()) {
                    var rangeFrom = new TextRange(from.ContentStart, from.ContentEnd);

                    System.Windows.Markup.XamlWriter.Save(rangeFrom, stream);
                    rangeFrom.Save(stream, DataFormats.XamlPackage);

                    if (insertNewLine) {
                        var lb = new LineBreak();
                        var p = (Paragraph)to.Blocks.LastBlock;
                        p.LineHeight = 1;
                        p.Inlines.Add(lb);
                    }

                    var rangeTo = new TextRange(to.ContentEnd, to.ContentEnd);
                    rangeTo.Load(stream, DataFormats.XamlPackage);

                    fd = to;
                }
            }, priority);
            return fd;
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
                MonkeyPaste.MpConsole.WriteLine("MpHelpers Currency Conversion exception: " + ex.ToString());
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

        public bool IsOnMainThread() {
            return Thread.CurrentThread == System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread;
        }
        
        public void RunOnMainThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal) {
            Application.Current.Dispatcher.Invoke(action, priority);
        }
        
        public DispatcherOperation RunOnMainThreadAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal) {
            return Application.Current.Dispatcher.InvokeAsync(action, priority);
        }
        
        public DispatcherOperation<TResult> RunOnMainThreadAsync<TResult>(Func<TResult> action, DispatcherPriority priority = DispatcherPriority.Normal) where TResult : class {
            return Application.Current.Dispatcher.InvokeAsync<TResult>(action, priority);
        }

        public string GetTempFileNameWithExtension(string ext) {
            if(string.IsNullOrEmpty(ext)) {
                return Path.GetTempFileName();
            }
            return Path.GetTempFileName().Replace(@".tmp",string.Empty) + ext;
        }

        public void PassKeysListToWindow(IntPtr handle,List<List<Key>> keyList) {     
            try {
                WinApi.SetForegroundWindow(handle);
                WinApi.SetActiveWindow(handle);
                for (int i = 0; i < keyList.Count; i++) {
                    var combo = keyList[i];
                    var vkCombo = new List<WindowsInput.Native.VirtualKeyCode>();
                    foreach (var key in combo) {
                        WindowsInput.Native.VirtualKeyCode vk = (WindowsInput.Native.VirtualKeyCode)KeyInterop.VirtualKeyFromKey(key);
                        vkCombo.Add(vk);
                    }
                    sim.Keyboard.KeyPress(vkCombo.ToArray());
                }
            }
            catch(Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("MpHelpers.PassKeysListToWindow exception: " + ex);
            }
        }

        public InstalledVoice GetInstalledVoiceByName(string voiceName) {
            var speechSynthesizer = new SpeechSynthesizer();
            foreach (var voice in speechSynthesizer.GetInstalledVoices()) {
                if(voice.VoiceInfo.Name.Contains(voiceName)) {
                    return voice;
                }
            }
            return null;
        }

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

        public Random Rand { get; set; } = null;

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

        
        
        public double FileListSize(string[] paths) {
            long total = 0;
            foreach (string path in paths) {
                if (Directory.Exists(path)) {
                    total += CalcDirSize(path, true);
                } else if (File.Exists(path)) {
                    total += new FileInfo(path).Length;
                }
            }
            return ConvertBytesToMegabytes(total);
        }

        public string GetUniqueFileName(MpExternalDropFileType fileType,string baseName = "", string baseDir = "") {
            //only support Image and RichText fileTypes
            string fp = string.IsNullOrEmpty(baseDir) ? Path.GetTempPath() : baseDir;
            string fn = string.IsNullOrEmpty(baseName) ? Path.GetRandomFileName() : MpHelpers.Instance.RemoveSpecialCharacters(baseName.Trim());
            if (string.IsNullOrEmpty(fn)) {
                fn = Path.GetRandomFileName();
            }
            string fe = "." + Enum.GetName(typeof(MpExternalDropFileType), fileType).ToLower(); //fileType == MpCopyItemType.RichText ? ".txt" : ".png";

            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fp + fn + fe);
            string extension = Path.GetExtension(fp + fn + fe);
            string path = Path.GetDirectoryName(fp + fn + fe);
            string newFullPath = fp + fn + fe;

            while (File.Exists(newFullPath)) {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }

        public string ReadTextFromFile(string filePath) {
            try {
                using (StreamReader f = new StreamReader(filePath)) {
                    string outStr = string.Empty;
                    outStr = f.ReadToEnd();
                    f.Close();
                    return outStr;
                }
            }
            catch(Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return string.Empty;
            }
        }

        public string WriteTextToFile(string filePath, string text, bool isTemporary = false) {
            if (filePath.ToLower().Contains(@".tmp")) {
                string extension = string.Empty;
                if (MpHelpers.Instance.IsStringRichText(text)) {
                    extension = @".rtf";
                } else if (MpHelpers.Instance.IsStringCsv(text)) {
                    extension = @".csv";
                } else {
                    extension = @".txt";
                }
                filePath = filePath.ToLower().Replace(@".tmp", extension);
            }
            using (StreamWriter of = new StreamWriter(filePath)) {
                of.Write(text);
                of.Close();
                if (isTemporary) {
                    MpMainWindowViewModel.Instance.AddTempFile(filePath);
                }
                return filePath;
            }
        }
        public string WriteBitmapSourceToFile(string filePath, BitmapSource bmpSrc, bool isTemporary = false) {
            if (filePath.ToLower().Contains(@".tmp")) {
                filePath = filePath.ToLower().Replace(@".tmp", @".png");
            }
            using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmpSrc));
                encoder.Save(fileStream);
            }

            if (isTemporary) {
                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).AddTempFile(filePath);
            }
            return filePath;
        }

        public string WriteStringListToCsvFile(string filePath, IList<string> strList, bool isTemporary = false) {
            var textList = new List<string>();
            foreach (var str in strList) {
                if (!string.IsNullOrEmpty(str.Trim())) {
                    textList.Add(str);
                }
            }
            if (filePath.ToLower().Contains(@".tmp")) {
                filePath = filePath.ToLower().Replace(@".tmp", @".csv");
            }
            //using (var writer = new StreamWriter(filePath)) {
            //    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
            //        csv.WriteRecords(textList);
            //    }
            //}
            if (isTemporary) {
                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).AddTempFile(filePath);
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

        public IntPtr StartProcess(
            string args, 
            string processPath, 
            bool asAdministrator, 
            bool isSilent, 
            WinApi.ShowWindowCommands windowState = WinApi.ShowWindowCommands.Normal) {
            try {
                IntPtr outHandle = IntPtr.Zero;
                if (isSilent) {
                    windowState = WinApi.ShowWindowCommands.Hide;
                }
                ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo();
                processInfo.FileName = processPath;//Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe"; //Sets the FileName property of myProcessInfo to %SystemRoot%\System32\cmd.exe where %SystemRoot% is a system variable which is expanded using Environment.ExpandEnvironmentVariables
                if (!string.IsNullOrEmpty(args)) {
                    processInfo.Arguments = args;
                }
                processInfo.WindowStyle = isSilent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal; //Sets the WindowStyle of myProcessInfo which indicates the window state to use when the process is started to Hidden
                processInfo.Verb = asAdministrator ? "runas" : string.Empty; //The process should start with elevated permissions

                if (asAdministrator) {
                    using (var process = Process.Start(processInfo)) {
                        while (!process.WaitForInputIdle(100)) {
                            Thread.Sleep(100);
                            process.Refresh();
                        }
                        outHandle = process.Handle;
                    }
                } else {
                    using (var process = UACHelper.UACHelper.StartLimited(processInfo)) {
                        while (!process.WaitForInputIdle(100)) {
                            Thread.Sleep(100);
                            process.Refresh();
                        }
                        outHandle = process.Handle;
                    }
                }
                if (outHandle == IntPtr.Zero) {
                    MonkeyPaste.MpConsole.WriteLine("Error starting process: " + processPath);
                    return outHandle;
                }

                WinApi.ShowWindowAsync(outHandle, GetShowWindowValue(windowState));
                return outHandle;
            }
            catch (Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("Start Process error (Admin to Normal mode): " + ex);
                return IntPtr.Zero;
            }
            // TODO pass args to clipboard (w/ ignore in the manager) then activate window and paste
        }

        public IntPtr RunAsDesktopUser(string fileName, string args = "") {            
            if (string.IsNullOrWhiteSpace(fileName)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
            }

            // To start process as shell user you will need to carry out these steps:
            // 1. Enable the SeIncreaseQuotaPrivilege in your current token
            // 2. Get an HWND representing the desktop shell (GetShellWindow)
            // 3. Get the Process ID(PID) of the process associated with that window(GetWindowThreadProcessId)
            // 4. Open that process(OpenProcess)
            // 5. Get the access token from that process (OpenProcessToken)
            // 6. Make a primary token with that token(DuplicateTokenEx)
            // 7. Start the new process with that primary token(CreateProcessWithTokenW)

            var hProcessToken = IntPtr.Zero;
            // Enable SeIncreaseQuotaPrivilege in this process.  (This won't work if current process is not elevated.)
            try {
                var process = WinApi.GetCurrentProcess();
                if (!WinApi.OpenProcessToken(process, 0x0020, ref hProcessToken)) {
                    return IntPtr.Zero;
                }

                var tkp = new WinApi.TOKEN_PRIVILEGES {
                    PrivilegeCount = 1,
                    Privileges = new WinApi.LUID_AND_ATTRIBUTES[1]
                };

                if (!WinApi.LookupPrivilegeValue(null, "SeIncreaseQuotaPrivilege", ref tkp.Privileges[0].Luid)) {
                    return IntPtr.Zero;
                }

                tkp.Privileges[0].Attributes = 0x00000002;

                if (!WinApi.AdjustTokenPrivileges(hProcessToken, false, ref tkp, 0, IntPtr.Zero, IntPtr.Zero)) {
                    return IntPtr.Zero;
                }
            } finally {
                WinApi.CloseHandle(hProcessToken);
            }

            // Get an HWND representing the desktop shell.
            // CAVEATS:  This will fail if the shell is not running (crashed or terminated), or the default shell has been
            // replaced with a custom shell.  This also won't return what you probably want if Explorer has been terminated and
            // restarted elevated.
            var hwnd = WinApi.GetShellWindow();
            if (hwnd == IntPtr.Zero) {
                return IntPtr.Zero;
            }

            var hShellProcess = IntPtr.Zero;
            var hShellProcessToken = IntPtr.Zero;
            var hPrimaryToken = IntPtr.Zero;
            try {
                // Get the PID of the desktop shell process.
                uint dwPID;
                if (WinApi.GetWindowThreadProcessId(hwnd, out dwPID) == 0) {
                    return IntPtr.Zero;
                }
                // Open the desktop shell process in order to query it (get the token)
                hShellProcess = WinApi.OpenProcess(WinApi.ProcessAccessFlags.QueryInformation, false, dwPID);
                if (hShellProcess == IntPtr.Zero) {
                    return IntPtr.Zero;
                }

                // Get the process token of the desktop shell.
                if (!WinApi.OpenProcessToken(hShellProcess, 0x0002, ref hShellProcessToken)) {
                    return IntPtr.Zero;
                }

                var dwTokenRights = 395U;

                // Duplicate the shell's process token to get a primary token.
                // Based on experimentation, this is the minimal set of rights required for CreateProcessWithTokenW (contrary to current documentation).
                if (!WinApi.DuplicateTokenEx(hShellProcessToken, dwTokenRights, IntPtr.Zero, WinApi.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, WinApi.TOKEN_TYPE.TokenPrimary, out hPrimaryToken)) {
                    return IntPtr.Zero;
                }

                // Start the target process with the new token.
                var si = new WinApi.STARTUPINFO();
                var pi = new WinApi.PROCESS_INFORMATION();
                if(string.IsNullOrEmpty(args)) {
                    args = "";
                }
                if (!WinApi.CreateProcessWithTokenW(hPrimaryToken, 0, fileName, args, 0, IntPtr.Zero, Path.GetDirectoryName(fileName), ref si, out pi)) {
                    return IntPtr.Zero;
                }

                return pi.hProcess;
            } finally {
                WinApi.CloseHandle(hShellProcessToken);
                WinApi.CloseHandle(hPrimaryToken);
                WinApi.CloseHandle(hShellProcess);
            }            
        }

        public int GetShowWindowValue(WinApi.ShowWindowCommands cmd) {
            int winType = 0;
            switch (cmd) {
                case WinApi.ShowWindowCommands.Normal:
                    winType = WinApi.Windows.NORMAL;
                    break;
                case WinApi.ShowWindowCommands.Maximized:
                    winType = WinApi.Windows.MAXIMIXED;
                    break;
                case WinApi.ShowWindowCommands.Minimized:
                case WinApi.ShowWindowCommands.Hide:
                    winType = WinApi.Windows.HIDE;
                    break;
                default:
                    winType = WinApi.Windows.NORMAL;
                    break;
            }
            return winType;
        }

        public IntPtr GetThisAppHandle() {
            return Process.GetCurrentProcess().Handle;
        }

        public bool IsThisAppAdmin() {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        public bool IsProcessAdmin(IntPtr handle) {
            if(handle == null || handle == IntPtr.Zero) {
                return false;
            }
            try {
                WinApi.GetWindowThreadProcessId(handle, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    IntPtr ph = IntPtr.Zero;
                    WinApi.OpenProcessToken(proc.Handle,WinApi.TOKEN_ALL_ACCESS, out ph);
                    WindowsIdentity iden = new WindowsIdentity(ph);
                    bool result = false;

                    foreach (IdentityReference role in iden.Groups) {
                        if (role.IsValidTargetType(typeof(SecurityIdentifier))) {
                            SecurityIdentifier sid = role as SecurityIdentifier;
                            if (sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid) || sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid)) {
                                result = true;
                                break;
                            }
                        }
                    }
                    WinApi.CloseHandle(ph);
                    return result;
                }                    
            }
            catch(Exception ex) {
                //if app is started using "Run as" is if you get "Access Denied" error. 
                //That means that running app has rights that your app does not have. 
                //in this case ADMIN rights
                MonkeyPaste.MpConsole.WriteLine("IsProcessAdmin error: " + ex.ToString());
                return true;
            }
        }
        
        public string GetApplicationDirectory() {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public string GetResourcesDirectory() {
            return Path.Combine(GetApplicationDirectory(), "Resources");
        }

        public string GetImagesDirectory() {
            return Path.Combine(GetResourcesDirectory(), "Images");
        }

        public string GetApplicationProcessPath() {
            try {
                var process = Process.GetCurrentProcess();
                return process.MainModule.FileName;
            } catch(Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("Error getting this application process path: " + ex.ToString());
                MonkeyPaste.MpConsole.WriteLine("Attempting queryfullprocessimagename...");
                //return GetExecutablePathAboveVista(Process.GetCurrentProcess().Handle);
                return GetApplicationProcessPath();
            }
        }

        public string GetProcessApplicationName(IntPtr hWnd) {
            string mwt = GetProcessMainWindowTitle(hWnd);
            if (string.IsNullOrEmpty(mwt)) {
                return mwt;
            }
            var mwta = mwt.Split(new string[] { "-" },StringSplitOptions.RemoveEmptyEntries);
            if (mwta.Length == 1) {
                if(string.IsNullOrEmpty(mwta[0])) {
                    return "Explorer";
                }
                return mwta[0];
            }
            return mwta[mwta.Length - 1].Trim();
        }

        private static string GetExecutablePathAboveVista(IntPtr dwProcessId) {
            StringBuilder buffer = new StringBuilder(1024);
            IntPtr hprocess = WinApi.OpenProcess(WinApi.ProcessAccessFlags.QueryLimitedInformation, false, (int)dwProcessId);
            if (hprocess != IntPtr.Zero) {
                try {
                    int size = buffer.Capacity;
                    if (WinApi.QueryFullProcessImageName(hprocess, 0, buffer, ref size)) {
                        return buffer.ToString(0, size);
                    }
                } finally {
                    WinApi.CloseHandle(hprocess);
                }
            }
            return string.Empty;
        }

        public string GetProcessPath(IntPtr hwnd) { 
            try {
                if (hwnd == null || hwnd == IntPtr.Zero) {
                    return GetApplicationProcessPath();
                }

                WinApi.GetWindowThreadProcessId(hwnd, out uint pid);
                using (Process proc = Process.GetProcessById((int)pid)) {
                    if(proc.ProcessName == @"csrss") {
                        //occurs with messageboxes and dialogs
                        return GetApplicationProcessPath();
                    }
                    if(proc.MainWindowHandle == IntPtr.Zero) {
                        return GetApplicationProcessPath();
                    }
                    return proc.MainModule.FileName.ToString();
                }
            }
            catch (Exception e) {
                MonkeyPaste.MpConsole.WriteLine("MpHelpers.Instance.GetProcessPath error (likely) cannot find process path (w/ Handle "+hwnd.ToString()+") : " + e.ToString());
                //return GetExecutablePathAboveVista(hwnd);
                return GetApplicationProcessPath();
            }
        }

        public string GetProcessMainWindowTitle(IntPtr hWnd) {
            try {
                if (hWnd == null || hWnd == IntPtr.Zero) {
                    return "Unknown Application";
                }
                //uint processId;
                //WinApi.GetWindowThreadProcessId(hWnd, out processId);
                //using (Process proc = Process.GetProcessById((int)processId)) {
                //    return proc.MainWindowTitle;
                //}
                int length = WinApi.GetWindowTextLength(hWnd);
                if (length == 0) {
                    return string.Empty;
                }

                StringBuilder builder = new StringBuilder(length);
                WinApi.GetWindowText(hWnd, builder, length + 1);

                return builder.ToString();
            }
            catch(Exception ex) {
                return "MpHelpers.GetProcessMainWindowTitle Exception: "+ex.ToString();
            }
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

        public bool IsPathDirectory(string str) {
            // get the file attributes for file or directory
            return File.GetAttributes(str).HasFlag(FileAttributes.Directory);
        }

        
        #endregion

        #region Visual

        public void ResizeImages(string sourceDir,string targetDir, double newWidth,double newHeight) {
            if(!Directory.Exists(sourceDir)) {
                throw new DirectoryNotFoundException(sourceDir);
            }
            foreach(var f in Directory.GetFiles(sourceDir)) {
                if(Directory.Exists(f)) {
                    continue;
                }
                var bmpSrc = ReadImageFromFile(f);
                var newSize = new Size(newWidth, newHeight);

                if (bmpSrc.Width != bmpSrc.Height) {
                    if(bmpSrc.Width > bmpSrc.Height) {
                        newSize.Height *= bmpSrc.Height / bmpSrc.Width;
                    } else {
                        newSize.Width *= bmpSrc.Width / bmpSrc.Height;
                    }
                }
                var rbmpSrc = ResizeBitmapSource(bmpSrc, newSize);
                string targetPath = Path.Combine(targetDir, Path.GetFileName(f));
                WriteBitmapSourceToFile(targetPath, rbmpSrc);
            }
        }

        public void PrintVisualTree(int depth, object obj) {
            // Print the object with preceding spaces that represent its depth
            Trace.WriteLine(new string(' ', depth) + obj.GetType().ToString());

            // If current element is a grid, display information about its rows and columns
            if (obj is Grid) {
                Grid gd = (Grid)obj;
                Trace.WriteLine(new string(' ', depth) + "Grid has " + gd.RowDefinitions.Count +
                                " rows and " + gd.ColumnDefinitions.Count + " columns.");
                foreach (UIElement element in gd.Children) {
                    Trace.WriteLine(new string(' ', depth) +
                        element.GetType().ToString() + " in row " + Grid.GetRow(element) +
                        " column " + Grid.GetColumn(element));
                }
            }

            // Recursive call for each visual child
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                PrintVisualTree(depth + 1, VisualTreeHelper.GetChild(obj as DependencyObject, i));
        }

        public List<string> CreatePrimaryColorList(BitmapSource bmpSource,int palleteSize = 5) {
            //var sw = new Stopwatch();
            //sw.Start();
            var primaryIconColorList = new List<string>();
            var hist = MpImageHistogram.Instance.GetStatistics(bmpSource);
            foreach (var kvp in hist) {
                var c = Color.FromArgb(255, kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue);

                //MonkeyPaste.MpConsole.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if (primaryIconColorList.Count == palleteSize) {
                    break;
                }
                //between 0-255 where 0 is black 255 is white
                var rgDiff = Math.Abs((int)c.R - (int)c.G);
                var rbDiff = Math.Abs((int)c.R - (int)c.B);
                var gbDiff = Math.Abs((int)c.G - (int)c.B);
                var totalDiff = rgDiff + rbDiff + gbDiff;

                //0-255 0 is black
                var grayScaleValue = 0.2126 * (int)c.R + 0.7152 * (int)c.G + 0.0722 * (int)c.B;
                var relativeDist = primaryIconColorList.Count == 0 ? 1 : MpHelpers.Instance.ColorDistance(MpHelpers.Instance.ConvertHexToColor(primaryIconColorList[primaryIconColorList.Count - 1]), c);
                if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
                    primaryIconColorList.Add(MpHelpers.Instance.ConvertColorToHex(c));
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < palleteSize; i++) {
                primaryIconColorList.Add(MpHelpers.Instance.ConvertColorToHex(MpHelpers.Instance.GetRandomColor()));
            }
            //sw.Stop();
            //MonkeyPaste.MpConsole.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }

        public async Task<List<string>> CreatePrimaryColorListAsync(BitmapSource bmpSource, int palleteSize = 5, DispatcherPriority priority = DispatcherPriority.Normal) {
            List<string> result = null;
            
            await Task.Run(() => {
                result = CreatePrimaryColorList(bmpSource, palleteSize);
            });

            return result;
        }

        public BitmapSource CreateBorder(BitmapSource img, double scale, Color bgColor) {
            var borderBmpSrc = MpHelpers.Instance.TintBitmapSource(img, bgColor, true);
            //var borderSize = new Size(borderBmpSrc.Width * scale, bordherBmpSrc.Height * scale);
            return MpHelpers.Instance.ScaleBitmapSource(borderBmpSrc, new Size(scale,scale));
        }

        public async Task<BitmapSource> CreateBorderAsync(BitmapSource img, double scale, Color bgColor) {
            var borderBmpSrc = await MpHelpers.Instance.TintBitmapSourceAsync(img, bgColor, true);
            //var borderSize = new Size(borderBmpSrc.Width * scale, bordherBmpSrc.Height * scale);
            return MpHelpers.Instance.ScaleBitmapSource(borderBmpSrc, new Size(scale, scale));
        }

        public BitmapSource CopyScreen() {
            double left = 0;//System.Windows.Forms.Screen.AllScreens.Min(screen => screen.Bounds.X);
            double top = 0;// System.Windows.Forms.Screen.AllScreens.Min(screen => screen.Bounds.Y);
            double right = MpMeasurements.Instance.ScreenWidth * MpPreferences.Instance.ThisAppDip;//System.Windows.Forms.Screen.AllScreens.Max(screen => screen.Bounds.X + screen.Bounds.Width);
            double bottom = MpMeasurements.Instance.ScreenHeight * MpPreferences.Instance.ThisAppDip;//System.Windows.Forms.Screen.AllScreens.Max(screen => screen.Bounds.Y + screen.Bounds.Height);
            int width = (int)(right - left);
            int height = (int)(bottom - top);

            using (var screenBmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                using (var bmpGraphics = System.Drawing.Graphics.FromImage(screenBmp)) {
                    bmpGraphics.CopyFromScreen((int)left, (int)top, 0, 0, new System.Drawing.Size(width, height));
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }       

        public IList<T> GetRandomizedList<T>(IList<T> orderedList) where T : class {
            var preRandomList = new List<T>();
            foreach (var c in orderedList) {
                preRandomList.Add(c);
            }
            var randomList = new List<T>();
            for (int i = 0; i < orderedList.Count; i++) {
                int randIdx = MpHelpers.Instance.Rand.Next(0, preRandomList.Count - 1);
                var t = preRandomList[randIdx];
                preRandomList.RemoveAt(randIdx);
                randomList.Add(t);
            }
            return randomList;
        }

        

        public Brush GetContentColor(int c, int r) {
            return MpThemeColors.Instance.ContentColors[c][r];
        }

        public void SetColorChooserMenuItem(
            ContextMenu cm,
            MenuItem cmi,
            MouseButtonEventHandler selectedEventHandler) {
            var cmic = new Canvas();
            var _ContentColors = MpThemeColors.Instance.ContentColors; 
            double s = 15;
            double pad = 2.5;
            double w = (_ContentColors.Count * (s + pad)) + pad;
            double h = (_ContentColors[0].Count * (s + pad)) + pad;
            for (int x = 0; x < _ContentColors.Count; x++) {
                for (int y = 0; y < _ContentColors[0].Count; y++) {
                    Border b = new Border();
                    if(x == _ContentColors.Count -1 && y == _ContentColors[0].Count - 1) {
                        var addBmpSrc = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/add2.png"));
                        b.Background = new ImageBrush(addBmpSrc);
                        MouseButtonEventHandler bMouseLeftButtonUp = (object o, MouseButtonEventArgs e3) => {
                            var result = MpHelpers.Instance.ShowColorDialog(GetRandomBrushColor());
                            if (result != null) {
                                b.Tag = result;
                            }
                        };
                        b.MouseLeftButtonUp += bMouseLeftButtonUp;

                        RoutedEventHandler bUnload = null;
                        bUnload = (object o, RoutedEventArgs e) =>{
                            b.MouseLeftButtonUp -= bMouseLeftButtonUp;
                            b.Unloaded -= bUnload;
                        };
                        b.Unloaded += bUnload;
                    } else {
                        b.Background = _ContentColors[x][y];
                        b.Tag = b.Background;
                    }
                    
                    b.BorderThickness = new Thickness(1.5);
                    b.BorderBrush = Brushes.DarkGray;
                    b.CornerRadius = new CornerRadius(2);
                    b.Width = b.Height = s;

                    MouseEventHandler bMouseEnter = (object o, MouseEventArgs e3) => {
                        b.BorderBrush = Brushes.DimGray;
                    };

                    MouseEventHandler bMouseLeave = (object o, MouseEventArgs e3) => {
                        b.BorderBrush = Brushes.DarkGray;
                    };
                    b.MouseEnter += bMouseEnter;

                    RoutedEventHandler bGotFocus = (object o, RoutedEventArgs e3) => {
                        b.BorderBrush = Brushes.DimGray;
                    };
                    b.GotFocus += bGotFocus;

                    b.MouseLeave += bMouseLeave;

                    b.MouseLeftButtonUp += selectedEventHandler;

                    RoutedEventHandler bUnloaded = null;
                    bUnloaded = (object o, RoutedEventArgs e3) => {
                        b.MouseEnter -= bMouseEnter;
                        b.MouseLeave -= bMouseLeave;
                        b.GotFocus -= bGotFocus;
                        b.MouseLeftButtonUp -= selectedEventHandler;
                        b.Unloaded -= bUnloaded;
                    };

                    b.Unloaded += bUnloaded;

                    RoutedEventHandler cmClosed = null;
                    cmClosed = (object o, RoutedEventArgs e3) => {
                        b.MouseEnter -= bMouseEnter;
                        b.MouseLeave -= bMouseLeave;
                        b.GotFocus -= bGotFocus;
                        b.MouseLeftButtonUp -= selectedEventHandler;
                        b.Unloaded -= bUnloaded;
                        cm.Closed -= cmClosed;
                    };
                    cm.Closed += cmClosed;

                    cmic.Children.Add(b);

                    Canvas.SetLeft(b, (x * (s + pad)) + pad);
                    Canvas.SetTop(b, (y * (s + pad)) + pad);
                }
            }
            cmic.Background = Brushes.Transparent;
            cmi.Header = cmic;
            cmi.Height = h;
            cmi.Style = (Style)Application.Current.MainWindow.FindResource("ColorPalleteMenuItemStyle");
            cm.Width = 300;
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

        public double DistanceBetweenValues(double a,double b) {
            return Math.Abs(Math.Abs(b) - Math.Abs(a));
        }
        public DoubleAnimation AnimateDoubleProperty(
            double from, 
            double to, 
            double dt, 
            object obj, 
            DependencyProperty property, 
            EventHandler onCompleted) {
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
                    if(fe == null) {
                        continue;
                    }
                    fe.BeginAnimation(property, animation);
                }
            } else {
                ((FrameworkElement)obj)?.BeginAnimation(property, animation);
            }

            return animation;
        }

        public void AnimateVisibilityChange(
            object obj, 
            Visibility tv, 
            EventHandler onCompleted, 
            double ms = 1000, 
            double bt = 0) {
            var da = new DoubleAnimation {
                Duration = new Duration(TimeSpan.FromMilliseconds(ms))
            };
            var easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            da.EasingFunction = easing;

            da.Completed += (o, e) => {
                if (obj.GetType() == typeof(List<FrameworkElement>)) {
                    foreach (var fe in (List<FrameworkElement>)obj) {
                        if(fe == null) {
                            continue;
                        }
                        fe.Visibility = tv;
                    }
                } else if(obj != null) {
                    ((FrameworkElement)obj).Visibility = tv;
                }
            };
            if(onCompleted != null) {
                da.Completed += onCompleted;
            }
            
            da.From = tv == Visibility.Visible ? 0 : 1;
            da.To = tv == Visibility.Visible ? 1 : 0;
            da.BeginTime = TimeSpan.FromMilliseconds(bt);

            if (tv == Visibility.Visible) {
                if (obj.GetType() == typeof(List<FrameworkElement>)) {
                    foreach (var fe in (List<FrameworkElement>)obj) {
                        if (fe == null) {
                            continue;
                        }
                        fe.Opacity = 0;
                        fe.Visibility = Visibility.Visible;
                    }
                } else if(obj != null) {
                    ((FrameworkElement)obj).Opacity = 0;
                    ((FrameworkElement)obj).Visibility = Visibility.Visible;
                }
            }

            if (obj.GetType() == typeof(List<FrameworkElement>)) {
                foreach (var fe in (List<FrameworkElement>)obj) {
                    if(fe == null) {
                        continue;
                    }
                    fe.BeginAnimation(FrameworkElement.OpacityProperty, da);
                    if(onCompleted != null) {
                        // this ensures the oncompleted is only called ONCE for the items
                        da = da.Clone();
                        da.Completed -= onCompleted;
                    }
                }
            } else if(obj != null) {
                ((FrameworkElement)obj).BeginAnimation(FrameworkElement.OpacityProperty, da);
            }
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

        public Brush ShowColorDialog(Brush currentBrush,bool showFullOpen = false) {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowHelp = true;
            cd.Color = MpHelpers.Instance.ConvertSolidColorBrushToWinFormsColor((SolidColorBrush)currentBrush);
            cd.CustomColors = Properties.Settings.Default.UserCustomColorIdxArray;
            cd.FullOpen = showFullOpen;
            var mw = (MpMainWindow)Application.Current.MainWindow;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = true;
            // Update the text box color if the user clicks OK 
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                return MpHelpers.Instance.ConvertWinFormsColorToSolidColorBrush(cd.Color);
            }
            return null;
        }

        public BitmapSource TintBitmapSource(BitmapSource bmpSrc, Color tint, bool retainAlpha = false) {
            BitmapSource formattedBmpSrc = null;
            if(bmpSrc.Width != bmpSrc.PixelWidth || bmpSrc.Height != bmpSrc.PixelHeight) {
                //means bmp dpi isn't 96
                double dpi = 96;
                int width = bmpSrc.PixelWidth;
                int height = bmpSrc.PixelHeight;

                int stride = width * 4; // 4 bytes per pixel
                byte[] pixelData = new byte[stride * height];
                bmpSrc.CopyPixels(pixelData, stride, 0);

                formattedBmpSrc = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
            } else {
                formattedBmpSrc = bmpSrc;
            }
            var bmp = new WriteableBitmap(formattedBmpSrc);
            var pixels = GetPixels(bmp);
            var pixelColor = new PixelColor[1, 1];
            pixelColor[0, 0] = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    PixelColor c = pixels[x, y];
                    if (c.Alpha > 0) {
                        if(retainAlpha) {
                            pixelColor[0, 0].Alpha = c.Alpha;
                        }
                        PutPixels(bmp, pixelColor, x, y);
                    }
                }
            }
            return bmp;
        }

        public async Task<BitmapSource> TintBitmapSourceAsync(BitmapSource bmpSrc, Color tint, bool retainAlpha = false, DispatcherPriority priority = DispatcherPriority.Background) {
            BitmapSource bmpSource = null;
            await Task.Run(() => {
                bmpSource = TintBitmapSource(bmpSrc, tint, retainAlpha);
            });
            return bmpSource;
        }

        public double ColorDistance(Color e1, Color e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = ((long)e1.R + (long)e2.R) / 2;
            long r = (long)e1.R - (long)e2.R;
            long g = (long)e1.G - (long)e2.G;
            long b = (long)e1.B - (long)e2.B;
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }

        public Color ConvertHexToColor(string hexString) {
            if (hexString.IndexOf('#') != -1) {
                hexString = hexString.Replace("#", string.Empty);
            }
            //
            int x = hexString.Length == 8 ? 2 : 0;
            byte r = byte.Parse(hexString.Substring(x, 2), NumberStyles.AllowHexSpecifier);
            byte g = byte.Parse(hexString.Substring(x+2, 2), NumberStyles.AllowHexSpecifier);
            byte b = byte.Parse(hexString.Substring(x+4, 2), NumberStyles.AllowHexSpecifier);
            byte a = x > 0 ? byte.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier) : (byte)255;
            return Color.FromArgb(a, r, g, b);
        }

        public string ConvertColorToHex(Color c, byte forceAlpha = 255) {
            if(c == null) {
                return "#FF0000";
            }
            c.A = forceAlpha;
            return c.ToString();
        }

        public BitmapSource GetIconImage(string sourcePath) {
            BitmapSource iconBmp = new BitmapImage();
            try {
                if (!File.Exists(sourcePath)) {
                    if (!Directory.Exists(sourcePath)) {
                        //return (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/monkey (2).png"));
                        //return ConvertBitmapToBitmapSource(System.Drawing.SystemIcons.Question.ToBitmap());
                        iconBmp = ConvertBitmapToBitmapSource(System.Drawing.SystemIcons.Exclamation.ToBitmap());
                    } else {
                        iconBmp = GetBitmapFromFolderPath(sourcePath, IconSizeEnum.MediumIcon32);
                    }

                } else {
                    iconBmp = GetBitmapFromFilePath(sourcePath, IconSizeEnum.MediumIcon32);
                }                
            }
            catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
                iconBmp = ConvertBitmapToBitmapSource(System.Drawing.SystemIcons.Question.ToBitmap());
            }
            return iconBmp;
        }

        public BitmapSource ScaleBitmapSource(BitmapSource bmpSrc, Size newScale) {
            try {
                var sbmpSrc = new TransformedBitmap(bmpSrc, new ScaleTransform(newScale.Width, newScale.Height));
                return sbmpSrc;
            } catch(Exception ex) {
                MpConsole.WriteTraceLine("Error scaling bmp", ex);
                return bmpSrc;
            }
        }

        public BitmapSource ResizeBitmapSource(BitmapSource bmpSrc, Size newSize) {
            try {
                double sw = newSize.Width / bmpSrc.Width;
                double sh = newSize.Height / bmpSrc.Height;
                var rbmpSrc = new TransformedBitmap(bmpSrc, new ScaleTransform(sw,sh));
                return rbmpSrc;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Error scaling bmp", ex);
                return bmpSrc;
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

        public bool IsBright(Color c, int brightThreshold = 150) {
            int grayVal = (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
            return grayVal > brightThreshold;
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

        public Brush GetDarkerBrush(Brush b, double factor = -0.5) {
            return ChangeBrushBrightness((SolidColorBrush)b, factor);
        }

        public Brush GetLighterBrush(Brush b, double factor = 0.5) {
            return ChangeBrushBrightness((SolidColorBrush)b, factor);
        }

        public Color GetRandomColor(byte alpha = 255) {
            //if (alpha == 255) {
            //    return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            //}
            //return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));

            //int x = Rand.Next(0, _ContentColors.Count);
            //int y = Rand.Next(0, _ContentColors[0].Count);
            //return ((SolidColorBrush)_ContentColors[x][y]).Color;

            return new MpContentColors().GetRandomColor();
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

        public BitmapSource MergeImages2(IList<BitmapSource> bmpSrcList,bool scaleToSmallestSize = false, bool scaleToLargestDpi = true) {
            // if not scaled to smallest, will be scaled to largest
            int w = scaleToSmallestSize ? bmpSrcList.Min(x => x.PixelWidth) : bmpSrcList.Max(x => x.PixelWidth);
            int h = scaleToSmallestSize ? bmpSrcList.Min(x => x.PixelHeight) : bmpSrcList.Max(x => x.PixelHeight);

            double dpiX = scaleToLargestDpi ? bmpSrcList.Max(x => x.DpiX) : bmpSrcList.Min(x => x.DpiX);
            double dpiY = scaleToLargestDpi ? bmpSrcList.Max(x => x.DpiY) : bmpSrcList.Max(x=>x.DpiY);

            for (int i = 0;i < bmpSrcList.Count;i++) {
                BitmapSource bmp = bmpSrcList[i];
                if(bmp.PixelWidth != w || bmp.PixelHeight != h) {
                    bmpSrcList[i] = ScaleBitmapSource(bmp, new Size(w / bmp.PixelWidth, h / bmp.PixelHeight));
                }
            }

            var renderTargetBitmap = new RenderTargetBitmap(w, h, dpiX, dpiY, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen()) {
                foreach (var image in bmpSrcList) {
                    drawingContext.DrawImage(image, new Rect(0, 0, w, h));
                }
            }
            renderTargetBitmap.Render(drawingVisual);



            return ConvertRenderTargetBitmapToBitmapSource(renderTargetBitmap);
        }

        public BitmapSource MergeImages(IList<BitmapSource> bmpSrcList, Size size = default) {
            // from https://stackoverflow.com/a/14661969/105028
            size = size == default ? new Size(32, 32) : size;

            // Gets the size of the images (I assume each image has the same size)

            // Draws the images into a DrawingVisual component
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
                foreach(BitmapSource bmpSrc in bmpSrcList) {
                    Size scale = new Size(size.Width / (double)bmpSrc.PixelWidth, size.Height / (double)bmpSrc.PixelHeight);
                    var rbmpSrc = ScaleBitmapSource(bmpSrc, scale);
                    drawingContext.DrawImage(rbmpSrc,new Rect(0, 0, (int)size.Width, (int)size.Width));
                }
            }

            // Converts the Visual (DrawingVisual) into a BitmapSource
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            return ConvertRenderTargetBitmapToBitmapSource(bmp);
        }




        public async Task<BitmapSource> MergeImagesAsync(IList<BitmapSource> bmpSrcList, DispatcherPriority priority = DispatcherPriority.Background) {
            BitmapSource mergedImage = null;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                mergedImage = MergeImages(bmpSrcList);
            }, priority);
            return mergedImage;
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
            if(bmpSrcList.Count == 0) {
                return new BitmapImage();
            }
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
        public BitmapSource ConvertBitmapSourceToGrayScale(BitmapSource bmpSrc) {
            var grayScaleSsBmp = new FormatConvertedBitmap();

            // BitmapSource objects like FormatConvertedBitmap can only have their properties
            // changed within a BeginInit/EndInit block.
            grayScaleSsBmp.BeginInit();

            // Use the BitmapSource object defined above as the source for this new
            // BitmapSource (chain the BitmapSource objects together).
            grayScaleSsBmp.Source = bmpSrc;

            // Set the new format to Gray32Float (grayscale).
            grayScaleSsBmp.DestinationFormat = PixelFormats.Gray32Float;
            grayScaleSsBmp.EndInit();
            return grayScaleSsBmp;
        }
        public BitmapSource ConvertFlowDocumentToBitmap(FlowDocument document, Size size, Brush bgBrush = null) {
            if (size.Width <= 0) {
                size.Width = 1;
            }
            if (size.Height <= 0) {
                size.Height = 1;
            }
            var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow);
            size.Width *= dpi.DpiScaleX;
            size.Height *= dpi.DpiScaleY;

            document.PagePadding = new Thickness(0);
            document.ColumnWidth = size.Width;
            document.PageWidth = size.Width;
            document.PageHeight = size.Height;

            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            paginator.PageSize = size;

            var visual = new DrawingVisual();
            using (var drawingContext = visual.RenderOpen()) {
                // draw white background
                drawingContext.DrawRectangle(bgBrush ?? Brushes.White, null, new Rect(size));
            }
            visual.Children.Add(paginator.GetPage(0).Visual);
            var bitmap = new RenderTargetBitmap(
                (int)size.Width, 
                (int)size.Height,
                dpi.PixelsPerInchX, 
                dpi.PixelsPerInchY, 
                PixelFormats.Pbgra32);
            
            bitmap.Render(visual);
            //RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);
            return bitmap;
        }

        public List<List<Key>> ConvertStringToKeySequence(string keyStr) {
            var keyList = new List<List<Key>>();
            var combos = keyStr.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            foreach (var c in combos) {
                var keys = c.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                keyList.Add(new List<Key>());
                foreach (var k in keys) {
                    keyList[keyList.Count - 1].Add(MpHelpers.Instance.ConvertStringToKey(k));
                }
            }
            return keyList;
        }

        public string ConvertKeySequenceToString(List<List<Key>> keyList) {
            var outStr = string.Empty;
            foreach (var kl in keyList) {
                if (!string.IsNullOrEmpty(outStr)) {
                    outStr += ", ";
                }
                foreach (var k in kl) {
                    outStr += GetKeyLiteral(k) + "+";
                }
                outStr = outStr.Remove(outStr.Length - 1, 1);
            }
            if (!string.IsNullOrEmpty(outStr)) {
                if (outStr.EndsWith(", ")) {
                    outStr = outStr.Remove(outStr.Length - 2, 2);
                }
            }
            return outStr;
        }

        public Key ConvertStringToKey(string keyStr) {
            string lks = keyStr.ToLower();
            if(lks == "control") {
                return Key.LeftCtrl;
            }
            if(lks == "alt") {
                return Key.LeftAlt;
            }
            if (lks == "shift") {
                return Key.LeftShift;
            }
            if (lks == ";") {
                return Key.Oem1;
}
            if (lks == "`") {
                return Key.Oem3;
}
            if (lks == "'") {
                return Key.OemQuotes;
            }
            if (lks == "-") {
                return Key.OemMinus;
            }
            if (lks == "=") {
                return Key.OemPlus;
            }
            if (lks == ",") {
                return Key.OemComma;
            }
            if (lks == @"/") {
                return Key.OemQuestion;
            }
            if (lks == ".") {
                return Key.OemPeriod;
            }
            if (lks == "[") {
                return Key.OemOpenBrackets;
            }
            if (lks == "]") {
                return Key.Oem6;
            }
            if (lks == "|") {
                return Key.Oem5;
            }
            if (lks == "PageDown") {
                return Key.Next;
            }
            return (Key)Enum.Parse(typeof(Key), keyStr, true);
        }

        public string ConvertKeyToString(Key key) {
            if(key == Key.LeftCtrl || key == Key.RightCtrl) {
                return "Control";
            }
            if(key == Key.LeftAlt || key == Key.RightAlt || key == Key.System) {
                return "Alt";
            }
            if(key == Key.LeftShift || key == Key.RightShift) {
                return "Shift";
            }
            
            return key.ToString();
        }

        public string GetKeyLiteral(Key key) {
            if (key == Key.LeftShift) {
                return "Shift";
            }
            if (key == Key.LeftAlt) {
                return "Alt";
            }
            if (key == Key.LeftCtrl) {
                return "Control";
            }
            if (key == Key.Oem1) {
                return ";";
            }
            if (key == Key.Oem3) {
                return "`";
            }
            if (key == Key.OemQuotes) {
                return "'";
            }
            if (key == Key.OemMinus) {
                return "-";
            }
            if (key == Key.OemPlus) {
                return "=";
            }
            if (key == Key.OemComma) {
                return ",";
            }
            if (key == Key.OemQuestion) {
                return @"/";
            }
            if (key == Key.OemPeriod) {
                return ".";
            }
            if (key == Key.OemOpenBrackets) {
                return "[";
            }
            if (key == Key.Oem6) {
                return "]";
            }
            if (key == Key.Oem5) {
                return "|";
            }
            if (key == Key.Next) {
                return "PageDown";
            }
            return key.ToString();
        }

        public System.Windows.Input.Key WinformsToWPFKey(System.Windows.Forms.Keys formsKey) {
            
            // Put special case logic here if there's a key you need but doesn't map...  
            try {
                return KeyInterop.KeyFromVirtualKey((int)formsKey);
            }
            catch {
                // There wasn't a direct mapping...    
                return System.Windows.Input.Key.None;
            }
        }

        public System.Windows.Forms.Keys WpfKeyToWinformsKey(Key wpfKey) {

            // Put special case logic here if there's a key you need but doesn't map...  
            try {
                return (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(wpfKey);
            }
            catch {
                // There wasn't a direct mapping...    
                return System.Windows.Forms.Keys.None;
            }
        }


        public string ConvertFlowDocumentToXaml(MpEventEnabledFlowDocument fd) {
            TextRange range = new TextRange(fd.ContentStart, fd.ContentEnd);
            using (MemoryStream stream = new MemoryStream()) {
                range.Save(stream, DataFormats.Xaml);
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

        public async Task<MpEventEnabledFlowDocument> ConvertXamlToFlowDocumentAsync(string xaml, DispatcherPriority priority = DispatcherPriority.Background) {
            var doc = new MpEventEnabledFlowDocument();
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                using (var stringReader = new StringReader(xaml)) {
                    var xmlReader = XmlReader.Create(stringReader);
                    //if (!IsStringFlowSection(xaml)) {
                    //    return (MpEventEnabledFlowDocument)XamlReader.Load(xmlReader);
                    //}
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
                }
            }, priority);

            return doc;
        }

        public string ConvertPlainTextToRichText(string plainText) {
            using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                rtb.Text = plainText;
                rtb.Font = new System.Drawing.Font(Properties.Settings.Default.DefaultFontFamily, (float)Properties.Settings.Default.DefaultFontSize);
                return rtb.Rtf;
            }                
        }

        public async Task<string> ConvertPlainTextToRichTextAsync(string plainText, DispatcherPriority priority = DispatcherPriority.Background) {
            var rtfString = string.Empty;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                        using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                            rtb.Text = plainText;
                            rtb.Font = new System.Drawing.Font(Properties.Settings.Default.DefaultFontFamily, (float)Properties.Settings.Default.DefaultFontSize);
                            rtfString = rtb.Rtf;
                        }
                    }, priority);
            return rtfString;
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
                try {
                    //var rtb = new RichTextBox();
                    var fd = new FlowDocument();
                    fd.SetRtf(richText);
                    string pt = new TextRange(fd.ContentStart, fd.ContentEnd).Text;
                    int rtcount = GetRowCount(richText);
                    int ptcount = GetRowCount(pt);
                    if (rtcount != ptcount) {
                        pt = pt.Trim(new char[] { '\r', '\n' });
                    }
                    return pt;
                }
                catch (Exception ex) {
                    MonkeyPaste.MpConsole.WriteLine("ConvertRichTextToPlainText Exception, fallingt back to WinForms Rtb...");
                    MonkeyPaste.MpConsole.WriteLine("Exception was: " + ex.ToString());
                    //rtb.SetRtf throws an exception when richText is from excel (contains cell information?)
                    //so falling back winforms richtextbox
                    using (System.Windows.Forms.RichTextBox wf_rtb = new System.Windows.Forms.RichTextBox()) {
                        wf_rtb.Rtf = richText;
                        return wf_rtb.Text;
                    }

                }
            } else {
                return richText;
            }
        }

        public async Task<string> ConvertRichTextToPlainTextAsync(string richText, DispatcherPriority priority = DispatcherPriority.Background) {
            var plainText = richText;
            if (IsStringRichText(richText)) {
                await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                    plainText = ConvertRichTextToPlainText(richText);
                }, priority);               
            }
            return plainText;
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
            if(string.IsNullOrEmpty(rtf)) {
                return string.Empty.ToRichText().ToFlowDocument();
            }
            
            if(IsStringRichText(rtf)) {
                //using (var stream = new MemoryStream(Encoding.Default.GetBytes(rtf))) {
                using (var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(rtf))) {
                    try {
                        var flowDocument = new MpEventEnabledFlowDocument();
                        var range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                        range.Load(stream, System.Windows.DataFormats.Rtf);

                        var tr = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                        var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                        if(rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
                            //ignore to r
                        } else if((TextAlignment)rtbAlignment == TextAlignment.Justify) {
                            tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                        }
                        var ps = flowDocument.GetDocumentSize();
                        flowDocument.PageWidth = ps.Width;
                        flowDocument.PageHeight = ps.Height;
                        flowDocument.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                        flowDocument.ConfigureLineHeight();
                        return flowDocument;
                    }
                    catch(Exception ex) {
                        MonkeyPaste.MpConsole.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...");
                        MonkeyPaste.MpConsole.WriteLine("Exception Details: " + ex);
                        return rtf.ToPlainText().ToFlowDocument();
                    }
                }
            } else if(IsStringPlainText(rtf)) {
                return ConvertRichTextToFlowDocument(rtf.ToRichText());
            }
            return ConvertXamlToFlowDocument(rtf);
        }

        public async Task<MpEventEnabledFlowDocument> ConvertRichTextToFlowDocumentAsync(string rtf, DispatcherPriority priority = DispatcherPriority.Background) {
            if (IsStringRichText(rtf)) {
                var flowDocument = new MpEventEnabledFlowDocument();
                await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                    flowDocument = ConvertRichTextToFlowDocument(rtf);
                }, priority);
                return flowDocument;
            }
            return await ConvertXamlToFlowDocumentAsync(rtf,priority);
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
            RichTextBox rtb = null;
            TextSelection rtbSelection = null;
            if(fd.Parent != null && fd.Parent.GetType() == typeof(RichTextBox)) {
                rtb = (RichTextBox)fd.Parent;
                rtbSelection = rtb.Selection;
            }
            string rtf = string.Empty;
            using (var ms = new MemoryStream()) {
                try {
                    var range2 = new TextRange(fd.ContentStart, fd.ContentEnd);
                    range2.Save(ms, System.Windows.DataFormats.Rtf);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms)) {
                        rtf = sr.ReadToEnd();
                    }
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine("Error converting flow document to text: ", ex);
                    return rtf;
                }
            }
            if(rtb != null && rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return rtf;
        }

        public async Task<string> ConvertFlowDocumentToRichTextAsync(FlowDocument fd, DispatcherPriority priority = DispatcherPriority.Background) {
            var rtf = string.Empty;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                rtf = ConvertFlowDocumentToRichText(fd);
            }, priority);
            return rtf;
        }

        public string ConvertBitmapSourceToPlainTextAsciiArt(BitmapSource bmpSource) {
            string[] asciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
            using (System.Drawing.Bitmap image = ConvertBitmapSourceToBitmap(ScaleBitmapSource(bmpSource, new Size(MpMeasurements.Instance.ClipTileBorderMinSize, MpMeasurements.Instance.ClipTileContentHeight)))) {
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
            if (bs == null) {
                return null;
            }
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            using (MemoryStream stream = new MemoryStream()) {
                try {
                    var bf = System.Windows.Media.Imaging.BitmapFrame.Create(bs);                    
                    encoder.Frames.Add(bf);
                    encoder.Save(stream);
                    byte[] bit = stream.ToArray();
                    stream.Close();
                    return bit;
                }
                catch (Exception ex) {
                    MonkeyPaste.MpConsole.WriteLine("MpHelpers.ConvertBitmapSourceToByteArray exception: " + ex);
                    return null;
                }
                
            }
            
        }

        public async Task<byte[]> ConvertBitmapSourceToByteArrayAsync(BitmapSource bs, DispatcherPriority priority) {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            byte[] bit = null;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                using (MemoryStream stream = new MemoryStream()) {
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bs));
                    encoder.Save(stream);
                    bit = stream.ToArray();
                    stream.Close();
                }
            }, priority);            
            return bit;
        }

        public BitmapSource ConvertByteArrayToBitmapSource(byte[] bytes) {
            var bmpSrc = (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);
            bmpSrc.Freeze();
            return bmpSrc;
        }

        public async Task<BitmapSource> ConvertByteArrayToBitmapSourceAsync(byte[] bytes, DispatcherPriority priority) {
            BitmapSource bmpSource = null;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                bmpSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);
            }, priority);
            return bmpSource;
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
            //bitmap.Dispose();
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

        #endregion

        #region Http

        public string ExecuteCurl(string curlCommand, int timeoutInSeconds = 60) {
            if (string.IsNullOrEmpty(curlCommand))
                return "";

            curlCommand = curlCommand.Trim();

            // remove the curl keworkd
            if (curlCommand.StartsWith("curl")) {
                curlCommand = curlCommand.Substring("curl".Length).Trim();
            }

            // this code only works on windows 10 or higher
            {

                curlCommand = curlCommand.Replace("--compressed", "");

                // windows 10 should contain this file
                var fullPath = System.IO.Path.Combine(Environment.SystemDirectory, "curl.exe");

                if (System.IO.File.Exists(fullPath) == false) {
                    if (Debugger.IsAttached) { Debugger.Break(); }
                    throw new Exception("Windows 10 or higher is required to run this application");
                }

                // on windows ' are not supported. For example: curl 'http://ublux.com' does not work and it needs to be replaced to curl "http://ublux.com"
                List<string> parameters = new List<string>();

                // separate parameters to escape quotes
                try {
                    Queue<char> q = new Queue<char>();

                    foreach (var c in curlCommand.ToCharArray()) {
                        q.Enqueue(c);
                    }

                    StringBuilder currentParameter = new StringBuilder();

                    void insertParameter() {
                        var temp = currentParameter.ToString().Trim();
                        if (string.IsNullOrEmpty(temp) == false) {
                            parameters.Add(temp);
                        }

                        currentParameter.Clear();
                    }

                    while (true) {
                        if (q.Count == 0) {
                            insertParameter();
                            break;
                        }

                        char x = q.Dequeue();

                        if (x == '\'') {
                            insertParameter();

                            // add until we find last '
                            while (true) {
                                x = q.Dequeue();

                                // if next 2 characetrs are \' 
                                if (x == '\\' && q.Count > 0 && q.Peek() == '\'') {
                                    currentParameter.Append('\'');
                                    q.Dequeue();
                                    continue;
                                }

                                if (x == '\'') {
                                    insertParameter();
                                    break;
                                }

                                currentParameter.Append(x);
                            }
                        } else if (x == '"') {
                            insertParameter();

                            // add until we find last "
                            while (true) {
                                x = q.Dequeue();

                                // if next 2 characetrs are \"
                                if (x == '\\' && q.Count > 0 && q.Peek() == '"') {
                                    currentParameter.Append('"');
                                    q.Dequeue();
                                    continue;
                                }

                                if (x == '"') {
                                    insertParameter();
                                    break;
                                }

                                currentParameter.Append(x);
                            }
                        } else {
                            currentParameter.Append(x);
                        }
                    }
                }
                catch {
                    if (Debugger.IsAttached) { Debugger.Break(); }
                    throw new Exception("Invalid curl command");
                }

                StringBuilder finalCommand = new StringBuilder();

                foreach (var p in parameters) {
                    if (p.StartsWith("-")) {
                        finalCommand.Append(p);
                        finalCommand.Append(" ");
                        continue;
                    }

                    var temp = p;

                    if (temp.Contains("\"")) {
                        temp = temp.Replace("\"", "\\\"");
                    }
                    if (temp.Contains("'")) {
                        temp = temp.Replace("'", "\\'");
                    }

                    finalCommand.Append($"\"{temp}\"");
                    finalCommand.Append(" ");
                }


                using (var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "curl.exe",
                        Arguments = finalCommand.ToString(),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Environment.SystemDirectory
                    }
                }) {
                    proc.Start();

                    proc.WaitForExit(timeoutInSeconds * 1000);

                    return proc.StandardOutput.ReadToEnd();
                }
            }
        }


        public void OpenUrl(string url, bool openInNewWindow = true) {
            if (url.StartsWith(@"http") && !openInNewWindow) {
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
            if (body.GetType() == typeof(BitmapSource)) {
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = string.Format("<img src='{0}'>", MpHelpers.Instance.WriteBitmapSourceToFile(Path.GetTempPath(), (BitmapSource)body), true);
            } else {
                mailMessage.Body = (string)body;
            }

            if (!string.IsNullOrEmpty(attachmentPath)) {
                mailMessage.Attachments.Add(new Attachment(attachmentPath));
            }

            var filename = Path.GetTempPath() + "mymessage.eml";

            //save the MailMessage to the filesystem
            mailMessage.Save(filename);

            //Open the file with the default associated application registered on the local machine
            Process.Start(filename);
            return filename;
        }
        //public string GetLocalIp4Address() {
        //    Ping ping = new Ping();
        //    var replay = ping.Send(Dns.GetHostName());

        //    if (replay.Status == IPStatus.Success) {
        //        return replay.Address.MapToIPv4().ToString();
        //    }
        //    return null;
        //}
        public string GetLocalIp4Address() {
            return MonkeyPaste.MpHelpers.Instance.GetLocalIp4Address();
            //string localIP;
            //using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
            //    socket.Connect("8.8.8.8", 65530);
            //    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            //    localIP = endPoint.Address.ToString();
            //}
            //return localIP;
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in ipHostInfo.AddressList) { 
            //    if(ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast || ip.IsIPv6Teredo) {
            //        continue;
            //    }
            //    string a = ip.MapToIPv4().ToString();
            //    MonkeyPaste.MpConsole.WriteLine(a);
            //}
            //IPAddress ipAddress = ipHostInfo.AddressList[ipHostInfo.AddressList.Length - 1];
            //if (ipAddress != null) {
            //    return ipAddress.MapToIPv4().ToString();
            //}
            //return "0.0.0.0";
        }

        public string GetExternalIp4Address() {
            return MonkeyPaste.MpHelpers.Instance.GetExternalIp4Address();
            //return new System.Net.WebClient().DownloadString("https://api.ipify.org");
        }

        public bool IsConnectedToNetwork() {
            return MonkeyPaste.MpHelpers.Instance.IsConnectedToNetwork();            
        }

        public bool IsConnectedToInternet() {
            try {
                var client = new WebClient();
                var stream = client.OpenRead("http://www.google.com");
                bool isConnected = false;
                if(stream != null) {
                    isConnected = true;
                }
                stream?.Dispose();
                client?.Dispose();
                return isConnected;
            }
            catch (Exception e) {
                MonkeyPaste.MpConsole.WriteLine(e.ToString());
                return false;
            }
        }
        public async Task<string> GetUrlTitle(string url) {
            string urlSource = await GetHttpSourceCode(url);

            //sdf<title>poop</title>
            //pre 3
            //post 14
            return GetXmlElementContent(urlSource, @"title");
        }

        public string GetXmlElementContent(string xml, string element) {
            if (string.IsNullOrEmpty(xml) || string.IsNullOrEmpty(element)) {
                return string.Empty;
            }
            element = element.Replace(@"<", string.Empty).Replace(@"/>", string.Empty);
            element = @"<" + element + @">";
            var strl = xml.Split(new string[] { element }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if(strl.Count > 1) {
                element = element.Replace(@"<", @"</");
                return strl[1].Substring(0, strl[1].IndexOf(element));
            }
            return string.Empty;
            //int sIdx = xml.IndexOf(element);
            //if (sIdx < 0) {
            //    return string.Empty;
            //}
            //sIdx += element.Length;
            //element = element.Replace(@"<", @"</");
            //int eIdx = xml.IndexOf(element);
            //if (eIdx < 0) {
            //    return string.Empty;
            //}
            //return xml.Substring(sIdx, eIdx - sIdx);
        }
        
        public async Task<string> GetHttpSourceCode(string url) {
            if(!IsValidUrl(url)) {
                return string.Empty;
            }

            using (HttpClient client = new HttpClient()) {
                using (HttpResponseMessage response = client.GetAsync(url).Result) {
                    using (HttpContent content = response.Content) {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }        

        public bool IsValidUrl(string str) {
            bool hasValidExtension = false;
            string lstr = str.ToLower();
            foreach (var ext in _domainExtensions) {
                if (lstr.Contains(ext)) {
                    hasValidExtension = true;
                    break;
                }
            }
            if (!hasValidExtension) {
                return false;
            }
            var mc = MpRegEx.Instance.GetRegExForTokenType(MpSubTextTokenType.Uri).Match(str);//Regex.Match(str, , RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            return mc.Success;
        }

        public BitmapSource GetUrlFavicon(String url, int resolution = 128) {
            try {
                string urlDomain = MonkeyPaste.MpHelpers.Instance.GetUrlDomain(url);
                Uri favicon = new Uri(
                    string.Format(
                        @"https://www.google.com/s2/favicons?sz={0}&domain_url={1}",
                        resolution,
                        urlDomain)
                    , UriKind.Absolute);
                var img = new BitmapImage(favicon);
                if((img as BitmapSource).IsEqual(_defaultFavIcon)) {
                    return null;
                }
                return img;
            } catch(Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("MpHelpers.GetUrlFavicon error for url: " + url + " with exception: "+ex);
                return null;
            }
        }

        public BitmapSource ConvertUrlToQrCode(string url) {
            using (var qrGenerator = new QRCodeGenerator()) {
                using (var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q)) {
                    using (var qrCode = new QRCoder.PngByteQRCode(qrCodeData)) {
                        var qrCodeAsXaml = qrCode.GetGraphic(20);                        
                        //var bmpSrc= ConvertDrawingImageToBitmapSource(qrCodeAsXaml);
                        return MpHelpers.Instance.ScaleBitmapSource(qrCodeAsXaml.ToBitmapSource(), new Size(0.2, 0.2));
                    }
                }
            }
        }

        public BitmapSource ConvertDrawingImageToBitmapSource(DrawingImage source) {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen(); 
            drawingContext.DrawImage(source, new Rect(new Point(0, 0), new Size(source.Width, source.Height)));
            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)source.Width, (int)source.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual); 
            return bmp;
        }

        public BitmapSource ConvertStringToBitmapSource(string base64Str) {
            if(string.IsNullOrEmpty(base64Str) || !base64Str.IsBase64String()) {
                return new BitmapImage();
            }
            var bytes = System.Convert.FromBase64String(base64Str);
            return ConvertByteArrayToBitmapSource(bytes);
        }

        public string ConvertBitmapSourceToBase64String(BitmapSource bmpSrc) {
            var bytes = ConvertBitmapSourceToByteArray(bmpSrc);
            return Convert.ToBase64String(bytes);
        }
        #endregion

        #region Private Methods
        public PixelColor[,] GetPixels(BitmapSource source) {
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

        private string[] _quillTags = new string[] {
            "p",
            "ol",
            "li",
            "#text",
            "img",
            "em",
            "span",
            "strong",
            "u",
            "br",
            "a"
        };

        private string[] _domainExtensions = new string[] {
            // TODO try to sort these by common use to make more efficient
            ".com",
            ".org",
            ".gov",
            ".abbott",
            ".abogado",
            ".ac",
            ".academy",
            ".accountant",
            ".accountants",
            ".active",
            ".actor",
            ".ad",
            ".ads",
            ".adult",
            ".ae",
            ".aero",
            ".af",
            ".afl",
            ".ag",
            ".agency",
            ".ai",
            ".airforce",
            ".al",
            ".allfinanz",
            ".alsace",
            ".am",
            ".amsterdam",
            ".an",
            ".android",
            ".ao",
            ".apartments",
            ".aq",
            ".aquarelle",
            ".ar",
            ".archi",
            ".army",
            ".arpa",
            ".as",
            ".asia",
            ".associates",
            ".at",
            ".attorney",
            ".au",
            ".auction",
            ".audio",
            ".autos",
            ".aw",
            ".ax",
            ".axa",
            ".az",
            ".ba",
            ".band",
            ".bank",
            ".bar",
            ".barclaycard",
            ".barclays",
            ".bargains",
            ".bauhaus",
            ".bayern",
            ".bb",
            ".bbc",
            ".bd",
            ".be",
            ".beer",
            ".berlin",
            ".best",
            ".bf",
            ".bg",
            ".bh",
            ".bi",
            ".bid",
            ".bike",
            ".bingo",
            ".bio",
            ".biz",
            ".bj",
            ".bl",
            ".black",
            ".blackfriday",
            ".bloomberg",
            ".blue",
            ".bm",
            ".bmw",
            ".bn",
            ".bnpparibas",
            ".bo",
            ".boats",
            ".bond",
            ".boo",
            ".boutique",
            ".bq",
            ".br",
            ".brussels",
            ".bs",
            ".bt",
            ".budapest",
            ".build",
            ".builders",
            ".business",
            ".buzz",
            ".bv",
            ".bw",
            ".by",
            ".bz",
            ".bzh",
            ".ca",
            ".cab",
            ".cafe",
            ".cal",
            ".camera",
            ".camp",
            ".cancerresearch",
            ".canon",
            ".capetown",
            ".capital",
            ".caravan",
            ".cards",
            ".care",
            ".career",
            ".careers",
            ".cartier",
            ".casa",
            ".cash",
            ".casino",
            ".cat",
            ".catering",
            ".cbn",
            ".cc",
            ".cd",
            ".center",
            ".ceo",
            ".cern",
            ".cf",
            ".cfd",
            ".cg",
            ".ch",
            ".channel",
            ".chat",
            ".cheap",
            ".chloe",
            ".christmas",
            ".chrome",
            ".church",
            ".ci",
            ".citic",
            ".city",
            ".ck",
            ".cl",
            ".claims",
            ".cleaning",
            ".click",
            ".clinic",
            ".clothing",
            ".club",
            ".cm",
            ".cn",
            ".co",
            ".coach",
            ".codes",
            ".coffee",
            ".college",
            ".cologne",
            ".community",
            ".company",
            ".computer",
            ".condos",
            ".construction",
            ".consulting",
            ".contractors",
            ".cooking",
            ".cool",
            ".coop",
            ".country",
            ".courses",
            ".cr",
            ".credit",
            ".creditcard",
            ".cricket",
            ".crs",
            ".cruises",
            ".cu",
            ".cuisinella",
            ".cv",
            ".cw",
            ".cx",
            ".cy",
            ".cymru",
            ".cyou",
            ".cz",
            ".dabur",
            ".dad",
            ".dance",
            ".date",
            ".dating",
            ".datsun",
            ".day",
            ".dclk",
            ".de",
            ".deals",
            ".degree",
            ".delivery",
            ".democrat",
            ".dental",
            ".dentist",
            ".desi",
            ".design",
            ".dev",
            ".diamonds",
            ".diet",
            ".digital",
            ".direct",
            ".directory",
            ".discount",
            ".dj",
            ".dk",
            ".dm",
            ".dnp",
            ".do",
            ".docs",
            ".doha",
            ".domains",
            ".doosan",
            ".download",
            ".durban",
            ".dvag",
            ".dz",
            ".eat",
            ".ec",
            ".edu",
            ".education",
            ".ee",
            ".eg",
            ".eh",
            ".email",
            ".emerck",
            ".energy",
            ".engineer",
            ".engineering",
            ".enterprises",
            ".epson",
            ".equipment",
            ".er",
            ".erni",
            ".es",
            ".esq",
            ".estate",
            ".et",
            ".eu",
            ".eurovision",
            ".eus",
            ".events",
            ".everbank",
            ".exchange",
            ".expert",
            ".exposed",
            ".express",
            ".fail",
            ".faith",
            ".fan",
            ".fans",
            ".farm",
            ".fashion",
            ".feedback",
            ".fi",
            ".film",
            ".finance",
            ".financial",
            ".firmdale",
            ".fish",
            ".fishing",
            ".fit",
            ".fitness",
            ".fj",
            ".fk",
            ".flights",
            ".florist",
            ".flowers",
            ".flsmidth",
            ".fly",
            ".fm",
            ".fo",
            ".foo",
            ".football",
            ".forex",
            ".forsale",
            ".foundation",
            ".fr",
            ".frl",
            ".frogans",
            ".fund",
            ".furniture",
            ".futbol",
            ".ga",
            ".gal",
            ".gallery",
            ".garden",
            ".gb",
            ".gbiz",
            ".gd",
            ".gdn",
            ".ge",
            ".gent",
            ".gf",
            ".gg",
            ".ggee",
            ".gh",
            ".gi",
            ".gift",
            ".gifts",
            ".gives",
            ".gl",
            ".glass",
            ".gle",
            ".global",
            ".globo",
            ".gm",
            ".gmail",
            ".gmo",
            ".gmx",
            ".gn",
            ".gold",
            ".goldpoint",
            ".golf",
            ".goo",
            ".goog",
            ".google",
            ".gop",
            ".gp",
            ".gq",
            ".gr",
            ".graphics",
            ".gratis",
            ".green",
            ".gripe",
            ".gs",
            ".gt",
            ".gu",
            ".guge",
            ".guide",
            ".guitars",
            ".guru",
            ".gw",
            ".gy",
            ".hamburg",
            ".hangout",
            ".haus",
            ".healthcare",
            ".help",
            ".here",
            ".hermes",
            ".hiphop",
            ".hiv",
            ".hk",
            ".hm",
            ".hn",
            ".holdings",
            ".holiday",
            ".homes",
            ".horse",
            ".host",
            ".hosting",
            ".house",
            ".how",
            ".hr",
            ".ht",
            ".hu",
            ".ibm",
            ".id",
            ".ie",
            ".ifm",
            ".il",
            ".im",
            ".immo",
            ".immobilien",
            ".in",
            ".industries",
            ".infiniti",
            ".info",
            ".ing",
            ".ink",
            ".institute",
            ".insure",
            ".int",
            ".international",
            ".investments",
            ".io",
            ".iq",
            ".ir",
            ".irish",
            ".is",
            ".it",
            ".iwc",
            ".java",
            ".jcb",
            ".je",
            ".jetzt",
            ".jm",
            ".jo",
            ".jobs",
            ".joburg",
            ".jp",
            ".juegos",
            ".kaufen",
            ".kddi",
            ".ke",
            ".kg",
            ".kh",
            ".ki",
            ".kim",
            ".kitchen",
            ".kiwi",
            ".km",
            ".kn",
            ".koeln",
            ".komatsu",
            ".kp",
            ".kr",
            ".krd",
            ".kred",
            ".kw",
            ".ky",
            ".kyoto",
            ".kz",
            ".la",
            ".lacaixa",
            ".land",
            ".lat",
            ".latrobe",
            ".lawyer",
            ".lb",
            ".lc",
            ".lds",
            ".lease",
            ".leclerc",
            ".legal",
            ".lgbt",
            ".li",
            ".lidl",
            ".life",
            ".lighting",
            ".limited",
            ".limo",
            ".link",
            ".lk",
            ".loan",
            ".loans",
            ".london",
            ".lotte",
            ".lotto",
            ".love",
            ".lr",
            ".ls",
            ".lt",
            ".ltda",
            ".lu",
            ".luxe",
            ".luxury",
            ".lv",
            ".ly",
            ".ma",
            ".madrid",
            ".maif",
            ".maison",
            ".management",
            ".mango",
            ".market",
            ".marketing",
            ".markets",
            ".marriott",
            ".mc",
            ".md",
            ".me",
            ".media",
            ".meet",
            ".melbourne",
            ".meme",
            ".memorial",
            ".menu",
            ".mf",
            ".mg",
            ".mh",
            ".miami",
            ".mil",
            ".mini",
            ".mk",
            ".ml",
            ".mm",
            ".mma",
            ".mn",
            ".mo",
            ".mobi",
            ".moda",
            ".moe",
            ".monash",
            ".money",
            ".mormon",
            ".mortgage",
            ".moscow",
            ".motorcycles",
            ".mov",
            ".movie",
            ".mp",
            ".mq",
            ".mr",
            ".ms",
            ".mt",
            ".mtn",
            ".mtpc",
            ".mu",
            ".museum",
            ".mv",
            ".mw",
            ".mx",
            ".my",
            ".mz",
            ".na",
            ".nagoya",
            ".name",
            ".navy",
            ".nc",
            ".ne",
            ".net",
            ".network",
            ".neustar",
            ".new",
            ".news",
            ".nexus",
            ".nf",
            ".ng",
            ".ngo",
            ".nhk",
            ".ni",
            ".nico",
            ".ninja",
            ".nissan",
            ".nl",
            ".no",
            ".np",
            ".nr",
            ".nra",
            ".nrw",
            ".ntt",
            ".nu",
            ".nyc",
            ".nz",
            ".okinawa",
            ".om",
            ".one",
            ".ong",
            ".onl",
            ".online",
            ".ooo",
            ".organic",
            ".osaka",
            ".otsuka",
            ".ovh",
            ".pa",
            ".page",
            ".panerai",
            ".paris",
            ".partners",
            ".parts",
            ".party",
            ".pe",
            ".pf",
            ".pg",
            ".ph",
            ".pharmacy",
            ".photo",
            ".photography",
            ".photos",
            ".physio",
            ".piaget",
            ".pics",
            ".pictet",
            ".pictures",
            ".pink",
            ".pizza",
            ".pk",
            ".pl",
            ".place",
            ".plumbing",
            ".plus",
            ".pm",
            ".pn",
            ".pohl",
            ".poker",
            ".porn",
            ".post",
            ".pr",
            ".praxi",
            ".press",
            ".pro",
            ".prod",
            ".productions",
            ".prof",
            ".properties",
            ".property",
            ".ps",
            ".pt",
            ".pub",
            ".pw",
            ".py",
            ".qa",
            ".qpon",
            ".quebec",
            ".racing",
            ".re",
            ".realtor",
            ".recipes",
            ".red",
            ".redstone",
            ".rehab",
            ".reise",
            ".reisen",
            ".reit",
            ".ren",
            ".rentals",
            ".repair",
            ".report",
            ".republican",
            ".rest",
            ".restaurant",
            ".review",
            ".reviews",
            ".rich",
            ".rio",
            ".rip",
            ".ro",
            ".rocks",
            ".rodeo",
            ".rs",
            ".rsvp",
            ".ru",
            ".ruhr",
            ".rw",
            ".ryukyu",
            ".sa",
            ".saarland",
            ".sale",
            ".samsung",
            ".sap",
            ".sarl",
            ".saxo",
            ".sb",
            ".sc",
            ".sca",
            ".scb",
            ".schmidt",
            ".scholarships",
            ".school",
            ".schule",
            ".schwarz",
            ".science",
            ".scot",
            ".sd",
            ".se",
            ".services",
            ".sew",
            ".sexy",
            ".sg",
            ".sh",
            ".shiksha",
            ".shoes",
            ".shriram",
            ".si",
            ".singles",
            ".site",
            ".sj",
            ".sk",
            ".sky",
            ".sl",
            ".sm",
            ".sn",
            ".so",
            ".social",
            ".software",
            ".sohu",
            ".solar",
            ".solutions",
            ".soy",
            ".space",
            ".spiegel",
            ".spreadbetting",
            ".sr",
            ".ss",
            ".st",
            ".study",
            ".style",
            ".su",
            ".sucks",
            ".supplies",
            ".supply",
            ".support",
            ".surf",
            ".surgery",
            ".suzuki",
            ".sv",
            ".sx",
            ".sy",
            ".sydney",
            ".systems",
            ".sz",
            ".taipei",
            ".tatar",
            ".tattoo",
            ".tax",
            ".tc",
            ".td",
            ".tech",
            ".technology",
            ".tel",
            ".temasek",
            ".tennis",
            ".tf",
            ".tg",
            ".th",
            ".tickets",
            ".tienda",
            ".tips",
            ".tires",
            ".tirol",
            ".tj",
            ".tk",
            ".tl",
            ".tm",
            ".tn",
            ".to",
            ".today",
            ".tokyo",
            ".tools",
            ".top",
            ".toshiba",
            ".tours",
            ".town",
            ".toys",
            ".tp",
            ".tr",
            ".trade",
            ".trading",
            ".training",
            ".travel",
            ".trust",
            ".tt",
            ".tui",
            ".tv",
            ".tw",
            ".tz",
            ".ua",
            ".ug",
            ".uk",
            ".um",
            ".university",
            ".uno",
            ".uol",
            ".us",
            ".uy",
            ".uz",
            ".va",
            ".vacations",
            ".vc",
            ".ve",
            ".vegas",
            ".ventures",
            ".versicherung",
            ".vet",
            ".vg",
            ".vi",
            ".viajes",
            ".video",
            ".villas",
            ".vision",
            ".vlaanderen",
            ".vn",
            ".vodka",
            ".vote",
            ".voting",
            ".voto",
            ".voyage",
            ".vu",
            ".wales",
            ".wang",
            ".watch",
            ".webcam",
            ".website",
            ".wed",
            ".wedding",
            ".wf",
            ".whoswho",
            ".wien",
            ".wiki",
            ".williamhill",
            ".win",
            ".wme",
            ".work",
            ".works",
            ".world",
            ".ws",
            ".wtc",
            ".wtf",
            ".xin",
            ".æµ‹è¯•",
            ".à¤ªà¤°à¥€à¤•à¥à¤·à¤¾",
            ".ä½›å±±",
            ".æ…ˆå–„",
            ".é›†å›¢",
            ".åœ¨çº¿",
            ".í•œêµ­",
            ".à¦­à¦¾à¦°à¦¤",
            ".å…«å¦",
            ".Ù…ÙˆÙ‚Ø¹",
            ".à¦¬à¦¾à¦‚à¦²à¦¾",
            ".å…¬ç›Š",
            ".å…¬å¸",
            ".ç§»åŠ¨",
            ".æˆ‘çˆ±ä½ ",
            ".Ð¼Ð¾ÑÐºÐ²Ð°",
            ".Ð¸ÑÐ¿Ñ‹Ñ‚Ð°Ð½Ð¸Ðµ",
            ".Ò›Ð°Ð·",
            ".Ð¾Ð½Ð»Ð°Ð¹Ð½",
            ".ÑÐ°Ð¹Ñ‚",
            ".ÑÑ€Ð±",
            ".Ð±ÐµÐ»",
            ".æ—¶å°š",
            ".í…ŒìŠ¤íŠ¸",
            ".æ·¡é©¬é”¡",
            ".Ð¾Ñ€Ð³",
            ".ì‚¼ì„±",
            ".à®šà®¿à®™à¯à®•à®ªà¯à®ªà¯‚à®°à¯",
            ".å•†æ ‡",
            ".å•†åº—",
            ".å•†åŸŽ",
            ".Ð´ÐµÑ‚Ð¸",
            ".Ð¼ÐºÐ´",
            ".×˜×¢×¡×˜",
            ".ä¸­æ–‡ç½‘",
            ".ä¸­ä¿¡",
            ".ä¸­å›½",
            ".ä¸­åœ‹",
            ".è°·æ­Œ",
            ".à°­à°¾à°°à°¤à±",
            ".à¶½à¶‚à¶šà·",
            ".æ¸¬è©¦",
            ".àª­àª¾àª°àª¤",
            ".à¤­à¤¾à¤°à¤¤",
            ".Ø¢Ø²Ù…Ø§ÛŒØ´ÛŒ",
            ".à®ªà®°à®¿à®Ÿà¯à®šà¯ˆ",
            ".ç½‘åº—",
            ".à¤¸à¤‚à¤—à¤ à¤¨",
            ".ç½‘ç»œ",
            ".ÑƒÐºÑ€",
            ".é¦™æ¸¯",
            ".Î´Î¿ÎºÎ¹Î¼Î®",
            ".é£žåˆ©æµ¦",
            ".Ø¥Ø®ØªØ¨Ø§Ø±",
            ".å°æ¹¾",
            ".å°ç£",
            ".æ‰‹æœº",
            ".Ð¼Ð¾Ð½",
            ".Ø§Ù„Ø¬Ø²Ø§Ø¦Ø±",
            ".Ø¹Ù…Ø§Ù†",
            ".Ø§ÛŒØ±Ø§Ù†",
            ".Ø§Ù…Ø§Ø±Ø§Øª",
            ".Ø¨Ø§Ø²Ø§Ø±",
            ".Ù¾Ø§Ú©Ø³ØªØ§Ù†",
            ".Ø§Ù„Ø§Ø±Ø¯Ù†",
            ".Ø¨Ú¾Ø§Ø±Øª",
            ".Ø§Ù„Ù…ØºØ±Ø¨",
            ".Ø§Ù„Ø³Ø¹ÙˆØ¯ÙŠØ©",
            ".Ø³ÙˆØ¯Ø§Ù†",
            ".Ø¹Ø±Ø§Ù‚",
            ".Ù…Ù„ÙŠØ³ÙŠØ§",
            ".æ”¿åºœ",
            ".Ø´Ø¨ÙƒØ©",
            ".áƒ’áƒ”",
            ".æœºæž„",
            ".ç»„ç»‡æœºæž„",
            ".å¥åº·",
            ".à¹„à¸—à¸¢",
            ".Ø³ÙˆØ±ÙŠØ©",
            ".Ñ€ÑƒÑ",
            ".Ñ€Ñ„",
            ".ØªÙˆÙ†Ø³",
            ".ã¿ã‚“ãª",
            ".ã‚°ãƒ¼ã‚°ãƒ«",
            ".ä¸–ç•Œ",
            ".à¨­à¨¾à¨°à¨¤",
            ".ç½‘å€",
            ".æ¸¸æˆ",
            ".vermÃ¶gensberater",
            ".vermÃ¶gensberatung",
            ".ä¼ä¸š",
            ".ä¿¡æ¯",
            ".Ù…ØµØ±",
            ".Ù‚Ø·Ø±",
            ".å¹¿ä¸œ",
            ".à®‡à®²à®™à¯à®•à¯ˆ",
            ".à®‡à®¨à¯à®¤à®¿à®¯à®¾",
            ".Õ°Õ¡Õµ",
            ".æ–°åŠ å¡",
            ".ÙÙ„Ø³Ø·ÙŠÙ†",
            ".ãƒ†ã‚¹ãƒˆ",
            ".æ”¿åŠ¡",
            ".xxx",
            ".xyz",
            ".yachts",
            ".yandex",
            ".ye",
            ".yodobashi",
            ".yoga",
            ".yokohama",
            ".youtube",
            ".yt",
            ".za",
            ".zip",
            ".zm",
            ".zone",
            ".zuerich",
            ".zw"
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }
}
