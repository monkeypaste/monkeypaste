using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIKeyStrokeSimulator {
        bool IsSimulating { get; }
        Task<bool> SimulateKeyStrokeSequenceAsync(string keystr, int holdDelay = 0, int releaseDelay = 0);
        Task<bool> SimulateKeyStrokeSequenceAsync<T>(IReadOnlyList<IReadOnlyList<T>> gesture, IEnumerable<T> physical_downs = null, IEnumerable<T> sup_downs = null, int holdDelay = 0, int releaseDelay = 0);
    }
}
