using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonkeyPaste;

namespace MonkeyPaste {
    public class MpYoloResponse : MpJsonMessage {
        [JsonProperty("detectedObjects")]
        public List<MpYoloDetectedObject> DetectedObjects { get; set; } = new List<MpYoloDetectedObject>();
    }

    public class MpYoloDetectedObject : MpJsonMessage {
        [JsonProperty("score")]
        public double Score { get; set; } = 0;

        [JsonProperty("x")]
        public double X { get; set; } = 0;

        [JsonProperty("y")]
        public double Y { get; set; } = 0;

        [JsonProperty("width")]
        public double Width { get; set; } = 0;

        [JsonProperty("height")]
        public double Height { get; set; } = 0;

        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;
    }
}
