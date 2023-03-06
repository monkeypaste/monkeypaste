using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIKeyStrokeSimulator {
        Task<bool> SimulateKeyStrokeSequenceAsync(string keystr, int holdDelay = 0, int releaseDelay = 0);
    }
}
