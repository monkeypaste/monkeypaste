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
    public interface MpIPlainHtmlConverterView : MpIJsonMessenger, MpIHasDevTools, MpIPlatformView {

    }
    public interface MpIWebView : MpIHasDataContext {
        void ExecuteJavascript(string script);
    }
    public interface MpIRecyclableLocatorItem : MpILocatorItem {
        DateTime? LocatedDateTime { get; set; }
    }
    public interface MpILocatorItem {
        int LocationId { get; }
    }
    public interface MpIContentViewLocator {
        MpIContentView LocateContentView(int contentId);
        void AddView(MpIContentView cv);
        void RemoveView(MpIContentView cv);
    }
    public interface MpIContentView :
        MpIHasDataContext, MpIHasDevTools, MpIRecyclableLocatorItem, MpIJsonMessenger {
        bool IsViewInitialized { get; }
        bool IsContentLoaded { get; }
        bool IsSubSelectable { get; }
        Task LoadContentAsync(bool isSearchEnabled = true);
        Task ReloadAsync();
        Task<bool> UpdateContentAsync(MpJsonObject contentJsonObj);

    }
}
