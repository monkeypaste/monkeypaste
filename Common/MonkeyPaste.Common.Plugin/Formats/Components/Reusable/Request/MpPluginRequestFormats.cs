using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Plugin {

    public class MpPluginRequestFormatBase : MpJsonObject {
        public static MpPluginRequestFormatBase Parse(string json) {
            var req_lookup = MpJsonObject.DeserializeObject<Dictionary<string, object>>(json);
            if (req_lookup != null &&
                req_lookup.TryGetValue("items", out var itemsObj) && itemsObj is JArray items_jarray) {
                Dictionary<object, string> param_lookup = new Dictionary<object, string>();
                foreach (var kvp_jtoken in items_jarray) {
                    if (kvp_jtoken.SelectToken("paramId", false) is JToken param_token &&
                        kvp_jtoken.SelectToken("value", false) is JToken val_token) {

                        param_lookup.Add(param_token.Value<string>(), val_token.Value<string>());
                    }
                }
                return new MpPluginRequestFormatBase() {
                    items = param_lookup.Select(x=>new MpParameterRequestItemFormat(x.Key,x.Value)).ToList()
                };
            }
            return null;
        }

        public List<MpParameterRequestItemFormat> items { get; set; }

        [JsonIgnore]
        public Dictionary<object, string> ParamLookup =>
            items == null ?
                new Dictionary<object, string>() :
                items.ToDictionary(
                    x => (object)x.paramId,
                    x => x.value);
    }
}
