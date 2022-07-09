using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpRandomContentProvider {
        #region Private Variables

        private static List<int> _manualQueryIds = new List<int>();

        #endregion
        #region Properties


        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public void Init() {
            _manualQueryIds.Clear();
        }

        #endregion
    }
}
