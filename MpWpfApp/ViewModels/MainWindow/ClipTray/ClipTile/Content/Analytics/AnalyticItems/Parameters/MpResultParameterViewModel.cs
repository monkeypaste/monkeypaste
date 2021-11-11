using System;

namespace MpWpfApp {
    public class MpResultParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        public bool HasResult => !string.IsNullOrEmpty(CurrentValueViewModel?.Value);

        public override bool IsValid => true;

        public string Result {
            get {
                return CurrentValueViewModel.Value;
            }
            set {
                if(CurrentValueViewModel.Value != value) {
                    CurrentValueViewModel.Value = value;
                    OnPropertyChanged(nameof(Result));
                    OnPropertyChanged(nameof(HasResult));
                }
            }
        }
        #endregion

        #region Constructors
        public MpResultParameterViewModel() : base() { }

        public MpResultParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }


        #endregion
    }
}
