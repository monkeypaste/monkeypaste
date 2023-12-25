using Newtonsoft.Json;
using System;
using System.Windows.Input;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginUserNotificationFormat : MpJsonObject, MpINotificationFormat {
        public int MaxShowTimeMs { get; set; } = 3_000; // < 0 means indefinite
        public MpPluginNotificationType NotificationType { get; set; }
        public object Body { get; set; }
        public string Detail { get; set; }
        public object IconSourceObj { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public object AnchorTarget { get; set; }
        [JsonIgnore]
        public object OtherArgs { get; set; }
        [JsonIgnore]
        public object Owner { get; set; }
        [JsonIgnore]
        public Func<object, object> RetryAction { get; set; }
        [JsonIgnore]
        public object RetryActionObj { get; set; }

        [JsonIgnore]
        public ICommand FixCommand { get; set; }
        [JsonIgnore]
        public object FixCommandArgs { get; set; }
    }
}