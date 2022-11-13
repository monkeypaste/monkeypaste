using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIContentDocument {
        IControl Owner { get; }

        string ContentScreenShotBase64 { get; }

        Task<MpAvDataObject> GetDataObjectAsync(bool ignoreSelection, bool fillTemplates, bool isCutOrCopy, string[] formats = null);
        MpAvITextPointer ContentStart { get; }
        MpAvITextPointer ContentEnd { get; }

        Task<MpAvITextPointer> GetPosisitionFromPointAsync(MpPoint point, bool snapToText);

        Task<IEnumerable<MpAvITextRange>> FindAllTextAsync(string matchText, bool isCaseSensitive, bool matchWholeWord, bool useRegex);
    }
}
