using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class MpQuillDelta : MpJsonObject {
        public List<Op> ops { get; set; }
    }

    public class Attributes {
        public string align { get; set; }
        public int indent { get; set; }
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

    public class DeltaRange {
        public int index { get; set; }
        public int length { get; set; }
    }

    public class Op {
        public object insert { get; set; }
        public Attributes attributes { get; set; }

        public DeltaRange format { get; set; }
        public DeltaRange delete { get; set; }
    }

    public class ImageInsert {
        public string image { get; set; }
    }


    public class TableCellLine {
        public string rowspan { get; set; }
        public string colspan { get; set; }
        public string row { get; set; }
        public string cell { get; set; }
    }

    public class TableCol {
        public string width { get; set; }
    }


}
