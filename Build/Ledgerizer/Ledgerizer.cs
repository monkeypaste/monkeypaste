using GoogleLiteTextTranslator;
using Microsoft.Extensions.Configuration;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Ledgerizer {
    [Flags]
    enum MpLedgerizerFlags : long {
        None = 0,
        DO_LOCAL_PACKAGING = 1L << 1,
        DO_REMOTE_PACKAGING = 1L << 2,
        FORCE_REPLACE_REMOTE_TAG = 1L << 3,
        DO_LOCAL_VERSIONS = 1L << 4,
        DO_REMOTE_VERSIONS = 1L << 5,
        DO_LOCAL_CULTURE_INDEX = 1L << 6,
        DO_REMOTE_CULTURE_INDEX = 1L << 7,
        LOCAL_MOVE_CORE_TO_DAT = 1L << 8,
        GEN_LOCALIZED_MANIFESTS = 1L << 9,
        GEN_EMPTY_RESX = 1L << 10,
        GEN_ADDON_LISTING = 1L << 11,
        GEN_PROD_LISTING = 1L << 12,
        TRANSLATE_RESX = 1L << 13,
        VERIFY_CONSISTENT_CULTURES = 1L << 14,
        GEN_EDITOR_UISTRS = 1L << 15,
        REMOTE_MOVE_CORE_TO_DAT = 1L << 16,
        DO_JS_UISTRINGS = 1L << 17,
        DO_LOCAL_LEDGER = 1L << 18,
        DO_REMOTE_LEDGER = 1L << 19,
        MOVE_NUGET_CACHE = 1L << 20,
        DEBUG = 1L << 21,
        RELEASE = 1L << 22,
    }
    internal class Ledgerizer {

        #region Properties

        static MpLedgerizerFlags LEDGERIZER_FLAGS =
            //MpLedgerizerFlags.GEN_EDITOR_UISTRS |
            //MpLedgerizerFlags.GEN_ADDON_LISTING |
            //MpLedgerizerFlags.GEN_PROD_LISTING |
            //MpLedgerizerFlags.TRANSLATE_RESX |
            //MpLedgerizerFlags.GEN_EMPTY_RESX
            //MpLedgerizerFlags.GEN_LOCALIZED_MANIFESTS |
            //MpLedgerizerFlags.DO_CULTURE_VERIFY |
            //MpLedgerizerFlags.DO_LOCAL_PACKAGING |
            //MpLedgerizerFlags.DO_LOCAL_LEDGER |
            //MpLedgerizerFlags.DO_REMOTE_PACKAGING |
            //MpLedgerizerFlags.DO_REMOTE_LEDGER |
            //MpLedgerizerFlags.DO_LOCAL_VERSIONS |
            //MpLedgerizerFlags.DO_REMOTE_VERSIONS |
            //MpLedgerizerFlags.DO_LOCAL_CULTURE_INDEX |
            //MpLedgerizerFlags.DO_REMOTE_CULTURE_INDEX |
            //MpLedgerizerFlags.LOCAL_MOVE_CORE_TO_DAT |
            //MpLedgerizerFlags.REMOTE_MOVE_CORE_TO_DAT |
            //MpLedgerizerFlags.DO_JS_UISTRINGS |
            //MpLedgerizerFlags.MOVE_NUGET_CACHE |
            //MpLedgerizerFlags.FORCE_REPLACE_REMOTE_TAG |
            //MpLedgerizerFlags.VERIFY_CONSISTENT_CULTURES
            //MpLedgerizerFlags.DEBUG  |
            //MpLedgerizerFlags.RELEASE
            MpLedgerizerFlags.None
            ;

        #region Localizer Props

        static GoogleLiteTextTranslatorPlugin _Translator = new GoogleLiteTextTranslatorPlugin();
        //static string ALL_CULTURES_CSV = "ar,ar-sa,ar-ae,ar-bh,ar-dz,ar-eg,ar-iq,ar-jo,ar-kw,ar-lb,ar-ly,ar-ma,ar-om,ar-qa,ar-sy,ar-tn,ar-ye,af,af-za,sq,sq-al,am,am-et,hy,hy-am,as,as-in,az-arab,az-arab-az,az-cyrl,az-cyrl-az,az-latn,az-latn-az,eu,eu-es,be,be-by,bn,bn-bd,bn-in,bs,bs-cyrl,bs-cyrl-ba,bs-latn,bs-latn-ba,bg,bg-bg,ca,ca-es,ca-es-valencia,chr-cher,chr-cher-us,chr-latn,zh-Hans,zh-cn,zh-hans-cn,zh-sg,zh-hans-sg,zh-Hant,zh-hk,zh-mo,zh-tw,zh-hant-hk,zh-hant-mo,zh-hant-tw,hr,hr-hr,hr-ba,cs,cs-cz,da,da-dk,prs,prs-af,prs-arab,nl,nl-nl,nl-be,en,en-au,en-ca,en-gb,en-ie,en-in,en-nz,en-sg,en-us,en-za,en-bz,en-hk,en-id,en-jm,en-kz,en-mt,en-my,en-ph,en-pk,en-tt,en-vn,en-zw,en-053,en-021,en-029,en-011,en-018,en-014,et,et-ee,fil,fil-latn,fil-ph,fi,fi-fi,fr,fr-be ,fr-ca ,fr-ch ,fr-fr ,fr-lu,fr-015,fr-cd,fr-ci,fr-cm,fr-ht,fr-ma,fr-mc,fr-ml,fr-re,frc-latn,frp-latn,fr-155,fr-029,fr-021,fr-011,gl,gl-es,ka,ka-ge,de,de-at,de-ch,de-de,de-lu,de-li,el,el-gr,gu,gu-in,ha,ha-latn,ha-latn-ng,he,he-il,hi,hi-in,hu,hu-hu,is,is-is,ig-latn,ig-ng,id,id-id,iu-cans,iu-latn,iu-latn-ca,ga,ga-ie,xh,xh-za,zu,zu-za,it,it-it,it-ch,ja ,ja-jp,kn,kn-in,kk,kk-kz,km,km-kh,quc-latn,qut-gt,qut-latn,rw,rw-rw,sw,sw-ke,kok,kok-in,ko,ko-kr,ku-arab,ku-arab-iq,ky-kg,ky-cyrl,lo,lo-la,lv,lv-lv,lt,lt-lt,lb,lb-lu,mk,mk-mk,ms,ms-bn,ms-my,ml,ml-in,mt,mt-mt,mi,mi-latn,mi-nz,mr,mr-in,mn-cyrl,mn-mong,mn-mn,mn-phag,ne,ne-np,nb,nb-no,nn,nn-no,no,no-no,or,or-in,fa,fa-ir,pl,pl-pl,pt-br,pt,pt-pt,pa,pa-arab,pa-arab-pk,pa-deva,pa-in,quz,quz-bo,quz-ec,quz-pe,ro,ro-ro,ru ,ru-ru,gd-gb,gd-latn,sr-Latn,sr-latn-cs,sr,sr-latn-ba,sr-latn-me,sr-latn-rs,sr-cyrl,sr-cyrl-ba,sr-cyrl-cs,sr-cyrl-me,sr-cyrl-rs,nso,nso-za,tn,tn-bw,tn-za,sd-arab,sd-arab-pk,sd-deva,si,si-lk,sk,sk-sk,sl,sl-si,es,es-cl,es-co,es-es,es-mx,es-ar,es-bo,es-cr,es-do,es-ec,es-gt,es-hn,es-ni,es-pa,es-pe,es-pr,es-py,es-sv,es-us,es-uy,es-ve,es-019,es-419,sv,sv-se,sv-fi,tg-arab,tg-cyrl,tg-cyrl-tj,tg-latn,ta,ta-in,tt-arab,tt-cyrl,tt-latn,tt-ru,te,te-in,th,th-th,ti,ti-et,tr,tr-tr,tk-cyrl,tk-latn,tk-tm,tk-latn-tr,tk-cyrl-tr,uk,uk-ua,ur,ur-pk,ug-arab,ug-cn,ug-cyrl,ug-latn,uz,uz-cyrl,uz-latn,uz-latn-uz,vi,vi-vn,cy,cy-gb,wo,wo-sn,yo-latn,yo-ng";
        static string MS_STORE_CULTURES_CSV = "ar-SA,ar-AE,ar-BH,ar-DZ,ar-EG,ar-IQ,ar-JO,ar-KW,ar-LB,ar-LY,ar-MA,ar-OM,ar-QA,ar-SY,ar-TN,ar-YE,af-ZA,sq-AL,am-ET,hy-AM,as-IN,az-Arab,az-Arab-AZ,az-Cyrl,az-Cyrl-AZ,az-Latn,az-Latn-AZ,eu-ES,be-BY,bn-BD,bn-IN,bs-Cyrl,bs-Cyrl-BA,bs-Latn,bs-Latn-BA,bg-BG,ca-ES,ca-ES-VALENCIA,chr-Cher,chr-Cher-US,chr-Latn,zh-Hans,zh-CN,zh-Hans-CN,zh-SG,zh-Hans-SG,zh-Hant,zh-HK,zh-MO,zh-TW,zh-Hant-HK,zh-Hant-MO,zh-Hant-TW,hr-HR,hr-BA,cs-CZ,da-DK,prs-AF,prs-Arab,nl-NL,nl-BE,en-AU,en-CA,en-GB,en-IE,en-IN,en-NZ,en-SG,en-US,en-ZA,en-BZ,en-HK,en-ID,en-JM,en-KZ,en-MT,en-MY,en-PH,en-PK,en-TT,en-VN,en-ZW,en-053,en-021,en-029,en-011,en-018,en-014,et-EE,fil-Latn,fil-PH,fi-FI,fr-LU,fr-015,fr-CD,fr-CI,fr-CM,fr-HT,fr-MA,fr-MC,fr-ML,fr-RE,frc-Latn,frp-Latn,fr-155,fr-029,fr-021,fr-011,gl-ES,ka-GE,de-AT,de-CH,de-DE,de-LU,de-LI,el-GR,gu-IN,ha-Latn,ha-Latn-NG,he-IL,hi-IN,hu-HU,is-IS,ig-Latn,ig-NG,id-ID,iu-Cans,iu-Latn,iu-Latn-CA,ga-IE,xh-ZA,zu-ZA,it-IT,it-CH,ja-JP,kn-IN,kk-KZ,km-KH,quc-Latn,qut-GT,qut-Latn,rw-RW,sw-KE,kok-IN,ko-KR,ku-Arab,ku-Arab-IQ,ky-KG,ky-Cyrl,lo-LA,lv-LV,lt-LT,lb-LU,mk-MK,ms-BN,ms-MY,ml-IN,mt-MT,mi-Latn,mi-NZ,mr-IN,mn-Cyrl,mn-Mong,mn-MN,mn-Phag,ne-NP,nb-NO,nn-NO,no-NO,or-IN,fa-IR,pl-PL,pt-BR,pt-PT,pa-Arab,pa-Arab-PK,pa-Deva,pa-IN,quz-BO,quz-EC,quz-PE,ro-RO,ru-RU,gd-GB,gd-Latn,sr-Latn,sr-Latn-CS,sr-Latn-BA,sr-Latn-ME,sr-Latn-RS,sr-Cyrl,sr-Cyrl-BA,sr-Cyrl-CS,sr-Cyrl-ME,sr-Cyrl-RS,nso-ZA,tn-BW,tn-ZA,sd-Arab,sd-Arab-PK,sd-Deva,si-LK,sk-SK,sl-SI,es-CL,es-CO,es-ES,es-MX,es-AR,es-BO,es-CR,es-DO,es-EC,es-GT,es-HN,es-NI,es-PA,es-PE,es-PR,es-PY,es-SV,es-US,es-UY,es-VE,es-019,es-419,sv-SE,sv-FI,tg-Arab,tg-Cyrl,tg-Cyrl-TJ,tg-Latn,ta-IN,tt-Arab,tt-Cyrl,tt-Latn,tt-RU,te-IN,th-TH,ti-ET,tr-TR,tk-Cyrl,tk-Latn,tk-TM,tk-Latn-TR,tk-Cyrl-TR,uk-UA,ur-PK,ug-Arab,ug-CN,ug-Cyrl,ug-Latn,uz-Cyrl,uz-Latn,uz-Latn-UZ,vi-VN,cy-GB,wo-SN,yo-Latn,yo-NG";
        static List<string> MsStoreCultures =>
            MS_STORE_CULTURES_CSV.ToListFromCsv();

        static IEnumerable<string> WorkingCultures {
            get {
                //return new string[] { "en-US" };

                // these give 400 error on google translate
                string[] omitted = new string[] {
                    "iu-Latn",
                    "iu-Latn-CA",
                    "kok-IN",
                };
                // get cultures resx tool supports
                var specificCultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => c.GetAncestors().Any())
                .OrderBy(c => c.DisplayName)
                .ToArray();

                // get azure langs
                var azure_langs = new (string, string)[] {
                    ("Afrikaans-Afrikaans","af"),("Amharic-አማርኛ","am"),("Arabic-العربية","ar"),("Assamese-অসমীয়া","as"),("Azerbaijani-Azərbaycan","az"),("Bashkir-Bashkir","ba"),("Bulgarian-Български","bg"),("Bangla-বাংলা","bn"),("Tibetan-བོད་སྐད་","bo"),("Bosnian-Bosnian","bs"),("Catalan-Català","ca"),("Czech-Čeština","cs"),("Welsh-Cymraeg","cy"),("Danish-Dansk","da"),("German-Deutsch","de"),("Divehi-ދިވެހިބަސް","dv"),("Greek-Ελληνικά","el"),("English-English","en"),("Spanish-Español","es"),("Estonian-Eesti","et"),("Persian-فارسی","fa"),("Finnish-Suomi","fi"),("Filipino-Filipino","fil"),("Fijian-NaVosaVakaviti","fj"),("French-Français","fr"),("French(Canada)-Français(Canada)","fr-CA"),("Irish-Gaeilge","ga"),("Gujarati-ગુજરાતી","gu"),("Hebrew-עברית","he"),("Hindi-हिन्दी","hi"),("Croatian-Hrvatski","hr"),("HaitianCreole-HaitianCreole","ht"),("Hungarian-Magyar","hu"),("Armenian-Հայերեն","hy"),("Indonesian-Indonesia","id"),("Inuinnaqtun-Inuinnaqtun","ikt"),("Icelandic-Íslenska","is"),("Italian-Italiano","it"),("Inuktitut-ᐃᓄᒃᑎᑐᑦ","iu"),("Inuktitut(Latin)-Inuktitut(Latin)","iu-Latn"),("Japanese-日本語","ja"),("Georgian-ქართული","ka"),("Kazakh-ҚазақТілі","kk"),("Khmer-ខ្មែរ","km"),("Kurdish(Northern)-Kurdî(Bakur)","kmr"),("Kannada-ಕನ್ನಡ","kn"),("Korean-한국어","ko"),("Kurdish(Central)-Kurdî(Navîn)","ku"),("Kyrgyz-Kyrgyz","ky"),("Lao-ລາວ","lo"),("Lithuanian-Lietuvių","lt"),("Latvian-Latviešu","lv"),("Chinese(Literary)-中文(文言文)","lzh"),("Malagasy-Malagasy","mg"),("Māori-TeReoMāori","mi"),("Macedonian-Македонски","mk"),("Malayalam-മലയാളം","ml"),("Mongolian(Cyrillic)-Mongolian(Cyrillic)","mn-Cyrl"),("Mongolian(Traditional)-ᠮᠣᠩᠭᠣᠯᠬᠡᠯᠡ","mn-Mong"),("Marathi-मराठी","mr"),("Malay-Melayu","ms"),("Maltese-Malti","mt"),("HmongDaw-HmongDaw","mww"),("Myanmar(Burmese)-မြန်မာ","my"),("Norwegian-NorskBokmål","nb"),("Nepali-नेपाली","ne"),("Dutch-Nederlands","nl"),("Odia-ଓଡ଼ିଆ","or"),("QuerétaroOtomi-Hñähñu","otq"),("Punjabi-ਪੰਜਾਬੀ","pa"),("Polish-Polski","pl"),("Dari-دری","prs"),("Pashto-پښتو","ps"),("Portuguese(Brazil)-Português(Brasil)","pt"),("Portuguese(Portugal)-Português(Portugal)","pt-PT"),("Romanian-Română","ro"),("Russian-Русский","ru"),("Slovak-Slovenčina","sk"),("Slovenian-Slovenščina","sl"),("Samoan-GaganaSāmoa","sm"),("Albanian-Shqip","sq"),("Serbian(Cyrillic)-Српски(ћирилица)","sr-Cyrl"),("Serbian(Latin)-Srpski(latinica)","sr-Latn"),("Swedish-Svenska","sv"),("Swahili-Kiswahili","sw"),("Tamil-தமிழ்","ta"),("Telugu-తెలుగు","te"),("Thai-ไทย","th"),("Tigrinya-ትግር","ti"),("Turkmen-TürkmenDili","tk"),("Klingon(Latin)-Klingon(Latin)","tlh-Latn"),("Klingon(pIqaD)-Klingon(pIqaD)","tlh-Piqd"),("Tongan-LeaFakatonga","to"),("Turkish-Türkçe","tr"),("Tatar-Татар","tt"),("Tahitian-ReoTahiti","ty"),("Uyghur-ئۇيغۇرچە","ug"),("Ukrainian-Українська","uk"),("Urdu-اردو","ur"),("Uzbek(Latin)-Uzbek(Latin)","uz"),("Vietnamese-TiếngViệt","vi"),("YucatecMaya-YucatecMaya","yua"),("Cantonese(Traditional)-粵語(繁體)","yue"),("ChineseSimplified-中文(简体)","zh-Hans"),("ChineseTraditional-繁體中文(繁體)","zh-Hant")
                };
                // get ms store cultures resx tool AND azure supports
                var common_cultures_before = specificCultures.Where(x => MsStoreCultures.Any(y => y == x.Name)).ToList();
                var common_cultures =
                    specificCultures
                    .Where(x =>
                        MsStoreCultures.Any(y => y == x.Name) &&
                        azure_langs.Any(y => x.Name.ToLower().StartsWith(y.Item2.ToLower())) &&
                        omitted.All(y => y.ToLower() != x.Name.ToLower())).ToList();

                //string diffs = string.Join(",", common_cultures_before.Select(x => x.Name).Difference(common_cultures.Select(x => x.Name)).ToList());

                //return common_cultures.Select(x => x.Name.SplitNoEmpty("-").FirstOrDefault()).Distinct().OrderBy(x => x);
                return common_cultures.Select(x => x.Name).OrderBy(x => x);

            }
        }

        static IEnumerable<string> GetAllNeutralResxPaths() {
            return
            [
                Path.Combine(
                    MpPlatformHelpers.GetSolutionDir(),
                    "MonkeyPaste.Avalonia",
                    "Resources",
                    "Localization",
                    "Listings",
                    "ListingStrings.resx"),
                //Path.Combine(
                //    MpPlatformHelpers.GetSolutionDir(),
                //    "MonkeyPaste.Avalonia",
                //    "Resources",
                //    "Localization",
                //    "Enums",
                //    "EnumUiStrings.resx"),
                //Path.Combine(
                //    MpPlatformHelpers.GetSolutionDir(),
                //    "MonkeyPaste.Avalonia",
                //    "Resources",
                //    "Localization",
                //    "UiStrings",
                //    "UiStrings.resx"),
            //.. WorkingPluginNames.Select(x =>
            //        Path.Combine(
            //            GetPluginProjDir(x),
            //            "Resources",
            //            "Resources.resx")),
            ];
        }
        #endregion

        #region Version Props
        static string APP_VERSION => MpPlatformHelpers.GetAppVersion().ToString();
        #endregion

        #region General Props

        static List<string> WorkingPluginNames { get; set; } = []; //=> [
            //"AzureComputerVision",
            //"AzureTextTranslator",
            //"ChatGpt",
            //"CoreAnnotator",
           // "CoreOleHandler",
            //"FileConverter",
            //"GoogleLiteTextTranslator",
            //"QrCoder",
            //"TextToSpeech",
            //"WebSearch",
            //"YoloImageAnnotator",
       // ];
        static IEnumerable<string> WorkingCorePlugins =>
            WorkingPluginNames.Where(x => CorePlugins.Contains(x));

        static string[] AllPluginNames => [
            "AzureComputerVision",
            "AzureTextTranslator",
            "ChatGpt",
            "CoreAnnotator",
            "CoreOleHandler",
            "FileConverter",
            "GoogleLiteTextTranslator",
            "QrCoder",
            "TextToSpeech",
            "WebSearch",
            "YoloImageAnnotator",
        ];


        static bool DO_LOCAL_PACKAGING => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_LOCAL_PACKAGING);

        static bool DO_REMOTE_PACKAGING => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_REMOTE_PACKAGING);
        static bool FORCE_REPLACE_REMOTE_TAG => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.FORCE_REPLACE_REMOTE_TAG);

        static bool DO_LOCAL_VERSIONS => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_LOCAL_VERSIONS);
        static bool DO_REMOTE_VERSIONS => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_REMOTE_VERSIONS);

        static bool DO_LOCAL_CULTURE_INDEX => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_LOCAL_CULTURE_INDEX);
        static bool DO_REMOTE_CULTURE_INDEX => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_REMOTE_CULTURE_INDEX);

        static bool DO_LOCAL_LEDGER => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_LOCAL_LEDGER);
        static bool DO_REMOTE_LEDGER => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_REMOTE_LEDGER);

        static bool LOCAL_MOVE_CORE_TO_DAT => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.LOCAL_MOVE_CORE_TO_DAT);
        static bool REMOTE_MOVE_CORE_TO_DAT => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.REMOTE_MOVE_CORE_TO_DAT);

        static bool GEN_LOCALIZED_MANIFESTS => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.GEN_LOCALIZED_MANIFESTS);

        static bool GEN_ADDON_LISTING => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.GEN_ADDON_LISTING);
        static bool GEN_PROD_LISTING => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.GEN_PROD_LISTING);

        static bool GEN_EMPTY_RESX => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.GEN_EMPTY_RESX);

        static bool TRANS_RESX => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.TRANSLATE_RESX);
        static bool DO_CULTURE_VERIFY => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.VERIFY_CONSISTENT_CULTURES);

        static bool GEN_EDITOR_UISTRS => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.GEN_EDITOR_UISTRS);

        static bool DO_JS_UISTRINGS => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_JS_UISTRINGS);

        static bool MOVE_NUGET_CACHE => LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.MOVE_NUGET_CACHE);

        const string BUILD_CONFIG =
