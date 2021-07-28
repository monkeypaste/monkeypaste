using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpColorChooserContextMenuItemViewModel : MpContextMenuItemViewModel {
        #region Private Variables

        #endregion

        #region Properties
        public int RowCount {
            get {
                return 5;
            }
        }

        public int ColumnCount {
            get {
                return 14;
            }
        }
        #endregion

        #region Public Methods
        public MpColorChooserContextMenuItemViewModel() : base() { }
        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion
    }
}
