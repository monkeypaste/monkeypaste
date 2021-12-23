using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonkeyPaste;

namespace MonkeyPaste {
    /*
      "object": "text_completion",
    "created": 1586839808,
    "model": "davinci:2020-05-03",
    "choices": [{
        "text": " of reading speed. You",
        "index": 0,
        "logprobs": null,
        "finish_reason": "length"
    }]
     */
    public class MpOpenAiResponse : MpJsonMessage {
        public string id { get; set; }

        public long created { get; set; }

        public string model { get; set; }

        public List<MpOpenAiChoice> choices { get; set; }
    }

    public class MpOpenAiChoice : MpJsonMessage {
        public string text { get; set; }

        public int index { get; set; }

        public int? logprobs { get; set; }

        public string finish_reason { get; set; }
    }

    public class MpOpenAiRequest : MpJsonMessage {
        //[JsonProperty("engine")]
        //public string Engine { get; set; }
        [JsonProperty("prompt", Order = 0)]
        public string Prompt { get; set; }

        [JsonProperty("temperature", Order = 1)]
        public double Temperature { get; set; }

        [JsonProperty("max_tokens", Order = 2)]
        public int MaxTokens { get; set; }

        [JsonProperty("top_p", Order = 3)]
        public double TopP { get; set; }

        [JsonProperty("frequency_penalty", Order = 4)]
        public double FrequencyPenalty { get; set; }

        [JsonProperty("presence_penalty", Order = 5)]
        public double PresencePenalty { get; set; }

    }
}
