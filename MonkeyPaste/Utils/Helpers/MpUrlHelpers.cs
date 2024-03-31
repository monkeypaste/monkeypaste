using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Nager.PublicSuffix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste {

    public class MpUrlProperties {
        public string Title { get; set; }
        public string FavIconBase64 { get; set; }

        public string FullyFormattedUriStr { get; set; }
        public string Domain { get; set; }

        public string HeadSource { get; set; }
    }

    public static class MpUrlHelpers {
        #region Constants
        public const int MAX_DOT_NET_URL_LENGTH = 65519;
        public const string BLANK_URL = "about:blank";
        #endregion

        #region Statics
        private static DomainParser _domainParser;

        private static IEnumerable<Func<MpUrlProperties,Task<string>>> _fav_icon_finders;
        private static IEnumerable<Func<MpUrlProperties, Task<string>>> fav_icon_finders {
            get {
                if(_fav_icon_finders == null) {
                    _fav_icon_finders = [
                        GetDomainFavIcon_parse_head,
                        GetDomainFavIcon_domain_favicon,
                        GetDomainFavIcon_use_google_service,
                        GetDomainFavIcon_parse_domain_head,
                        ];
                }
                return _fav_icon_finders;
            }
        }
        #endregion

        #region Public Methods
        public static async Task<MpUrlProperties> DiscoverUrlPropertiesAsync(string url = "") {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }
            MpUrlProperties url_props = new MpUrlProperties() {
                FullyFormattedUriStr = GetFullyFormattedUrl(url)
            };
            url_props.Domain = GetUrlDomain(url_props.FullyFormattedUriStr);
            url_props.HeadSource = await ReadUrlHeadAsString(url);

            foreach(var fav_func in fav_icon_finders) {
                url_props.FavIconBase64 = await fav_func.Invoke(url_props);
                if(url_props.FavIconBase64 is not null) {
                    if(Mp.Services.IconBuilder.IsStringBase64Image(url_props.FavIconBase64)) {
                        break;
                    }
                    url_props.FavIconBase64 = null;
                }
            }

            if(url_props.HeadSource.ToStringOrEmpty().SplitNoEmpty("<title") is { } tl &&
                tl.Skip(1).FirstOrDefault().ToStringOrEmpty().SplitNoEmpty("</title>").FirstOrDefault() is { } t_inner) {
                int close_idx = t_inner.IndexOf(">");
                if(close_idx >= 0 && close_idx < t_inner.Length - 1 && t_inner.Substring(close_idx + 1) is { } title_str) {
                    url_props.Title = title_str;
                }
            }
            if (string.IsNullOrEmpty(url_props.Title) &&
                !string.IsNullOrEmpty(url_props.HeadSource)) {
                url_props.Title = ParseHtmlTitle(url_props.HeadSource);
            }

            return url_props;
        }

        #region Favicon
        private static async Task<string> GetDomainFavIcon_domain_favicon(MpUrlProperties url_props) {
            string domain = url_props.Domain.ToStringOrEmpty();
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
                return null;
            }
        }

        private static async Task<string> GetDomainFavIcon_use_google_service(MpUrlProperties url_props) {
            try {
                string urlDomain = url_props.Domain.ToStringOrEmpty();
                Uri favicon = new Uri(@"https://www.google.com/s2/favicons?sz=128&domain_url=" + urlDomain, UriKind.Absolute);
                var bytes = await MpFileIo.ReadBytesFromUriAsync(favicon.AbsoluteUri);
                if (bytes != null &&
                    bytes.Length > 0 &&
                    Convert.ToBase64String(bytes) is { } base64 && 
                    !base64.Equals(MpBase64Images.UnknownFavIcon)) {
                    // avoid googles fallback 
                    return base64;
                }
                return null;
            }
            catch (Exception ex) {
                Console.WriteLine("MpHelpers.GetUrlFavicon error for url: " + url_props.FullyFormattedUriStr + " with exception: " + ex);
                return null;
            }
        }
        private static async Task<string> GetDomainFavIcon_parse_domain_head(MpUrlProperties url_props) {
            string domain_head_html = await ReadUrlHeadAsString(url_props.Domain);
            var result = await GetDomainFavIcon_parse_head(new() { HeadSource = domain_head_html });
            return result;
        }
        private static async Task<string> GetDomainFavIcon_parse_head(MpUrlProperties url_props) {
            string html_source = url_props.HeadSource.ToStringOrEmpty();

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
            string icon_base64 = null;
            // icon based on https://www.how7o.com/t/how-to-get-a-websites-favicon-url-with-javascript/57/2
            var icon_link_nodes =
                headNode.ChildNodes
                .Where(x =>
                    x.Name.ToLowerInvariant() == "link" &&
                    (x.GetAttributeValue("rel", string.Empty).Trim().ToLowerInvariant() == "icon" ||
                    x.GetAttributeValue("rel", string.Empty).Trim().ToLowerInvariant() == "shortcut icon"));

            string icon_uri = null;
            if (icon_link_nodes.Any()) {
                var icon_node = icon_link_nodes.FirstOrDefault(x => x.GetAttributeValue("rel", string.Empty).Trim().ToLowerInvariant() == "icon");
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

            return icon_base64;
        }

        #endregion
        public static bool IsBlankUrl(string url) {
            if (string.IsNullOrEmpty(url)) {
                return false;
            }
            string url_lwc = url.ToLowerInvariant().Replace("http://", string.Empty).Replace("https://", string.Empty);
            return url_lwc.StartsWith(BLANK_URL);
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


        private static string ParseHtmlTitle3(HtmlDocument doc) {
            // from https://stackoverflow.com/a/49487198/105028
            if (doc.DocumentNode.SelectSingleNode("html/head/title") is not HtmlNode titleNode) {
                return string.Empty;
            }
            return titleNode.InnerText;
        }

        private static async Task<string> ReadUrlHeadAsString(string url) {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                return string.Empty;
            }
            try {
                var sb = new StringBuilder();
                using (HttpClient client = new HttpClient()) {
                    client.SetDefaultUserAgent();

                    try {
                        using (HttpResponseMessage response = await client.GetAsync(url)) {
                            using (var stream = await response.Content.ReadAsStreamAsync()) {
                                while (true) {
                                    long rem = stream.Length - stream.Position;
                                    if (rem <= 0) {
                                        // no head found?
                                        break;
                                    }
                                    int len = Math.Min(1000, (int)rem);
                                    var bytes = new byte[len];
                                    var bytesread = stream.Read(bytes, 0, len);
                                    string text = Encoding.UTF8.GetString(bytes);
                                    int head_end_idx = text.ToLower().IndexOf("</head>");
                                    if (head_end_idx < 0) {
                                        sb.Append(text);
                                        continue;
                                    }
                                    sb.Append(text.Substring(0, head_end_idx + "</head>".Length));
                                    // complete doc
                                    sb.Append("</html>");
                                    break;
                                }
                                return sb.ToString();
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
            if (trimmed_url.ToLowerInvariant().EndsWith(url_domain.ToLowerInvariant())) {
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

        #endregion
    }
}
