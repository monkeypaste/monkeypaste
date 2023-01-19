using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public abstract class MpOmitNullJsonObject : MpJsonObject {
        public override string SerializeJsonObject() {

            return base.SerializeJsonObject(new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
        public override string SerializeJsonObjectToBase64(Encoding enc = null) {
            return base.SerializeJsonObjectToBase64(new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore
            },enc);
        }
    }
    public class MpQuillDelta : MpOmitNullJsonObject {
        public List<Op> ops { get; set; }
    }

    public class Op : MpOmitNullJsonObject {
        public object insert { get; set; }
        public Attributes attributes { get; set; }

        public DeltaRange format { get; set; }
        public int? delete { get; set; }
        public int? retain { get; set; }

    }

    public class DeltaRange : MpOmitNullJsonObject {
        public int index { get; set; }
        public int length { get; set; }
    }
    public class Attributes : MpOmitNullJsonObject {
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
        public TableCol tablecol { get; set; }

        [JsonProperty("table-cell-line")]
        public TableCellLine tablecellline { get; set; }
        public string row { get; set; }
        public string rowspan { get; set; }
        public string colspan { get; set; }
    }



    public class ImageInsert : MpOmitNullJsonObject {
        public string image { get; set; }
    }


    public class TableCellLine : MpOmitNullJsonObject {
        public string rowspan { get; set; }
        public string colspan { get; set; }
        public string row { get; set; }
        public string cell { get; set; }
    }

    public class TableCol : MpOmitNullJsonObject {
        public string width { get; set; }
    }


}
