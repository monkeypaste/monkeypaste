using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIKeyStrokeSimulator {
        bool SimulateKeyStrokeSequence(string keystr);
        bool SimulateKeyStrokeSequence<T>(IReadOnlyList<IReadOnlyList<T>> gesture);
    }
}
