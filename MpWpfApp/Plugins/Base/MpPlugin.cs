using System;
using System.Collections.Generic;
using System.Text;

namespace MpWpfApp {
    public enum MpPluginComponentType {
        None = 0,
        Clipboard,
        Input,
        Restful,
        Gui,
        Composite
    }

    public class MpPlugin {
        #region Private Variables

        #endregion

        #region Properties
        public string Version { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public object[] Components { get; set; }

        #endregion

        #region Public Methods        
        
        #endregion

        
    }
}
