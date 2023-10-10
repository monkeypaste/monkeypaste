using MonkeyPaste.Common;
using Newtonsoft.Json.Converters;
using System;
using System.Text.Json.Serialization;

namespace MonkeyPaste {
    public class MpUserAccountFormat : MpJsonObject {
        [JsonConverter(typeof(StringEnumConverter))]
        public MpUserAccountType AccountType { get; set; }
        public bool IsActive { get; set; }
        public bool IsMonthly { get; set; } = true;
        [JsonIgnore]
        public bool IsYearly =>
            !IsMonthly;
        public bool IsTrial { get; set; }
        public DateTimeOffset ExpireOffset { get; set; }
    }
}
