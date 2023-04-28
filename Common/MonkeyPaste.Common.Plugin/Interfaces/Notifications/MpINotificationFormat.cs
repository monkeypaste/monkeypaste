using System;
using System.Windows.Input;

namespace MonkeyPaste.Common.Plugin {
    public interface MpINotificationFormat {
        object Body { get; set; }
        string Detail { get; set; }
        ICommand FixCommand { get; set; }
        object FixCommandArgs { get; set; }
        object IconSourceObj { get; set; }
        object OtherArgs { get; set; }
        Func<object, object> RetryAction { get; set; }
        object RetryActionObj { get; set; }
        string Title { get; set; }

        object AnchorTarget { get; set; }
        object Owner { get; set; }
    }
}