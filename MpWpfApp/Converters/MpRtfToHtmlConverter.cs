using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HtmlAgilityPack;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MpWpfApp {
    public class MpRtfToHtmlConverter {
        #region Singleton
        private static readonly Lazy<MpRtfToHtmlConverter> _Lazy = new Lazy<MpRtfToHtmlConverter>(() => new MpRtfToHtmlConverter());
        public static MpRtfToHtmlConverter Instance { get { return _Lazy.Value; } }

        private MpRtfToHtmlConverter() { }
        #endregion

        #region Private Variables
        private RichTextBox _rtb;
        private double _indentCharCount = 5;
        #endregion

        #region Properties

        #endregion

        public string ConvertRtfToHtml(string rtf) {
            string html = string.Empty;
            _rtb = new RichTextBox();
            _rtb.Document = rtf.ToFlowDocument();
            foreach(Block b in _rtb.Document.Blocks) {
                string atHtml = string.Empty;
                switch (b.TextAlignment) {
                    case TextAlignment.Left:
                        atHtml = @"class='ql-align-left";
                        break;
                    case TextAlignment.Center:
                        atHtml = @"class='ql-align-center";
                        break;
                    case TextAlignment.Right:
                        atHtml = @"class='ql-align-right";
                        break;
                    case TextAlignment.Justify:
                        atHtml = @"class='ql-align-justify";
                        break;
                }
                if (b is Paragraph) {
                    var p = b as Paragraph;
                    html += "<p";

                    if (p.TextIndent > 0) {
                        int indentLevel = (int)(p.TextIndent / _indentCharCount);
                        atHtml += " ql-indent-" + indentLevel + "'";
                    } else {
                        atHtml += "'";
                    }
                    html += " " + atHtml + ">";
                    foreach(var i in p.Inlines) {
                        if(i is Bold) {
                            html += "<strong>";
                        }
                    }
                }
            }
            return html;
        }
        
        private string ToHtml(Block b) {
            string html = string.Empty;
            if(b is List) {
                return ToHtml(b as List);
            } else if (b is Paragraph) {
                return ToHtml(b as Paragraph);
            }
            throw new Exception("Unknown block: " + b.ToString());
        }

        private string GetAttributes(Block b) {
            string atHtml = string.Empty;
            switch (b.TextAlignment) {
                case TextAlignment.Left:
                    atHtml = @"class='ql-align-left'";
                    break;
                case TextAlignment.Center:
                    atHtml = @"class='ql-align-center'";
                    break;
                case TextAlignment.Right:
                    atHtml = @"class='ql-align-right'";
                    break;
                case TextAlignment.Justify:
                    atHtml = @"class='ql-align-justify'";
                    break;
            }
            return atHtml;
        }

        private string ToHtml(List l) {
            string html = string.Empty;

            return html;
        }

        private string ToHtml(ListItem li) {
            string html = string.Empty;

            return html;
        }

        private string ToHtml(Paragraph p) {
            string html = @"<p";
            string attr = GetAttributes(p);
            if(!string.IsNullOrEmpty(attr)) {
                html += " " + attr + ">";
            }
            foreach(var i in p.Inlines) {
                html += ToHtml(i);
            }
            return html + "</p>";
        }

        private string ToHtml(Inline i) {
            if (i is Run) {
                return ToHtml(i as Run);
            } else {
                return ToHtml(i as Span);
            }
        }

        private string ToHtml(Span s) {
            string html = string.Empty;
            foreach(var i in s.Inlines) {
                html += ToHtml(i);
            }
            if(s is Underline) {

            }
            return html;
        }

        private string ToHtml(Run r) {
            return r.Text;
        }

        public void Test() {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpRtfToHtmlConverter)).Assembly;
            var stream = assembly.GetManifestResourceStream("MpWpfApp.Resources.TestData.quillFormattedTextSample1.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                var html = reader.ReadToEnd();
                //string rtf = ConvertHtmlToRtf(html);

                //Clipboard.SetData(DataFormats.Rtf, rtf);

                //MpHelpers.Instance.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtftest.rtf", rtf, false);
            }
        }
    }
}
