using System;
using System.Windows.Input;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginUserNotificationFormat : MpINotificationFormat {
        public string NotificationTypeStr => NotificationType.ToString();

        public MpPluginNotificationType NotificationType { get; set; }
        public string Body { get; set; }
        public string Detail { get; set; }
        public ICommand FixCommand { get; set; }
        public object FixCommandArgs { get; set; }
        public string IconSourceStr { get; set; }
        public object OtherArgs { get; set; }
        public Action<object> RetryAction { get; set; }
        public object RetryActionObj { get; set; }
        public string Title { get; set; }
    }
}