using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MonkeyPaste {
    public interface MpIPlugin {
        string GetName();

    }

    public interface MpIContentPluginComponent {
        void Create(object dobj);

        object GetDataObject();

        string[] GetHandledDataFormats();
    }


}
