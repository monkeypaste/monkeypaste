using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {
    public interface MpIAnalyzerComponent : MpIActionComponentBase {
        Task<object> AnalyzeAsync(object args);
    }

    public interface MpICommandComponentBase : MpIActionComponentBase {

    }

    public interface MpIShellCommandComponent : MpICommandComponentBase {

    }

    public interface MpIRestCommandComponent : MpICommandComponentBase {

    }
}
