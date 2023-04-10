using HtmlAgilityPack;
using MonkeyPaste.Common;
//using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public class MpAvHtmlClipboardData {

        public string Version { get; private set; }
        public string SourceUrl { get; set; }

        public string Html { get; set; }
        public string RichHtml { get; set; }
        public string Delta { get; set; }


        public static MpAvHtmlClipboardData Parse(string htmlClipboardData) {
            // NOTE only used as fallback when not using cef

            var hcd = new MpAvHtmlClipboardData();

            if (ParseHtmlFragmentForPlainHtml(htmlClipboardData) is string plain_html &&
                !string.IsNullOrEmpty(plain_html)) {
                hcd.Html = plain_html;
                // BUG if plain html is set and quill is later loaded setHtml won't parse it correctly
                // (theres escape issues once in js and it just doesn't follow same conversion flow and its annoying, pointless to change) without using dangerouslyPaste
                // which makes problems w/ templates and since this is plain text mode there's no need to keep html anyways so downsample to plain text
                hcd.RichHtml = MpAvContentDataConverter.Instance.Convert(plain_html, null, "plaintext", null) as string;
            } else {
                // must not be html fragment
                hcd.Html = htmlClipboardData;
                hcd.RichHtml = htmlClipboardData;
            }
            hcd.SourceUrl = ParseHtmlFragmentForSourceUrl(htmlClipboardData);
            return hcd;
        }


        public static string FindSourceUrl(MpPortableDataObject mpdo) {
            string html_clipboard_fragment = null;
            string html_str = null;
            if (mpdo.ContainsData(MpPortableDataFormats.LinuxSourceUrl) &&
                       mpdo.GetData(MpPortableDataFormats.LinuxSourceUrl) is byte[] url_bytes &&
                       url_bytes.ToDecodedString(Encoding.ASCII, true) is string source_url_str) {
                // on linux html is not in fragment format like windows and firefox supports this format
                // but chrome doesn't
                //source_url_str = System.Web.HttpUtility.HtmlDecode(source_url_str);
                return source_url_str;
            }
            if (mpdo.ContainsData(MpPortableDataFormats.AvHtml_bytes) &&
                        mpdo.GetData(MpPortableDataFormats.AvHtml_bytes) is byte[] htmlBytes &&
                        htmlBytes.ToDecodedString() is string avhtmlStr) {

                // HTML
                html_clipboard_fragment = avhtmlStr;
            }
            if (mpdo.ContainsData(MpPortableDataFormats.CefHtml) &&
                mpdo.GetData(MpPortableDataFormats.CefHtml) is string cefhtmlStr) {
                html_str = cefhtmlStr;
            }
            if (string.IsNullOrWhiteSpace(html_clipboard_fragment)) {
                return null;
            }
            return ParseHtmlFragmentForSourceUrl(html_clipboard_fragment);
        }

        private static string ParseHtmlFragmentForPlainHtml(string htmlClipboardData) {
            string htmlStartToken = @"<!--StartFragment-->";
            string htmlEndToken = @"<!--EndFragment-->";


            int html_start_idx = htmlClipboardData.IndexOf(htmlStartToken) + htmlStartToken.Length;
            if (html_start_idx > htmlStartToken.Length) {
                int html_end_idx = htmlClipboardData.IndexOf(htmlEndToken);
                int html_length = html_end_idx - html_start_idx;

                if (html_length > 0) {
                    string plainHtml = htmlClipboardData.Substring(html_start_idx, html_length);
                    return plainHtml;
                }
            }
            return null;
        }
        private static string ParseHtmlFragmentForSourceUrl(string htmlFragStr) {
            string sourceUrlToken = "SourceURL:";
            if (htmlFragStr.Contains(sourceUrlToken)) {
                int source_url_start_idx = htmlFragStr.IndexOf(sourceUrlToken) + sourceUrlToken.Length;
                if (source_url_start_idx >= 0) {
                    int source_url_length = htmlFragStr.Substring(source_url_start_idx).IndexOf(Environment.NewLine);
                    if (source_url_length >= 0) {
                        string parsed_url = htmlFragStr.Substring(source_url_start_idx, source_url_length);
                        if (Uri.IsWellFormedUriString(parsed_url, UriKind.Absolute)) {
                            return parsed_url;
                        }
                    }
                }
            } else if (ParseHtmlFragmentForPlainHtml(htmlFragStr) is string plain_html &&
                ParseHtmlForRootImageSource(plain_html) is string img_src &&
                Uri.IsWellFormedUriString(img_src, UriKind.Absolute)) {
                // NOTE chrome (windows) clipboard for images (from open image in tab) doesn't have SourceUrl part
                // but the html fragment will be an empty html doc w/ an img element and its source
                // this parses that fragment for src attr
                return img_src;
            }


            MpConsole.WriteTraceLine($"Could not find source url in html fragment: '{htmlFragStr}'");
            return string.Empty;
        }

        private static string ParseHtmlForRootImageSource(string htmlStr) {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlStr);
            var img_nodes = htmlDoc.DocumentNode.SelectNodes("//img");
            if (img_nodes.FirstOrDefault() is HtmlNode img_node) {
                return img_node.GetAttributeValue("src", null);
            }
            return null;
        }
    }
}
