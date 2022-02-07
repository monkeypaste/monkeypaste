using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpMacroActionViewModel : MpActionViewModelBase {
        #region Properties

        #region View Models


        #endregion

        #region Model


        #endregion

        #endregion

        #region Constructors

        public MpMacroActionViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion


        #region Protected Overrides

        public override async Task PerformAction(MpCopyItem arg) {
            await Task.Delay(1);
        }
        #endregion
    }
}
