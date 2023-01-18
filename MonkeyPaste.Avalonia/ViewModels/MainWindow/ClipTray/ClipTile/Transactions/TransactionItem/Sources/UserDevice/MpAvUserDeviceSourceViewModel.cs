using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserDeviceSourceViewModel : MpAvTransactionSourceViewModelBase {

        #region Interfaces
        #endregion

        #region Properties

        #region View Models


        #endregion

        #region State
        #endregion

        #region Model

        public int UserDeviceId { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvUserDeviceSourceViewModel(MpAvTransactionItemViewModelBase parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpTransactionSource ts) {
            IsBusy = true;
            await base.InitializeAsync(ts);

            UserDeviceId = SourceObjId;

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Body));

            IsBusy = false;
        }

        #endregion

        #region Commands
        #endregion
    }
}
