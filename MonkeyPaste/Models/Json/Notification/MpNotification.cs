using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MonkeyPaste {
    public class MpNotificationFormat : MpJsonObject {
        #region Properties

        public MpNotificationType NotificationType { get; set; }

        public int MaxShowTimeMs { get; set; } = -1;
        public string Title { get; set; }
        public string Body { get; set; }
        public string Detail { get; set; }

        public string IconSourceStr { get; set; }

        public Action<object> RetryAction { get; set; }
        public object RetryActionObj { get; set; }

        public ICommand FixCommand { get; set; }
        public object FixCommandArgs { get; set; }

        public object OtherArgs { get; set; } // used to pass MpIloader to loader notification

        #endregion
    }
}
