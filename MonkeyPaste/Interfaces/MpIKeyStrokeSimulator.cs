using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIKeyStrokeSimulator {
        bool IsSimulating { get; }
        Task<bool> SimulateKeyStrokeSequenceAsync(string keystr, int holdDelay = 0, int releaseDelay = 0);
    }
}
