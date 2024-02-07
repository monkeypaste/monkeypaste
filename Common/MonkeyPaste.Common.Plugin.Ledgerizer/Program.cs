using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
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
        DO_LOCAL_INDEX = 1L << 6,
        DO_REMOTE_INDEX = 1L << 7,
        MOVE_CORE_TO_DAT = 1L << 8,
        LOCALIZE_MANIFESTS = 1L << 9,
        GEN_ADDON_LISTING = 1L << 10,
        GEN_PROD_LISTING = 1L << 11,
    }
    internal class Program {
        //static string ALL_CULTURES_CSV = "ar,ar-sa,ar-ae,ar-bh,ar-dz,ar-eg,ar-iq,ar-jo,ar-kw,ar-lb,ar-ly,ar-ma,ar-om,ar-qa,ar-sy,ar-tn,ar-ye,af,af-za,sq,sq-al,am,am-et,hy,hy-am,as,as-in,az-arab,az-arab-az,az-cyrl,az-cyrl-az,az-latn,az-latn-az,eu,eu-es,be,be-by,bn,bn-bd,bn-in,bs,bs-cyrl,bs-cyrl-ba,bs-latn,bs-latn-ba,bg,bg-bg,ca,ca-es,ca-es-valencia,chr-cher,chr-cher-us,chr-latn,zh-Hans,zh-cn,zh-hans-cn,zh-sg,zh-hans-sg,zh-Hant,zh-hk,zh-mo,zh-tw,zh-hant-hk,zh-hant-mo,zh-hant-tw,hr,hr-hr,hr-ba,cs,cs-cz,da,da-dk,prs,prs-af,prs-arab,nl,nl-nl,nl-be,en,en-au,en-ca,en-gb,en-ie,en-in,en-nz,en-sg,en-us,en-za,en-bz,en-hk,en-id,en-jm,en-kz,en-mt,en-my,en-ph,en-pk,en-tt,en-vn,en-zw,en-053,en-021,en-029,en-011,en-018,en-014,et,et-ee,fil,fil-latn,fil-ph,fi,fi-fi,fr,fr-be ,fr-ca ,fr-ch ,fr-fr ,fr-lu,fr-015,fr-cd,fr-ci,fr-cm,fr-ht,fr-ma,fr-mc,fr-ml,fr-re,frc-latn,frp-latn,fr-155,fr-029,fr-021,fr-011,gl,gl-es,ka,ka-ge,de,de-at,de-ch,de-de,de-lu,de-li,el,el-gr,gu,gu-in,ha,ha-latn,ha-latn-ng,he,he-il,hi,hi-in,hu,hu-hu,is,is-is,ig-latn,ig-ng,id,id-id,iu-cans,iu-latn,iu-latn-ca,ga,ga-ie,xh,xh-za,zu,zu-za,it,it-it,it-ch,ja ,ja-jp,kn,kn-in,kk,kk-kz,km,km-kh,quc-latn,qut-gt,qut-latn,rw,rw-rw,sw,sw-ke,kok,kok-in,ko,ko-kr,ku-arab,ku-arab-iq,ky-kg,ky-cyrl,lo,lo-la,lv,lv-lv,lt,lt-lt,lb,lb-lu,mk,mk-mk,ms,ms-bn,ms-my,ml,ml-in,mt,mt-mt,mi,mi-latn,mi-nz,mr,mr-in,mn-cyrl,mn-mong,mn-mn,mn-phag,ne,ne-np,nb,nb-no,nn,nn-no,no,no-no,or,or-in,fa,fa-ir,pl,pl-pl,pt-br,pt,pt-pt,pa,pa-arab,pa-arab-pk,pa-deva,pa-in,quz,quz-bo,quz-ec,quz-pe,ro,ro-ro,ru ,ru-ru,gd-gb,gd-latn,sr-Latn,sr-latn-cs,sr,sr-latn-ba,sr-latn-me,sr-latn-rs,sr-cyrl,sr-cyrl-ba,sr-cyrl-cs,sr-cyrl-me,sr-cyrl-rs,nso,nso-za,tn,tn-bw,tn-za,sd-arab,sd-arab-pk,sd-deva,si,si-lk,sk,sk-sk,sl,sl-si,es,es-cl,es-co,es-es,es-mx,es-ar,es-bo,es-cr,es-do,es-ec,es-gt,es-hn,es-ni,es-pa,es-pe,es-pr,es-py,es-sv,es-us,es-uy,es-ve,es-019,es-419,sv,sv-se,sv-fi,tg-arab,tg-cyrl,tg-cyrl-tj,tg-latn,ta,ta-in,tt-arab,tt-cyrl,tt-latn,tt-ru,te,te-in,th,th-th,ti,ti-et,tr,tr-tr,tk-cyrl,tk-latn,tk-tm,tk-latn-tr,tk-cyrl-tr,uk,uk-ua,ur,ur-pk,ug-arab,ug-cn,ug-cyrl,ug-latn,uz,uz-cyrl,uz-latn,uz-latn-uz,vi,vi-vn,cy,cy-gb,wo,wo-sn,yo-latn,yo-ng";
        static string MS_STORE_CULTURES_CSV = "ar-SA,ar-AE,ar-BH,ar-DZ,ar-EG,ar-IQ,ar-JO,ar-KW,ar-LB,ar-LY,ar-MA,ar-OM,ar-QA,ar-SY,ar-TN,ar-YE,af-ZA,sq-AL,am-ET,hy-AM,as-IN,az-Arab,az-Arab-AZ,az-Cyrl,az-Cyrl-AZ,az-Latn,az-Latn-AZ,eu-ES,be-BY,bn-BD,bn-IN,bs-Cyrl,bs-Cyrl-BA,bs-Latn,bs-Latn-BA,bg-BG,ca-ES,ca-ES-VALENCIA,chr-Cher,chr-Cher-US,chr-Latn,zh-Hans,zh-CN,zh-Hans-CN,zh-SG,zh-Hans-SG,zh-Hant,zh-HK,zh-MO,zh-TW,zh-Hant-HK,zh-Hant-MO,zh-Hant-TW,hr-HR,hr-BA,cs-CZ,da-DK,prs-AF,prs-Arab,nl-NL,nl-BE,en-AU,en-CA,en-GB,en-IE,en-IN,en-NZ,en-SG,en-US,en-ZA,en-BZ,en-HK,en-ID,en-JM,en-KZ,en-MT,en-MY,en-PH,en-PK,en-TT,en-VN,en-ZW,en-053,en-021,en-029,en-011,en-018,en-014,et-EE,fil-Latn,fil-PH,fi-FI,fr-LU,fr-015,fr-CD,fr-CI,fr-CM,fr-HT,fr-MA,fr-MC,fr-ML,fr-RE,frc-Latn,frp-Latn,fr-155,fr-029,fr-021,fr-011,gl-ES,ka-GE,de-AT,de-CH,de-DE,de-LU,de-LI,el-GR,gu-IN,ha-Latn,ha-Latn-NG,he-IL,hi-IN,hu-HU,is-IS,ig-Latn,ig-NG,id-ID,iu-Cans,iu-Latn,iu-Latn-CA,ga-IE,xh-ZA,zu-ZA,it-IT,it-CH,ja-JP,kn-IN,kk-KZ,km-KH,quc-Latn,qut-GT,qut-Latn,rw-RW,sw-KE,kok-IN,ko-KR,ku-Arab,ku-Arab-IQ,ky-KG,ky-Cyrl,lo-LA,lv-LV,lt-LT,lb-LU,mk-MK,ms-BN,ms-MY,ml-IN,mt-MT,mi-Latn,mi-NZ,mr-IN,mn-Cyrl,mn-Mong,mn-MN,mn-Phag,ne-NP,nb-NO,nn-NO,no-NO,or-IN,fa-IR,pl-PL,pt-BR,pt-PT,pa-Arab,pa-Arab-PK,pa-Deva,pa-IN,quz-BO,quz-EC,quz-PE,ro-RO,ru-RU,gd-GB,gd-Latn,sr-Latn,sr-Latn-CS,sr-Latn-BA,sr-Latn-ME,sr-Latn-RS,sr-Cyrl,sr-Cyrl-BA,sr-Cyrl-CS,sr-Cyrl-ME,sr-Cyrl-RS,nso-ZA,tn-BW,tn-ZA,sd-Arab,sd-Arab-PK,sd-Deva,si-LK,sk-SK,sl-SI,es-CL,es-CO,es-ES,es-MX,es-AR,es-BO,es-CR,es-DO,es-EC,es-GT,es-HN,es-NI,es-PA,es-PE,es-PR,es-PY,es-SV,es-US,es-UY,es-VE,es-019,es-419,sv-SE,sv-FI,tg-Arab,tg-Cyrl,tg-Cyrl-TJ,tg-Latn,ta-IN,tt-Arab,tt-Cyrl,tt-Latn,tt-RU,te-IN,th-TH,ti-ET,tr-TR,tk-Cyrl,tk-Latn,tk-TM,tk-Latn-TR,tk-Cyrl-TR,uk-UA,ur-PK,ug-Arab,ug-CN,ug-Cyrl,ug-Latn,uz-Cyrl,uz-Latn,uz-Latn-UZ,vi-VN,cy-GB,wo-SN,yo-Latn,yo-NG";
        static List<string> MsStoreCultures =>
            MS_STORE_CULTURES_CSV.ToListFromCsv();

        static IEnumerable<string> WorkingCultures {
            get {
                var specificCultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => c.GetAncestors().Any())
                .OrderBy(c => c.DisplayName)
                .ToArray();

                var common_cultures = specificCultures.Where(x => MsStoreCultures.Any(y => y == x.Name)).ToArray();
                return common_cultures.Select(x => x.Name).OrderBy(x => x);
            }
        }

        const string VERSION_PHRASE = "Im the big T pot check me out";
        static string VERSION => "1.0.7.0";


        static MpLedgerizerFlags LEDGERIZER_FLAGS =
            MpLedgerizerFlags.GEN_ADDON_LISTING |
            MpLedgerizerFlags.GEN_PROD_LISTING |
            MpLedgerizerFlags.DO_LOCAL_PACKAGING |
            MpLedgerizerFlags.DO_LOCAL_INDEX
            | MpLedgerizerFlags.MOVE_CORE_TO_DAT
            //| MpLedgerizerFlags.DO_LOCAL_VERSIONS
            //| MpLedgerizerFlags.LOCALIZE_MANIFESTS
            ;

        static bool DO_LOCAL_PACKAGING = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_LOCAL_PACKAGING);

        static bool DO_REMOTE_PACKAGING = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_REMOTE_PACKAGING);
        static bool FORCE_REPLACE_REMOTE_TAG = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.FORCE_REPLACE_REMOTE_TAG);

        static bool DO_LOCAL_VERSIONS = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_LOCAL_VERSIONS);
        static bool DO_REMOTE_VERSIONS = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_REMOTE_VERSIONS);

        static bool DO_LOCAL_INDEX = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_LOCAL_INDEX);
        static bool DO_REMOTE_INDEX = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.DO_REMOTE_INDEX);

        static bool MOVE_CORE_TO_DAT = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.MOVE_CORE_TO_DAT);

        static bool LOCALIZE_MANIFESTS = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.LOCALIZE_MANIFESTS);

        static bool GEN_ADDON_LISTING = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.GEN_ADDON_LISTING);
        static bool GEN_PROD_LISTING = LEDGERIZER_FLAGS.HasFlag(MpLedgerizerFlags.GEN_PROD_LISTING);

        const string BUILD_CONFIG =
