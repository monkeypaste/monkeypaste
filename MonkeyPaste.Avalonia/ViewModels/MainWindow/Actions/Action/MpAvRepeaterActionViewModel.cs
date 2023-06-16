using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvRepeaterActionViewModel :
        MpAvActionViewModelBase,
        MpISliderViewModel {

        #region Private Variables

        private DispatcherTimer _actionTickTimer;
        private object _lastArgs;

        #endregion

        #region Constants

        public const string REPEAT_DELAY_MS_PARAM_ID = "RepeatDelayMs";
        public const string REPEAT_COUNT_PARAM_ID = "RepeatCount";

        #endregion

        #region MpISliderViewModel Implementation
        public double SliderValue {
            get => (double)RepeatCount;
            set {
                if ((int)value != RepeatCount) {
                    RepeatCount = (int)value;
                    OnPropertyChanged(nameof(SliderValue));
                }
            }
        }
        public double MinValue => 0;
        public double MaxValue => TimeSpan.FromMinutes(10).TotalMilliseconds;
        public int Precision => 0;

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Repeat Count",
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Integer,
                                minimum = 0,
                                maximum = int.MaxValue,
                                isRequired = true,
                                paramId = REPEAT_COUNT_PARAM_ID,
                                description = "A value of 0 will repeat parent action indefinitely",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value ="0"
                                    }
                                }
                            },
                             new MpParameterFormat() {
                                label = "Repeat Delay",
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Integer,
                                minimum = 0,
                                maximum = int.MaxValue,
                                isRequired = true,
                                paramId = REPEAT_DELAY_MS_PARAM_ID,
                                description = "The amount of time in milliseconds waited once parent action completes",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value ="0"
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


        #region Model

        // Arg1
        public int RepeatCount {
            get {
                if (ArgLookup.TryGetValue(REPEAT_COUNT_PARAM_ID, out var param_vm) &&
                    param_vm.IntValue is int curVal) {
                    return curVal;
                }
                return 0;
            }
            set {
                if (RepeatCount != value) {
                    ArgLookup[REPEAT_COUNT_PARAM_ID].IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(RepeatCount));
                }
            }
        }

        public int RepeatDelayMs {
            get {
                if (ArgLookup.TryGetValue(REPEAT_DELAY_MS_PARAM_ID, out var param_vm) &&
                    param_vm.IntValue is int curVal) {
                    return curVal;
                }
                return 0;
            }
            set {
                if (RepeatDelayMs != value) {
                    ArgLookup[REPEAT_DELAY_MS_PARAM_ID].IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(RepeatDelayMs));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvRepeaterActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvTimerActionViewModel_PropertyChanged;

        }


        #endregion

        #region Public Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            bool isInitialRun = _actionTickTimer == null || !_actionTickTimer.IsEnabled;
            var actionInput = GetInput(arg);

            object[] args = new object[] { ParentActionViewModel, actionInput.CopyItem };
            if (ParentActionViewModel != null) {
                ParentActionViewModel.PerformActionAsync(args).FireAndForgetSafeAsync(this);

                if (_actionTickTimer == null) {
                    _actionTickTimer = new DispatcherTimer();
                    _actionTickTimer.Tick += _actionTickTimer_Tick;
                    ParentActionViewModel.OnActionComplete += SelectedTickActionViewModel_OnActionComplete;
                }
                _actionTickTimer.Interval = TimeSpan.FromMilliseconds(RepeatCount);
                if (!_actionTickTimer.IsEnabled) {
                    _actionTickTimer.Start();
                }
            }
            if (isInitialRun) {
                // no unique output for timer, just pass through
                await base.PerformActionAsync(args);
            }
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpAction action && action.Id == RepeatDelayMs) {
                Task.Run(ValidateActionAsync);
            }
        }

        protected override async Task ValidateActionAsync() {
            await base.ValidateActionAsync();
            if (!IsValid) {
                return;
            }
            //if (!IsValid) {
            //    return IsValid;
            //}

            //if(TickActionId == 0) {
            //    return IsValid;
            //}

            //if(TickActionId == ActionId) {
            //    ValidationText = $"Timed Action '{FullName}' cannot invoke itself";
            //} else if (SelectedTickActionViewModel == null) {
            //    ValidationText = $"Selected Timed Action for Timer '{FullName}' not found";
            //} else {
            //    ValidationText = string.Empty;
            //}
            //if(!string.IsNullOrEmpty(ValidationText)) {
            //    ShowValidationNotification();
            //}
            //return IsValid;
            await Task.Delay(1);
        }

        #endregion

        #region Private Methods

        private void MpAvTimerActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(RootTriggerActionViewModel):
                    if (RootTriggerActionViewModel == null) {
                        break;
                    }
                    RootTriggerActionViewModel.PropertyChanged += RootTriggerActionViewModel_PropertyChanged;
                    break;
            }
        }

        private void RootTriggerActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(RootTriggerActionViewModel.IsEnabled):
                    if (!RootTriggerActionViewModel.IsEnabled) {
                        if (_actionTickTimer != null && _actionTickTimer.IsEnabled) {
                            _actionTickTimer.Stop();
                        }
                    }
                    break;
            }
        }

        private void _actionTickTimer_Tick(object sender, EventArgs e) {
            PerformActionAsync(_lastArgs).FireAndForgetSafeAsync(this);
        }

        private void SelectedTickActionViewModel_OnActionComplete(object sender, object e) {
            _lastArgs = e;
        }

        #endregion

        #region Commands
        #endregion
    }
}
