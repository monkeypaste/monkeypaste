using MonkeyPaste;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;

namespace My.Namespace {
    public static class DesignData {
        public static MpAvLoaderNotificationViewModel ExampleViewModel { get; } = new MpAvLoaderNotificationViewModel {
            NotificationFormat = new MpNotificationFormat() {
                Title = "Test loader",
                NotificationType = MpNotificationType.Loader,
                OtherArgs = new MpAvLoaderItemViewModel()
            }
        };
    }
}
