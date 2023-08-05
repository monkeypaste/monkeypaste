using MonkeyPaste.Common;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvDataObjectViewModel : MpAvViewModelBase {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        public Dictionary<string, MpAvITransactionNodeViewModel> DataLookup { get; private set; }
        #endregion

        #region Model

        public MpPortableDataObject DataObject { get; private set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvDataObjectViewModel() : base() {
            DataObject = new MpPortableDataObject();
        }

        #endregion

        #region Public Methods


        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
