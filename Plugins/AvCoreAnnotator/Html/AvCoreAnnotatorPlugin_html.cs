using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvCoreAnnotator {
    public enum AvAnnnotaterParameterType {
        None = 0,
        Content,
        RefUrl,
        FileOrFolder,
        Uri,
        Email,
        PhoneNumber,
        Currency,
        HexColor,
        StreetAddress,
    }

    public class AvCoreAnnotatorPlugin : MpIAnalyzeComponent {
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            var resp = new MpAnalyzerPluginResponseFormat();

            string content_html_str = req.GetRequestParamStringValue((int)AvAnnnotaterParameterType.Content);
            if (string.IsNullOrWhiteSpace(content_html_str)) {
                return null;
            }
            var html_doc = new HtmlDocument();
            try {
                html_doc.LoadHtml(content_html_str);
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error loading html from content: '{content_html_str}'. Ex: ", ex);
                resp.errorMessage = ex.Message;
                return null;
            }

            
            var formats = new List<MpRegExType>();
            for (int i = 3; i < Enum.GetValues(typeof(AvAnnnotaterParameterType)).Length; i++) {
                if(req.GetRequestParamBoolValue(i)) {
                    formats.Add((MpRegExType)i - 2);
                }
            }
            string pt = MpRichHtmlToPlainTextConverter.Convert(content_html_str);
            pt = pt.DecodeSpecialHtmlEntities();
            var plain_doc = new HtmlDocument();
            plain_doc.LoadHtml(pt);
            var plain_annotations = HtmlAnnotator.Annotate(plain_doc, formats);
            var formatted_annotations = HtmlAnnotator.Annotate(html_doc, formats);
            MpConsole.WriteLine($"Plain annotation Count: {plain_annotations.Count} Formatted annotation Count: {formatted_annotations.Count}");

            HtmlDocument out_doc = null;
            List<MpTextAnnotationFormat> out_annotations = null;
            if(formatted_annotations.Count != plain_annotations.Count) {
                // content contains inline formatting so cannot match correctly on text nodes, 
                // TODO maybe add checkbox that defaults to true to prefer annotations over rich formatting
                // OPTIMIZATION figure out how to traverse matches and insert links between inline elements
                out_doc = plain_doc;
                out_annotations = plain_annotations;
            } else {
                out_doc = html_doc;
                out_annotations = formatted_annotations;
            }

            resp.annotations = out_annotations.Select(x => new MpPluginResponseAnnotationFormat() { range = x }).ToList();
            if (resp.annotations != null && 
                resp.annotations.Count > 0) {
                // content was annotated, return it as dataobjectitem
                resp.dataObject = new MpPortableDataObject(MpPortableDataFormats.CefHtml, out_doc.DocumentNode.InnerHtml);
                return resp;
            }

            return null;
        }
    }
}