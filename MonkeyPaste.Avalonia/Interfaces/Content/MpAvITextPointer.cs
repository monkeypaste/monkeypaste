using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public enum MpAvLogicalDirection {
        Backward = 0,
        Forward
    }

    public interface MpAvITextPointer : IComparable {
        MpAvIContentDocument Document { get; }
        int Offset { get; set; }

        MpAvITextPointer DocumentStart { get; }
        MpAvITextPointer DocumentEnd { get; }

        MpAvITextPointer GetLineStartPosition(int lineOffset);
        MpAvITextPointer GetLineEndPosition(int lineOffset);
        MpAvITextPointer GetNextInsertionPosition(MpAvLogicalDirection dir);
        bool IsInSameDocument(MpAvITextPointer otp);
        int GetOffsetToPosition(MpAvITextPointer tp);
        MpAvITextPointer GetPositionAtOffset(int offset);
        MpAvITextPointer GetInsertionPosition(MpAvLogicalDirection dir);

        Task<MpRect> GetCharacterRectAsync(MpAvLogicalDirection dir);
    }
}
