using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvQueryTrayViewModel :
        MpAvSelectorViewModelBase<MpAvClipTrayViewModel, MpAvClipTileViewModel> {
        #region Private Variables

        #endregion

        #region Statics

        #endregion

        #region Properties

        #region View Models

        public MpAvQueryInfoViewModel QueryInfoViewModel { get; private set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvQueryTrayViewModel() : this(null) { }

        public MpAvQueryTrayViewModel(MpAvClipTrayViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