#if DEBUG
            "Debug";
#else
            "Release";
#endif
        const string BUILD_OS =
#if WINDOWS
            "WINDOWS";
#elif MAC
            "MAC";
#elif LINUX
            "LINUX";
#elif ANDROID
            "ANDROID;
#elif IOS
            "IOS";
#else
            "";
#endif
        static string[] ExcludedPublishFileNames = [
            "PresentationFramework.dll",
            "PresentationFramework.xml",
            ];

        static string[] CorePlugins => [
            "CoreAnnotator",
            "CoreOleHandler",
        ];

        static string[] LedgerIgnoredPlugins = [
            "MinimalExample"
        ];


        static string ManifestPrefix = "manifest";
        static string ManifestExt = "json";
        static string ManifestFileName => ManifestPrefix + "." + ManifestExt;
        #endregion

        #region Packaging Props

        const string README_URL_FORMAT = @"https://raw.githubusercontent.com/monkeypaste/{0}/master/README.md";
        const string PROJ_URL_FORMAT = @"https://github.com/monkeypaste/{0}";
        const string ICON_URL_FORMAT = @"https://raw.githubusercontent.com/monkeypaste/{0}/master/icon.png";
        const string PUBLIC_PACKAGE_URL_FORMAT = @"https://github.com/monkeypaste/{0}/releases/download/{1}/{1}.zip";
        static string PRIVATE_ICON_URL_FORMAT = $"{ServerSecrets["httpUrl"]}/dat/{{0}}.png";

        #endregion

        #region Env
        private static IConfiguration _config;
        static IConfigurationSection ServerSecrets { 
            get {
                if(_config == null) {
                    var cb = new ConfigurationBuilder();
                    _config = cb.AddUserSecrets<Ledgerizer>().Build();
                }
                return _config.GetSection("server");
            }
        }
        static IConfigurationSection UbuntuSecrets { 
            get {
                if(_config == null) {
                    var cb = new ConfigurationBuilder();
                    _config = cb.AddUserSecrets<Ledgerizer>().Build();
                }
                return _config.GetSection("ubuntu");
            }
        }
        #endregion

        #endregion


        static void Main(string[] args) {
            SelectTasks();
            Console.WriteLine($"Tasks: {LEDGERIZER_FLAGS}");
            Console.WriteLine("Starting...");

            ProcessAll();

            Console.WriteLine("Done.. press key to finish");
            Console.ReadLine();
        }
        static void ProcessAll() {
            if (GEN_EDITOR_UISTRS) {
                GenEditorUiStrings();
            }
            if (GEN_ADDON_LISTING) {
                GenAddOnListings();
            }
            if (GEN_PROD_LISTING) {
                GenProdListing();
            }
            if (TRANS_RESX) {
                TranslateAllResxWrapper();
            }
            if (GEN_EMPTY_RESX) {
                GenAllEmptyLocalizedResx();
            }
            if (GEN_LOCALIZED_MANIFESTS) {
                LocalizeManifests();
            }
            if (DO_CULTURE_VERIFY) {
                VerifyConsitentCultures();
            }
            if (DO_LOCAL_PACKAGING) {
                PublishLocal();
            }
            if (DO_LOCAL_LEDGER) {
                GenInvLedger(false);
            }
            if (DO_REMOTE_PACKAGING) {
                PublishRemote();
            }
            if (DO_REMOTE_LEDGER) {
                GenInvLedger(true);
            }
            if (DO_LOCAL_VERSIONS) {
                UpdateVersions(false);
            }
            if (DO_REMOTE_VERSIONS) {
                UpdateVersions(true);
            }
            if (DO_LOCAL_CULTURE_INDEX) {
                CreateCultureIndex(false);
            }
            if (DO_REMOTE_CULTURE_INDEX) {
                CreateCultureIndex(true);
            }
            if (LOCAL_MOVE_CORE_TO_DAT) {
                MoveCoresToDat_local();
            }
            if (REMOTE_MOVE_CORE_TO_DAT) {
                MoveCoresToDat_remote();
            }
            if (DO_JS_UISTRINGS) {
                MoveJsUiStrings();
            }
            if(MOVE_NUGET_CACHE) {
                MoveNugetCache();
            }
        }

        #region Menu
        static void SelectTasks() {
            Console.Clear();
            var task_names = Enum.GetNames(typeof(MpLedgerizerFlags));
            foreach(var (task_name,idx) in task_names.WithIndex()) {
                if(idx == 0) {
                    Console.WriteLine($"0. Select working plugin list...");
                    continue;
                }
                Console.WriteLine($"{idx}.{task_name}");
            }
            Console.WriteLine($"Select tasks/flags (# seperated by comma):");
            var tasks_str = Console.ReadLine().SplitNoEmpty(",").Select(x=>x.Trim());

            foreach(var task_str in tasks_str) {
                if(!int.TryParse(task_str,out int task_idx) || task_idx < 0 || task_idx >= task_names.Length) {
                    continue;
                }
                if(task_idx == 0) {
                    LEDGERIZER_FLAGS = MpLedgerizerFlags.None;
                    SelectPlugins();
                    SelectTasks();
                    return;
                }
                LEDGERIZER_FLAGS.AddFlag(task_names[task_idx].ToEnum<MpLedgerizerFlags>());
            }
        }

        static void SelectPlugins() {
            Console.Clear();
            foreach (var (plugin_name, idx) in AllPluginNames.WithIndex()) {
                if (idx == 0) {
                    continue;
                }
                Console.WriteLine($"{idx}.{plugin_name}");
            }
            Console.WriteLine($"Select plugin (# seperated by comma):");
            var plugins_str = Console.ReadLine().SplitNoEmpty(",").Select(x => x.Trim());
            foreach (var plugin_idx_str in plugins_str) {
                if (!int.TryParse(plugin_idx_str, out int plugin_idx) || 
                    plugin_idx < 0 || 
                    plugin_idx >= AllPluginNames.Length ||
                    WorkingPluginNames.Contains(AllPluginNames[plugin_idx])) {
                    continue;
                }
                WorkingPluginNames.Add(AllPluginNames[plugin_idx]);
            }
        }
        #endregion

        #region Listing
        static void DoSwap() {
            string inv_listing_path =
                    Path.Combine(
                        GetListingDir(),
                        $"ListingStrings.resx");
            MpDebug.Assert(inv_listing_path.IsFile(), $"Error listing for inv not found: '{inv_listing_path}'");
            var inv_listing_lookup = MpResxTools.ReadResxFromPath(inv_listing_path);
            var ccl = MpLocalizationHelpers.FindCulturesInDirectory(GetListingDir(), "ListingStrings").Select(x => x.Name);
            var listing_paths = new List<string>() { inv_listing_path };
            listing_paths.AddRange(ccl.Select(x => Path.Combine(
                        GetListingDir(),
                        $"ListingStrings.{x}.resx")));
            foreach (string lp in listing_paths) {
                var source_lplu = MpResxTools.ReadResxFromPath(lp);
                var target_lplu = MpResxTools.ReadResxFromPath(lp);
                for (int i = 1; i <= 6; i++) {
                    string source_key = $"ssSrc{i}";
                    string target_key = i == 4 ? "ssSrc1" : $"ssSrc{i + 1}";
                    var source_kvp = source_lplu[source_key];
                    target_lplu[target_key] = source_kvp;
                }
                MpResxTools.WriteResxToPath(lp, target_lplu);
            }
        }
        static void GenProdListing() {
            MpConsole.WriteLine($"Generating Product Listings...STARTED", true);
            // (short) translation prefix fields:
            // TeaserCaption
            // ss1*

            // (long) translation prefix fields:
            // StoreDescription

            #region Setup

            string raw_csv =
@"Field,ID,Type (Type),default
Description,2,Text,
ReleaseNotes,3,Text,
Title,4,Text,
ShortTitle,5,Text,
SortTitle,6,Text,
VoiceTitle,7,Text,
ShortDescription,8,Text,
DevStudio,9,Text,
CopyrightTrademarkInformation,12,Text,
AdditionalLicenseTerms,13,Text,
DesktopScreenshot1,100,Relative path (or URL to file in Partner Center),
DesktopScreenshot2,101,Relative path (or URL to file in Partner Center),
DesktopScreenshot3,102,Relative path (or URL to file in Partner Center),
DesktopScreenshot4,103,Relative path (or URL to file in Partner Center),
DesktopScreenshot5,104,Relative path (or URL to file in Partner Center),
DesktopScreenshot6,105,Relative path (or URL to file in Partner Center),
DesktopScreenshot7,106,Relative path (or URL to file in Partner Center),
DesktopScreenshot8,107,Relative path (or URL to file in Partner Center),
DesktopScreenshot9,108,Relative path (or URL to file in Partner Center),
DesktopScreenshot10,109,Relative path (or URL to file in Partner Center),
DesktopScreenshot11,110,Relative path (or URL to file in Partner Center),
DesktopScreenshot12,111,Relative path (or URL to file in Partner Center),
DesktopScreenshot13,112,Relative path (or URL to file in Partner Center),
DesktopScreenshot14,113,Relative path (or URL to file in Partner Center),
DesktopScreenshot15,114,Relative path (or URL to file in Partner Center),
DesktopScreenshot16,115,Relative path (or URL to file in Partner Center),
DesktopScreenshot17,116,Relative path (or URL to file in Partner Center),
DesktopScreenshot18,117,Relative path (or URL to file in Partner Center),
DesktopScreenshot19,118,Relative path (or URL to file in Partner Center),
DesktopScreenshot20,119,Relative path (or URL to file in Partner Center),
DesktopScreenshot21,120,Relative path (or URL to file in Partner Center),
DesktopScreenshot22,121,Relative path (or URL to file in Partner Center),
DesktopScreenshot23,122,Relative path (or URL to file in Partner Center),
DesktopScreenshot24,123,Relative path (or URL to file in Partner Center),
DesktopScreenshot25,124,Relative path (or URL to file in Partner Center),
DesktopScreenshot26,125,Relative path (or URL to file in Partner Center),
DesktopScreenshot27,126,Relative path (or URL to file in Partner Center),
DesktopScreenshot28,127,Relative path (or URL to file in Partner Center),
DesktopScreenshot29,128,Relative path (or URL to file in Partner Center),
DesktopScreenshot30,129,Relative path (or URL to file in Partner Center),
DesktopScreenshotCaption1,150,Text,
DesktopScreenshotCaption2,151,Text,
DesktopScreenshotCaption3,152,Text,
DesktopScreenshotCaption4,153,Text,
DesktopScreenshotCaption5,154,Text,
DesktopScreenshotCaption6,155,Text,
DesktopScreenshotCaption7,156,Text,
DesktopScreenshotCaption8,157,Text,
DesktopScreenshotCaption9,158,Text,
DesktopScreenshotCaption10,159,Text,
DesktopScreenshotCaption11,160,Text,
DesktopScreenshotCaption12,161,Text,
DesktopScreenshotCaption13,162,Text,
DesktopScreenshotCaption14,163,Text,
DesktopScreenshotCaption15,164,Text,
DesktopScreenshotCaption16,165,Text,
DesktopScreenshotCaption17,166,Text,
DesktopScreenshotCaption18,167,Text,
DesktopScreenshotCaption19,168,Text,
DesktopScreenshotCaption20,169,Text,
DesktopScreenshotCaption21,170,Text,
DesktopScreenshotCaption22,171,Text,
DesktopScreenshotCaption23,172,Text,
DesktopScreenshotCaption24,173,Text,
DesktopScreenshotCaption25,174,Text,
DesktopScreenshotCaption26,175,Text,
DesktopScreenshotCaption27,176,Text,
DesktopScreenshotCaption28,177,Text,
DesktopScreenshotCaption29,178,Text,
DesktopScreenshotCaption30,179,Text,
MobileScreenshot1,200,Relative path (or URL to file in Partner Center),
MobileScreenshot2,201,Relative path (or URL to file in Partner Center),
MobileScreenshot3,202,Relative path (or URL to file in Partner Center),
MobileScreenshot4,203,Relative path (or URL to file in Partner Center),
MobileScreenshot5,204,Relative path (or URL to file in Partner Center),
MobileScreenshot6,205,Relative path (or URL to file in Partner Center),
MobileScreenshot7,206,Relative path (or URL to file in Partner Center),
MobileScreenshot8,207,Relative path (or URL to file in Partner Center),
MobileScreenshot9,208,Relative path (or URL to file in Partner Center),
MobileScreenshot10,209,Relative path (or URL to file in Partner Center),
MobileScreenshot11,210,Relative path (or URL to file in Partner Center),
MobileScreenshot12,211,Relative path (or URL to file in Partner Center),
MobileScreenshot13,212,Relative path (or URL to file in Partner Center),
MobileScreenshot14,213,Relative path (or URL to file in Partner Center),
MobileScreenshot15,214,Relative path (or URL to file in Partner Center),
MobileScreenshot16,215,Relative path (or URL to file in Partner Center),
MobileScreenshot17,216,Relative path (or URL to file in Partner Center),
MobileScreenshot18,217,Relative path (or URL to file in Partner Center),
MobileScreenshot19,218,Relative path (or URL to file in Partner Center),
MobileScreenshot20,219,Relative path (or URL to file in Partner Center),
MobileScreenshot21,220,Relative path (or URL to file in Partner Center),
MobileScreenshot22,221,Relative path (or URL to file in Partner Center),
MobileScreenshot23,222,Relative path (or URL to file in Partner Center),
MobileScreenshot24,223,Relative path (or URL to file in Partner Center),
MobileScreenshot25,224,Relative path (or URL to file in Partner Center),
MobileScreenshot26,225,Relative path (or URL to file in Partner Center),
MobileScreenshot27,226,Relative path (or URL to file in Partner Center),
MobileScreenshot28,227,Relative path (or URL to file in Partner Center),
MobileScreenshot29,228,Relative path (or URL to file in Partner Center),
MobileScreenshot30,229,Relative path (or URL to file in Partner Center),
MobileScreenshotCaption1,250,Text,
MobileScreenshotCaption2,251,Text,
MobileScreenshotCaption3,252,Text,
MobileScreenshotCaption4,253,Text,
MobileScreenshotCaption5,254,Text,
MobileScreenshotCaption6,255,Text,
MobileScreenshotCaption7,256,Text,
MobileScreenshotCaption8,257,Text,
MobileScreenshotCaption9,258,Text,
MobileScreenshotCaption10,259,Text,
MobileScreenshotCaption11,260,Text,
MobileScreenshotCaption12,261,Text,
MobileScreenshotCaption13,262,Text,
MobileScreenshotCaption14,263,Text,
MobileScreenshotCaption15,264,Text,
MobileScreenshotCaption16,265,Text,
MobileScreenshotCaption17,266,Text,
MobileScreenshotCaption18,267,Text,
MobileScreenshotCaption19,268,Text,
MobileScreenshotCaption20,269,Text,
MobileScreenshotCaption21,270,Text,
MobileScreenshotCaption22,271,Text,
MobileScreenshotCaption23,272,Text,
MobileScreenshotCaption24,273,Text,
MobileScreenshotCaption25,274,Text,
MobileScreenshotCaption26,275,Text,
MobileScreenshotCaption27,276,Text,
MobileScreenshotCaption28,277,Text,
MobileScreenshotCaption29,278,Text,
MobileScreenshotCaption30,279,Text,
XboxScreenshot1,300,Relative path (or URL to file in Partner Center),
XboxScreenshot2,301,Relative path (or URL to file in Partner Center),
XboxScreenshot3,302,Relative path (or URL to file in Partner Center),
XboxScreenshot4,303,Relative path (or URL to file in Partner Center),
XboxScreenshot5,304,Relative path (or URL to file in Partner Center),
XboxScreenshot6,305,Relative path (or URL to file in Partner Center),
XboxScreenshot7,306,Relative path (or URL to file in Partner Center),
XboxScreenshot8,307,Relative path (or URL to file in Partner Center),
XboxScreenshot9,308,Relative path (or URL to file in Partner Center),
XboxScreenshot10,309,Relative path (or URL to file in Partner Center),
XboxScreenshot11,310,Relative path (or URL to file in Partner Center),
XboxScreenshot12,311,Relative path (or URL to file in Partner Center),
XboxScreenshot13,312,Relative path (or URL to file in Partner Center),
XboxScreenshot14,313,Relative path (or URL to file in Partner Center),
XboxScreenshot15,314,Relative path (or URL to file in Partner Center),
XboxScreenshot16,315,Relative path (or URL to file in Partner Center),
XboxScreenshot17,316,Relative path (or URL to file in Partner Center),
XboxScreenshot18,317,Relative path (or URL to file in Partner Center),
XboxScreenshot19,318,Relative path (or URL to file in Partner Center),
XboxScreenshot20,319,Relative path (or URL to file in Partner Center),
XboxScreenshot21,320,Relative path (or URL to file in Partner Center),
XboxScreenshot22,321,Relative path (or URL to file in Partner Center),
XboxScreenshot23,322,Relative path (or URL to file in Partner Center),
XboxScreenshot24,323,Relative path (or URL to file in Partner Center),
XboxScreenshot25,324,Relative path (or URL to file in Partner Center),
XboxScreenshot26,325,Relative path (or URL to file in Partner Center),
XboxScreenshot27,326,Relative path (or URL to file in Partner Center),
XboxScreenshot28,327,Relative path (or URL to file in Partner Center),
XboxScreenshot29,328,Relative path (or URL to file in Partner Center),
XboxScreenshot30,329,Relative path (or URL to file in Partner Center),
XboxScreenshotCaption1,350,Text,
XboxScreenshotCaption2,351,Text,
XboxScreenshotCaption3,352,Text,
XboxScreenshotCaption4,353,Text,
XboxScreenshotCaption5,354,Text,
XboxScreenshotCaption6,355,Text,
XboxScreenshotCaption7,356,Text,
XboxScreenshotCaption8,357,Text,
XboxScreenshotCaption9,358,Text,
XboxScreenshotCaption10,359,Text,
XboxScreenshotCaption11,360,Text,
XboxScreenshotCaption12,361,Text,
XboxScreenshotCaption13,362,Text,
XboxScreenshotCaption14,363,Text,
XboxScreenshotCaption15,364,Text,
XboxScreenshotCaption16,365,Text,
XboxScreenshotCaption17,366,Text,
XboxScreenshotCaption18,367,Text,
XboxScreenshotCaption19,368,Text,
XboxScreenshotCaption20,369,Text,
XboxScreenshotCaption21,370,Text,
XboxScreenshotCaption22,371,Text,
XboxScreenshotCaption23,372,Text,
XboxScreenshotCaption24,373,Text,
XboxScreenshotCaption25,374,Text,
XboxScreenshotCaption26,375,Text,
XboxScreenshotCaption27,376,Text,
XboxScreenshotCaption28,377,Text,
XboxScreenshotCaption29,378,Text,
XboxScreenshotCaption30,379,Text,
HolographicScreenshot1,400,Relative path (or URL to file in Partner Center),
HolographicScreenshot2,401,Relative path (or URL to file in Partner Center),
HolographicScreenshot3,402,Relative path (or URL to file in Partner Center),
HolographicScreenshot4,403,Relative path (or URL to file in Partner Center),
HolographicScreenshot5,404,Relative path (or URL to file in Partner Center),
HolographicScreenshot6,405,Relative path (or URL to file in Partner Center),
HolographicScreenshot7,406,Relative path (or URL to file in Partner Center),
HolographicScreenshot8,407,Relative path (or URL to file in Partner Center),
HolographicScreenshot9,408,Relative path (or URL to file in Partner Center),
HolographicScreenshot10,409,Relative path (or URL to file in Partner Center),
HolographicScreenshot11,410,Relative path (or URL to file in Partner Center),
HolographicScreenshot12,411,Relative path (or URL to file in Partner Center),
HolographicScreenshot13,412,Relative path (or URL to file in Partner Center),
HolographicScreenshot14,413,Relative path (or URL to file in Partner Center),
HolographicScreenshot15,414,Relative path (or URL to file in Partner Center),
HolographicScreenshot16,415,Relative path (or URL to file in Partner Center),
HolographicScreenshot17,416,Relative path (or URL to file in Partner Center),
HolographicScreenshot18,417,Relative path (or URL to file in Partner Center),
HolographicScreenshot19,418,Relative path (or URL to file in Partner Center),
HolographicScreenshot20,419,Relative path (or URL to file in Partner Center),
HolographicScreenshot21,420,Relative path (or URL to file in Partner Center),
HolographicScreenshot22,421,Relative path (or URL to file in Partner Center),
HolographicScreenshot23,422,Relative path (or URL to file in Partner Center),
HolographicScreenshot24,423,Relative path (or URL to file in Partner Center),
HolographicScreenshot25,424,Relative path (or URL to file in Partner Center),
HolographicScreenshot26,425,Relative path (or URL to file in Partner Center),
HolographicScreenshot27,426,Relative path (or URL to file in Partner Center),
HolographicScreenshot28,427,Relative path (or URL to file in Partner Center),
HolographicScreenshot29,428,Relative path (or URL to file in Partner Center),
HolographicScreenshot30,429,Relative path (or URL to file in Partner Center),
HolographicScreenshotCaption1,450,Text,
HolographicScreenshotCaption2,451,Text,
HolographicScreenshotCaption3,452,Text,
HolographicScreenshotCaption4,453,Text,
HolographicScreenshotCaption5,454,Text,
HolographicScreenshotCaption6,455,Text,
HolographicScreenshotCaption7,456,Text,
HolographicScreenshotCaption8,457,Text,
HolographicScreenshotCaption9,458,Text,
HolographicScreenshotCaption10,459,Text,
HolographicScreenshotCaption11,460,Text,
HolographicScreenshotCaption12,461,Text,
HolographicScreenshotCaption13,462,Text,
HolographicScreenshotCaption14,463,Text,
HolographicScreenshotCaption15,464,Text,
HolographicScreenshotCaption16,465,Text,
HolographicScreenshotCaption17,466,Text,
HolographicScreenshotCaption18,467,Text,
HolographicScreenshotCaption19,468,Text,
HolographicScreenshotCaption20,469,Text,
HolographicScreenshotCaption21,470,Text,
HolographicScreenshotCaption22,471,Text,
HolographicScreenshotCaption23,472,Text,
HolographicScreenshotCaption24,473,Text,
HolographicScreenshotCaption25,474,Text,
HolographicScreenshotCaption26,475,Text,
HolographicScreenshotCaption27,476,Text,
HolographicScreenshotCaption28,477,Text,
HolographicScreenshotCaption29,478,Text,
HolographicScreenshotCaption30,479,Text,
SurfaceHubScreenshot11,510,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot12,511,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot13,512,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot14,513,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot15,514,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot16,515,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot17,516,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot18,517,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot19,518,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot20,519,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot21,520,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot22,521,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot23,522,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot24,523,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot25,524,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot26,525,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot27,526,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot28,527,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot29,528,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshot30,529,Relative path (or URL to file in Partner Center),
SurfaceHubScreenshotCaption11,560,Text,
SurfaceHubScreenshotCaption12,561,Text,
SurfaceHubScreenshotCaption13,562,Text,
SurfaceHubScreenshotCaption14,563,Text,
SurfaceHubScreenshotCaption15,564,Text,
SurfaceHubScreenshotCaption16,565,Text,
SurfaceHubScreenshotCaption17,566,Text,
SurfaceHubScreenshotCaption18,567,Text,
SurfaceHubScreenshotCaption19,568,Text,
SurfaceHubScreenshotCaption20,569,Text,
SurfaceHubScreenshotCaption21,570,Text,
SurfaceHubScreenshotCaption22,571,Text,
SurfaceHubScreenshotCaption23,572,Text,
SurfaceHubScreenshotCaption24,573,Text,
SurfaceHubScreenshotCaption25,574,Text,
SurfaceHubScreenshotCaption26,575,Text,
SurfaceHubScreenshotCaption27,576,Text,
SurfaceHubScreenshotCaption28,577,Text,
SurfaceHubScreenshotCaption29,578,Text,
SurfaceHubScreenshotCaption30,579,Text,
StoreLogo720x1080,600,Relative path (or URL to file in Partner Center),
StoreLogo1080x1080,601,Relative path (or URL to file in Partner Center),
StoreLogo300x300,602,Relative path (or URL to file in Partner Center),
OverrideLogosForWin10,603,True/False,
StoreLogoOverride150x150,604,Relative path (or URL to file in Partner Center),
StoreLogoOverride71x71,605,Relative path (or URL to file in Partner Center),
PromoImage1920x1080,606,Relative path (or URL to file in Partner Center),
PromoImage2400x1200,607,Relative path (or URL to file in Partner Center),
XboxBrandedKeyArt584x800,608,Relative path (or URL to file in Partner Center),
XboxTitledHero1920x1080,609,Relative path (or URL to file in Partner Center),
XboxFeaturedPromo1080x1080,610,Relative path (or URL to file in Partner Center),
OptionalPromo358x358,611,Relative path (or URL to file in Partner Center),
OptionalPromo1000x800,612,Relative path (or URL to file in Partner Center),
OptionalPromo414x180,613,Relative path (or URL to file in Partner Center),
Feature1,700,Text,
Feature2,701,Text,
Feature3,702,Text,
Feature4,703,Text,
Feature5,704,Text,
Feature6,705,Text,
Feature7,706,Text,
Feature8,707,Text,
Feature9,708,Text,
Feature10,709,Text,
Feature11,710,Text,
Feature12,711,Text,
Feature13,712,Text,
Feature14,713,Text,
Feature15,714,Text,
Feature16,715,Text,
Feature17,716,Text,
Feature18,717,Text,
Feature19,718,Text,
Feature20,719,Text,
MinimumHardwareReq1,800,Text,
MinimumHardwareReq2,801,Text,
MinimumHardwareReq3,802,Text,
MinimumHardwareReq4,803,Text,
MinimumHardwareReq5,804,Text,
MinimumHardwareReq6,805,Text,
MinimumHardwareReq7,806,Text,
MinimumHardwareReq8,807,Text,
MinimumHardwareReq9,808,Text,
MinimumHardwareReq10,809,Text,
MinimumHardwareReq11,810,Text,
RecommendedHardwareReq1,850,Text,
RecommendedHardwareReq2,851,Text,
RecommendedHardwareReq3,852,Text,
RecommendedHardwareReq4,853,Text,
RecommendedHardwareReq5,854,Text,
RecommendedHardwareReq6,855,Text,
RecommendedHardwareReq7,856,Text,
RecommendedHardwareReq8,857,Text,
RecommendedHardwareReq9,858,Text,
RecommendedHardwareReq10,859,Text,
RecommendedHardwareReq11,860,Text,
SearchTerm1,900,Text,
SearchTerm2,901,Text,
SearchTerm3,902,Text,
SearchTerm4,903,Text,
SearchTerm5,904,Text,
SearchTerm6,905,Text,
SearchTerm7,906,Text,
TrailerToPlayAtTopOfListing,999,Relative path (or URL to file in Partner Center),
Trailer1,1000,Relative path (or URL to file in Partner Center),
Trailer2,1001,Relative path (or URL to file in Partner Center),
Trailer3,1002,Relative path (or URL to file in Partner Center),
Trailer4,1003,Relative path (or URL to file in Partner Center),
Trailer5,1004,Relative path (or URL to file in Partner Center),
Trailer6,1005,Relative path (or URL to file in Partner Center),
Trailer7,1006,Relative path (or URL to file in Partner Center),
Trailer8,1007,Relative path (or URL to file in Partner Center),
Trailer9,1008,Relative path (or URL to file in Partner Center),
Trailer10,1009,Relative path (or URL to file in Partner Center),
Trailer11,1010,Relative path (or URL to file in Partner Center),
Trailer12,1011,Relative path (or URL to file in Partner Center),
Trailer13,1012,Relative path (or URL to file in Partner Center),
Trailer14,1013,Relative path (or URL to file in Partner Center),
Trailer15,1014,Relative path (or URL to file in Partner Center),
TrailerTitle1,1020,Text,
TrailerTitle2,1021,Text,
TrailerTitle3,1022,Text,
TrailerTitle4,1023,Text,
TrailerTitle5,1024,Text,
TrailerTitle6,1025,Text,
TrailerTitle7,1026,Text,
TrailerTitle8,1027,Text,
TrailerTitle9,1028,Text,
TrailerTitle10,1029,Text,
TrailerTitle11,1030,Text,
TrailerTitle12,1031,Text,
TrailerTitle13,1032,Text,
TrailerTitle14,1033,Text,
TrailerTitle15,1034,Text,
TrailerThumbnail1,1040,Relative path (or URL to file in Partner Center),
TrailerThumbnail2,1041,Relative path (or URL to file in Partner Center),
TrailerThumbnail3,1042,Relative path (or URL to file in Partner Center),
TrailerThumbnail4,1043,Relative path (or URL to file in Partner Center),
TrailerThumbnail5,1044,Relative path (or URL to file in Partner Center),
TrailerThumbnail6,1045,Relative path (or URL to file in Partner Center),
TrailerThumbnail7,1046,Relative path (or URL to file in Partner Center),
TrailerThumbnail8,1047,Relative path (or URL to file in Partner Center),
TrailerThumbnail9,1048,Relative path (or URL to file in Partner Center),
TrailerThumbnail10,1049,Relative path (or URL to file in Partner Center),
TrailerThumbnail11,1050,Relative path (or URL to file in Partner Center),
TrailerThumbnail12,1051,Relative path (or URL to file in Partner Center),
TrailerThumbnail13,1052,Relative path (or URL to file in Partner Center),
TrailerThumbnail14,1053,Relative path (or URL to file in Partner Center),
TrailerThumbnail15,1054,Relative path (or URL to file in Partner Center),
";

            (int, string)[] field_kvpl = new (int, string)[]{
                (1,"StoreDescription"),
                (2,"WhatsNew"),
                (3,"StoreTitle"),
                (4,"StoreShortName"),
                (5,"StoreSortName"),
                (6,"StoreSortName"),
                (7,"StoreVoiceName"),
                //(8,"StoreDevStudio"),
                (11,"ssSrc4"),
                (12,"ssSrc6"),
                (13,"ssSrc1"),
                (14,"ssSrc2"),
                (15,"ssSrc3"),
                (16,"ssSrc5"),
                (17,"ssSrc7"),
                (41,"ssCaption4"),
                (42,"ssCaption6"),
                (43,"ssCaption1"),
                (44,"ssCaption2"),
                (45,"ssCaption3"),
                (46,"ssCaption5"),
                (47,"ssCaption7"),
                (291,"logo720x1080"),
                (292,"logo1080x1080"),
                (293,"logo300x300"),
                (294,"OverrideLogosForWin10"),
                (295,"logo150x150"),
                (296,"logo71x71"),
                (297,"logo1920x1080"),
                (305,"StoreFeature1"),
                (306,"StoreFeature2"),
                (307,"StoreFeature3"),
                (308,"StoreFeature4"),
                (309,"StoreFeature5"),
                (310,"StoreFeature6"),
                (311,"StoreFeature7"),
                (312,"StoreFeature8"),
                (313,"StoreFeature9"),
                (314,"StoreFeature10"),
                (315,"StoreFeature11"),
                (316,"StoreFeature12"),
                (317,"StoreFeature13"),
                (318,"StoreFeature14"),
                (319,"StoreFeature15"),
                (320,"StoreFeature16"),
                (321,"StoreFeature17"),
                (322,"StoreFeature18"),
                (323,"StoreFeature19"),
                (324,"StoreFeature20"),
                (347,"SearchTerm1"),
                (348,"SearchTerm2"),
                (349,"SearchTerm3"),
                (350,"SearchTerm4"),
                (351,"SearchTerm5"),
                (352,"SearchTerm6"),
                (353,"SearchTerm7"),
                (354,"TrailerToPlayAtTopOfListing"),
                (355,"TeaserSrc"),
                (370,"TeaserCaption"),
                (385,"logo1920x1080"),
            };
            string[] long_trans_warning_keys = [
                "StoreDescription"
            ];
            string[] short_trans_warning_keys = [
                "TeaserCaption",
                "ssCaption1",
                "ssCaption2",
                "ssCaption3",
                "ssCaption4",
                "ssCaption5",
                "ssCaption6",
                "ssCaption7",
            ];

            string[] rel_path_keys = [
                "ssSrc1",
                "ssSrc2",
                "ssSrc3",
                "ssSrc4",
                "ssSrc5",
                "ssSrc6",
                "ssSrc7",
                "logo720x1080",
                "logo1080x1080",
                "logo300x300",
                "logo150x150",
                "logo71x71",
                "logo1920x1080",
                "TrailerToPlayAtTopOfListing",
                "TeaserSrc"
            ];

            #endregion

            var csv = raw_csv.ToTableFromCsv();

            string prod_listing_dir_name = $"listing_product_{APP_VERSION}";
            string prod_listing_dir_path =
                Path.Combine(
                    GetListingDir(),
                    "imports",
                    prod_listing_dir_name);
            MpFileIo.DeleteDirectory(prod_listing_dir_path);
            MpFileIo.CreateDirectory(prod_listing_dir_path);

            string img_dir_path =
                Path.Combine(
                    GetListingDir(),
                    "files");
            MpFileIo.CopyContents(img_dir_path, prod_listing_dir_path, true, true);
            string inv_listing_path =
                    Path.Combine(
                        GetListingDir(),
                        $"ListingStrings.resx");
            MpDebug.Assert(inv_listing_path.IsFile(), $"Error listing for inv not found: '{inv_listing_path}'");
            var inv_listing_lookup = MpResxTools.ReadResxFromPath(inv_listing_path);
            var ccl = GetListingCultures();
            int max_feature_len = 200;
            foreach (string cc in ccl) {
                string local_listing_path =
                    Path.Combine(
                        GetListingDir(),
                        $"ListingStrings.{cc}.resx");
                MpDebug.Assert(local_listing_path.IsFile(), $"Error listing for '{cc}' not found: '{local_listing_path}'");
                var listing_lookup = MpResxTools.ReadResxFromPath(local_listing_path);

                for (int r = 0; r < csv.Count; r++) {
                    string new_cell_val = string.Empty;
                    if (r == 0) {
                        new_cell_val = cc;
                    } else if (field_kvpl.FirstOrDefault(x => x.Item1 == r) is { } field_kvp &&
                        !field_kvp.IsDefault()) {
                        string row_key = field_kvp.Item2;//
                        if (inv_listing_lookup.TryGetValue(row_key, out var inv_listing_kvp) &&
                            inv_listing_kvp.comment == "@Invariant") {
                            // when invariant, pass neutral value
                            new_cell_val = inv_listing_kvp.value;
                        } else if (listing_lookup.TryGetValue(row_key, out var listing_kvp)) {
                            // use translated text
                            new_cell_val = listing_kvp.value;
                        } else {
                            // huh?!?
                            throw new KeyNotFoundException(row_key);
                        }

                        if (cc == "en" || cc.StartsWith("en-")) {
                            // do nothing 
                        } else { 
                            // add translation warning prefix
                            if (long_trans_warning_keys.Any(x => x == row_key)) {
                                new_cell_val = listing_lookup["TranslationWarning"] + Environment.NewLine + new_cell_val;
                            } else if (short_trans_warning_keys.Any(x => x == row_key)) {
                                new_cell_val = listing_lookup["TranslationShortWarning"] + Environment.NewLine + new_cell_val;
                            }
                        }

                        if (rel_path_keys.Any(x => x == row_key)) {
                            // format relative uri
                            new_cell_val = string.Format(new_cell_val, prod_listing_dir_name);
                        }
                        if (row_key.StartsWith("StoreFeature") && new_cell_val.Length > max_feature_len) {
                            MpDebug.Break();
                        }
                    }
                    csv[r].Add(new_cell_val.ToCsvCell());
                }

            }

            string prod_listing_path =
                 Path.Combine(
                     prod_listing_dir_path,
                     $"{prod_listing_dir_name}.csv");
            string output_csv = csv.ToCsv();
            MpFileIo.WriteTextToFile(prod_listing_path, output_csv);
            MpConsole.WriteLine(prod_listing_path);

            MpConsole.WriteLine($"Generating Product Listings...DONE", false, true);
        }
        static void GenAddOnListings() {
            MpConsole.WriteLine($"Generating AddOn Listings...STARTED", true);
            GenAddOnListing("Basic", "Monthly");
            GenAddOnListing("Basic", "Yearly");
            GenAddOnListing("Unlimited", "Monthly");
            GenAddOnListing("Unlimited", "Yearly");
            MpConsole.WriteLine($"Generating AddOn Listings...DONE", false, true);
        }
        static void GenAddOnListing(string plan_name, string cycle_type) {
            // outputs path to listing file

            //GenEmptyLocalizedListings();
            string listing_dir_name = $"listing_{plan_name}_{cycle_type}_{APP_VERSION}";

            var ccl = GetListingCultures();
            var line1 = "Field,ID,Type (Type),default".Split(",").ToList();
            line1.AddRange(ccl.Select(x => x));

            var line2 = "Description,2,Text,".Split(",").ToList();
            string line2_key = $"{plan_name}Description";

            var line3 = "Title,4,Text,".Split(",").ToList();
            string line3_key = $"{plan_name}{cycle_type}Title";

            var line4 = "StoreLogo300x300,602,Relative path (or URL to file in Partner Center),".Split(",").ToList();
            string logo_file_name = $"{plan_name}Logo.png";
            string line4_val = $"{listing_dir_name}/{logo_file_name}";


            foreach (string cc in ccl) {
                string local_listing_path =
                    Path.Combine(
                        GetListingDir(),
                        $"ListingStrings.{cc}.resx");
                MpDebug.Assert(local_listing_path.IsFile(), $"Error listing for '{cc}' not found: '{local_listing_path}'");
                var listing_lookup = MpResxTools.ReadResxFromPath(local_listing_path);

                line2.Add(listing_lookup[line2_key].value.ToCsvCell());
                line3.Add(listing_lookup[line3_key].value.ToCsvCell());
                line4.Add(line4_val.ToCsvCell());
            }
            MpDebug.Assert(line1.Count == line2.Count, "Line 2 mismatch");
            MpDebug.Assert(line1.Count == line3.Count, "Line 3 mismatch");
            MpDebug.Assert(line1.Count == line4.Count, "Line 4 mismatch");


            string output_dir =
                Path.Combine(
                    GetListingDir(),
                    "imports",
                    listing_dir_name);
            MpFileIo.DeleteDirectory(output_dir);
            MpFileIo.CreateDirectory(output_dir);

            string logo_src_path = Path.Combine(
                    GetListingDir(),
                    "files",
                    logo_file_name);
            string logo_dst_path = Path.Combine(
                output_dir,
                logo_file_name);
            MpFileIo.CopyFileOrDirectory(logo_src_path, logo_dst_path);

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", line1));
            sb.AppendLine(string.Join(",", line2));
            sb.AppendLine(string.Join(",", line3));
            sb.AppendLine(string.Join(",", line4));
            string output_path =
                Path.Combine(
                    output_dir,
                    $"{listing_dir_name}.csv");
            MpFileIo.WriteTextToFile(output_path, sb.ToString(), overwrite: true);
            MpConsole.WriteLine(output_path);
        }
        static string GetListingDir() {

            string listing_dir = Path.Combine(
                MpPlatformHelpers.GetSolutionDir(),
                "MonkeyPaste.Avalonia",
                "Resources",
                "Localization",
                "Listings");
            return listing_dir;
        }
        static IEnumerable<string> GetListingCultures() {
            var all_ccl =
                MpLocalizationHelpers.FindCulturesInDirectory(GetListingDir(), "ListingStrings")
                .Select(x => x.Name);
            var ccl =
                all_ccl
                .Where(x => WorkingCultures.Contains(x));
            if (ccl.Difference(WorkingCultures) is { } culture_diffs && culture_diffs.Any()) {
                MpConsole.WriteLine($"Cultures found/working culture mismatch detected.");

                MpConsole.WriteLine($"Working cultures not to be generated: {string.Join(",", WorkingCultures.Where(x => !ccl.Contains(x)))}");
                MpConsole.WriteLine($"Available cultures not to be generated: {string.Join(",", all_ccl.Where(x => !ccl.Contains(x)))}");
            }
            return ccl;
        }
        #endregion

        #region Move Core 
        static void MoveCorePluginToServer(string core_plugin_name, bool is_debug) {
            string root_pack_dir = GetPackagesDir(is_debug);
            string core_plugin_zip_path = Path.Combine(root_pack_dir, $"{core_plugin_name}.zip");
            if (!core_plugin_zip_path.IsFile()) {
                MpConsole.WriteLine($"Error! No package found for '{core_plugin_name}' at '{core_plugin_zip_path}'");
                return;
            }
            if (ReadPluginManifestFromProjDir(core_plugin_name) is not { } core_mf) {
                MpConsole.WriteLine($"Error could not find core manifest for '{core_plugin_name}'");
                return;
            }

            try {
                // transfer icon
                string plugin_icon_path =
                    Path.Combine(
                        GetPluginProjDir(core_plugin_name),
                        "icon.png");
                string url = ServerSecrets["ftpDatUrlPrefix"].ToString();
                string username = ServerSecrets["ftpUserName"].ToString();
                string password = ServerSecrets["ftpPassword"].ToString();
                var icon_result = MpFtpTools.FtpFileUpload(
                    ftpUrl: $"{url}/{core_plugin_name}.png",
                    userName: username,
                    password: password,
                    filePath: plugin_icon_path);
                MpConsole.WriteLine($"{core_plugin_name} icon result: {(icon_result == System.Net.FtpStatusCode.ClosingData).ToTestResultLabel()}");

                // transfer package
                var zip_result = MpFtpTools.FtpFileUpload(
                    ftpUrl: $"{url}/{core_mf.guid}/v{core_mf.version}.zip",
                    userName: username,
                    password: password,
                    filePath: core_plugin_zip_path);

                MpConsole.WriteLine($"{core_plugin_name} {core_mf.version} zip result: {(zip_result == System.Net.FtpStatusCode.ClosingData).ToTestResultLabel()}");

                // duplicate as latest
                var latest_result = MpFtpTools.FtpFileUpload(
                    ftpUrl: $"{url}/{core_mf.guid}/latest.zip",
                    userName: username,
                    password: password,
                    filePath: core_plugin_zip_path);
                MpConsole.WriteLine($"{core_plugin_name} latest zip result: {(latest_result == System.Net.FtpStatusCode.ClosingData).ToTestResultLabel()}");
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error moving remote dat {core_plugin_name}.", ex);
            }
        }
        static void MoveCoresToDat_remote() {
            MpConsole.WriteLine($"[REMOTE] Moving core plugins to dat STARTED", true);


            if (LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DEBUG)) {
                foreach (string core_plugin_name in WorkingCorePlugins) {
                    MoveCorePluginToServer(core_plugin_name, true);
                }
            }
            if (LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.RELEASE)) {
                foreach (string core_plugin_name in WorkingCorePlugins) {
                    MoveCorePluginToServer(core_plugin_name, false);
                }
            }

            MpConsole.WriteLine($"[REMOTE] Moving core plugins to dat DONE", false, true);
        }
        static void MoveCoresToDat_local() {
            MpConsole.WriteLine($"[LOCAL] Moving core plugins to dat STARTED", true);
            void DoLocalMove(bool is_debug) {
                string root_pack_dir = GetPackagesDir(is_debug);
                string proj_dat_dir =
                    Path.Combine(
                        MpPlatformHelpers.GetSolutionDir(),
                        "MonkeyPaste.Avalonia",
                        "Assets",
                        "dat");
                if (!proj_dat_dir.IsDirectory()) {
                    MpFileIo.CreateDirectory(proj_dat_dir);
                }

                foreach (string core_plugin_name in WorkingCorePlugins) {
                    string core_plugin_zip_path = Path.Combine(root_pack_dir, $"{core_plugin_name}.zip");
                    if (!core_plugin_zip_path.IsFile()) {
                        MpConsole.WriteLine($"Error! No package found for '{core_plugin_name}' at '{core_plugin_zip_path}'");
                        continue;
                    }
                    if (ReadPluginManifestFromProjDir(core_plugin_name) is not { } core_mf) {
                        MpConsole.WriteLine($"Error could not find core manifest for '{core_plugin_name}'");
                        continue;
                    }

                    string target_dat_path = Path.Combine(proj_dat_dir, $"{core_mf.guid}.zip");
                    MpFileIo.CopyFileOrDirectory(core_plugin_zip_path, target_dat_path, forceOverwrite: true);
                    MpConsole.WriteLine(target_dat_path);
                }
            }

            if (LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DEBUG)) {
                DoLocalMove(true);
            }
            if (LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.RELEASE)) {
                DoLocalMove(false);
            }
            MpConsole.WriteLine($"[LOCAL] Moving core plugins to dat DONE", false, true);
        }
        #endregion

        #region Localizing
        static void MoveJsUiStrings() {
            foreach (string cc in WorkingCultures) {
                string uistr_path = Path.Combine(
                    MpPlatformHelpers.GetSolutionDir(),
                    "MonkeyPaste.Avalonia",
                    "Resources",
                    "Localization",
                    "UiStrings",
                    $"UiStrings.{cc}.resx");
                var uistr_lookup = MpResxTools.ReadResxFromPath(uistr_path);

                string EDITOR_KEY_PREFIX = "Editor";
                string EDITOR_STR_INSERT_MARKER = "<INSERT KVP HERE>";
                string EditorUiStringJsContentTemplate = string.Format(
@"var UiStrings = {{
{0}
}};", EDITOR_STR_INSERT_MARKER);

                string inner_content =
                    string.Join(
                        string.Empty,
                        uistr_lookup
                        .Where(x => x.Key.StartsWith(EDITOR_KEY_PREFIX))
                        .OrderBy(x => x.Key)
                        .Select(x => $"\t{x.Key}: `{x.Value.value}`,{Environment.NewLine}"));

                // swap placeholder w/ key-values
                string runtime_content =
                    EditorUiStringJsContentTemplate.Replace(EDITOR_STR_INSERT_MARKER, inner_content);

                string js_uistr_path =
                    Path.Combine(
                        MpPlatformHelpers.GetSolutionDir(),
                        "MonkeyPaste.Avalonia",
                        "Resources",
                        "Editor",
                        "src",
                        "components",
                        "localizer",
                        $"UiStrings.{cc}.js");
                MpFileIo.WriteTextToFile(js_uistr_path, runtime_content, overwrite: true);
            }
        }
        static void LocalizeManifests() {
            MpConsole.WriteLine("Localize Manifest...STARTED", true);
            foreach (string plugin_name in WorkingPluginNames) {
                LocalizeManifest(plugin_name);
            }
            MpConsole.WriteLine("Localize Manifest...DONE", false, true);
        }
        static void LocalizeManifest(string plugin_name) {
            // when plugin has Resources/Resources.resx, presume manifest is templated
            // and create localized manifests of all Resources.<culture> in /Resources
            // otherwise ignore
            string plugin_res_dir = GetPluginResourcesDir(plugin_name);
            string invariant_resource_path = Path.Combine(plugin_res_dir, "Resources.resx");
            if (!plugin_res_dir.IsDirectory() || !invariant_resource_path.IsFile()) {
                return;
            }
            string inv_mf_path =
                Path.Combine(GetPluginProjDir(plugin_name), ManifestFileName);

            string templated_manifest_json = MpFileIo.ReadTextFromFile(inv_mf_path);

            var lang_codes = MpLocalizationHelpers.FindCulturesInDirectory(
                    dir: plugin_res_dir,
                    file_name_filter: "Resources",
                    file_ext_filter: "resx");

            foreach (string lang_code in lang_codes.Where(x => !x.IsInvariant()).Select(x => x.Name)) {
                Localizer.Program.LocalizeManifest(invariant_resource_path, inv_mf_path, lang_code, plugin_res_dir);
            }
            MpConsole.WriteLine("", stampless: true);
        }
        static void GenAllEmptyLocalizedResx() {
            var all_ref_resxs = GetAllNeutralResxPaths();

            foreach (string ref_resx_path in all_ref_resxs) {
                var resx_lookup = MpResxTools.ReadResxFromPath(ref_resx_path);
                var empty_lookup = resx_lookup.ToDictionary(x => x.Key, x => (string.Empty, string.Empty));
                foreach (var cc in WorkingCultures) {
                    string empty_localized_resx_path = GetLocalizedPathFromNeutral(ref_resx_path, cc);
                    if (empty_localized_resx_path.IsFile()) {
                        continue;
                    }
                    //MpFileIo.DeleteFile(empty_localized_resx_path);
                    //if (ref_resx_dir.Contains("Plugins")) {
                    //    MpFileIo.DeleteFile(Path.Combine(
                    //        Path.GetDirectoryName(empty_localized_resx_path), $"manifest.{cc}.json"));
                    //}
                    var lookup_to_write = cc == "en-US" ? resx_lookup : empty_lookup;
                    MpResxTools.WriteResxToPath(empty_localized_resx_path, lookup_to_write);
                    MpConsole.WriteLine(empty_localized_resx_path);
                }
            }
        }

        static void GenEditorUiStrings() {

        }
        static void TranslateAllResxWrapper() {
            var all_neutral_resxs = GetAllNeutralResxPaths();
            int done_count = 0;
            async Task RunTranslateAsync(string resx_path) {
                await TranslateAllResxAsync(resx_path);
                done_count++;
            }
            foreach (string neutral_resx_path in all_neutral_resxs) {
                int pre_count = done_count;
                RunTranslateAsync(neutral_resx_path).FireAndForgetSafeAsync();
                while (pre_count == done_count) {
                    Thread.Sleep(100);
                }
            }
            PrintResxStats();
        }
        static async Task TranslateAllResxAsync(string neutral_resx_path) {
            foreach (string cc in WorkingCultures) {
                var sw = Stopwatch.StartNew();
                await TranslateResxOneLinersAsync(neutral_resx_path, cc);
                //await TranslateResxHtmlAsync(neutral_resx_path, cc);
            }
        }

        static async Task TranslateResxHtmlAsync(string neutral_resx_path, string cc) {
            var neutral_resx_lookup = MpResxTools.ReadResxFromPath(neutral_resx_path);

            string trans_resx_path = GetLocalizedPathFromNeutral(neutral_resx_path, cc);
            if (!trans_resx_path.IsFile()) {
                MpResxTools.WriteResxToPath(trans_resx_path, neutral_resx_lookup.ToDictionary(x => x.Key, x => (string.Empty, string.Empty)));
            }
            var trans_resx_lookup = MpResxTools.ReadResxFromPath(trans_resx_path);

            // get non-empty neutral single line keys that aren't html, invariant or have localized data
            var neutral_html_kvps =
                neutral_resx_lookup
                .Where(x =>
                    x.Value.value.ContainsHtml() &&// (!trans_resx_lookup.ContainsKey(x.Key) || trans_resx_lookup[x.Key].value.IsNullOrEmpty()) &&
                    x.Value.comment != "@Invariant")
                .OrderBy(x => x.Key)
                .ToArray();
            if (!neutral_html_kvps.Any()) {
                return;
            }

            List<string> neutral_text_node_texts = [];

            foreach (var neutral_html_kvp in neutral_html_kvps) {
                //var html_doc = new HtmlDocument();
                //html_doc.LoadHtml(neutral_html_kvp.Value.value);
                //var text_nodes = html_doc.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']").Cast<HtmlTextNode>().ToList();
                //neutral_text_node_texts.AddRange(text_nodes.Select(x => x.Text));
                string pt = MpRichHtmlToPlainTextConverter.Convert(neutral_html_kvp.Value.value);
                string trans_pt = await TranslateTextAsync(pt, cc);
                trans_resx_lookup.AddOrReplace(neutral_html_kvp.Key, (trans_pt, neutral_html_kvp.Value.value));
            }
            //var translated_text_node_texts = await TranslateTextsAsync(neutral_text_node_texts.ToArray(), cc);
            //int cur_idx = 0;
            //foreach (var neutral_html_kvp in neutral_html_kvps) {
            //    var html_doc = new HtmlDocument();
            //    html_doc.LoadHtml(neutral_html_kvp.Value.value);
            //    var text_nodes = html_doc.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']").Cast<HtmlTextNode>().ToList();
            //    foreach (var node in text_nodes) {
            //        node.Text = translated_text_node_texts[cur_idx++];
            //    }
            //    trans_resx_lookup[neutral_html_kvp.Key] = (html_doc.DocumentNode.OuterHtml, neutral_html_kvp.Value.value);
            //}

            MpResxTools.WriteResxToPath(trans_resx_path, trans_resx_lookup);
            MpConsole.WriteLine(trans_resx_path);

        }
        static async Task TranslateResxOneLinersAsync(string neutral_resx_path, string cc) {
            var translator = new GoogleLiteTextTranslatorPlugin();
            int max_len = 5000;
            // load neutral resx
            var neutral_resx_lookup = MpResxTools.ReadResxFromPath(neutral_resx_path);
            // create/load trans resx
            string trans_resx_path = GetLocalizedPathFromNeutral(neutral_resx_path, cc);
            if (!trans_resx_path.IsFile()) {
                MpResxTools.WriteResxToPath(trans_resx_path, neutral_resx_lookup.ToDictionary(x => x.Key, x => (string.Empty, x.Value.comment)));
            }
            var trans_resx_lookup = MpResxTools.ReadResxFromPath(trans_resx_path);
            // add missing keys
            neutral_resx_lookup
                .Where(x => !trans_resx_lookup.ContainsKey(x.Key))
                .ForEach(x => trans_resx_lookup.Add(x.Key, (string.Empty, x.Value.comment)));

            // write invariants
            neutral_resx_lookup
                .Where(x => x.Value.comment == "@Invariant")
                .ForEach(x => trans_resx_lookup[x.Key] = x.Value);

            MpResxTools.WriteResxToPath(trans_resx_path, trans_resx_lookup);
            MpConsole.WriteLine(trans_resx_path);

            // get non-empty neutral single line keys that aren't html, invariant or have localized data
            var neutral_single_line_kvps =
                neutral_resx_lookup
                .Where(x =>
                    !x.Value.value.Contains(Environment.NewLine) &&
                    !x.Value.value.ContainsHtml() &&
                    !string.IsNullOrEmpty(x.Value.value) &&
                    string.IsNullOrEmpty(trans_resx_lookup[x.Key].value) &&
                    x.Value.comment != "@Invariant")
                .OrderBy(x => x.Key)
                .ToArray();
            string splitter = Environment.NewLine;// + "**";

            List<List<string>> batches = new() { new() };
            for (int i = 0; i < neutral_single_line_kvps.Length; i++) {
                var batch = batches.Last();
                var cur_kvp = neutral_single_line_kvps[i];
                batch.Add(cur_kvp.Value.value);
                int batch_length = batch.Sum(x => x.Length) + (Math.Max(0, batch.Count - 1) * splitter.Length);
                if (batch_length > max_len) {
                    List<string> next_batch = new() { batch.Last() };
                    batch.RemoveAt(batch.Count - 1);
                    batches.Add(next_batch);
                }
            }
            int base_idx = 0;
            for (int i = 0; i < batches.Count; i++) {
                var batch = batches[i];
                string neutral_text = string.Join(splitter, batch);
                string result_single_line_str = await TranslateTextAsync(neutral_text, cc);
                var trans_results = result_single_line_str.Split(new string[] { splitter }, StringSplitOptions.None);
                int expected_count = neutral_text.Split(new string[] { splitter }, StringSplitOptions.None).Length;
                MpDebug.Assert(trans_results.Length == expected_count, $"Count mismiatch. Source {expected_count} Target {trans_results.Length}");

                for (int j = 0; j < trans_results.Length; j++) {
                    if (base_idx >= neutral_single_line_kvps.Length) {
                        break;
                    }
                    string key = neutral_single_line_kvps[base_idx].Key;
                    string val = trans_results[j];
                    trans_resx_lookup[key] = (val, neutral_single_line_kvps[base_idx].Value.comment);
                    base_idx++;
                }
            }
            MpDebug.Assert(base_idx == neutral_single_line_kvps.Length, $"Base idx mismatch");

            MpResxTools.WriteResxToPath(trans_resx_path, trans_resx_lookup);
            MpConsole.WriteLine(trans_resx_path);

        }
        static async Task<string[]> TranslateTextsAsync(string[] neutral_texts, string cc, int max_len = 5000) {
            // create batches of text in max len chunks (including splitter length)
            string splitter = Environment.NewLine + "<> ";
            List<List<string>> batches = new() { new() };
            for (int i = 0; i < neutral_texts.Length; i++) {
                var batch = batches.Last();
                var cur_text = neutral_texts[i];
                batch.Add(cur_text);
                int batch_length = batch.Sum(x => x.Length) + (Math.Max(0, batch.Count - 1) * splitter.Length);
                if (batch_length > max_len) {
                    List<string> next_batch = new() { batch.Last() };
                    batch.RemoveAt(batch.Count - 1);
                    batches.Add(next_batch);
                }
            }
            List<string> translated_texts = [];
            for (int i = 0; i < batches.Count; i++) {
                // loop through each batch
                var batch = batches[i];
                // translate batch by splitter into single string
                string neutral_batched_text = string.Join(splitter, batch);
                string translated_batch_text = await TranslateTextAsync(neutral_batched_text, cc);
                var trans_results = translated_batch_text.Split(new string[] { splitter }, StringSplitOptions.None);
                int expected_count = neutral_batched_text.Split(new string[] { splitter }, StringSplitOptions.None).Length;
                MpDebug.Assert(trans_results.Length == expected_count, $"Count mismiatch. Source {expected_count} Target {trans_results.Length}");

                for (int j = 0; j < trans_results.Length; j++) {
                    if (j >= neutral_texts.Length) {
                        break;
                    }
                    translated_texts.Add(trans_results[j]);
                }
            }
            MpDebug.Assert(translated_texts.Count == neutral_texts.Length, $"Count mismiatch. Source {neutral_texts.Length} Target {translated_texts.Count}");
            return translated_texts.ToArray();
        }
        static async Task<string> TranslateTextAsync(string neutral_text, string cc) {
            var resp = await _Translator.AnalyzeAsync(new() {
                culture = "en-US",
                items = new List<MpParameterRequestItemFormat>() {
                            new MpParameterRequestItemFormat() {
                                paramId = "from",
                                paramValue = "en-US"
                            },
                            new MpParameterRequestItemFormat() {
                                paramId = "to",
                                paramValue = cc
                            },
                            new MpParameterRequestItemFormat() {
                                paramId = "text",
                                paramValue = neutral_text
                            }
                        }
            });
            if (!resp.dataObjectLookup.TryGetValue(MpPortableDataFormats.Text, out var translated_text_obj) ||
                    translated_text_obj is not string translated_text) {
                return neutral_text;
            }
            return translated_text;
        }

        static string GetLocalizedPathFromNeutral(string ref_resx_path, string cc) {
            string ref_resx_dir = Path.GetDirectoryName(ref_resx_path);
            string ref_resx_file_name = Path.GetFileNameWithoutExtension(ref_resx_path);
            string ref_resx_ext = Path.GetExtension(ref_resx_path).Replace(".", string.Empty);
            string localized_resx_file_name_format = $"{ref_resx_file_name}.{cc}.{ref_resx_ext}";

            string localized_resx_path = Path.Combine(
                        ref_resx_dir,
                        string.Format(localized_resx_file_name_format, cc));
            return localized_resx_path;
        }
        static void PrintResxStats() {
            var all_neu_resxs = GetAllNeutralResxPaths().First();
            var all_cultures_per_neu_lookup = MpLocalizationHelpers.FindCulturesInDirectory(Path.GetDirectoryName(all_neu_resxs)).ToList();
            var lang_groups = all_cultures_per_neu_lookup.Where(x => !x.Name.IsNullOrEmpty()).GroupBy(x => x.Name.SplitNoEmpty("-").First()).ToList();
            MpConsole.WriteLine($"Languages: {lang_groups.Count} Dialects: {all_cultures_per_neu_lookup.Count}");
        }

        static bool VerifyConsitentCultures() {
            var all_neutral_resx = GetAllNeutralResxPaths();
            var neutral_resx_culture_lookup =
                all_neutral_resx
                .ToDictionary(x => x, x => MpLocalizationHelpers.FindCulturesInDirectory(Path.GetDirectoryName(x), file_ext_filter: "resx"));
            var all_cultures = neutral_resx_culture_lookup.SelectMany(x => x.Value).DistinctBy(x => x.Name);
            bool success = true;
            foreach (string neutral_resx_path in all_neutral_resx) {
                var missing_cultures = all_cultures.Where(x => neutral_resx_culture_lookup[neutral_resx_path].All(y => y.Name != x.Name)).ToList();
                if (missing_cultures.Any()) {
                    success = false;
                    MpConsole.WriteLine($"Culture Check FAILED: {neutral_resx_path}", true);
                    MpConsole.WriteLine("Missing Cultures: ");
                    missing_cultures.ForEach((x, idx) => MpConsole.WriteLine($"{x.Name}", false, idx == missing_cultures.Count - 1));
                } else {
                    MpConsole.WriteLine($"Culture Check PASSED: {neutral_resx_path}", true, true);
                }
            }
            return success;
        }
        #endregion

        #region Index
        static void CreateCultureIndex(bool is_remote) {
            MpConsole.WriteLine($"Creating {(is_remote ? "REMOTE" : "LOCAL")} Cultures...", true);

            List<string> found_cultures = [];
            // find all distinct cultures
            foreach (var plugin_name in AllPluginNames) {
                string plugin_cultures_dir = GetPluginResourcesDir(plugin_name);
                if (plugin_cultures_dir == null) {
                    // no resources dir
                    continue;
                }
                if (MpLocalizationHelpers.FindCulturesInDirectory(plugin_cultures_dir, file_name_filter: ManifestPrefix) is { } cil) {
                    var to_add = cil.Where(x => !found_cultures.Contains(x.Name) && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name);
                    found_cultures.AddRange(to_add);
                }
            }

            // recreate invariant ledger
            var ledger = ReadInvLedger(is_remote);

            // create localized ledger for each distinct culture in /Cultures dir
            foreach (string cc in found_cultures) {
                var culture_manifests = new List<MpManifestFormat>();
                foreach (string plugin_name in AllPluginNames) {
                    try {
                        // find closest culture for each plugin and create that manifest
                        var culture_manifest = GetLocalizedManifest(plugin_name, cc);
                        if (ledger.manifests.FirstOrDefault(x => x.guid == culture_manifest.guid) is { } ledger_manifest) {
                            // use inv ledger packageUrl
                            culture_manifest.publishedAppVersion = APP_VERSION;
                            culture_manifest.packageUrl = ledger_manifest.packageUrl;
                        }

                        culture_manifests.Add(culture_manifest);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error deserializing {plugin_name} '{cc}'", ex);
                    }
                }
                var culture_ledger = new MpManifestLedger() {
                    manifests = culture_manifests
                };
                // save ledger to /Cultures dir
                string culture_ledger_file_name =
                    $"{MpLedgerConstants.LEDGER_PREFIX}{(is_remote ? string.Empty : MpLedgerConstants.LOCAL_SUFFIX)}.{cc}.{MpLedgerConstants.LEDGER_EXT}";
                string culture_ledger_path = Path.Combine(
                    MpLedgerConstants.LOCAL_CULTURES_DIR_URI.ToPathFromUri(),
                    culture_ledger_file_name);
                MpFileIo.WriteTextToFile(culture_ledger_path, culture_ledger.SerializeObject(omitNulls: true).ToPrettyPrintJson());
                MpConsole.WriteLine(culture_ledger_path);
            }

            MpConsole.WriteLine($"Creating {(is_remote ? "REMOTE" : "LOCAL")} index...", true);
            // create index of all written cultures
            string ledger_index_file_name = is_remote ?
                MpLedgerConstants.REMOTE_LEDGER_INDEX_NAME :
                MpLedgerConstants.LOCAL_LEDGER_INDEX_NAME;
            string ledger_index_path = Path.Combine(
                MpLedgerConstants.LEDGER_PROJ_DIR,
                ledger_index_file_name);
            MpFileIo.WriteTextToFile(ledger_index_path, found_cultures.SerializeObject().ToPrettyPrintJson());
            MpConsole.WriteLine(ledger_index_path);
        }

        static MpManifestFormat GetLocalizedManifest(string plugin_name, string culture) {
            string plugin_proj_cultures_dir = GetPluginResourcesDir(plugin_name);
            string inv_manifest_path = Path.Combine(GetPluginProjDir(plugin_name), ManifestFileName);
            if (plugin_proj_cultures_dir != null) {
                string resolved_cultre = MpLocalizationHelpers.FindClosestCultureCode(
                culture, plugin_proj_cultures_dir,
                file_name_filter: ManifestPrefix);
                if (!string.IsNullOrEmpty(resolved_cultre)) {
                    inv_manifest_path = Path.Combine(
                    plugin_proj_cultures_dir,
                    $"{ManifestPrefix}.{resolved_cultre}.{ManifestExt}").Replace("..", ".");
                    MpDebug.Assert(inv_manifest_path.IsFile(), $"ERror can't find manifest {inv_manifest_path}");
                }
            }
            return JsonConvert.DeserializeObject<MpManifestFormat>(MpFileIo.ReadTextFromFile(inv_manifest_path));
        }


        #endregion

        #region Packaging
        static void GenInvLedger(bool is_remote) {
            // filter any ledger ignored plugins (minimal example)
            void GenLedger(bool is_release) {
                var output_ledger = new MpManifestLedger() {
                    manifests = []
                };
                foreach (string plugin_name in AllPluginNames) {
                    var mf = MpFileIo.ReadTextFromFile(Path.Combine(GetPluginProjDir(plugin_name), ManifestFileName)).DeserializeObject<MpManifestFormat>();
                    // set pub app version for all plugins
                    mf.publishedAppVersion = APP_VERSION;
                    mf.packageUrl = GetPluginPackageUri(plugin_name, !is_remote, !is_release);
                    mf.readmeUrl = GetPluginPropertyUri(plugin_name, "readme");
                    mf.projectUrl = GetPluginPropertyUri(plugin_name, "project");
                    mf.iconUri = GetPluginPropertyUri(plugin_name, "icon");
                    output_ledger.manifests.Add(mf);
                }

                string output_path = is_remote ?
                    MpLedgerConstants.REMOTE_INV_LEDGER_PATH :
                    MpLedgerConstants.LOCAL_INV_LEDGER_PATH;

                MpFileIo.WriteTextToFile(
                        output_path,
                        output_ledger.SerializeObject(true).ToPrettyPrintJson());
                MpConsole.WriteLine($"{(is_remote ? "REMOTE" : "LOCAL")} ledger written to: {output_path}", true);
            }
            if (LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DEBUG)) {
                GenLedger(false);
            }
            if (LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.RELEASE)) {
                GenLedger(true);
            }
        }
        static string PackPlugin(string plugin_name, bool is_release) {
            // returns zip uri to use for local packageUrl
            string proj_dir = GetPluginProjDir(plugin_name);
            string guid = ReadPluginManifestFromProjDir(plugin_name).guid;
            string output_path = GetPluginPackageUri(plugin_name, true, !is_release).ToPathFromUri();
            string root_pack_dir = Path.GetDirectoryName(output_path);


            if (!root_pack_dir.IsDirectory()) {
                // create packages dir if first pack
                MpFileIo.CreateDirectory(root_pack_dir);
            }

            // delete build stuff
            MpFileIo.DeleteDirectory(Path.Combine(proj_dir, "bin"));
            MpFileIo.DeleteDirectory(Path.Combine(proj_dir, "obj"));

            // perform publish and output to ledger proj/packages_* dir
            string publish_dir = Path.Combine(root_pack_dir, plugin_name);
            // clear last output
            MpFileIo.DeleteDirectory(publish_dir);
            string config = is_release ? "Release" : "Debug";
            //string args = CorePlugins.Contains(plugin_name) ?
            //    $"msbuild /p:OutDir={publish_dir} -target:Publish /property:Configuration={config} /property:DefineConstants=AUX%3B{BUILD_OS} -restore" :
            //    $"publish --configuration {config} --output {publish_dir}";
            
            string args = $"publish --configuration {config} --output {publish_dir}";

            (int exit_code, string proc_output) =
                RunProcess(
                    file: "dotnet",
                    dir: proj_dir,
                    args: args);

            if (exit_code != 0) {
                MpConsole.WriteLine("");
                MpConsole.WriteLine($"Error from '{plugin_name}' exit code '{exit_code}'");
                MpConsole.WriteLine(proc_output);
                MpConsole.WriteLine("");
                return null;
            }

            if (!publish_dir.IsDirectory()) {
                return null;
            }
            // clear previous package
            MpFileIo.DeleteFile(output_path);

            // remove any important excluded files (ref assemblies, only in CoreOle)
            new DirectoryInfo(publish_dir)
                .GetFiles()
                .Where(x => ExcludedPublishFileNames.Contains(x.Name))
                .Select(x => x.FullName)
                .ForEach(x => MpFileIo.DeleteFile(x));

            // zip publish output 
            ZipFile.CreateFromDirectory(publish_dir, output_path, CompressionLevel.Fastest, true);

            // get plugin install dir

            string plugin_install_dir =
                Path.Combine(
                    MpPlatformHelpers.GetStorageDir(),
                    "Plugins",
                    guid).LocalStoragePathToPackagePath();
            string install_update_suffix = string.Empty;
            if (plugin_install_dir.IsDirectory()) {
                if (plugin_install_dir.IsDirectory()) {
                    // if plugin is installed we need to use this build output 
                    // at least for debugging but probably in general too
                    string inner_install_dir = Path.Combine(plugin_install_dir, plugin_name);
                    MpFileIo.DeleteDirectory(inner_install_dir);
                    // duplicate just published dir to plugin container dir
                    MpFileIo.CreateDirectory(inner_install_dir);
                    MpFileIo.CopyDirectory(publish_dir, inner_install_dir);
                    install_update_suffix = " install UPDATED";
                }
            }
            // cleanup published output
            MpFileIo.DeleteDirectory(publish_dir);
            MpConsole.WriteLine($"{plugin_name} local [{BUILD_OS}-{config.ToUpper()}] DONE" + install_update_suffix);

            // return zip uri to use for local packageUrl
            return output_path.ToFileSystemUriFromPath();
        }
        static void PublishLocal() {
            if (LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DEBUG)) {
                foreach (var plugin_name in WorkingPluginNames) {
                    string local_package_uri = PackPlugin(plugin_name, false);
                }
            }
            if (LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.RELEASE)) {
                foreach (var plugin_name in WorkingPluginNames) {
                    string local_package_uri = PackPlugin(plugin_name, true);
                }
            }
        }
        static void PublishRemote() {
            // returns the complete remote ledger

            foreach (var plugin_name in WorkingPluginNames) {
                PushReleaseToRemote(plugin_name);
            }
        }
        static string PushReleaseToRemote(string plugin_name, string forced_new_version = null) {
            // NOTE this should always be release
            string proj_dir = GetPluginProjDir(plugin_name);
            var manifest = ReadPluginManifestFromProjDir(plugin_name);
            string working_version = forced_new_version ?? manifest.version;
            string source_package_path = GetPluginPackageUri(plugin_name, true, false).ToPathFromUri();
            if (!source_package_path.IsFile()) {
                // local package missing so pack local first
                MpConsole.WriteLine($"Local package missing for {plugin_name}. Packing...");
                string local_pack_path_uri = PackPlugin(plugin_name, true);
                MpDebug.Assert(local_pack_path_uri.ToPathFromUri() == source_package_path, $"Remote publish error package path mismatch. Expected: '{source_package_path}' Found: '{local_pack_path_uri}'");
            }
            string target_tag_name = $"v{working_version}";
            string target_package_file_name = $"{target_tag_name}.zip";
            string target_package_path = Path.Combine(proj_dir, target_package_file_name);

            if (CorePlugins.Contains(plugin_name)) {
                MoveCorePluginToServer(plugin_name, false);
                return GetPluginPackageUri(plugin_name, false, false);
            }
            MpConsole.WriteLine($"Pushing {target_package_file_name} for {plugin_name} to github...");
            MpFileIo.CopyFileOrDirectory(source_package_path, target_package_path, forceOverwrite: true);

            // see this about gh release https://cli.github.com/manual/gh_release_create
            (int exit_code, string proc_output) = RunProcess(
                file: "gh.exe",
                dir: proj_dir,
                args: $"release create {target_tag_name} --latest --generate-notes {target_package_file_name}");


            if (exit_code == 1) {
                // version exist
                if (FORCE_REPLACE_REMOTE_TAG) {
                    // delete version, call again
                    if (forced_new_version != null) {
                        // should only occur once 
                        MpConsole.WriteLine($"Uncaught error after delete for '{proj_dir}' skipping upload");
                        MpFileIo.DeleteFile(target_package_path);
                        return null;
                    }
                    (int del_exit_code, string del_proc_output) =
                        RunProcess(
                            file: "gh.exe",
                            dir: proj_dir,
                            args: $"release delete {target_tag_name} --yes --cleanup-tag");
                    if (del_exit_code != 0) {
                        MpConsole.WriteLine($"Error delete failed exit code {del_exit_code}. Output: {del_proc_output}");
                        MpFileIo.DeleteFile(target_package_path);
                        return null;
                    }
                } else {
                    // increment, call again
                    if (working_version.SplitNoEmpty(".") is not { } verParts ||
                        !int.TryParse(verParts.Last(), out int minor_rev)) {
                        MpConsole.WriteLine($"Error bad version for plugin at '{proj_dir}'");
                        MpFileIo.DeleteFile(target_package_path);
                        return null;
                    }
                    working_version = $"{verParts[0]}.{verParts[1]}.{minor_rev + 1}";
                }
                // if first fail use failed version
                var new_ver_result = PushReleaseToRemote(plugin_name, working_version);
                return new_ver_result;
            } else if (exit_code == 0 && forced_new_version != null) {
                // new rev works, update local manifest to match

                // NOTE avoiding full re-write since manifest can be subclass, just replacing version...
                string manifest_json = MpFileIo.ReadTextFromFile(Path.Combine(proj_dir, ManifestFileName));
                string old_ver_json = $"\"version\": \"{manifest.version}\"";
                string new_ver_json = $"\"version\": \"{working_version}\"";
                if (manifest_json.Contains(old_ver_json)) {
                    manifest_json = manifest_json.Replace(old_ver_json, new_ver_json);
                    MpFileIo.WriteTextToFile(Path.Combine(proj_dir, ManifestFileName), manifest_json);
                } else {
                    MpConsole.WriteLine($"Error! Could not find old ver string '{old_ver_json}' trying to replace with '{new_ver_json}' in plugin '{proj_dir}'");
                }
            }

            if (exit_code != 0) {
                MpConsole.WriteLine($"Error from '{plugin_name}' exit code '{exit_code}'", true);
                MpConsole.WriteLine(proc_output, false, true);
                MpFileIo.DeleteFile(target_package_path);
                return null;
            }

            MpFileIo.DeleteFile(target_package_path);
            string github_release_uri = GetPluginPackageUri(plugin_name, false, false);
            MpConsole.WriteLine($"{plugin_name} remote DONE");
            return github_release_uri;
        }
        #endregion

        #region Version

        static void UpdateVersions(bool is_remote) {
            MpManifestLedger ledger = ReadInvLedger(is_remote);
            bool is_done = false;

            _ = Task.Run(async () => {
                foreach (var mf in ledger.manifests) {
                    var req_args = new Dictionary<string, string>() {
                        {"plugin_guid", mf.guid },
                        {"version", mf.version},
                        {"is_install", "0" },
                        {"add_phrase", ServerSecrets["versionUpdatePhrase"].ToString() }
                    };
                    string url = is_remote ?
                        $"{MpServerConstants.REMOTE_SERVER_URL}/plugins/plugin-info-check.php" :
                        $"{MpServerConstants.LOCAL_SERVER_URL}/plugins/plugin-info-check.php";

                    var resp = await MpHttpRequester.SubmitPostDataToUrlAsync(url, req_args);
                    bool success = MpHttpRequester.ProcessServerResponse(resp, out var resp_args);

                    var msg_mf = mf;
                    if(GetLocalizedManifest(mf.title,"en-US") is { } neu_mf) {
                        msg_mf = neu_mf;
                    }
                    MpConsole.WriteLine($"{msg_mf} {success.ToTestResultLabel()} info check resp: {resp}");
                }
                is_done = true;
            });

            while (!is_done) {
                Thread.Sleep(100);
            }

        }
        #endregion

        #region Nuget Cache
        private static void MoveNugetCache() {
            MpConsole.WriteLine($"Moving Nuget Cache...");
            var source_dirs =
                Directory.GetDirectories(UbuntuSecrets["sourceDir"].ToString())
                .SelectMany(x => Directory.GetDirectories(x).Where(y => y.Contains("9999.0.0-localbuild")));
            var sb = new StringBuilder();
            foreach (var sd in source_dirs) {
                string target_dir = sd.Replace(
                    UbuntuSecrets["sourceDir"].ToString(),
                    UbuntuSecrets["targetDir"].ToString())
                    .Replace("\\", "/");
                sb.AppendLine($"rmdir {target_dir}");
                sb.AppendLine($"put -r {sd} {target_dir}");
            }
            MpConsole.WriteLine($"Output:");
            MpConsole.WriteLine(sb.ToString());
            MpConsole.WriteLine($"DONE");
        }
        #endregion

        #region Helpers
        static string GetRemotePluginPackageName(string version) {
            return $"v{version}.zip";
        }
        static MpManifestLedger ReadInvLedger(bool is_remote) {
            string inv_ledger_path = Path.Combine(
                MpLedgerConstants.LEDGER_PROJ_DIR,
                is_remote ?
                MpLedgerConstants.REMOTE_LEDGER_NAME :
                MpLedgerConstants.LOCAL_LEDGER_NAME);
            return MpFileIo.ReadTextFromFile(inv_ledger_path).DeserializeObject<MpManifestLedger>();
        }
        static MpManifestFormat ReadPluginManifestFromProjDir(string plugin_name) {
            string plugin_proj_dir = GetPluginProjDir(plugin_name);
            string plugin_manifest_path = Path.Combine(
                    plugin_proj_dir,
                    ManifestFileName);

            string plugin_manifest_text = MpFileIo.ReadTextFromFile(plugin_manifest_path);
            return plugin_manifest_text.DeserializeObject<MpManifestFormat>();
        }
        static string GetPluginProjDir(string plugin_name) {
            return Path.Combine(
                        MpPlatformHelpers.GetSolutionDir(),
                        "Plugins",
                        plugin_name);
        }
        static string GetPluginResourcesDir(string plugin_name) {
            string res_dir = Path.Combine(
                GetPluginProjDir(plugin_name), "Resources");
            if (!res_dir.IsDirectory()) {
                return null;
            }
            return res_dir;
        }

        static string GetPluginPropertyUri(string plugin_name, string prop) {
            switch (prop) {
                case "readme":
                    if (CorePlugins.Contains(plugin_name)) {
                        return null;
                    }
                    return string.Format(README_URL_FORMAT, plugin_name);
                case "project":
                    if (CorePlugins.Contains(plugin_name)) {
                        return ServerSecrets["httpUrl"];
                    }
                    return string.Format(PROJ_URL_FORMAT, plugin_name);
                case "icon":
                    if (CorePlugins.Contains(plugin_name)) {
                        return string.Format(PRIVATE_ICON_URL_FORMAT, plugin_name);
                    }
                    return string.Format(ICON_URL_FORMAT, plugin_name);

            }
            return null;
        }
        static string GetPluginPackageUri(string plugin_name, bool is_local, bool is_debug) {
            if (is_local) {
                string root_pack_dir = GetPackagesDir(is_debug);
                string output_path = Path.Combine(root_pack_dir, $"{plugin_name}.zip");
                return output_path.ToFileSystemUriFromPath();
            }
            var mf = ReadPluginManifestFromProjDir(plugin_name);
            if (CorePlugins.Contains(plugin_name)) {
                // NOTE returning THIS version not latest
                string package_name = $"v{mf.version}.zip";
                return $"{MpLedgerConstants.REMOTE_CORE_PLUGIN_BASE_URI}/{mf.guid}/{package_name}";
            }
            return string.Format(PUBLIC_PACKAGE_URL_FORMAT, plugin_name, $"v{mf.version}");
        }

        static string GetPackagesDir(bool is_debug) {
            return is_debug ?
                    MpLedgerConstants.DEBUG_PACKAGES_DIR :
                    MpLedgerConstants.RELEASE_PACKAGES_DIR;
        }

        static (int, string) RunProcess(string file, string dir, string args) {
            var proc = new Process();
            proc.StartInfo.FileName = file;
            proc.StartInfo.WorkingDirectory = dir;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string proc_output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            int exit_code = proc.ExitCode;
            proc.Close();
            proc.Dispose();
            return (exit_code, proc_output);
        }
        #endregion
    }
}