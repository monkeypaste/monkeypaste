using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIDropTarget {
        bool IsDropping { get; }
    }
    public interface MpAvIDragSource {

        PointerEventArgs DragPointerEventArgs { get; }
        bool IsDragging { get; set; }
        void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc);
        void NotifyDropComplete(DragDropEffects dropEffect);
        Task<MpAvDataObject> GetDataObjectAsync(bool ignoreSelection, bool fillTemplates, bool isCutOrCopy, string[] formats = null);

    }

    public interface MpAvIContentDocument {
        IControl Owner { get; }


        MpAvITextPointer ContentStart { get; }
        MpAvITextPointer ContentEnd { get; }

        Task<MpAvITextPointer> GetPosisitionFromPointAsync(MpPoint point, bool snapToText);

        Task<IEnumerable<MpAvITextRange>> FindAllTextAsync(string matchText, bool isCaseSensitive, bool matchWholeWord, bool useRegex);
    }
}
