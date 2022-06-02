using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {
    public interface MpIAnnotationComponent : MpIActionComponentBase {
        Task<object> AnnotateAsync(object args);
    }
}
