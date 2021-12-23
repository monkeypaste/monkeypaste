using Newtonsoft.Json;

namespace MonkeyPaste {
    public class MpCurrency {
        [JsonProperty("currencyName")]
        public string CurrencyName { get; set; }

        [JsonProperty("currencySymbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
