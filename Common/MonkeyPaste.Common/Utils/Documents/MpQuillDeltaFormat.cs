﻿using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MonkeyPaste.Common {
    public class MpQuillDelta {
        public static MpQuillDelta Parse(string json) {
            var req_lookup = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            //var req_lookup = MJsonConverter.DeserializeObject<Dictionary<string, object>>(json);
            if (req_lookup != null &&
                req_lookup.TryGetValue("ops", out var itemsObj) && itemsObj is JArray items_jarray) {
                Dictionary<object, string> param_lookup = new Dictionary<object, string>();
                foreach (var kvp_jtoken in items_jarray) {
                    if (kvp_jtoken.SelectToken("paramId", false) is JToken param_token &&
                        kvp_jtoken.SelectToken("paramValue", false) is JToken val_token) {
                        param_lookup.Add(param_token.Value<string>(), val_token.Value<string>());
                    }
                }
                return new MpQuillDelta() {
                    ops = null
                };
            }
            return null;
        }
        public List<MpQuillOp> ops { get; set; }
    }

    public class MpQuillOp {
        public object insert { get; set; }
        public MpQuillAttributes attributes { get; set; }

        public MpQuillDeltaRange format { get; set; }
        public int? delete { get; set; }
        public int? retain { get; set; }

    }

    public class MpQuillDeltaRange : MpITextRange {
        public int index { get; set; }
        public int length { get; set; }

        [JsonIgnore]
        int MpITextRange.Offset =>
            index;

        [JsonIgnore]
        int MpITextRange.Length =>
            length;
    }
    public class MpQuillAttributes {
        public string align { get; set; }
        public int? indent { get; set; }
        public string templateGuid { get; set; }
        public bool? isFocus { get; set; }
        public string templateName { get; set; }
        public string templateColor { get; set; }
        public string templateText { get; set; }
        public string templateType { get; set; }
        public string templateData { get; set; }
        public string templateDeltaFormat { get; set; }
        public string templateHtmlFormat { get; set; }
        public bool? wasVisited { get; set; }
        public bool? italic { get; set; }
        public bool? bold { get; set; }
        public string size { get; set; }
        public string color { get; set; }
        public string background { get; set; }
        public string list { get; set; }
        public string link { get; set; }
        public string linkType { get; set; }

        [JsonProperty("table-col")]
        public MpQuillTableCol tablecol { get; set; }

        [JsonProperty("table-cell-line")]
        public MpQuillTableCellLine tablecellline { get; set; }
        public string row { get; set; }
        public string rowspan { get; set; }
        public string colspan { get; set; }

        // non-quill attributes

        public string rect { get; set; }
        public string tag { get; set; }
    }



    public class MpQuillImageInsert {
        public string image { get; set; }
    }


    public class MpQuillTableCellLine {
        public string rowspan { get; set; }
        public string colspan { get; set; }
        public string row { get; set; }
        public string cell { get; set; }
    }

    public class MpQuillTableCol {
        public string width { get; set; }
    }


}