using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public enum MpPluginType {
        None = 0,
        Clipboard,
        Input,
        Restful,
        Gui,
        Composite
    }

    public class MpPluginDescriptor {
        #region Private Variables

        #endregion

        #region Properties
        public string Version { get; set; }

        public string PluginName { get; set; }

        public string PluginId { get; set; }

        public List<MpPluginType> PluginTypes { get; set; } = new List<MpPluginType>();

        #endregion

        #region Public Methods
        
        #endregion

        
    }
}
