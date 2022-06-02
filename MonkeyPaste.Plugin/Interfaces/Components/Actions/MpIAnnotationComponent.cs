using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {
    public interface MpIAnnotationComponent : MpIActionComponentBase {
        object Annotate(object args);
    }
}
