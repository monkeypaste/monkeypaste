using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIDefaultDataCreator {
        Task CreateDefaultDataAsync();
        Task ResetShortcutsAsync();
    }
}
