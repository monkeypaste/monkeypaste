using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAccountViewModel : MpViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #region State

        public bool IsContentAddPausedByAccount { get; private set; }
        #endregion

        #region Model

        public MpContentCapInfo CapInfo { get; private set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvAccountViewModel() {
            InitializeAsync().FireAndForgetSafeAsync();
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            base.Instance_OnItemAdded(sender, e);
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            base.Instance_OnItemUpdated(sender, e);
        }
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            base.Instance_OnItemDeleted(sender, e);
        }
        #endregion

        #region Private Methods

        private async Task InitializeAsync() {
            Dispatcher.UIThread.VerifyAccess();

            IsBusy = true;

            CapInfo = await Mp.Services.AccountTools.RefreshCapInfoAsync(MpUserAccountType.Free);
            IsBusy = false;
        }
        #endregion

        #region Commands
        #endregion
    }
}
