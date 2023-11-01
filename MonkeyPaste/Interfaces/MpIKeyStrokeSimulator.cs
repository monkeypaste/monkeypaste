using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIKeyStrokeSimulator {
        bool SimulateKeyStrokeSequence(string keystr);
        bool SimulateKeyStrokeSequence<T>(IReadOnlyList<IReadOnlyList<T>> gesture);
        bool IsSimulatingKey<T>(T key);
    }
}
