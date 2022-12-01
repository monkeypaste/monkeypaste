using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpINotification : MpIViewModel {
        string Title { get; set; }
        string Body { get; set; }
        string Detail { get; set; }

        string IconResourceKey { get; }

        MpNotificationType DialogType { get; }
    }

    public interface MpIUserActionNotification : MpINotification {
        MpNotificationLayoutType ExceptionType { get; set; }
    }

    public interface MpIProgressLoader : MpINotification {        
        double PercentLoaded { get; }
        Task BeginLoaderAsync();
        Task FinishLoaderAsync();
    }
}
