using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIClonableDbModel<T> where T: MpDbModelBase {
        Task<T> CloneDbModel();
    }
}
