using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIKeyStrokeSimulator {
        bool IsSimulating { get; }
        Task<bool> SimulateKeyStrokeSequenceAsync(string keystr);
        Task<bool> SimulateKeyStrokeSequenceAsync<T>(IReadOnlyList<IReadOnlyList<T>> gesture);
    }
}
