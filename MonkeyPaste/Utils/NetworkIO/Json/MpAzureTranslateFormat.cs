﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonkeyPaste {
    // from https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate#response-body

    //get languages
    public class MpAzureTranslatorLanguageFormat : MpJsonMessage {
        [JsonProperty("name")]
        public string LanguageName { get; set; } = string.Empty;

        [JsonProperty("nativeName")]
        public string NativeName { get; set; } = string.Empty;

        [JsonProperty("dir")]
        public string Directionality { get; set; } = string.Empty;
    }

    public class MpAzureTranslatableLanguagesRequestFormat : MpJsonMessage {
        [JsonProperty("translation")]
        public Dictionary<string, MpAzureTranslatorLanguageFormat> Translation { get; set; } = new Dictionary<string, MpAzureTranslatorLanguageFormat>();

    }

    public class MpAzureTranslateRequestFormat : MpJsonMessage {
        [JsonProperty("from")]
        public string FromCode { get; set; } = string.Empty;

        [JsonProperty("to")]
        public string ToCode { get; set; } = string.Empty;
    }
    // translate

    public class MpAzureTranslation : MpJsonMessage {
        [JsonProperty("to")]
        public string To { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class MpAzureDetectedLanguageFormat : MpJsonMessage {
        [JsonProperty("language")]
        public string Language { get; set; } = string.Empty;

        [JsonProperty("score")]
        public double Score { get; set; } = 0;
    }

    public class MpAzureTranslateResultFormat : MpJsonMessage  {
        [JsonProperty("translations")]
        public List<MpAzureTranslation> Translations { get; set; }

        [JsonProperty("detectedLanguage")]
        public MpAzureDetectedLanguageFormat DetectedLanguage { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class MpAzureDetectedLanguage {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }
    }

    public class Root {
        [JsonProperty("detectedLanguage")]
        public MpAzureDetectedLanguage DetectedLanguage { get; set; }

        [JsonProperty("translations")]
        public List<MpAzureTranslation> Translations { get; set; }
    }
}