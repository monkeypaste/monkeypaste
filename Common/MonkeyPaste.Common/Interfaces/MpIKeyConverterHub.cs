
using System;
using System.Collections.Generic;
namespace MonkeyPaste.Common {
    public interface MpIKeyConverterHub {
        string ConvertKeySequenceToString<T>(IEnumerable<IEnumerable<T>> keyList);
        IReadOnlyList<IReadOnlyList<string>> ConvertStringToKeyLiteralSequence(string keyStr);
        IReadOnlyList<IReadOnlyList<T>> ConvertStringToKeySequence<T>(string keyStr) where T : Enum;

    }
}