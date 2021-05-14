using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpHelpers {
        #region Singleton
        private static readonly Lazy<MpHelpers> _Lazy = new Lazy<MpHelpers>(() => new MpHelpers());
        public static MpHelpers Instance { get { return _Lazy.Value; } }

        private MpHelpers() { }
        #endregion
    }
}
