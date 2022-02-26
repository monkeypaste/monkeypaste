using System;
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
        Hand
    }

    public interface MpICursor {
        void SetCursor(MpCursorType newCursor);
    }
}
