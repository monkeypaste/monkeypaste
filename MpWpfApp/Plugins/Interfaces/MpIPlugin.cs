using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MonkeyPaste {
    public interface MpIPlugin {
        string GetName();

    }

    public interface MpIClipboardItemPluginComponent {
        void Create(object dobj);

        object GetDataObject();

        string[] GetHandledDataFormats();
    }


}
