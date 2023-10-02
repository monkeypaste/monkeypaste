using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvDownKeyHelper : MpIDownKeyHelper {
        #region Interfaces

        #region MpIDownKeyHelper Implementation
        public IReadOnlyList<object> Downs =>
            Downs_internal.Cast<object>().ToList();
        #endregion
        #endregion

        #region Properties

        ObservableCollection<KeyCode> Downs_internal { get; } = new ObservableCollection<KeyCode>();

        #endregion

        public event EventHandler<(bool, KeyCode)> OnDownsChanged;
    }
}
