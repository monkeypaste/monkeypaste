using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using MonkeyPaste.Common.Plugin;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace HexColorFormatter {
    public class HexColorFormatter : MpIAnalyzeComponent {
        public object Analyze(object args) {
            var reqParts = MpJsonObject.DeserializeObject<MpAnalyzerPluginRequestFormat>(args.ToString());

            bool doHighlighting = reqParts.items.FirstOrDefault(x => x.paramId == 1).value.ToLower() == "true";
            string rtf = reqParts.items.FirstOrDefault(x => x.paramId == 2).value;
            var fd = rtf.ToFlowDocument();
            string pt = fd.ToPlainText();

            var mc = MpRegEx.RegExLookup[MpRegExType.HexColor].Matches(pt);
            var tp = fd.ContentStart;
            var trl = new List<TextRange>();

            foreach (Match m in mc) {
                var tr = tp.FindText(fd.ContentEnd, m.Value, MpWpfRichDocumentExtensions.FindFlags.None);
                if(tr == null) {
                    break;
                }
                trl.Add(tr);

                tr.Text = string.Empty;

                string hexColor = m.Value.Replace("#", string.Empty);
                var hl = new Hyperlink(tr.Start, tr.End) {
                    IsEnabled = true,
                    NavigateUri = new Uri($"https://www.hexcolortool.com/{hexColor}"),
                    Background = m.Value.ToWpfBrush()
                };

                var run = new Run(m.Value);
                hl.Inlines.Clear();
                hl.Inlines.Add(run);
                var bgBrush = m.Value.ToWpfBrush();
                var fgBrush = MpWpfColorHelpers.IsBright(((SolidColorBrush)bgBrush).Color) ? Brushes.Black : Brushes.White;
                var rtr = new TextRange(run.ElementStart, run.ElementEnd);
                rtr.ApplyPropertyValue(TextElement.BackgroundProperty, bgBrush);
                rtr.ApplyPropertyValue(TextElement.ForegroundProperty, fgBrush);

                tp = tr.End.GetInsertionPosition(LogicalDirection.Forward);
            }

            return new MpPluginResponseFormat() {
                message = "SUCCESS",
                dataObject = new MpPortableDataObject(MpPortableDataFormats.Rtf,fd.ToRichText())
            };
        }
    }
}
