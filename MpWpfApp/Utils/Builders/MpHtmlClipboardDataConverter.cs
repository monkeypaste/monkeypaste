using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;

namespace MpWpfApp {
    public class MpHtmlClipboardDataConverter {
        public string Version { get; private set; }
        public string SourceUrl { get; private set; }
        public string Html { get; private set; }

        public string Rtf { get; private set; }

        public static async Task<MpHtmlClipboardDataConverter> Parse(string htmlClipboardData) {
            if(string.IsNullOrWhiteSpace(htmlClipboardData)) {
                return null;
            }

            var hcd = new MpHtmlClipboardDataConverter();

            //string versionToken = @"Version:";
            //string startHtmlToken = @"StartHTML:";
            //string endHtmlToken = @"EndHTML:";

            string htmlStartToken = @"<!--StartFragment-->";
            string htmlEndToken = @"<!--EndFragment-->";
            

            int html_start_idx = htmlClipboardData.IndexOf(htmlStartToken) + htmlStartToken.Length;
            if(html_start_idx >= 0) {
                int html_end_idx = htmlClipboardData.IndexOf(htmlEndToken);
                hcd.Html = htmlClipboardData.Substring(html_start_idx, html_end_idx - html_start_idx);
                hcd.Rtf = await MpQuillHtmlToRtfConverter.ConvertStandardHtmlToRtf(hcd.Html);


                //string xaml = HtmlToXamlDemo.HtmlToXamlConverter.ConvertHtmlToXaml(hcd.Html, true);                
                //using (StringReader stringReader = new StringReader(xaml.ToRichText())) {
                //    System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
                //    var fd = XamlReader.Load(xmlReader) as FlowDocument;
                //    hcd.Rtf = fd.ToRichText();
                //}                    

                //MpConsole.WriteLine("Extracted Html: ");
                //MpConsole.WriteLine(hcd.Html);



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

        /// <summary>
        /// Extracts Html string from clipboard data by parsing header information in htmlDataString
        /// </summary>
        /// <param name="htmlDataString">
        /// String representing Html clipboard data. This includes Html header
        /// </param>
        /// <returns>
        /// String containing only the Html data part of htmlDataString, without header
        /// </returns>
        internal static string ExtractHtmlFromClipboardData(string htmlDataString) {
            int startHtmlIndex = htmlDataString.IndexOf("StartHTML:");
            if (startHtmlIndex < 0) {
                return "ERROR: Urecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            startHtmlIndex = Int32.Parse(htmlDataString.Substring(startHtmlIndex + "StartHTML:".Length, "0123456789".Length));
            if (startHtmlIndex < 0 || startHtmlIndex > htmlDataString.Length) {
                return "ERROR: Urecognized html header";
            }

            int endHtmlIndex = htmlDataString.IndexOf("EndHTML:");
            if (endHtmlIndex < 0) {
                return "ERROR: Urecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            endHtmlIndex = Int32.Parse(htmlDataString.Substring(endHtmlIndex + "EndHTML:".Length, "0123456789".Length));
            if (endHtmlIndex > htmlDataString.Length) {
                endHtmlIndex = htmlDataString.Length;
            }

            return htmlDataString.Substring(startHtmlIndex, endHtmlIndex - startHtmlIndex);
        }

        /// <summary>
        /// Extracts selected Html fragment string from clipboard data by parsing header information 
        /// in htmlDataString
        /// </summary>
        /// <param name="htmlDataString">
        /// String representing Html clipboard data. This includes Html header
        /// </param>
        /// <returns>
        /// String containing only the Html selection part of htmlDataString, without header
        /// </returns>
        internal static string ExtractHtmlFragmentFromClipboardData(string htmlDataString) {
            // HTML Clipboard Format
            // (https://msdn.microsoft.com/en-us/library/aa767917(v=vs.85).aspx)

            // The fragment contains valid HTML representing the area the user has selected. This 
            // includes the information required for basic pasting of an HTML fragment, as follows:
            //  - Selected text. 
            //  - Opening tags and attributes of any element that has an end tag within the selected text. 
            //  - End tags that match the included opening tags. 

            // The fragment should be preceded and followed by the HTML comments <!--StartFragment--> and 
            // <!--EndFragment--> (no space allowed between the !-- and the text) to indicate where the 
            // fragment starts and ends. So the start and end of the fragment are indicated by these 
            // comments as well as by the StartFragment and EndFragment byte counts. Though redundant, 
            // this makes it easier to find the start of the fragment (from the byte count) and mark the 
            // position of the fragment directly in the HTML tree.

            // Byte count from the beginning of the clipboard to the start of the fragment.
            int startFragmentIndex = htmlDataString.IndexOf("StartFragment:");
            if (startFragmentIndex < 0) {
                return "ERROR: Unrecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            startFragmentIndex = Int32.Parse(htmlDataString.Substring(startFragmentIndex + "StartFragment:".Length, 10));
            if (startFragmentIndex < 0 || startFragmentIndex > htmlDataString.Length) {
                return "ERROR: Unrecognized html header";
            }

            // Byte count from the beginning of the clipboard to the end of the fragment.
            int endFragmentIndex = htmlDataString.IndexOf("EndFragment:");
            if (endFragmentIndex < 0) {
                return "ERROR: Unrecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            endFragmentIndex = Int32.Parse(htmlDataString.Substring(endFragmentIndex + "EndFragment:".Length, 10));
            if (endFragmentIndex > htmlDataString.Length) {
                endFragmentIndex = htmlDataString.Length;
            }

            // CF_HTML is entirely text format and uses the transformation format UTF-8
            byte[] bytes = Encoding.UTF8.GetBytes(htmlDataString);
            return Encoding.UTF8.GetString(bytes, startFragmentIndex, endFragmentIndex - startFragmentIndex);
        }
    }
}
