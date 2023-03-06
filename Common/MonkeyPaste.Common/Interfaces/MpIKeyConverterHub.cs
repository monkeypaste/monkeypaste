
using System;
using System.Collections.Generic;
namespace MonkeyPaste.Common {
    public interface MpIKeyConverterHub {
        string ConvertKeySequenceToString<T>(IEnumerable<IEnumerable<T>> keyList);
        IEnumerable<IEnumerable<string>> ConvertStringToKeyLiteralSequence(string keyStr);
        IEnumerable<IEnumerable<T>> ConvertStringToKeySequence<T>(string keyStr) where T : Enum;
    }
}