using System;

namespace MpWpfApp {
    public class MpHtmlClipboardData {
        public string SourceUrl { get; private set; }
        public string Html { get; private set; }

        public static MpHtmlClipboardData Parse(string htmlClipboardData) {
            if(string.IsNullOrWhiteSpace(htmlClipboardData)) {
                return null;
            }

            var hcd = new MpHtmlClipboardData();
            string htmlStartToken = @"<!--StartFragment-->";
            string htmlEndToken = @"<!--EndFragment-->";
            int html_start_idx = htmlClipboardData.IndexOf(htmlStartToken) + htmlStartToken.Length;
            if(html_start_idx >= 0) {
                int html_end_idx = htmlClipboardData.IndexOf(htmlEndToken);
                hcd.Html = htmlClipboardData.Substring(html_start_idx,html_end_idx - html_start_idx);
            }
            string sourceUrlToken = "SourceURL:";
            int source_url_start_idx = htmlClipboardData.IndexOf(sourceUrlToken) + sourceUrlToken.Length;
            if(source_url_start_idx >= 0) {
                int source_url_length = htmlClipboardData.Substring(source_url_start_idx).IndexOf(Environment.NewLine);
                if(source_url_length >= 0) {
                    hcd.SourceUrl = htmlClipboardData.Substring(source_url_start_idx, source_url_length);
                }
            }
            return hcd;
        }
    }
}
