using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public enum Type {
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

        public List<Type> PluginTypes { get; set; } = new List<Type>();

        #endregion

        #region Public Methods
        
        #endregion        
    }
}
