using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CefNet.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
//using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

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

            //string versionToken = @"Version:";
            //string startHtmlToken = @"StartHTML:";
            //string endHtmlToken = @"EndHTML:";

            string htmlStartToken = @"<!--StartFragment-->";
            string htmlEndToken = @"<!--EndFragment-->";


            int html_start_idx = htmlClipboardData.IndexOf(htmlStartToken) + htmlStartToken.Length;
            if (html_start_idx >= 0) {
                int html_end_idx = htmlClipboardData.IndexOf(htmlEndToken);
                int html_length = html_end_idx - html_start_idx;

                if (html_length > 0) {
                    string plainHtml = htmlClipboardData.Substring(html_start_idx, html_length);
                    // BUG if plain html is set and quill is later loaded setHtml won't parse it correctly
                    // (theres escape issues once in js and it just doesn't follow same conversion flow and its annoying, pointless to change) without using dangerouslyPaste
                    // which makes problems w/ templates and since this is plain text mode there's no need to keep html anyways so downsample to plain text
                    hcd.RichHtml = MpAvContentDataConverter.Instance.Convert(plainHtml, null, "plaintext", null) as string;
                }
            }
            hcd.SourceUrl = ParseHtmlFragmentForSourceUrl(htmlClipboardData);
            return hcd;
        }


        public static string FindSourceUrl(MpPortableDataObject mpdo) {
            string htmlStr = null;

            if (mpdo.ContainsData(MpPortableDataFormats.LinuxSourceUrl) &&
                       mpdo.GetData(MpPortableDataFormats.LinuxSourceUrl) is byte[] url_bytes &&
                       url_bytes.ToDecodedString(Encoding.ASCII,true) is string source_url_str) {
                // on linux html is not in fragment format like windows and firefox supports this format
                // but chrome doesn't
                //source_url_str = System.Web.HttpUtility.HtmlDecode(source_url_str);
                return source_url_str;
            }
            if (mpdo.ContainsData(MpPortableDataFormats.AvHtml_bytes) &&
                        mpdo.GetData(MpPortableDataFormats.AvHtml_bytes) is byte[] htmlBytes &&
                        htmlBytes.ToDecodedString() is string avhtmlStr) {

                // HTML
                htmlStr = avhtmlStr;
            }
            if (string.IsNullOrEmpty(htmlStr) &&
                mpdo.ContainsData(MpPortableDataFormats.CefHtml) &&
                mpdo.GetData(MpPortableDataFormats.CefHtml) is string cefhtmlStr) {
                htmlStr = cefhtmlStr;
            }
            if(string.IsNullOrWhiteSpace(htmlStr)) {
                return null;
            }
            return ParseHtmlFragmentForSourceUrl(htmlStr);
        }

        private static string ParseHtmlFragmentForSourceUrl(string htmlFragStr) {

            string sourceUrlToken = "SourceURL:";
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

            MpConsole.WriteTraceLine($"Could not find source url in html fragment: '{htmlFragStr}'");
            return null;
        }
    }
}
