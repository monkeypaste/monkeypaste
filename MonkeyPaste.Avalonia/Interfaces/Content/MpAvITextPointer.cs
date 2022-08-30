using AvaloniaEdit.Document;
using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvITextPointer : IComparable {
        MpAvIContentDocument Document { get; }
        int Offset { get; set; }

        MpAvITextPointer DocumentStart { get; }
        MpAvITextPointer DocumentEnd { get; }

        MpAvITextPointer GetLineStartPosition(int lineOffset);
        MpAvITextPointer GetLineEndPosition(int lineOffset);
        MpAvITextPointer GetNextInsertionPosition(LogicalDirection dir);
        bool IsInSameDocument(MpAvITextPointer otp);
        int GetOffsetToPosition(MpAvITextPointer tp);
        MpAvITextPointer GetPositionAtOffset(int offset);
        MpAvITextPointer GetInsertionPosition(LogicalDirection dir);

        Task<MpRect> GetCharacterRectAsync(LogicalDirection dir);
    }
}
