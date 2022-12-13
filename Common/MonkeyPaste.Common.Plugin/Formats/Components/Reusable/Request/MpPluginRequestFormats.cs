using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginRequestItemFormat : MpJsonObject, MpIParameterKeyValuePair {
        public object paramId { get; set; }
        public string value { get; set; } = string.Empty;
    }

    public class MpPluginRequestFormatBase : MpJsonObject {
        public List<MpIParameterKeyValuePair> items { get; set; }

        [JsonIgnore]
        public Dictionary<object, string> ParamLookup =>
            items == null ?
                new Dictionary<object, string>() :
                items.ToDictionary(
                    x => (object)x.paramId,
                    x => x.value);
    }
}
