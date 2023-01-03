using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using static System.Net.Mime.MediaTypeNames;

namespace AvCoreAnnotator {
    public static class HtmlAnnotator {

        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public static List<MpTextAnnotationFormat> Annotate(HtmlDocument htmlDoc,IEnumerable<MpRegExType> formats) {
            List<MpTextAnnotationFormat> annotations = new List<MpTextAnnotationFormat>();
            if(htmlDoc == null) {
                return annotations;
            }

            foreach(MpRegExType ft in formats) {
                var annotation_type_results = HtmlAnnotator.AnnotateType(htmlDoc, ft);
                annotations.AddRange(annotation_type_results);
            }

            return annotations;
        }
        public static List<MpTextAnnotationFormat> AnnotateType(HtmlDocument htmlDoc, MpRegExType annotationRegExType) {
            List<MpTextAnnotationFormat> annotations = new List<MpTextAnnotationFormat>();
            if(annotationRegExType == MpRegExType.None ||
                (int)annotationRegExType > MpRegEx.MAX_ANNOTATION_REGEX_TYPE) {
                return annotations;
            }
            string pt = htmlDoc.DocumentNode.InnerText.DecodeSpecialHtmlEntities();
            var pre_pass_mc = MpRegEx.RegExLookup[annotationRegExType].Matches(pt);
            if(pre_pass_mc.Count == 0) {
                return annotations;
            }

            var text_nodes = htmlDoc.DocumentNode.SelectNodes(".//text()");
            for (int i = 0; i < text_nodes.Count; i++) {
                var tn = text_nodes[i] as HtmlTextNode;
                if(tn == null) {
                    continue;
                }

                if(tn.Ancestors().Any(x=>x.Name.ToLower() == "a")) {
                    //ignore already defined links
                    continue;
                }

                // NOTE plain text maybe encoded xml content so decode for matching
                string tn_decoded_text = tn.Text.DecodeSpecialHtmlEntities();

                var mc = MpRegEx.RegExLookup[annotationRegExType].Matches(tn_decoded_text);
                if(mc.Count == 0) {
                    // no matches in this text node
                    continue;
                }
                // this text node continues 1 or more current pattern
                // ex 'Hey have you seen https://youtube.mw/blahblah yet? Its just like https://gumbo.com/clippers_fanatics

                // span_node will replace parents reference to this text node 
                // and contain 1 or more links split by non-matching text nodes
                HtmlNode match_span_wrapper_node = null;

                int cur_idx = 0;
                Match m = MpRegEx.RegExLookup[annotationRegExType].Match(tn_decoded_text);
                while (m.Success) {
                    MpConsole.WriteLine($"Annotation match: Type: {annotationRegExType} Value: {m.Value}");
                    if(match_span_wrapper_node == null) {
                        match_span_wrapper_node = htmlDoc.CreateElement("span");
                    }
                    // create annotation
                    var annotation = new MpTextAnnotationFormat(m.Index, m.Length) { annotationType = annotationRegExType.ToString().ToLower() };
                    annotations.Add(annotation);

                    int match_idx = tn_decoded_text.Substring(cur_idx).IndexOf(m.Value);

                    if (match_idx > 0) {
                        // create lead run (in example "Hey have you seen ")
                        string lead_text = tn_decoded_text.Substring(cur_idx, match_idx);
                        HtmlNode lead_text_node = htmlDoc.CreateTextNode(lead_text);
                        HtmlNode lead_text_node_wrapper_span = htmlDoc.CreateElement("span");
                        lead_text_node_wrapper_span.AppendChild(lead_text_node);
                        match_span_wrapper_node.AppendChild(lead_text_node_wrapper_span);
                    }

                    // create link
                    HtmlNode anchor_inner_node = htmlDoc.CreateTextNode(m.Value);
                    HtmlNode anchor_node = htmlDoc.CreateElement("a");

                    // give link class with annotation type for navigation, meta, etc.                    
                    anchor_node.AddClass(annotationRegExType.ToString().ToLower());
                    // set href to match value
                    string href_value;
                    switch(annotationRegExType) {
                        case MpRegExType.HexColor:
                            // add bg color to regex
                            var c = new MpColor(m.Value);
                            href_value = $"https://www.htmlcsscolor.com/hex/{c.ToHex(true).ToUpper().Replace("#", string.Empty)}";
                            break;
                        case MpRegExType.Email:
                            href_value = $"mailto:{m.Value}";
                            break;
                        case MpRegExType.FileOrFolder:
                            if(Uri.IsWellFormedUriString(m.Value,UriKind.Absolute)) {
                                href_value = new Uri(m.Value).AbsoluteUri;
                            } else {
                                href_value = string.Empty;
                            }
                            break;
                        case MpRegExType.PhoneNumber:
                        case MpRegExType.StreetAddress:
                        case MpRegExType.Currency:
                            href_value = $"https://www.google.com/search?q={m.Value}";
                            break;
                        default:
                            href_value = m.Value;
                            break;

                    }
                    anchor_node.SetAttributeValue("href", href_value);

                    anchor_node.AppendChild(anchor_inner_node);
                    match_span_wrapper_node.AppendChild(anchor_node);

                    cur_idx += match_idx + m.Value.Length;
                    m = MpRegEx.RegExLookup[annotationRegExType].Match(tn_decoded_text.Substring(cur_idx));
                }
                if(match_span_wrapper_node != null) {
                    if (cur_idx < tn_decoded_text.Length) {
                        // create trailing run after encoded special entities
                        string trailing_text = tn_decoded_text.Substring(cur_idx);
                        HtmlNode trail_text_node = htmlDoc.CreateTextNode(trailing_text);
                        HtmlNode trailing_text_node_wrapper_span = htmlDoc.CreateElement("span");
                        trailing_text_node_wrapper_span.AppendChild(trail_text_node);
                        match_span_wrapper_node.AppendChild(trailing_text_node_wrapper_span);
                    }

                    // ensure html special entities are encoded into document
                    var subbed_text_nodes = match_span_wrapper_node.SelectNodes(".//text()");
                    subbed_text_nodes
                        .Cast<HtmlTextNode>()
                        .Where(x => x != null)
                        .ForEach(x => x.Text = x.Text.EncodeSpecialHtmlEntities());

                    tn.ParentNode.ReplaceChild(match_span_wrapper_node, tn);
                }
            }


            return annotations;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }

}
