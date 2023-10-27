using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Windows.Input;

namespace MonkeyPaste {

    public class MpNotificationFormat : MpJsonObject, MpINotificationFormat {
        #region Constants

        public const int MAX_MESSAGE_DISPLAY_MS = 3000;

        #endregion

        #region Properties

        public string NotificationTypeStr => NotificationType.ToString();
        public MpNotificationType NotificationType { get; set; }
        public int MaxShowTimeMs { get; set; } = -1;
        public string Title { get; set; }
        public object Body { get; set; }
        public string Detail { get; set; }

        public object IconSourceObj { get; set; }

        public object AnchorTarget { get; set; }
        public object Owner { get; set; }
        public Func<object, object> RetryAction { get; set; }
        public object RetryActionObj { get; set; }

        public ICommand FixCommand { get; set; }
        public object FixCommandArgs { get; set; }
        public bool CanRemember { get; set; }

        public object OtherArgs { get; set; } // used to pass MpIloader to loader notification

        public bool ForceShow { get; set; }

        public char PasswordChar { get; set; }
        //public MpTextContentFormat BodyFormat { get; set; } = MpTextContentFormat.PlainText;

        #endregion

        #region Constructors

        public MpNotificationFormat() { }

        public MpNotificationFormat(MpPluginUserNotificationFormat pluginNotfication) {
            if (pluginNotfication == null) {
                return;
            }
            NotificationType = pluginNotfication.NotificationType.ToString().ToEnum<MpNotificationType>();

            if (pluginNotfication.NotificationType == MpPluginNotificationType.PluginResponseWarningWithOption) {
                MaxShowTimeMs = -1;
            } else {
                MaxShowTimeMs = MAX_MESSAGE_DISPLAY_MS;
            }

            Title = pluginNotfication.Title;
            Body = pluginNotfication.Body;
            Detail = pluginNotfication.Detail;
            IconSourceObj = pluginNotfication.IconSourceObj;
            RetryAction = pluginNotfication.RetryAction;
            RetryActionObj = pluginNotfication.RetryActionObj;
            FixCommand = pluginNotfication.FixCommand;
            FixCommandArgs = pluginNotfication.FixCommandArgs;
            OtherArgs = pluginNotfication.OtherArgs;
            Owner = pluginNotfication.Owner;
        }
        #endregion

    }
}
