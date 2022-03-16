using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;

namespace MpWpfApp {
    public class MpAnalysisReportViewModel : 
        MpViewModelBase<MpAnalysisReportCollectionViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel{
        #region Private Variables

        private Size _defaultReportSize = new Size(816, 1056);
        #endregion

        #region Properties

        #region Appearance

        //public string Title { 
        //    get {
        //        if(ResponseFormat == null) {
        //            return null;
        //        }
        //        return ResponseFormat.annotations[0].
        //    }
        //}
        #endregion

        #region State

        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }

        #endregion

        #region Model
        public string ReportRtf { get; set; }

        public MpPluginResponseFormat ResponseFormat { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAnalysisReportViewModel() : base(null) { }
        public MpAnalysisReportViewModel(MpAnalysisReportCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(string responseJsonStr) {
            if(string.IsNullOrWhiteSpace(responseJsonStr)) {
                return;
            }
            IsBusy = true;
            await Task.Delay(1);

            ResponseFormat = JsonConvert.DeserializeObject<MpPluginResponseFormat>(responseJsonStr);

            ReportRtf = CreateReport().ToRichText();

            IsBusy = false;
        }

        private FlowDocument CreateReport() {
            var fd = new FlowDocument();
            if(ResponseFormat == null || ResponseFormat.annotations == null) {
                return fd;
            }
            fd.Blocks.Clear();

            fd.PagePadding = new Thickness(0);
            fd.PageWidth = _defaultReportSize.Width;
            fd.PageHeight = _defaultReportSize.Height;
            fd.LineHeight = 10;
            foreach (var a in ResponseFormat.annotations) {
                var te = ConvertAnnotationNode(null,a);
                if(te is Block b) {
                    fd.Blocks.Add(b);
                } else if(te is Inline i){
                    fd.Blocks.Add(new Paragraph(i));
                }
            }
            fd.LineStackingStrategy = LineStackingStrategy.MaxHeight;
            fd.TextAlignment = TextAlignment.Justify;
            fd.Background = Brushes.White;
            MpFileIoHelpers.WriteTextToFile(Path.Combine(Directory.GetCurrentDirectory(), "test.rtf"), fd.ToRichText());
            return fd;
        }

        private TextElement ConvertAnnotationNode(TextElement curElement, MpPluginResponseAnnotationFormat n) {
            if(n == null) {
                return null;
            }
            if(n.appearance == null) {
                n.appearance = new MpPluginResponseAppearanceFormat();
            }
            if(n.appearance.font == null) {
                n.appearance.font = new MpPluginResponseFontAppearanceFormat();
            }
            if(n.appearance.backgroundColor == null) {
                n.appearance.backgroundColor = new MpJsonPathProperty("#FFFFFFFF");
            }
            if (n.appearance.foregroundColor == null) {
                n.appearance.foregroundColor = new MpJsonPathProperty("#FF000000");
            }
            var nte = ConvertAnnotationItem(n);
            if(nte != null) {
                if (curElement == null) {
                    curElement = new Paragraph(nte);
                } else if (curElement is Paragraph p) {
                    p.Inlines.Add(nte);
                    p.Inlines.Add(new LineBreak());
                } else if (curElement is List l) {
                    l.ListItems.Add(new ListItem(new Paragraph(nte)));
                } else if (curElement is Table t) {
                    var curTableItem = t.RowGroups.LastOrDefault().Rows.LastOrDefault().Cells.LastOrDefault().Blocks.LastOrDefault();
                    if (curTableItem is List tl) {
                        tl.ListItems.Add(new ListItem(new Paragraph(nte)));
                    } else {
                        Debugger.Break();
                    }
                    
                } else {
                    Debugger.Break();
                }
            }

            if (n.appearance.isList || n.appearance.isNumberedList) {
                var l = new List() { MarkerStyle = n.appearance.isNumberedList ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc };
                Table t = null;
                if(curElement is Table table) {
                    t = table;
                    t.RowGroups.LastOrDefault().Rows.LastOrDefault().Cells.Add(new TableCell(l));
                } else {
                    t = new Table();
                    var trg = new TableRowGroup();
                    var tr = new TableRow();
                    var tcl = new TableCell();
                    tcl.Blocks.Add(l);
                    tr.Cells.Add(tcl);
                    trg.Rows.Add(tr);
                    t.RowGroups.Add(trg);
                    
                }
                var tc = new TableColumn();
                t.Columns.Add(tc);
                t.Columns.ForEach(x => x.Width = new GridLength(_defaultReportSize.Width / t.Columns.Count));
                
                curElement = t;
            }

            if (n.children != null) {
                foreach (var c in n.children) {
                    ConvertAnnotationNode(curElement, c);
                }
            }
            return curElement;
        }

        private Span ConvertAnnotationItem(MpPluginResponseAnnotationFormat ta) {
            if(ta.label == null) {
                return null;
            }
            var span = new Span();
            if (ta.appearance.font.isUnderlined) {
                span = new Underline();
            }
            if (ta.appearance.font.isBold) {
                span = new Bold(span);
            }
            if (ta.appearance.font.isItalic) {
                span = new Italic(span);
            }
            if (ta.appearance.font.isStrikethough) {
                span.TextDecorations.Add(TextDecorations.Strikethrough);
            }

            span.FontFamily = ParseFontFamily(ta.appearance.font.fontFamily);
            span.FontSize = ParseFontSize(ta.appearance.font.fontSize);

            span.Foreground = ParseBrush(ta.appearance.foregroundColor.value);
            span.Background = ParseBrush(ta.appearance.backgroundColor.value);

            span.Inlines.Add(new Run(ta.label.value));

            if (MpUrlHelpers.IsValidUrl(ta.label.value)) {
                var hl = new Hyperlink(span) {
                    NavigateUri = new Uri(ta.label.value, UriKind.Absolute)
                };
                return hl;
            }
            return span;
        }

        private Brush ParseBrush(string text, Brush fallbackBrush = null) {
            fallbackBrush = fallbackBrush == null ? Brushes.Red : fallbackBrush;

            string hexColor;
            if (text.IsStringHexColor()) {
                hexColor = text;
            } else {
                string temp = "#" + text;
                if (temp.IsStringHexColor()) {
                    hexColor = temp;
                } else if (MpSystemColors.X11ColorNames.Contains(text)) {
                    hexColor = typeof(MpSystemColors).GetProperty(text).GetValue(null) as string;
                }
            }
            if (string.IsNullOrWhiteSpace(text)) {
                return fallbackBrush;
            }
            return new SolidColorBrush(text.ToWinMediaColor());
        }

        private FontFamily ParseFontFamily(string text) {
            string defaultFontName = "arial";
            FontFamily defaultFontFamily = null;
            FontFamily closestFontFamily = null;
            string fontName = text.ToLower();
            foreach (var ff in Fonts.SystemFontFamilies) {
                string ffName = ff.ToString().ToLower();
                if (ffName.Contains(fontName)) {
                    closestFontFamily = ff;
                }
                if (ffName == fontName) {
                    closestFontFamily = ff;
                    break;
                }
                if (ffName == defaultFontName) {
                    defaultFontFamily = ff;
                }
            }

            if (closestFontFamily != null) {
                //MpConsole.WriteLine("Could not find exact system font: " + fontName + " using "+closestFontFamily.ToString()+" instead");
                MpRichTextFormatProperties.Instance.AddFont(closestFontFamily.ToString().ToLower());
                return closestFontFamily;
            }
            MpConsole.WriteLine("Could not find system font: " + fontName);
            return defaultFontFamily;
        }

        private double ParseFontSize(string text) {
            var fontSizeLookup = new Dictionary<string, double> {
                {"xx-small",8 },
                {"x-small", 10 },
                {"small", 12 },
                {"medium", 14 },
                {"large",18 },
                {"x-large",22 },
                {"xx-large", 36 },
                {"xxx-large", 72 },
            };
            if (!fontSizeLookup.ContainsKey(text.ToLower())) {
                return 14;
            }

            return fontSizeLookup[text.ToLower()];
        }
        #endregion
    }
}
