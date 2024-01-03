using Newtonsoft.Json;
using System;
using System.Windows.Input;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginUserNotificationFormat : MpINotificationFormat {
        public MpPluginNotificationType NotificationType { get; set; }
        public int MaxShowTimeMs { get; set; } = 3_000; // < 0 means indefinite
        public string Title { get; set; }
        public object Body { get; set; }
        public string Detail { get; set; }
        public object IconSourceObj { get; set; }

        [JsonIgnore]
        public Func<object, object> RetryAction { get; set; }
        [JsonIgnore]
        public object RetryActionObj { get; set; }
        [JsonIgnore]
        public ICommand FixCommand { get; set; }
        [JsonIgnore]
        public object FixCommandArgs { get; set; }
        [JsonIgnore]
        public object AnchorObj { get; set; }
        [JsonIgnore]
        public object OtherArgs { get; set; }
        [JsonIgnore]
        public object OwnerObj { get; set; }

    }
}