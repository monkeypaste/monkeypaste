using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MonkeyPaste {
    public class MpCurrentCultureViewModel : MpViewModelBase {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpCurrentCultureViewModel _instance;
        public static MpCurrentCultureViewModel Instance => _instance ?? (_instance = new MpCurrentCultureViewModel());
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        private Dictionary<string, string> _translatorLangLookup;
        public Dictionary<string, string> TranslatorCodeLookup {
            get {
                if (_translatorLangLookup == null) {
                    // NOTE language kvp is backwards flipping so code is key
                    var temp = new Dictionary<string, string>() {
                        {
                            "Afrikaans - Afrikaans",
                            "af"
                        },
                        {
                            "Amharic - አማርኛ",
                            "am"
                        },
                        {
                            "Arabic - العربية",
                            "ar"
                        },
                        {
                            "Assamese - অসমীয়া",
                            "as"
                        },
                        {
                            "Azerbaijani - Azərbaycan",
                            "az"
                        },
                        {
                            "Bashkir - Bashkir",
                            "ba"
                        },
                        {
                            "Bulgarian - Български",
                            "bg"
                        },
                        {
                            "Bangla - বাংলা",
                            "bn"
                        },
                        {
                            "Tibetan - བོད་སྐད་",
                            "bo"
                        },
                        {
                            "Bosnian - Bosnian",
                            "bs"
                        },
                        {
                            "Catalan - Català",
                            "ca"
                        },
                        {
                            "Czech - Čeština",
                            "cs"
                        },
                        {
                            "Welsh - Cymraeg",
                            "cy"
                        },
                        {
                            "Danish - Dansk",
                            "da"
                        },
                        {
                            "German - Deutsch",
                            "de"
                        },
                        {
                            "Divehi - ދިވެހިބަސް",
                            "dv"
                        },
                        {
                            "Greek - Ελληνικά",
                            "el"
                        },
                        {
                            "English - English",
                            "en"
                        },
                        {
                            "Spanish - Español",
                            "es"
                        },
                        {
                            "Estonian - Eesti",
                            "et"
                        },
                        {
                            "Persian - فارسی",
                            "fa"
                        },
                        {
                            "Finnish - Suomi",
                            "fi"
                        },
                        {
                            "Filipino - Filipino",
                            "fil"
                        },
                        {
                            "Fijian - Na Vosa Vakaviti",
                            "fj"
                        },
                        {
                            "French - Français",
                            "fr"
                        },
                        {
                            "French(Canada) - Français (Canada)",
                            "fr-CA"
                        },
                        {
                            "Irish - Gaeilge",
                            "ga"
                        },
                        {
                            "Gujarati - ગુજરાતી",
                            "gu"
                        },
                        {
                            "Hebrew - עברית",
                            "he"
                        },
                        {
                            "Hindi - हिन्दी",
                            "hi"
                        },
                        {
                            "Croatian - Hrvatski",
                            "hr"
                        },
                        {
                            "HaitianCreole - Haitian Creole",
                            "ht"
                        },
                        {
                            "Hungarian - Magyar",
                            "hu"
                        },
                        {
                            "Armenian - Հայերեն",
                            "hy"
                        },
                        {
                            "Indonesian - Indonesia",
                            "id"
                        },
                        {
                            "Inuinnaqtun - Inuinnaqtun",
                            "ikt"
                        },
                        {
                            "Icelandic - Íslenska",
                            "is"
                        },
                        {
                            "Italian - Italiano",
                            "it"
                        },
                        {
                            "Inuktitut - ᐃᓄᒃᑎᑐᑦ",
                            "iu"
                        },
                        {
                            "Inuktitut(Latin) - Inuktitut (Latin)",
                            "iu-Latn"
                        },
                        {
                            "Japanese - 日本語",
                            "ja"
                        },
                        {
                            "Georgian - ქართული",
                            "ka"
                        },
                        {
                            "Kazakh - Қазақ Тілі",
                            "kk"
                        },
                        {
                            "Khmer - ខ្មែរ",
                            "km"
                        },
                        {
                            "Kurdish(Northern) - Kurdî (Bakur)",
                            "kmr"
                        },
                        {
                            "Kannada - ಕನ್ನಡ",
                            "kn"
                        },
                        {
                            "Korean - 한국어",
                            "ko"
                        },
                        {
                            "Kurdish(Central) - Kurdî (Navîn)",
                            "ku"
                        },
                        {
                            "Kyrgyz - Kyrgyz",
                            "ky"
                        },
                        {
                            "Lao - ລາວ",
                            "lo"
                        },
                        {
                            "Lithuanian - Lietuvių",
                            "lt"
                        },
                        {
                            "Latvian - Latviešu",
                            "lv"
                        },
                        {
                            "Chinese(Literary) - 中文 (文言文)",
                            "lzh"
                        },
                        {
                            "Malagasy - Malagasy",
                            "mg"
                        },
                        {
                            "Māori - Te Reo Māori",
                            "mi"
                        },
                        {
                            "Macedonian - Македонски",
                            "mk"
                        },
                        {
                            "Malayalam - മലയാളം",
                            "ml"
                        },
                        {
                            "Mongolian(Cyrillic) - Mongolian (Cyrillic)",
                            "mn-Cyrl"
                        },
                        {
                            "Mongolian(Traditional) - ᠮᠣᠩᠭᠣᠯ ᠬᠡᠯᠡ",
                            "mn-Mong"
                        },
                        {
                            "Marathi - मराठी",
                            "mr"
                        },
                        {
                            "Malay - Melayu",
                            "ms"
                        },
                        {
                            "Maltese - Malti",
                            "mt"
                        },
                        {
                            "HmongDaw - Hmong Daw",
                            "mww"
                        },
                        {
                            "Myanmar(Burmese) - မြန်မာ",
                            "my"
                        },
                        {
                            "Norwegian - Norsk Bokmål",
                            "nb"
                        },
                        {
                            "Nepali - नेपाली",
                            "ne"
                        },
                        {
                            "Dutch - Nederlands",
                            "nl"
                        },
                        {
                            "Odia - ଓଡ଼ିଆ",
                            "or"
                        },
                        {
                            "QuerétaroOtomi - Hñähñu",
                            "otq"
                        },
                        {
                            "Punjabi - ਪੰਜਾਬੀ",
                            "pa"
                        },
                        {
                            "Polish - Polski",
                            "pl"
                        },
                        {
                            "Dari - دری",
                            "prs"
                        },
                        {
                            "Pashto - پښتو",
                            "ps"
                        },
                        {
                            "Portuguese(Brazil) - Português (Brasil)",
                            "pt"
                        },
                        {
                            "Portuguese(Portugal) - Português (Portugal)",
                            "pt-PT"
                        },
                        {
                            "Romanian - Română",
                            "ro"
                        },
                        {
                            "Russian - Русский",
                            "ru"
                        },
                        {
                            "Slovak - Slovenčina",
                            "sk"
                        },
                        {
                            "Slovenian - Slovenščina",
                            "sl"
                        },
                        {
                            "Samoan - Gagana Sāmoa",
                            "sm"
                        },
                        {
                            "Albanian - Shqip",
                            "sq"
                        },
                        {
                            "Serbian(Cyrillic) - Српски (ћирилица)",
                            "sr-Cyrl"
                        },
                        {
                            "Serbian(Latin) - Srpski (latinica)",
                            "sr-Latn"
                        },
                        {
                            "Swedish - Svenska",
                            "sv"
                        },
                        {
                            "Swahili - Kiswahili",
                            "sw"
                        },
                        {
                            "Tamil - தமிழ்",
                            "ta"
                        },
                        {
                            "Telugu - తెలుగు",
                            "te"
                        },
                        {
                            "Thai - ไทย",
                            "th"
                        },
                        {
                            "Tigrinya - ትግር",
                            "ti"
                        },
                        {
                            "Turkmen - Türkmen Dili",
                            "tk"
                        },
                        {
                            "Klingon(Latin) - Klingon (Latin)",
                            "tlh-Latn"
                        },
                        {
                            "Klingon(pIqaD) - Klingon (pIqaD)",
                            "tlh-Piqd"
                        },
                        {
                            "Tongan - Lea Fakatonga",
                            "to"
                        },
                        {
                            "Turkish - Türkçe",
                            "tr"
                        },
                        {
                            "Tatar - Татар",
                            "tt"
                        },
                        {
                            "Tahitian - Reo Tahiti",
                            "ty"
                        },
                        {
                            "Uyghur - ئۇيغۇرچە",
                            "ug"
                        },
                        {
                            "Ukrainian - Українська",
                            "uk"
                        },
                        {
                            "Urdu - اردو",
                            "ur"
                        },
                        {
                            "Uzbek(Latin) - Uzbek (Latin)",
                            "uz"
                        },
                        {
                            "Vietnamese - Tiếng Việt",
                            "vi"
                        },
                        {
                            "YucatecMaya - Yucatec Maya",
                            "yua"
                        },
                        {
                            "Cantonese(Traditional) - 粵語 (繁體)",
                            "yue"
                        },
                        {
                            "ChineseSimplified - 中文 (简体)",
                            "zh-Hans"
                        },
                        {
                            "ChineseTraditional - 繁體中文 (繁體)",
                            "zh-Hant"
                        }
                    };

                    _translatorLangLookup = temp.ToDictionary(
                        x => x.Value,
                        x => x.Key);
                }
                return _translatorLangLookup;
            }
        }

        private Dictionary<string, string> _availableCultureLookup;
        public Dictionary<string, string> AvailableCultureLookup {
            get {
                if (_availableCultureLookup == null) {
                    // NOTE language kvp is backwards flipping so code is key
                    _availableCultureLookup =
                            CultureInfo.GetCultures(CultureTypes.AllCultures)
                                    .Where(x => ConvertCultureToLanguageCode(x.Name, x.DisplayName) != null)
                                    .ToDictionary(
                                        x => x.Name,
                                        x => x.DisplayName
                                    );
                }
                return _availableCultureLookup;
            }
        }
        #endregion

        #region State

        #endregion

        #endregion

        #region Constructors
        public MpCurrentCultureViewModel() {

        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private string ConvertCultureToLanguageCode(string cultureCode, string displayName) {
            cultureCode = cultureCode.ToLower();
            var exact_match = TranslatorCodeLookup.Where(x => cultureCode.Equals(x.Key.ToLower()));
            if (exact_match.Any()) {
                return exact_match.First().Key;
            }
            var matches = TranslatorCodeLookup.Where(x => cultureCode.StartsWith(x.Key.ToLower()));
            int count = matches.Count();
            if (count == 0) {
                return null;
            }
            if (count == 1) {
                return matches.First().Key;
            }
            MpConsole.WriteLine($"Multiple possible translator codes detected for '{displayName}' '{cultureCode}': ");
            matches.ForEach(x => MpConsole.WriteLine($"'{x.Value}' '{x.Key}'"));

            // crudely using string comparision to determine closest language
            var best_match =
                matches.Aggregate((a, b) =>
                    a.Value.ComputeLevenshteinDistance(displayName) < b.Value.ComputeLevenshteinDistance(displayName) ? a : b);

            MpConsole.WriteLine($"Best match: '{best_match.Value}' '{best_match.Key}'");
            return best_match.Key;

        }
        #endregion

        #region Commands
        public ICommand SetLanguageCommand => new MpCommand<object>(
            (args) => {
                // NOTE I don't think this will be used in release and should be deployed w/ 
                // pre-defined UserUiStrings
                string newCultureCode = args.ToString();

                string trans_code = ConvertCultureToLanguageCode(newCultureCode, CultureInfo.GetCultureInfo(newCultureCode).DisplayName);


                //foreach (SettingsProperty dsp in Properties.DefaultUiStrings.Default.Properties) {
                //    foreach (SettingsProperty usp in Properties.UserUiStrings.Default.Properties) {
                //        if (dsp.Name == usp.Name) {
                //            //usp.DefaultValue = await MpLanguageTranslator.TranslateAsync((string)dsp.DefaultValue, newLanguage,"");
                //            MpConsole.WriteLine("Default: " + (string)dsp.DefaultValue + "New: " + (string)usp.DefaultValue);
                //        }
                //    }
                //}

                //Properties.UserUiStrings.Default.Save();
            }, (args) => {
                return args is string;
            });
        #endregion
    }
}
