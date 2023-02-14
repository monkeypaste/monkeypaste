using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIClonableDbModel<T> where T : MpDbModelBase {
        Task<T> CloneDbModelAsync(bool deepClone = true, bool suppressWrite = false);
    }
}
