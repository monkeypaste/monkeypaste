﻿using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvCheckBoxParameterViewModel : MpAvParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvCheckBoxParameterViewModel() : base(null) { }

        public MpAvCheckBoxParameterViewModel(MpIParameterHostViewModel parent) : base(parent) { }

        public override async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            IsBusy = false;
        }

        #endregion
    }
}