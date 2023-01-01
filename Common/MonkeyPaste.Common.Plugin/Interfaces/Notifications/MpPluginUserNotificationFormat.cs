using System;
using System.Windows.Input;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginUserNotificationFormat : MpINotificationFormat {
        public string NotificationTypeStr => NotificationType.ToString();

        public MpPluginNotificationType NotificationType { get; set; }
        public object Body { get; set; }
        public string Detail { get; set; }
        public ICommand FixCommand { get; set; }
        public object FixCommandArgs { get; set; }
        public object IconSourceObj { get; set; }
        public object AnchorTarget { get; set; }
        public object OtherArgs { get; set; }
        public Func<object,object> RetryAction { get; set; }
        public object RetryActionObj { get; set; }
        public string Title { get; set; }
    }
}