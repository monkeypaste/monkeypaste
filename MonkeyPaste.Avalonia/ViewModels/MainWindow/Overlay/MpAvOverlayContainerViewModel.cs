using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvOverlayContainerViewModel : MpAvViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvOverlayContainerViewModel _instance;
        public static MpAvOverlayContainerViewModel Instance => _instance ?? (_instance = new());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public bool IsOverlayVisible { get; set; }
        #endregion

        #region Constructors
        public MpAvOverlayContainerViewModel() { }
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
