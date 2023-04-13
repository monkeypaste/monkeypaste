using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvDelayActionViewModel :
        MpAvActionViewModelBase {

        #region Private Variables
        #endregion

        #region Constants

        public const string DELAY_MS_PARAM_ID = "DelayMs";

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                             new MpParameterFormat() {
                                label = "Delay",
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Integer,
                                minimum = 0,
                                maximum = 10_000,
                                isRequired = true,
                                paramId = DELAY_MS_PARAM_ID,
                                description = "The amount of time in milliseconds waited before children execute",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value ="1000"
                                    }
                                }
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion


        #region Properties


        #region State
        public override bool AllowNullArg =>
            true;

        #endregion

        #region Model

        public int DelayMs {
            get {
                if (ArgLookup.TryGetValue(DELAY_MS_PARAM_ID, out var param_vm) &&
                    param_vm.IntValue is int curVal) {
                    return curVal;
                }
                return 0;
            }
            set {
                if (DelayMs != value) {
                    ArgLookup[DELAY_MS_PARAM_ID].IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(DelayMs));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvDelayActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Public Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);
            await Task.Delay(DelayMs);
            await base.PerformActionAsync(actionInput);
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