#if DEBUG
            "Debug";
#else
            "Release";
#endif
        const string BUILD_OS =
#if WINDOWS
            "WINDOWS";
#else
            "";
#endif
        const string README_URL_FORMAT = @"https://raw.githubusercontent.com/monkeypaste/{0}/master/README.md";
        const string PROJ_URL_FORMAT = @"https://github.com/monkeypaste/{0}";
        const string ICON_URL_FORMAT = @"https://raw.githubusercontent.com/monkeypaste/{0}/master/icon.png";
        const string PUBLIC_PACKAGE_URL_FORMAT = @"https://github.com/monkeypaste/{0}/releases/download/{1}/{1}.zip";

        const string PRIVATE_PACKAGE_URL_FORMAT = @"https://www.monkeypaste.com/dat/{0}/{1}.zip";

        static string[] PluginNames => [
            "ChatGpt",
            "ComputerVision",
            "CoreAnnotator",
            "CoreOleHandler",
            "FileConverter",
            "GoogleLiteTextTranslator",
            "ImageAnnotator",
            //"MinimalExample",
            "QrCoder",
            "TextToSpeech",
            "TextTranslator",
            "WebSearch"
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

        static List<string> CulturesFound { get; set; } = [];
        static void Main(string[] args) {
            Console.WriteLine("Press any key to ledgerize!");
            Console.ReadKey();
            Console.WriteLine("Starting...");

            ProcessAll();

            MpConsole.WriteLine("Done.. press key to finish", true);
            Console.ReadLine();
        }
        static void ProcessAll() {
            if (GEN_ADDON_LISTING) {
                GenAddOnListings();
            }
            if (GEN_PROD_LISTING) {
                GenProdListing();
            }
            if (LOCALIZE_MANIFESTS) {
                LocalizeManifests();
            }
            if (DO_LOCAL_PACKAGING) {
                PublishLocal();
            }
            if (DO_REMOTE_PACKAGING) {
                PublishRemote();
            }
            if (DO_LOCAL_VERSIONS) {
                UpdateVersions(false);
            }
            if (DO_REMOTE_VERSIONS) {
                UpdateVersions(true);
            }
            if (DO_LOCAL_INDEX) {
                CreateIndex(false);
            }
            if (DO_REMOTE_INDEX) {
                CreateIndex(true);
            }
            if (MOVE_CORE_TO_DAT) {
                MoveCoreToDat();
            }
        }

        #region Listing
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
                (7,"StoreShortDescription"),
                (8,"StoreDevStudio"),
                (11,"ssSrc1"),
                (12,"ssSrc2"),
                (13,"ssSrc3"),
                (14,"ssSrc4"),
                (15,"ssSrc5"),
                (41,"ssCaption1"),
                (42,"ssCaption2"),
                (43,"ssCaption3"),
                (44,"ssCaption4"),
                (45,"ssCaption5"),
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
            ];

            string[] rel_path_keys = [
                "ssSrc1",
                "ssSrc2",
                "ssSrc3",
                "ssSrc4",
                "ssSrc5",
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

            string prod_listing_dir_name = $"listing_product_{VERSION}";
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
            var ccl = MpLocalizationHelpers.FindCulturesInDirectory(GetListingDir(), "ListingStrings").Select(x => x.Name);
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
                        string row_key = field_kvp.Item2;
                        if (listing_lookup.TryGetValue(row_key, out var listing_kvp)) {
                            // use translated text
                            new_cell_val = listing_kvp.value;
                        } else if (inv_listing_lookup.TryGetValue(row_key, out var inv_listing_kvp)) {
                            // use inv text
                            new_cell_val = inv_listing_kvp.value;
                        } else {
                            // huh?!?
                            throw new KeyNotFoundException(row_key);
                        }


                        if (cc != "en-US") {
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
            string listing_dir_name = $"listing_{plan_name}_{cycle_type}_{VERSION}";

            var ccl = MpLocalizationHelpers.FindCulturesInDirectory(GetListingDir(), "ListingStrings").Select(x => x.Name);
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
        static void GenEmptyLocalizedListings() {
            string listing_baseline_path =
                Path.Combine(
                    GetListingDir(),
                    "ListingStrings.resx");
            var listing_lookup = MpResxTools.ReadResxFromPath(listing_baseline_path);
            var empty_listing_lookup = listing_lookup.ToDictionary(x => x.Key, x => (string.Empty, x.Value.comment));

            foreach (var cc in WorkingCultures) {
                string listing_fn = $"ListingStrings.{cc}.resx";
                string listing_path = Path.Combine(
                    GetListingDir(),
                    listing_fn);
                if (listing_path.IsFile()) {
                    continue;
                }
                if (cc == "en-US") {
                    MpResxTools.WriteResxToPath(listing_path, listing_lookup);
                    continue;
                }
                MpResxTools.WriteResxToPath(listing_path, empty_listing_lookup);
            }
        }

        static string GetListingDir() {

            string listing_dir = Path.Combine(
                MpCommonHelpers.GetSolutionDir(),
                "MonkeyPaste.Avalonia",
                "Resources",
                "Localization",
                "Listings");
            return listing_dir;
        }
        #endregion

        #region Move Core 
        static void MoveCoreToDat() {
            string root_pack_dir = MpLedgerConstants.PLUGIN_PACKAGES_DIR;
            MpConsole.WriteLine($"Moving core plugins to dat STARTED", true);

            foreach (string core_plugin_name in CorePlugins) {
                string core_plugin_zip_path = Path.Combine(root_pack_dir, $"{core_plugin_name}.zip");
                if (!core_plugin_zip_path.IsFile()) {
                    MpConsole.WriteLine($"Error! No package found for '{core_plugin_name}' at '{core_plugin_zip_path}'");
                    continue;
                }
                if (ReadPluginManifestFromProjDir(core_plugin_name) is not { } core_mf) {
                    MpConsole.WriteLine($"Error could not find core manifest for '{core_plugin_name}'");
                    continue;
                }

                string target_dat_path = Path.Combine(MpCommonHelpers.GetTargetDatDir(), $"{core_mf.guid}.zip");
                if (!MpCommonHelpers.GetTargetDatDir().IsDirectory()) {
                    MpFileIo.CreateDirectory(MpCommonHelpers.GetTargetDatDir());
                }
                MpFileIo.CopyFileOrDirectory(core_plugin_zip_path, target_dat_path, forceOverwrite: true);
                MpConsole.WriteLine(target_dat_path);
            }
            MpConsole.WriteLine($"Moving core plugins to dat DONE", false, true);
        }
        #endregion

        #region Localizing
        static void LocalizeManifests() {
            MpConsole.WriteLine("Localize Manifest...STARTED", true);
            foreach (string plugin_name in PluginNames) {
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
                    file_name_prefix: "Resources");

            foreach (string lang_code in lang_codes.Where(x => !x.IsInvariant()).Select(x => x.Name)) {
                Localizer.Program.LocalizeManifest(invariant_resource_path, inv_mf_path, lang_code, plugin_res_dir);
            }
            MpConsole.WriteLine("", stampless: true);
        }

        #endregion

        #region Index
        static void CreateIndex(bool is_remote) {
            MpConsole.WriteLine($"Creating {(is_remote ? "REMOTE" : "LOCAL")} Cultures...", true);
            string inv_code = "";

            List<string> found_cultures = [];
            // find all distinct cultures
            foreach (var plugin_name in PluginNames) {
                string plugin_cultures_dir = GetPluginResourcesDir(plugin_name);
                if (plugin_cultures_dir == null) {
                    // no resources dir
                    continue;
                }
                if (MpLocalizationHelpers.FindCulturesInDirectory(plugin_cultures_dir, file_name_prefix: ManifestPrefix, inv_code: inv_code) is { } cil) {
                    var to_add = cil.Where(x => !found_cultures.Contains(x.Name) && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name);
                    found_cultures.AddRange(to_add);
                }
            }

            // recreate invariant ledger
            var ledger = GetInvLedger(is_remote);

            // create localized ledger for each distinct culture in /Cultures dir
            foreach (string cc in found_cultures) {
                var culture_manifests = new List<MpManifestFormat>();
                foreach (string plugin_name in PluginNames) {
                    // find closest culture for each plugin and create that manifest
                    var culture_manifest = GetLocalizedManifest(plugin_name, cc, inv_code);
                    if (ledger.manifests.FirstOrDefault(x => x.guid == culture_manifest.guid) is { } ledger_manifest) {
                        // use inv ledger packageUrl
                        culture_manifest.publishedAppVersion = VERSION;
                        culture_manifest.packageUrl = ledger_manifest.packageUrl;
                    }

                    culture_manifests.Add(culture_manifest);
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

        static MpManifestFormat GetLocalizedManifest(string plugin_name, string culture, string inv_code) {
            string plugin_proj_cultures_dir = GetPluginResourcesDir(plugin_name);
            string localized_manifest_path = Path.Combine(GetPluginProjDir(plugin_name), ManifestFileName);
            if (plugin_proj_cultures_dir != null) {
                string resolved_cultre = MpLocalizationHelpers.FindClosestCultureCode(
                culture, plugin_proj_cultures_dir,
                file_name_prefix: ManifestPrefix,
                inv_code: inv_code);
                if (!string.IsNullOrEmpty(resolved_cultre)) {
                    localized_manifest_path = Path.Combine(
                    plugin_proj_cultures_dir,
                    $"{ManifestPrefix}.{resolved_cultre}.{ManifestExt}").Replace("..", ".");
                    MpDebug.Assert(localized_manifest_path.IsFile(), $"ERror can't find manifest {localized_manifest_path}");
                }
            }
            return MpFileIo.ReadTextFromFile(localized_manifest_path).DeserializeObject<MpManifestFormat>();
        }


        #endregion

        #region Packaging
        static void WriteLedger(MpManifestLedger ledger, bool is_remote) {
            // filter any ledger ignored plugins (minimal example)
            //var output_ledger = new MpManifestLedger() {
            //    manifests =
            //}
            string output_path = is_remote ?
                MpLedgerConstants.REMOTE_INV_LEDGER_PATH :
                MpLedgerConstants.LOCAL_INV_LEDGER_PATH;

            MpFileIo.WriteTextToFile(
                    output_path,
                    ledger.SerializeObject(true).ToPrettyPrintJson());
            MpConsole.WriteLine($"{(is_remote ? "REMOTE" : "LOCAL")} ledger written to: {output_path}", true);
        }
        static string PackPlugin(string proj_dir, string guid) {
            // returns zip uri to use for local packageUrl
            string root_pack_dir = MpLedgerConstants.PLUGIN_PACKAGES_DIR;
            string plugin_name = Path.GetFileName(proj_dir);
            string output_path = Path.Combine(root_pack_dir, $"{plugin_name}.zip");


            if (!root_pack_dir.IsDirectory()) {
                // create packages dir if first pack
                MpFileIo.CreateDirectory(root_pack_dir);
            }
            string publish_dir = Path.Combine(root_pack_dir, plugin_name);

            // delete build stuff
            MpFileIo.DeleteDirectory(Path.Combine(proj_dir, "bin"));
            MpFileIo.DeleteDirectory(Path.Combine(proj_dir, "obj"));

            // perform publish and output to ledger proj/packages_* dir
            string args = CorePlugins.Contains(plugin_name) ?
                $"msbuild /p:OutDir={publish_dir} -target:Publish /property:Configuration={BUILD_CONFIG} /property:DefineConstants=AUX%3B{BUILD_OS} -restore" :
                $"publish --configuration {BUILD_CONFIG} --output {publish_dir}";

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
            // zip publish output 
            ZipFile.CreateFromDirectory(publish_dir, output_path, CompressionLevel.Fastest, true);

            // get plugin install dir
            string plugin_install_dir =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
#if DEBUG
                    "MonkeyPaste_DEBUG",
#else
                    "MonkeyPaste",
#endif
                    "Plugins",
                    guid);
            string install_update_suffix = string.Empty;
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
            // cleanup published output
            MpFileIo.DeleteDirectory(publish_dir);
            MpConsole.WriteLine($"{plugin_name} local DONE" + install_update_suffix);

            // return zip uri to use for local packageUrl
            return output_path.ToFileSystemUriFromPath();
        }
        static void PublishLocal() {
            MpFileIo.DeleteDirectory(MpLedgerConstants.PLUGIN_PACKAGES_DIR);

            MpManifestLedger ledger = new MpManifestLedger();
            foreach (var plugin_name in PluginNames) {
                string plugin_proj_dir = GetPluginProjDir(plugin_name);
                string plugin_manifest_path = Path.Combine(
                        plugin_proj_dir,
                        ManifestFileName);

                string plugin_manifest_text = MpFileIo.ReadTextFromFile(plugin_manifest_path);
                MpManifestFormat plugin_manifest = plugin_manifest_text.DeserializeObject<MpManifestFormat>();

                string local_package_uri = PackPlugin(plugin_proj_dir, plugin_manifest.guid);
                if (local_package_uri == null) {
                    continue;
                }
                // set pub app version for all plugins
                plugin_manifest.publishedAppVersion = VERSION;
                // set package uri to output of local packaging
                plugin_manifest.packageUrl = local_package_uri;
                ledger.manifests.Add(plugin_manifest);
            }
            // write ledger-local.js
            WriteLedger(ledger, false);
        }
        static void PublishRemote() {
            // returns the complete remote ledger

            var ledger = GetInvLedger(true);
            foreach (var manifest in ledger.manifests) {
                string proj_dir = Path.Combine(
                                MpCommonHelpers.GetSolutionDir(),
                                "Plugins",
                                Path.GetFileNameWithoutExtension(manifest.packageUrl.ToPathFromUri()));
                manifest.packageUrl = PushReleaseToRemote(manifest, proj_dir);
                if (manifest.packageUrl == null) {
                    // didn't upload
                    continue;
                }
                string plugin_name = Path.GetFileName(proj_dir);
                manifest.readmeUrl = string.Format(README_URL_FORMAT, plugin_name);
                manifest.projectUrl = string.Format(PROJ_URL_FORMAT, plugin_name);
                manifest.iconUri = string.Format(ICON_URL_FORMAT, plugin_name);
            }

            WriteLedger(ledger, true);
        }
        static string PushReleaseToRemote(MpManifestFormat manifest, string proj_dir, string initial_failed_ver = null) {
            string plugin_name = Path.GetFileName(proj_dir);
            string local_package_uri = manifest.packageUrl;
            string version = manifest.version;
            // see this about gh release https://cli.github.com/manual/gh_release_create
            string source_package_path = local_package_uri.ToPathFromUri();
            string target_tag_name = $"v{version}";
            string target_package_file_name = $"{target_tag_name}.zip";
            string target_package_path = Path.Combine(proj_dir, target_package_file_name);

            if (CorePlugins.Contains(plugin_name)) {
                // TODO would be nice to be able to ssh onto server and push core plugins
                // but for now must be handled manually
                return GetRemotePackageUrl(plugin_name, manifest.guid, target_tag_name);
            }

            MpFileIo.CopyFileOrDirectory(source_package_path, target_package_path, forceOverwrite: true);
            (int exit_code, string proc_output) = RunProcess(
                file: "gh.exe",
                dir: proj_dir,
                args: $"release create {target_tag_name} --latest --generate-notes {target_package_file_name}");

            MpFileIo.DeleteFile(target_package_path);

            if (exit_code == 1) {
                // version exist
                if (FORCE_REPLACE_REMOTE_TAG) {
                    // delete version, call again
                    if (initial_failed_ver != null) {
                        // should only occur once 
                        MpConsole.WriteLine($"Uncaught error after delete for '{proj_dir}' skipping upload");
                        return null;
                    }
                    (int del_exit_code, string del_proc_output) =
                        RunProcess(
                            file: "gh.exe",
                            dir: proj_dir,
                            args: $"release delete {target_tag_name} -yes --cleanup-tag");
                    if (exit_code != 0) {
                        MpConsole.WriteLine($"Error delete failed exit code {del_exit_code}");
                        return null;
                    }
                } else {
                    // increment, call again
                    if (version.SplitNoEmpty(".") is not { } verParts ||
                        !int.TryParse(verParts.Last(), out int minor_rev)) {
                        MpConsole.WriteLine($"Error bad version for plugin at '{proj_dir}'");
                        return null;
                    }
                    manifest.version = $"{verParts[0]}.{verParts[1]}.{minor_rev + 1}";
                }
                // if first fail use failed version
                var new_ver_result = PushReleaseToRemote(manifest, proj_dir, initial_failed_ver ?? version);
                return new_ver_result;

            } else if (exit_code == 0 && initial_failed_ver != null) {
                // new rev works, update local manifest to match

                // NOTE avoiding full re-write since manifest can be subclass, just replacing version...
                string manifest_json = MpFileIo.ReadTextFromFile(Path.Combine(proj_dir, ManifestFileName));
                string old_ver_json = $"\"version\": \"{initial_failed_ver}\"";
                string new_ver_json = $"\"version\": \"{version}\"";
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
                return null;
            }

            string github_release_uri = string.Format(PUBLIC_PACKAGE_URL_FORMAT, plugin_name, target_tag_name);
            MpConsole.WriteLine($"{plugin_name} remote DONE");
            return github_release_uri;
        }
        static string GetRemotePackageUrl(string plugin_name, string plugin_guid, string target_tag_name) {
            if (CorePlugins.Contains(plugin_name)) {
                return string.Format(PRIVATE_PACKAGE_URL_FORMAT, plugin_guid, target_tag_name);
            }
            return string.Format(PUBLIC_PACKAGE_URL_FORMAT, plugin_name, target_tag_name);
        }
        #endregion

        #region Version

        static void UpdateVersions(bool is_remote) {
            MpManifestLedger ledger = GetInvLedger(is_remote);
            bool is_done = false;

            _ = Task.Run(async () => {
                foreach (var mf in ledger.manifests) {
                    var req_args = new Dictionary<string, string>() {
                        {"plugin_guid", mf.guid },
                        {"version", mf.version},
                        {"is_install", "0" },
                        {"add_phrase", "Im the big T pot check me out" }
                    };
                    string url = is_remote ?
                        $"{MpServerConstants.REMOTE_SERVER_URL}/plugins/plugin-info-check.php" :
                        $"{MpServerConstants.LOCAL_SERVER_URL}/plugins/plugin-info-check.php";

                    var resp = await MpHttpRequester.SubmitPostDataToUrlAsync(url, req_args);
                    bool success = MpHttpRequester.ProcessServerResponse(resp, out var resp_args);
                    MpConsole.WriteLine($"{mf} {success.ToTestResultLabel()} info check resp: {resp}");
                }
                is_done = true;
            });

            while (!is_done) {
                Thread.Sleep(100);
            }

        }
        #endregion

        #region Helpers
        static MpManifestLedger GetInvLedger(bool is_remote) {
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
                        MpCommonHelpers.GetSolutionDir(),
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
