﻿using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpTimerActionViewModel : MpAvActionViewModelBase {
        #region Properties

        #region View Models


        #endregion

        #region Model


        #endregion

        #endregion

        #region Constructors

        public MpTimerActionViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Overrides

        public override async Task PerformAction(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            await Task.Delay(1);
        }
        #endregion
    }
}