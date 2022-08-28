using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    public interface MpAvITextRange : IComparable, IEquatable<MpAvITextRange> {
        MpAvITextPointer Start { get; set; }
        MpAvITextPointer End { get; set; }

        bool IsEmpty { get; }
        string Text { get; set; }

        bool IsPointInRange(MpPoint point);
    }
}
