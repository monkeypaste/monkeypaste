using AvaloniaEdit.Document;
using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    public interface MpAvITextPointer : IComparable, IEquatable<MpAvITextPointer> {
        MpAvIContentDocument Document { get; }
        int Offset { get; }

        MpAvITextPointer DocumentStart { get; }
        MpAvITextPointer DocumentEnd { get; }

        MpAvITextPointer GetNextInsertionPosition(LogicalDirection dir);
        bool IsInSameDocument(MpAvITextPointer otp);
        int GetOffsetToPosition(MpAvITextPointer tp);
        MpAvITextPointer GetPositionAtOffset(int offset);
        MpAvITextPointer GetInsertionPosition(LogicalDirection dir);

        MpRect GetCharacterRect(LogicalDirection dir);
    }
}
