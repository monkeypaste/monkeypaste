using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIActionPluginComponent : MpIPluginComponentBase {
        //Task<bool> Validate();
        //string ValidationText { get; set; }

        Task PerformAction(object arg);
        event EventHandler<object> OnActionComplete;
    }

}
