using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIAnnotationComponent : MpIPluginComponentBase {
        object Annotate(object args);
    }
}
