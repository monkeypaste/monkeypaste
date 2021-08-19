using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Plugin.Wpf_Template {
    public interface MpIPlugin {
        string GetName();

        MpIPluginComponent[] GetComponents();
    }

    public interface MpIPluginComponent {
        void Create(object obj);
    }

    public interface MpIClipboardItemPluginComponent : MpIPluginComponent {
        object GetDataObject();

        string[] GetHandledDataFormats();
    }
}
