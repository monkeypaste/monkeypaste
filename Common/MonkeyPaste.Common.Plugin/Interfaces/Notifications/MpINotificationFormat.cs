using System;
using System.Windows.Input;

namespace MonkeyPaste.Common.Plugin {
    public interface MpINotificationFormat {
        string NotificationTypeStr { get; }
        string Body { get; set; }
        string Detail { get; set; }
        ICommand FixCommand { get; set; }
        object FixCommandArgs { get; set; }
        string IconSourceStr { get; set; }
        object OtherArgs { get; set; }
        Action<object> RetryAction { get; set; }
        object RetryActionObj { get; set; }
        string Title { get; set; }
    }
}