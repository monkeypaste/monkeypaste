using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpHttpRequestType {
        None =0,
        HttpClient,
        WebRequest,
        Custom
    }
    
    public class MpHttpHeaderFormat : MpJsonMessage {
        [JsonProperty("key")]
        public string Key { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;

        [JsonProperty("isAuthorization")]
        public bool IsAuthorization { get; set; } = false;
    }

    public class MpHttpRequestFormat : MpJsonMessage {
        [JsonProperty("requestMethod")]
        public string RequestMethod { get; set; } = "POST";

        [JsonProperty("encoding")]
        public string Encoding { get; set; } = "UTF8";

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = "application/json";

        [JsonProperty("requestTypeId")]
        public int RequestTypeId { get; set; } = 0;

        [JsonProperty("uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonProperty("args")]
        public string[] Args { get; set; }

        [JsonProperty("headers")]
        public List<MpHttpHeaderFormat> Headers { get; set; } = new List<MpHttpHeaderFormat>();
    }
}
