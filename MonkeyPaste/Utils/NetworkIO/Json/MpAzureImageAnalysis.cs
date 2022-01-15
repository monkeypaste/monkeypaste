using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    //request
    public class MpAzureImageAnalysisRequest : MpJsonMessage {
        [JsonProperty("defaultLanguageCode")]
        public string DefaultLanguageCode { get; set; }

        [JsonProperty("visualFeatures")]
        public List<string> VisualFeatures { get; set; }

        [JsonProperty("details")]
        public List<string> Details { get; set; }
    }

    //response
    public class MpAzureImageAnalysis : MpJsonMessage {
        public List<MpAzureImageCategory> categories { get; set; }
        public MpAzureImageColor color { get; set; }
        public MpAzureImageDescription description { get; set; }
        public string requestId { get; set; }
        public MpAzureImageMetaData metadata { get; set; }
    }

    public class MpAzureImageCategory : MpJsonMessage {
        public string name { get; set; }
        public double score { get; set; }
    }
    public class MpAzureImageColor : MpJsonMessage {
        public string dominantColorForeground { get; set; }
        public string dominantColorBackground { get; set; }
        public List<string> dominantColors { get; set; }
        public string accentColor { get; set; }
        public bool isBwImg { get; set; }
    }
    public class MpAzureImageDescription : MpJsonMessage {
        public List<string> tags { get; set; }
        public List<MpAzureImageCaptions> captions { get; set; }
    }
    public class MpAzureImageCaptions : MpJsonMessage {
        public string text { get; set; }
        public double confidence { get; set; }
    }
    public class MpAzureImageMetaData : MpJsonMessage {
        public int height { get; set; }
        public int width { get; set; }
        public string format { get; set; }
    }
}
