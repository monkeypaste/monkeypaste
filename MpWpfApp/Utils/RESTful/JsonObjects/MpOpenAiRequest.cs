using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MpWpfApp {
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
    public class MpOpenAiResponse {
        public string id { get; set; }

        public long created { get; set; }

        public string model { get; set; }

        public List<MpOpenAiChoice> choices { get; set; }
    }

    public class MpOpenAiChoice {
        public string text { get; set; }

        public int index { get; set; }

        public int? logprobs { get; set; }

        public string finish_reason { get; set; }
    }

    public class MpOpenAiRequest {
        //[JsonProperty("engine")]
        //public string Engine { get; set; }
        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonProperty("top_p")]
        public double TopP { get; set; }

        [JsonProperty("frequency_penalty")]
        public double FrequencyPenalty { get; set; }

        [JsonProperty("presence_penalty")]
        public double PresencePenalty { get; set; }
    }
}
