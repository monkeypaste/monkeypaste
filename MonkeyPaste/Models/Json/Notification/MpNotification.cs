using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Windows.Input;

namespace MonkeyPaste {

    public class MpNotificationFormat : MpINotificationFormat {
        #region Constants

        public const int MAX_MESSAGE_DISPLAY_MS = 3_000;

        #endregion

        #region Properties

        public string NotificationTypeStr => NotificationType.ToString();
        public MpNotificationType NotificationType { get; set; }
        public int MaxShowTimeMs { get; set; } = -1;
        public string Title { get; set; }
        public object Body { get; set; }
        public string Detail { get; set; }

        public object IconSourceObj { get; set; }

        public object AnchorObj { get; set; }
        public object OwnerObj { get; set; }
        public Func<object, object> RetryAction { get; set; }
        public object RetryActionObj { get; set; }

        public ICommand FixCommand { get; set; }
        public object FixCommandArgs { get; set; }
        public bool CanRemember { get; set; }

        public object OtherArgs { get; set; } // used to pass MpIloader to loader notification

        public bool ForceShow { get; set; }

        public char PasswordChar { get; set; }

        #endregion

        #region Constructors

        public MpNotificationFormat() { }

        public MpNotificationFormat(MpPluginUserNotificationFormat pluginNotfication) {
            if (pluginNotfication == null) {
                return;
            }
            NotificationType = pluginNotfication.NotificationType.ToString().ToEnum<MpNotificationType>();
            MaxShowTimeMs = pluginNotfication.MaxShowTimeMs;
            Title = pluginNotfication.Title;
            Body = pluginNotfication.Body;
            Detail = pluginNotfication.Detail;
            IconSourceObj = pluginNotfication.IconSourceObj;
        }
        #endregion

    }
}
