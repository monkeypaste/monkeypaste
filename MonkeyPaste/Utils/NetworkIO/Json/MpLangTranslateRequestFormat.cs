using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonkeyPaste {
    // from https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate#response-body

    //get languages
    public class MpTranslatorLanguageFormat : MpJsonMessage {
        [JsonProperty("name")]
        public string LanguageName { get; set; } = string.Empty;

        [JsonProperty("nativeName")]
        public string NativeName { get; set; } = string.Empty;

        [JsonProperty("dir")]
        public string Directionality { get; set; } = string.Empty;
    }

    public class MpTranslatableLanguagesRequestFormat : MpJsonMessage {
        [JsonProperty("translation")]
        public Dictionary<string, MpTranslatorLanguageFormat> Translation { get; set; } = new Dictionary<string, MpTranslatorLanguageFormat>();

    }

    public class MpLangTranslateRequestFormat : MpJsonMessage {
        [JsonProperty("from")]
        public string FromCode { get; set; } = string.Empty;

        [JsonProperty("to")]
        public string ToCode { get; set; } = string.Empty;
    }

    public class MpLangTranslation : MpJsonMessage {
        [JsonProperty("to")]
        public string LangCode { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class MpDetectedLanguageFormat : MpJsonMessage {
        [JsonProperty("language")]
        public string Language { get; set; } = string.Empty;

        [JsonProperty("score")]
        public float Score { get; set; } = 0.0f;
    }

    public class MpLangTranslateResultFormat : MpJsonMessage  {
        [JsonProperty("translations")]
        public MpLangTranslation Translations { get; set; }

        [JsonProperty("detectedLanguage")]
        public MpDetectedLanguageFormat DetectedLanguage { get; set; }
    }

    public class MpLangTranslateResponseFormat : MpJsonMessage {
        public List<MpLangTranslateResultFormat> Results { get; set; } = new List<MpLangTranslateResultFormat>();
    }
}
