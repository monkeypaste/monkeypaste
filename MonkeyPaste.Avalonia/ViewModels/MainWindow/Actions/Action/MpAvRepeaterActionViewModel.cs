using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpRepeatType {
        None = 0,
        Time,
        Count
    }
    public class MpAvRepeaterActionViewModel : 
        MpAvActionViewModelBase,
        MpIPopupSelectorMenu,
        MpISliderViewModel {

        #region Private Variables

        private DispatcherTimer _actionTickTimer;
        private object _lastArgs;

        #endregion

        #region MpISliderViewModel Implementation
        public double SliderValue {
            get => (double)IntervalMs;
            set {
                if((int)value != IntervalMs) {
                    IntervalMs = (int)value;
                    OnPropertyChanged(nameof(SliderValue));
                }
            }
        }
        public double MinValue => 0;
        public double MaxValue => TimeSpan.FromMinutes(10).TotalMilliseconds;
        public int Precision => 0;

        #endregion

        #region MpIPopupSelectorMenu Implementation
        public bool IsOpen { get; set; }
        public MpMenuItemViewModel PopupMenu =>
            RootTriggerActionViewModel == null ? null : 
            RootTriggerActionViewModel.GetActionMenu(SelectTickActionCommand, new List<int> { TickActionId }, true);
        public MpMenuItemViewModel SelectedMenuItem =>
            SelectedTickActionViewModel == null ?
                null :
            SelectedTickActionViewModel.GetActionMenu(null, new int[] { TickActionId }, false);

        public string EmptyText => "Select Action...";
        public object EmptyIconResourceObj => GetDefaultActionIconResourceKey(MpActionType.Analyze, null);

        #endregion

        #region Properties

        #region View Models

        public MpAvActionViewModelBase SelectedTickActionViewModel =>
            Parent == null ? null : Parent.Items.FirstOrDefault(x => x.ActionId == TickActionId);

        #endregion

        #region Model

        // Arg1
        public int IntervalMs {
            get {
                if(Action == null || string.IsNullOrEmpty(Arg1)) {
                    return 0;
                }
                return int.Parse(Arg1);
            }
            set {
                if(IntervalMs != value) {
                    Arg1 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IntervalMs));
                }
            }
        }

        public int TickActionId {
            get {
                if (Action == null || string.IsNullOrEmpty(Arg2)) {
                    return 0;
                }
                return int.Parse(Arg2);
            }
            set {
                if (TickActionId != value) {
                    Arg2 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TickActionId));
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
            if (!CanPerformAction(arg)) {
                return;
            }

            bool isInitialRun = _actionTickTimer == null || !_actionTickTimer.IsEnabled;
            var actionInput = GetInput(arg);

            object[] args = new object[] { SelectedTickActionViewModel, actionInput.CopyItem };
            if (SelectedTickActionViewModel != null) {
                SelectedTickActionViewModel.PerformActionAsync(args).FireAndForgetSafeAsync(this);

                if(_actionTickTimer == null) {
                    _actionTickTimer = new DispatcherTimer();
                    _actionTickTimer.Tick += _actionTickTimer_Tick;
                    SelectedTickActionViewModel.OnActionComplete += SelectedTickActionViewModel_OnActionComplete;
                }
                _actionTickTimer.Interval = TimeSpan.FromMilliseconds(IntervalMs);
                if(!_actionTickTimer.IsEnabled) {
                    _actionTickTimer.Start();
                }
            } 
            if(isInitialRun) {
                // no unique output for timer, just pass through
                await base.PerformActionAsync(args);
            }
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpAction action && action.Id == TickActionId) {
                Task.Run(ValidateActionAsync);
            }
        }

        protected override async Task ValidateActionAsync() {

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
            switch(e.PropertyName) {
                case nameof(RootTriggerActionViewModel):
                    if(RootTriggerActionViewModel == null) {
                        break;
                    }
                    RootTriggerActionViewModel.PropertyChanged += RootTriggerActionViewModel_PropertyChanged;
                    break;
            }
        }

        private void RootTriggerActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(RootTriggerActionViewModel.IsEnabled):
                    if (RootTriggerActionViewModel.IsEnabled.IsFalseOrNull()) {
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

        public ICommand SelectTickActionCommand => new MpCommand<object>(
            (args) => {
                if (args is int tickActionId) {
                    TickActionId = tickActionId;
                    OnPropertyChanged(nameof(SelectedTickActionViewModel));
                    OnPropertyChanged(nameof(SelectedMenuItem));
                }
            });


        #endregion
    }
}
