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


    public interface MpIDragSource : MpIDraggable {
        bool WasDragCanceled { get; set; }
        object LastPointerPressedEventArgs { get; }
        //bool IsDragging { get; set; }
        void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc, bool meta);
        //void NotifyDragComplete(DragDropEffects dropEffect);
        Task<MpPortableDataObject> GetDataObjectAsync(string[] formats = null, bool use_placeholders = true, bool ignore_selection = false);

        string[] GetDragFormats();
    }
    public interface MpIPlainHtmlConverterView : MpIJsonMessenger, MpIHasDevTools, MpIPlatformView {

    }

    public interface MpIRecyclableLocatorItem : MpILocatorItem {
        DateTime? LocatedDateTime { get; set; }
    }
    public interface MpILocatorItem {
        int LocationId { get; }
    }
    public interface MpIContentView :
        MpIHasDataContext, MpIDragSource, MpIHasDevTools, MpIRecyclableLocatorItem, MpIJsonMessenger {
        bool IsContentLoaded { get; }
        bool IsSubSelectable { get; }
        Task LoadContentAsync(bool isSearchEnabled = true);
        Task ReloadAsync();
        Task<bool> UpdateContentAsync(MpJsonObject contentJsonObj);

    }
}
