using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvKeySimulatorActionViewModel :
        MpAvActionViewModelBase {
        #region Constants
        public const string SIM_KEY_STR_PARAM_ID = "SimKeyStr";
        #endregion


        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Keys to simulate",
                                controlType = MpParameterControlType.ShortcutRecorder,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = SIM_KEY_STR_PARAM_ID,
                                description = "Recorded key combinations will be simulated in the foreground application."
                            },
                            //new MpParameterFormat() {
                            //    label = "Pre Delay (ms)",
                            //    controlType = MpParameterControlType.Slider,
                            //    unitType = MpParameterValueUnitType.Integer,
                            //    minimum = 0,
                            //    maximum =
                            //    isRequired = true,
                            //    paramId = SIM_KEY_STR_PARAM_ID,
                            //    description = "Recorded key combinations will be simulated in the foreground application."
                            //},
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Properties

        #region View Models

        #endregion

        #region Appearance
        public override string ActionHintText =>
            "Gesture Simulator - Simulates the recorded key combination into whatever is the current active application. Only 1 gesture is supported so if you need more you will need to chain multiple instances of this toggether.";

        #endregion

        #region State
        public override bool AllowNullArg =>
            true;

        #endregion

        #region Model
        public string KeyString {
            get {
                if (ArgLookup.TryGetValue(SIM_KEY_STR_PARAM_ID, out var param_vm)) {
                    return param_vm.CurrentValue;
                }
                return string.Empty;
            }
            set {
                if (KeyString != value) {
                    ArgLookup[SIM_KEY_STR_PARAM_ID].CurrentValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(KeyString));
                }
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAvKeySimulatorActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvKeySimulatorActionViewModel_PropertyChanged;
            ActionArgs.CollectionChanged += ActionArgs_CollectionChanged;
        }

        #endregion

        #region Protected Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(KeyString);

            await base.PerformActionAsync(actionInput);
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods


        private void MpAvKeySimulatorActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ActionArgs):
                    //OnPropertyChanged(nameof(ShortcutViewModel));
                    //if (ArgLookup[CURRENT_SHORTCUT_PARAM_ID] is MpAvShortcutRecorderParameterViewModel scrpvm) {
                    //    scrpvm.OnPropertyChanged(nameof(scrpvm.KeyGroups));
                    //}
                    break;
            }
        }
        private void ActionArgs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (MpAvParameterViewModelBase pvm in e.NewItems) {
                    if (pvm is MpAvShortcutRecorderParameterViewModel srpvm) {
                        srpvm.ShortcutType = MpShortcutType.None;
                        srpvm.IsRawInput = true;
                    }
                }
            }
        }

        #endregion

        #region Commands

        #endregion
    }
}
