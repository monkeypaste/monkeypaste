using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvITextRange : IComparable, IEquatable<MpAvITextRange> {
        MpAvITextPointer Start { get; set; }
        MpAvITextPointer End { get; set; }

        int Length { get; }

        bool IsEmpty { get; }
        //string Text { get; set; }
        Task<string> GetTextAsync();
        Task SetTextAsync(string text);

        Task<bool> IsPointInRangeAsync(MpPoint point);
    }
}
