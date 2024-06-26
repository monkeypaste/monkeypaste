﻿using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIContentDragSource : MpIDraggable {
        Task<MpAvDataObject> GetDataObjectAsync(string[] formats = null, bool ignore_selection = false);

        string[] GetDragFormats();
    }

    public interface MpAvIContentWebViewDragSource : MpAvIContentDragSource {
        bool WasDragCanceled { get; set; }
        PointerEventArgs LastPointerPressedEventArgs { get; set; }
        void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc, bool meta);

    }
}
