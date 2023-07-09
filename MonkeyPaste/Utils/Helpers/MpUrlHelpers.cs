using HtmlAgilityPack;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace MonkeyPaste {

    public class MpUrlProperties {
        public string Title { get; set; }
        public string IconBase64 { get; set; }

        public string DomainStr { get; set; }
        public string FullyFormattedUriStr { get; set; }

        public string Source { get; set; }
    }

    public static class MpUrlHelpers {
        public const int MAX_DOT_NET_URL_LENGTH = 65519;

        public static async Task<MpUrlProperties> DiscoverUrlProperties(string url = "") {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }
            MpUrlProperties url_props = new MpUrlProperties() {
                FullyFormattedUriStr = GetFullyFormattedUrl(url)
            };
            url_props.Source = await ReadUrlAsString(url);

            //url_props.IconBase64 = await GetUrlFavIconAsync(url_props.FullyFormattedUriStr);
            string formatted_url = url_props.FullyFormattedUriStr;
            url_props.IconBase64 = string.Empty;
            if (Uri.IsWellFormedUriString(formatted_url, UriKind.Absolute)) {
                var uri = new Uri(formatted_url, UriKind.Absolute);
                url_props.IconBase64 = await GetDomainFavIcon1(uri.Host);
                if (!Mp.Services.IconBuilder.IsStringBase64Image(url_props.IconBase64)) {
                    url_props.IconBase64 = await GetDomainFavIcon2(formatted_url);
                    if (!Mp.Services.IconBuilder.IsStringBase64Image(url_props.IconBase64)) {
                        // NOTE #3 uses html not url
                        var result_tuple = await GetDomainFavIcon3(url_props.Source);
                        if (result_tuple != null &&
                            Mp.Services.IconBuilder.IsStringBase64Image(result_tuple.Item1)) {
                            url_props.IconBase64 = result_tuple.Item1;
                            url_props.Title = result_tuple.Item2;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(url_props.IconBase64) ||
                url_props.IconBase64.Equals(MpBase64Images.UnknownFavIcon)) {
                url_props.IconBase64 = MpBase64Images.AppIcon;
            }
            if (string.IsNullOrEmpty(url_props.Title) &&
                !string.IsNullOrEmpty(url_props.Source)) {
                url_props.Title = ParseHtmlTitle(url_props.Source);
            }

            url_props.DomainStr = GetUrlDomain(url_props.FullyFormattedUriStr);

            return url_props;
        }
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
                    client.DefaultRequestHeaders.Add("User-Agent", System.Guid.NewGuid().ToString());

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

        private static bool IsValidUrl(string str) {
            bool hasValidExtension = false;
            string lstr = str.ToLower();
            foreach (var ext in _domainExtensions) {
                if (lstr.Contains(ext)) {
                    hasValidExtension = true;
                    break;
                }
            }
            if (!hasValidExtension) {
                return false;
            }
            return MpRegEx.RegExLookup[MpRegExType.Uri].IsMatch(lstr);
        }

        private static string GetUrlDomain(string url) {
            //returns protocol prefixed domain url text
            try {
                url = GetFullyFormattedUrl(url);
                string host = new Uri(url).Host;
                var subDomainIdxList = host.IndexListOfAll(".");
                for (int i = subDomainIdxList.Count - 1; i > 0; i--) {
                    string subStr = host.Substring(subDomainIdxList[i]);
                    if (_domainExtensions.Contains(subStr)) {
                        return host.Substring(subDomainIdxList[i - 1] + 1);
                    }
                }
                return host;

                //int domainStartIdx = url.IndexOf(@"//") + 2;
                //if (url.Length <= domainStartIdx) {
                //    return string.Empty;
                //}
                //if (!url.Substring(domainStartIdx).Contains(@"/")) {
                //    if (subDomainIdxList.Count > 1) {
                //        return url.Substring(domainStartIdx).Substring(subDomainIdxList[subDomainIdxList.Count - 1]);
                //    }
                //    return url.Substring(domainStartIdx);
                //}
                //int domainEndIdx = url.Substring(domainStartIdx).IndexOf(@"/");
                //int preIdx = 0;
                //if (subDomainIdxList.Count > 1) {
                //    preIdx = subDomainIdxList[subDomainIdxList.Count - 1];
                //}
                //return url.Substring(domainStartIdx).Substring(preIdx, domainEndIdx - preIdx);
            }
            catch (Exception ex) {
                MpConsole.WriteLine("MpUrlHelpers.GetUrlDomain error for url: " + url + " with exception: " + ex);
            }
            return null;
        }

        private static async Task<string> GetUrlFavIconAsync(string url) {
            try {
                string base64FavIcon = string.Empty;
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                    var uri = new Uri(url, UriKind.Absolute);
                    base64FavIcon = await GetDomainFavIcon1(uri.Host);
                    if (Mp.Services.IconBuilder.IsStringBase64Image(base64FavIcon)) {
                        return base64FavIcon;
                    }
                    base64FavIcon = await GetDomainFavIcon2(url);
                    if (Mp.Services.IconBuilder.IsStringBase64Image(base64FavIcon)) {
                        return base64FavIcon;
                    }
                    if (string.IsNullOrEmpty(base64FavIcon) ||
                        base64FavIcon.Equals(MpBase64Images.UnknownFavIcon)) {
                        base64FavIcon = MpBase64Images.AppIcon;
                    }
                }
                return base64FavIcon;
            }
            catch (Exception ex) {
                Console.WriteLine("MpHelpers.GetUrlFavicon error for url: " + url + " with exception: " + ex);
                return string.Empty;
            }
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

        private static HashSet<string> _domainExtensions = new HashSet<string> {
            // TODO try to sort these by common use to make more efficient
            ".com",
            ".org",
            ".gov",
            ".abbott",
            ".abogado",
            ".ac",
            ".academy",
            ".accountant",
            ".accountants",
            ".active",
            ".actor",
            ".ad",
            ".ads",
            ".adult",
            ".ae",
            ".aero",
            ".af",
            ".afl",
            ".ag",
            ".agency",
            ".ai",
            ".airforce",
            ".al",
            ".allfinanz",
            ".alsace",
            ".am",
            ".amsterdam",
            ".an",
            ".android",
            ".ao",
            ".apartments",
            ".aq",
            ".aquarelle",
            ".ar",
            ".archi",
            ".army",
            ".arpa",
            ".as",
            ".asia",
            ".associates",
            ".at",
            ".attorney",
            ".au",
            ".auction",
            ".audio",
            ".autos",
            ".aw",
            ".ax",
            ".axa",
            ".az",
            ".ba",
            ".band",
            ".bank",
            ".bar",
            ".barclaycard",
            ".barclays",
            ".bargains",
            ".bauhaus",
            ".bayern",
            ".bb",
            ".bbc",
            ".bd",
            ".be",
            ".beer",
            ".berlin",
            ".best",
            ".bf",
            ".bg",
            ".bh",
            ".bi",
            ".bid",
            ".bike",
            ".bingo",
            ".bio",
            ".biz",
            ".bj",
            ".bl",
            ".black",
            ".blackfriday",
            ".bloomberg",
            ".blue",
            ".bm",
            ".bmw",
            ".bn",
            ".bnpparibas",
            ".bo",
            ".boats",
            ".bond",
            ".boo",
            ".boutique",
            ".bq",
            ".br",
            ".brussels",
            ".bs",
            ".bt",
            ".budapest",
            ".build",
            ".builders",
            ".business",
            ".buzz",
            ".bv",
            ".bw",
            ".by",
            ".bz",
            ".bzh",
            ".ca",
            ".cab",
            ".cafe",
            ".cal",
            ".camera",
            ".camp",
            ".cancerresearch",
            ".canon",
            ".capetown",
            ".capital",
            ".caravan",
            ".cards",
            ".care",
            ".career",
            ".careers",
            ".cartier",
            ".casa",
            ".cash",
            ".casino",
            ".cat",
            ".catering",
            ".cbn",
            ".cc",
            ".cd",
            ".center",
            ".ceo",
            ".cern",
            ".cf",
            ".cfd",
            ".cg",
            ".ch",
            ".channel",
            ".chat",
            ".cheap",
            ".chloe",
            ".christmas",
            ".chrome",
            ".church",
            ".ci",
            ".citic",
            ".city",
            ".ck",
            ".cl",
            ".claims",
            ".cleaning",
            ".click",
            ".clinic",
            ".clothing",
            ".club",
            ".cm",
            ".cn",
            ".co",
            ".coach",
            ".codes",
            ".coffee",
            ".college",
            ".cologne",
            ".community",
            ".company",
            ".computer",
            ".condos",
            ".construction",
            ".consulting",
            ".contractors",
            ".cooking",
            ".cool",
            ".coop",
            ".country",
            ".courses",
            ".cr",
            ".credit",
            ".creditcard",
            ".cricket",
            ".crs",
            ".cruises",
            ".cu",
            ".cuisinella",
            ".cv",
            ".cw",
            ".cx",
            ".cy",
            ".cymru",
            ".cyou",
            ".cz",
            ".dabur",
            ".dad",
            ".dance",
            ".date",
            ".dating",
            ".datsun",
            ".day",
            ".dclk",
            ".de",
            ".deals",
            ".degree",
            ".delivery",
            ".democrat",
            ".dental",
            ".dentist",
            ".desi",
            ".design",
            ".dev",
            ".diamonds",
            ".diet",
            ".digital",
            ".direct",
            ".directory",
            ".discount",
            ".dj",
            ".dk",
            ".dm",
            ".dnp",
            ".do",
            ".docs",
            ".doha",
            ".domains",
            ".doosan",
            ".download",
            ".durban",
            ".dvag",
            ".dz",
            ".eat",
            ".ec",
            ".edu",
            ".education",
            ".ee",
            ".eg",
            ".eh",
            ".email",
            ".emerck",
            ".energy",
            ".engineer",
            ".engineering",
            ".enterprises",
            ".epson",
            ".equipment",
            ".er",
            ".erni",
            ".es",
            ".esq",
            ".estate",
            ".et",
            ".eu",
            ".eurovision",
            ".eus",
            ".events",
            ".everbank",
            ".exchange",
            ".expert",
            ".exposed",
            ".express",
            ".fail",
            ".faith",
            ".fan",
            ".fans",
            ".farm",
            ".fashion",
            ".feedback",
            ".fi",
            ".film",
            ".finance",
            ".financial",
            ".firmdale",
            ".fish",
            ".fishing",
            ".fit",
            ".fitness",
            ".fj",
            ".fk",
            ".flights",
            ".florist",
            ".flowers",
            ".flsmidth",
            ".fly",
            ".fm",
            ".fo",
            ".foo",
            ".football",
            ".forex",
            ".forsale",
            ".foundation",
            ".fr",
            ".frl",
            ".frogans",
            ".fund",
            ".furniture",
            ".futbol",
            ".ga",
            ".gal",
            ".gallery",
            ".garden",
            ".gb",
            ".gbiz",
            ".gd",
            ".gdn",
            ".ge",
            ".gent",
            ".gf",
            ".gg",
            ".ggee",
            ".gh",
            ".gi",
            ".gift",
            ".gifts",
            ".gives",
            ".gl",
            ".glass",
            ".gle",
            ".global",
            ".globo",
            ".gm",
            ".gmail",
            ".gmo",
            ".gmx",
            ".gn",
            ".gold",
            ".goldpoint",
            ".golf",
            ".goo",
            ".goog",
            ".google",
            ".gop",
            ".gp",
            ".gq",
            ".gr",
            ".graphics",
            ".gratis",
            ".green",
            ".gripe",
            ".gs",
            ".gt",
            ".gu",
            ".guge",
            ".guide",
            ".guitars",
            ".guru",
            ".gw",
            ".gy",
            ".hamburg",
            ".hangout",
            ".haus",
            ".healthcare",
            ".help",
            ".here",
            ".hermes",
            ".hiphop",
            ".hiv",
            ".hk",
            ".hm",
            ".hn",
            ".holdings",
            ".holiday",
            ".homes",
            ".horse",
            ".host",
            ".hosting",
            ".house",
            ".how",
            ".hr",
            ".ht",
            ".hu",
            ".ibm",
            ".id",
            ".ie",
            ".ifm",
            ".il",
            ".im",
            ".immo",
            ".immobilien",
            ".in",
            ".industries",
            ".infiniti",
            ".info",
            ".ing",
            ".ink",
            ".institute",
            ".insure",
            ".int",
            ".international",
            ".investments",
            ".io",
            ".iq",
            ".ir",
            ".irish",
            ".is",
            ".it",
            ".iwc",
            ".java",
            ".jcb",
            ".je",
            ".jetzt",
            ".jm",
            ".jo",
            ".jobs",
            ".joburg",
            ".jp",
            ".juegos",
            ".kaufen",
            ".kddi",
            ".ke",
            ".kg",
            ".kh",
            ".ki",
            ".kim",
            ".kitchen",
            ".kiwi",
            ".km",
            ".kn",
            ".koeln",
            ".komatsu",
            ".kp",
            ".kr",
            ".krd",
            ".kred",
            ".kw",
            ".ky",
            ".kyoto",
            ".kz",
            ".la",
            ".lacaixa",
            ".land",
            ".lat",
            ".latrobe",
            ".lawyer",
            ".lb",
            ".lc",
            ".lds",
            ".lease",
            ".leclerc",
            ".legal",
            ".lgbt",
            ".li",
            ".lidl",
            ".life",
            ".lighting",
            ".limited",
            ".limo",
            ".link",
            ".lk",
            ".loan",
            ".loans",
            ".london",
            ".lotte",
            ".lotto",
            ".love",
            ".lr",
            ".ls",
            ".lt",
            ".ltda",
            ".lu",
            ".luxe",
            ".luxury",
            ".lv",
            ".ly",
            ".ma",
            ".madrid",
            ".maif",
            ".maison",
            ".management",
            ".mango",
            ".market",
            ".marketing",
            ".markets",
            ".marriott",
            ".mc",
            ".md",
            ".me",
            ".media",
            ".meet",
            ".melbourne",
            ".meme",
            ".memorial",
            ".menu",
            ".mf",
            ".mg",
            ".mh",
            ".miami",
            ".mil",
            ".mini",
            ".mk",
            ".ml",
            ".mm",
            ".mma",
            ".mn",
            ".mo",
            ".mobi",
            ".moda",
            ".moe",
            ".monash",
            ".money",
            ".mormon",
            ".mortgage",
            ".moscow",
            ".motorcycles",
            ".mov",
            ".movie",
            ".mp",
            ".mq",
            ".mr",
            ".ms",
            ".mt",
            ".mtn",
            ".mtpc",
            ".mu",
            ".museum",
            ".mv",
            ".mw",
            ".mx",
            ".my",
            ".mz",
            ".na",
            ".nagoya",
            ".name",
            ".navy",
            ".nc",
            ".ne",
            ".net",
            ".network",
            ".neustar",
            ".new",
            ".news",
            ".nexus",
            ".nf",
            ".ng",
            ".ngo",
            ".nhk",
            ".ni",
            ".nico",
            ".ninja",
            ".nissan",
            ".nl",
            ".no",
            ".np",
            ".nr",
            ".nra",
            ".nrw",
            ".ntt",
            ".nu",
            ".nyc",
            ".nz",
            ".okinawa",
            ".om",
            ".one",
            ".ong",
            ".onl",
            ".online",
            ".ooo",
            ".organic",
            ".osaka",
            ".otsuka",
            ".ovh",
            ".pa",
            ".page",
            ".panerai",
            ".paris",
            ".partners",
            ".parts",
            ".party",
            ".pe",
            ".pf",
            ".pg",
            ".ph",
            ".pharmacy",
            ".photo",
            ".photography",
            ".photos",
            ".physio",
            ".piaget",
            ".pics",
            ".pictet",
            ".pictures",
            ".pink",
            ".pizza",
            ".pk",
            ".pl",
            ".place",
            ".plumbing",
            ".plus",
            ".pm",
            ".pn",
            ".pohl",
            ".poker",
            ".porn",
            ".post",
            ".pr",
            ".praxi",
            ".press",
            ".pro",
            ".prod",
            ".productions",
            ".prof",
            ".properties",
            ".property",
            ".ps",
            ".pt",
            ".pub",
            ".pw",
            ".py",
            ".qa",
            ".qpon",
            ".quebec",
            ".racing",
            ".re",
            ".realtor",
            ".recipes",
            ".red",
            ".redstone",
            ".rehab",
            ".reise",
            ".reisen",
            ".reit",
            ".ren",
            ".rentals",
            ".repair",
            ".report",
            ".republican",
            ".rest",
            ".restaurant",
            ".review",
            ".reviews",
            ".rich",
            ".rio",
            ".rip",
            ".ro",
            ".rocks",
            ".rodeo",
            ".rs",
            ".rsvp",
            ".ru",
            ".ruhr",
            ".rw",
            ".ryukyu",
            ".sa",
            ".saarland",
            ".sale",
            ".samsung",
            ".sap",
            ".sarl",
            ".saxo",
            ".sb",
            ".sc",
            ".sca",
            ".scb",
            ".schmidt",
            ".scholarships",
            ".school",
            ".schule",
            ".schwarz",
            ".science",
            ".scot",
            ".sd",
            ".se",
            ".services",
            ".sew",
            ".sexy",
            ".sg",
            ".sh",
            ".shiksha",
            ".shoes",
            ".shriram",
            ".si",
            ".singles",
            ".site",
            ".sj",
            ".sk",
            ".sky",
            ".sl",
            ".sm",
            ".sn",
            ".so",
            ".social",
            ".software",
            ".sohu",
            ".solar",
            ".solutions",
            ".soy",
            ".space",
            ".spiegel",
            ".spreadbetting",
            ".sr",
            ".ss",
            ".st",
            ".study",
            ".style",
            ".su",
            ".sucks",
            ".supplies",
            ".supply",
            ".support",
            ".surf",
            ".surgery",
            ".suzuki",
            ".sv",
            ".sx",
            ".sy",
            ".sydney",
            ".systems",
            ".sz",
            ".taipei",
            ".tatar",
            ".tattoo",
            ".tax",
            ".tc",
            ".td",
            ".tech",
            ".technology",
            ".tel",
            ".temasek",
            ".tennis",
            ".tf",
            ".tg",
            ".th",
            ".tickets",
            ".tienda",
            ".tips",
            ".tires",
            ".tirol",
            ".tj",
            ".tk",
            ".tl",
            ".tm",
            ".tn",
            ".to",
            ".today",
            ".tokyo",
            ".tools",
            ".top",
            ".toshiba",
            ".tours",
            ".town",
            ".toys",
            ".tp",
            ".tr",
            ".trade",
            ".trading",
            ".training",
            ".travel",
            ".trust",
            ".tt",
            ".tui",
            ".tv",
            ".tw",
            ".tz",
            ".ua",
            ".ug",
            ".uk",
            ".um",
            ".university",
            ".uno",
            ".uol",
            ".us",
            ".uy",
            ".uz",
            ".va",
            ".vacations",
            ".vc",
            ".ve",
            ".vegas",
            ".ventures",
            ".versicherung",
            ".vet",
            ".vg",
            ".vi",
            ".viajes",
            ".video",
            ".villas",
            ".vision",
            ".vlaanderen",
            ".vn",
            ".vodka",
            ".vote",
            ".voting",
            ".voto",
            ".voyage",
            ".vu",
            ".wales",
            ".wang",
            ".watch",
            ".webcam",
            ".website",
            ".wed",
            ".wedding",
            ".wf",
            ".whoswho",
            ".wien",
            ".wiki",
            ".williamhill",
            ".win",
            ".wme",
            ".work",
            ".works",
            ".world",
            ".ws",
            ".wtc",
            ".wtf",
            ".xin",
            ".æµ‹è¯•",
            ".à¤ªà¤°à¥€à¤•à¥à¤·à¤¾",
            ".ä½›å±±",
            ".æ…ˆå–„",
            ".é›†å›¢",
            ".åœ¨çº¿",
            ".í•œêµ­",
            ".à¦­à¦¾à¦°à¦¤",
            ".å…«å¦",
            ".Ù…ÙˆÙ‚Ø¹",
            ".à¦¬à¦¾à¦‚à¦²à¦¾",
            ".å…¬ç›Š",
            ".å…¬å¸",
            ".ç§»åŠ¨",
            ".æˆ‘çˆ±ä½ ",
            ".Ð¼Ð¾ÑÐºÐ²Ð°",
            ".Ð¸ÑÐ¿Ñ‹Ñ‚Ð°Ð½Ð¸Ðµ",
            ".Ò›Ð°Ð·",
            ".Ð¾Ð½Ð»Ð°Ð¹Ð½",
            ".ÑÐ°Ð¹Ñ‚",
            ".ÑÑ€Ð±",
            ".Ð±ÐµÐ»",
            ".æ—¶å°š",
            ".í…ŒìŠ¤íŠ¸",
            ".æ·¡é©¬é”¡",
            ".Ð¾Ñ€Ð³",
            ".ì‚¼ì„±",
            ".à®šà®¿à®™à¯à®•à®ªà¯à®ªà¯‚à®°à¯",
            ".å•†æ ‡",
            ".å•†åº—",
            ".å•†åŸŽ",
            ".Ð´ÐµÑ‚Ð¸",
            ".Ð¼ÐºÐ´",
            ".×˜×¢×¡×˜",
            ".ä¸­æ–‡ç½‘",
            ".ä¸­ä¿¡",
            ".ä¸­å›½",
            ".ä¸­åœ‹",
            ".è°·æ­Œ",
            ".à°­à°¾à°°à°¤à±",
            ".à¶½à¶‚à¶šà·",
            ".æ¸¬è©¦",
            ".àª­àª¾àª°àª¤",
            ".à¤­à¤¾à¤°à¤¤",
            ".Ø¢Ø²Ù…Ø§ÛŒØ´ÛŒ",
            ".à®ªà®°à®¿à®Ÿà¯à®šà¯ˆ",
            ".ç½‘åº—",
            ".à¤¸à¤‚à¤—à¤ à¤¨",
            ".ç½‘ç»œ",
            ".ÑƒÐºÑ€",
            ".é¦™æ¸¯",
            ".Î´Î¿ÎºÎ¹Î¼Î®",
            ".é£žåˆ©æµ¦",
            ".Ø¥Ø®ØªØ¨Ø§Ø±",
            ".å°æ¹¾",
            ".å°ç£",
            ".æ‰‹æœº",
            ".Ð¼Ð¾Ð½",
            ".Ø§Ù„Ø¬Ø²Ø§Ø¦Ø±",
            ".Ø¹Ù…Ø§Ù†",
            ".Ø§ÛŒØ±Ø§Ù†",
            ".Ø§Ù…Ø§Ø±Ø§Øª",
            ".Ø¨Ø§Ø²Ø§Ø±",
            ".Ù¾Ø§Ú©Ø³ØªØ§Ù†",
            ".Ø§Ù„Ø§Ø±Ø¯Ù†",
            ".Ø¨Ú¾Ø§Ø±Øª",
            ".Ø§Ù„Ù…ØºØ±Ø¨",
            ".Ø§Ù„Ø³Ø¹ÙˆØ¯ÙŠØ©",
            ".Ø³ÙˆØ¯Ø§Ù†",
            ".Ø¹Ø±Ø§Ù‚",
            ".Ù…Ù„ÙŠØ³ÙŠØ§",
            ".æ”¿åºœ",
            ".Ø´Ø¨ÙƒØ©",
            ".áƒ’áƒ”",
            ".æœºæž„",
            ".ç»„ç»‡æœºæž„",
            ".å¥åº·",
            ".à¹„à¸—à¸¢",
            ".Ø³ÙˆØ±ÙŠØ©",
            ".Ñ€ÑƒÑ",
            ".Ñ€Ñ„",
            ".ØªÙˆÙ†Ø³",
            ".ã¿ã‚“ãª",
            ".ã‚°ãƒ¼ã‚°ãƒ«",
            ".ä¸–ç•Œ",
            ".à¨­à¨¾à¨°à¨¤",
            ".ç½‘å€",
            ".æ¸¸æˆ",
            ".vermÃ¶gensberater",
            ".vermÃ¶gensberatung",
            ".ä¼ä¸š",
            ".ä¿¡æ¯",
            ".Ù…ØµØ±",
            ".Ù‚Ø·Ø±",
            ".å¹¿ä¸œ",
            ".à®‡à®²à®™à¯à®•à¯ˆ",
            ".à®‡à®¨à¯à®¤à®¿à®¯à®¾",
            ".Õ°Õ¡Õµ",
            ".æ–°åŠ å¡",
            ".ÙÙ„Ø³Ø·ÙŠÙ†",
            ".ãƒ†ã‚¹ãƒˆ",
            ".æ”¿åŠ¡",
            ".xxx",
            ".xyz",
            ".yachts",
            ".yandex",
            ".ye",
            ".yodobashi",
            ".yoga",
            ".yokohama",
            ".youtube",
            ".yt",
            ".za",
            ".zip",
            ".zm",
            ".zone",
            ".zuerich",
            ".zw"
        };
    }
}
