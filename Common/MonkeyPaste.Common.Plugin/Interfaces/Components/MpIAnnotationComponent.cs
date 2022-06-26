using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIAnnotatorComponent : MpIPluginComponentBase {
        MpAnnotatorResponseFormat Annotate(MpAnnotatorRequestFormat request);
    }
}
