using HtmlAgilityPack;
using MonkeyPaste.Common;
using Nager.PublicSuffix;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public class MpUrlProperties {
        public string Title { get; set; }
        public string FavIconBase64 { get; set; }

        public string FullyFormattedUriStr { get; set; }

        public string Source { get; set; }
    }

    public static class MpUrlHelpers {
        #region Constants
        public const int MAX_DOT_NET_URL_LENGTH = 65519;
        #endregion

        #region Statics
        private static DomainParser _domainParser;
        #endregion

        #region Public Methods
        public static async Task<MpUrlProperties> DiscoverUrlPropertiesAsync(string url = "") {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }
            MpUrlProperties url_props = new MpUrlProperties() {
                FullyFormattedUriStr = GetFullyFormattedUrl(url)
            };
            url_props.Source = await ReadUrlAsString(url);

            string formatted_url = url_props.FullyFormattedUriStr;
            url_props.FavIconBase64 = string.Empty;
            if (Uri.IsWellFormedUriString(formatted_url, UriKind.Absolute)) {
                var uri = new Uri(formatted_url, UriKind.Absolute);
                url_props.FavIconBase64 = await GetDomainFavIcon1(uri.Host);
                if (!Mp.Services.IconBuilder.IsStringBase64Image(url_props.FavIconBase64)) {
                    url_props.FavIconBase64 = await GetDomainFavIcon2(formatted_url);
                    if (!Mp.Services.IconBuilder.IsStringBase64Image(url_props.FavIconBase64)) {
                        // NOTE #3 uses html not url
                        var result_tuple = await GetDomainFavIcon3(url_props.Source);
                        if (result_tuple != null &&
                            Mp.Services.IconBuilder.IsStringBase64Image(result_tuple.Item1)) {
                            url_props.FavIconBase64 = result_tuple.Item1;
                            url_props.Title = result_tuple.Item2;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(url_props.FavIconBase64) ||
                url_props.FavIconBase64.Equals(MpBase64Images.UnknownFavIcon)) {
                url_props.FavIconBase64 = null;// MpBase64Images.AppIcon;
            }
            if (string.IsNullOrEmpty(url_props.Title) &&
                !string.IsNullOrEmpty(url_props.Source)) {
                url_props.Title = ParseHtmlTitle(url_props.Source);
            }

            return url_props;
        }

        public static bool IsBlankUrl(string url) {
            if (string.IsNullOrEmpty(url)) {
                return false;
            }
            string url_lwc = url.ToLower().Replace("http://", string.Empty).Replace("https://", string.Empty);
            return url_lwc.StartsWith("about:blank");
        }


        #endregion

        #region Private Methods
        private static string GetFullyFormattedUrl(string str) {
            // reading linux moz url source pads every character of url w/ empty character
            // but trying to trim it doesn't work this manually parses string for actual characters
            // because Uri throws error on create

            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++) {
                if ((int)((char)str[i]) == 0) {
                    // one of the dudz
                    continue;
                } else {
                    sb.Append(str[i]);
                }
            }
            str = sb.ToString();
            if (str.StartsWith(@"http://")) {
                return str;
            }
            if (str.StartsWith(@"https://")) {
                return str;
            }
            //use http without s because if it is https then it will resolve to but otherwise will not load
            return @"http://" + str;
        }

        private static string ParseHtmlTitle(string html) {
            // from https://stackoverflow.com/a/329324/105028
            string title =
                Regex.Match(
                    html,
                    @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                    RegexOptions.IgnoreCase)
                .Groups["Title"]
                .Value;
            return title;
        }

        private static string ParseHtmlTitle2(string html) {
            string GetXmlElementContent(string xml, string element) {
                if (string.IsNullOrEmpty(xml) || string.IsNullOrEmpty(element)) {
                    return string.Empty;
                }
                element = element.Replace(@"<", string.Empty).Replace(@"/>", string.Empty);
                element = @"<" + element + @">";
                var strl = xml.Split(new string[] { element }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (strl.Count > 1) {
                    element = element.Replace(@"<", @"</");
                    return strl[1].Substring(0, strl[1].IndexOf(element));
                }
                return string.Empty;
            }

            return GetXmlElementContent(html, @"title");
        }

        private static string ParseHtmlTitle3(HtmlDocument doc) {
            // from https://stackoverflow.com/a/49487198/105028
            if (doc.DocumentNode.SelectSingleNode("html/head/title") is not HtmlNode titleNode) {
                return string.Empty;
            }
            return titleNode.InnerText;
        }

        private static async Task<string> ReadUrlAsString(string url) {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {//if (!IsValidUrl(url)) {
                return string.Empty;
            }
            try {
                using (HttpClient client = new HttpClient()) {
                    client.SetDefaultUserAgent();

                    try {
                        using (HttpResponseMessage response = await client.GetAsync(url)) {
                            using (HttpContent content = response.Content) {
                                var result = await content.ReadAsStringAsync();
                                return result;
                            }
                        }
                    }
                    catch (HttpRequestException) {
                        return string.Empty;
                    }

                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Error scanning for url title at " + url, ex);
                return string.Empty;
            }
        }

        public static bool IsUrlTopLevel(string url) {
            string url_domain = GetUrlDomain(url);
            if (string.IsNullOrEmpty(url_domain)) {
                return false;
            }
            string trimmed_url = url.TrimEnd(new[] { '/', '&', '?' });
            if (trimmed_url.ToLower().EndsWith(url_domain.ToLower())) {
                return true;
            }
            return false;
        }
        public static string GetUrlDomain(string url) {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) ||
                    new Uri(url) is not Uri uri) {
                return string.Empty;
            }
            if (_domainParser == null) {
                _domainParser = new DomainParser(new WebTldRuleProvider());
            }

            if (string.IsNullOrWhiteSpace(url)) {
                return string.Empty;
            }

            //domainInfo.Domain = "test";
            //domainInfo.Hostname = "sub.test.co.uk";
            //domainInfo.RegistrableDomain = "test.co.uk";
            //domainInfo.SubDomain = "sub";
            //domainInfo.TLD = "co.uk";
            //returns protocol prefixed domain url text
            try {
                var domainInfo = _domainParser.Parse(uri);
                string domain = domainInfo.RegistrableDomain;

                if (_domainParser.IsValidDomain(domain)) {
                    return domain;
                }
                MpDebug.Break($"what's wrong with this domain? '{domain}'");
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error resolving domain from url '{url}'.", ex);
            }
            return string.Empty;
        }

        private static async Task<string> GetDomainFavIcon1(string domain) {
            try {
                if (!domain.StartsWith("https://")) {
                    domain = "https://" + domain;
                }
                if (!domain.EndsWith("/")) {
                    domain += "/";
                }
                string favicon_uri_str = $"{domain}favicon.ico";
                if (!Uri.IsWellFormedUriString(favicon_uri_str, UriKind.Absolute)) {
                    // whats the domain?
                    MpDebug.Break();
                    return null;
                }
                Uri favicon = new Uri(favicon_uri_str, UriKind.Absolute);
                var bytes = await MpFileIo.ReadBytesFromUriAsync(favicon.AbsoluteUri);
                if (bytes == null || bytes.Length == 0) {
                    return null;
                }
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex) {
                Console.WriteLine("MpHelpers.GetUrlFavicon error for url: " + domain + " with exception: " + ex);
                return string.Empty;
            }
        }

        private static async Task<string> GetDomainFavIcon2(string url) {
            try {
                string urlDomain = GetUrlDomain(url);
                Uri favicon = new Uri(@"https://www.google.com/s2/favicons?sz=128&domain_url=" + urlDomain, UriKind.Absolute);
                var bytes = await MpFileIo.ReadBytesFromUriAsync(favicon.AbsoluteUri);
                if (bytes == null || bytes.Length == 0) {
                    return null;
                }
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex) {
                Console.WriteLine("MpHelpers.GetUrlFavicon error for url: " + url + " with exception: " + ex);
                return string.Empty;
            }
        }
        private static async Task<Tuple<string, string>> GetDomainFavIcon3(string html_source) {
            // use EXTREME fallback to find favicon (& title if found)
            // result [base64FavIcon,title]

            var url_doc = new HtmlDocument();
            try {
                url_doc.LoadHtml(html_source);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error loading html  w/ Source: '{html_source}'", ex);
                url_doc = null;
            }
            if (url_doc == null ||
                url_doc.DocumentNode.SelectSingleNode("//head") is not HtmlNode headNode) {
                return null;
            }

            string title = ParseHtmlTitle3(url_doc);

            string icon_base64 = null;
            // icon based on https://www.how7o.com/t/how-to-get-a-websites-favicon-url-with-javascript/57/2
            var icon_link_nodes =
                headNode.ChildNodes
                .Where(x =>
                    x.Name.ToLower() == "link" &&
                    (x.GetAttributeValue("rel", string.Empty).Trim().ToLower() == "icon" ||
                    x.GetAttributeValue("rel", string.Empty).Trim().ToLower() == "shortcut icon"));

            string icon_uri = null;
            if (icon_link_nodes.Count() > 0) {
                var icon_node = icon_link_nodes.FirstOrDefault(x => x.GetAttributeValue("rel", string.Empty).Trim().ToLower() == "icon");
                if (icon_node != null) {
                    // prefer 'icon' over 'shortcut icon' (i guess)
                    icon_uri = icon_node.GetAttributeValue("href", null);
                } else {
                    icon_uri = icon_link_nodes.First().GetAttributeValue("href", null);
                }
            }

            if (!string.IsNullOrEmpty(icon_uri)) {
                var bytes = await MpFileIo.ReadBytesFromUriAsync(icon_uri);
                if (bytes != null && bytes.Length > 0) {
                    icon_base64 = Convert.ToBase64String(bytes);
                }
            }

            return new Tuple<string, string>(icon_base64, title);
        }

        #endregion
    }
}
