using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpCursorType {
        None = 0,
        Default,
        OverDragItem,
        ContentMove,
        TileMove,
        ContentCopy,
        TileCopy,
        Invalid,
        Waiting,
        IBeam,
        ResizeNS,
        ResizeWE,
        ResizeNWSE,
        ResizeNESW,
        ResizeAll,
        Hand,
        Arrow
    }
    public interface MpICursor {
        MpCursorType CurrentCursor { get; }
        void SetCursor(object targetObj, MpCursorType newCursor);
        void UnsetCursor(object targetObj);
    }

    
}
