using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIHasDataContext {
        object DataContext { get; }

    }
    public interface MpIHasSettableDataContext : MpIHasDataContext {
        new object DataContext { get; set; }
    }

    public interface MpIPlatformView {
        EventHandler OnViewAttached { get; }
        bool IsViewInitialized { get; }

    }
    public interface MpIHasDevTools {
        void ShowDevTools();
    }

    public interface MpIJsonMessenger {
        void SendMessage(string msgJsonBase64Str);
    }

    public interface MpIAyncJsonMessenger {
        Task<string> SendMessageAsync(string msgJsonBase64Str);
    }

    public interface MpIPlainHtmlConverterView : MpIJsonMessenger, MpIHasDevTools, MpIPlatformView {

    }

    public interface MpIContentView :
        MpIHasDataContext, MpIHasDevTools, MpIJsonMessenger {
        bool IsSubSelectable { get; }
        Task LoadContentAsync();
        Task ReloadAsync();
        Task UpdateContentAsync(MpJsonObject contentJsonObj);

    }
}
