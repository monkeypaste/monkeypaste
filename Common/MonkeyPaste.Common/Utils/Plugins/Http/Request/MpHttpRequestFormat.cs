using System.Collections.Generic;

namespace MonkeyPaste.Common{
    public class MpHttpRequestFormat  {
        public string method { get; set; }
        public List<MpHttpHeaderItemFormat> header { get; set; } = new List<MpHttpHeaderItemFormat>();
        public MpHttpUrlFormat url { get; set; }
        public MpHttpBodyFormat body { get; set; }
        public string description { get; set; }
    }
}
