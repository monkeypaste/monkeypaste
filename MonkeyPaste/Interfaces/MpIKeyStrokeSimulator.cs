using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIKeyStrokeSimulator {
        Task<bool> SimulateKeyStrokeSequenceAsync(string keystr, bool restoreDownState = true);
        Task<bool> SimulateKeyStrokeSequenceAsync<T>(IReadOnlyList<IReadOnlyList<T>> gesture, bool restoreDownState = true);
    }
}
