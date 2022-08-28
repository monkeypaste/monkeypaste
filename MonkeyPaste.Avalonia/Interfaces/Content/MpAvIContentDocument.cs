using Avalonia.Controls;
using MonkeyPaste.Common;
using System.Collections;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIContentDocument {
        IControl Owner { get; }

        MpAvITextPointer ContentStart { get; }
        MpAvITextPointer ContentEnd { get; }

        string ContentData { get; set; }

        MpAvITextPointer GetPosisitionFromPoint(MpPoint point, bool snapToText);

        IEnumerable<MpAvITextRange> FindAllText(string matchText, bool isCaseSensitive, bool matchWholeWord, bool useRegex);
    }
}
