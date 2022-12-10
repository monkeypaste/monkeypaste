using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIDropTarget {
        bool IsDropping { get; }
    }
    public interface MpAvIDragSource {

        PointerPressedEventArgs LastPointerPressedEventArgs { get; }
        //bool IsDragging { get; set; }
        void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc);
        //void NotifyDropComplete(DragDropEffects dropEffect);
        Task<MpAvDataObject> GetDataObjectAsync(bool forOle, string[] formats = null);

    }
}
