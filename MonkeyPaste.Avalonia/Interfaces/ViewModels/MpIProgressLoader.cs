using MonkeyPaste.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
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

    public interface MpIProgressIndicatorViewModel : MpIViewModel {
        double PercentLoaded { get; }
    }
    public interface MpICancelableProgressIndicatorViewModel : MpIProgressIndicatorViewModel {
        CancellationToken CancellationToken { get; }
        ICommand CancelCommand { get; }
        bool UpdateProgress(long totalBytes, long? bytesReceived, double percentComplete);
    }

    public interface MpIProgressLoaderViewModel : MpIProgressIndicatorViewModel, MpINotification {
        Task BeginLoaderAsync();
        Task FinishLoaderAsync();
        bool ShowSpinner { get; }
    }
}
