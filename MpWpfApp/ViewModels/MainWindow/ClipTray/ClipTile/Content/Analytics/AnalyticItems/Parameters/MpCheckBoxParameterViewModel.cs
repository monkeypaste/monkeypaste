using System;

namespace MpWpfApp {
    public class MpCheckBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        //public bool IsChecked {
        //    get {
        //        if (CurrentValueViewModel == null) {
        //            return false;
        //        }
        //        //if (string.IsNullOrEmpty(Parameter.ValueCsv) &&
        //        //   !string.IsNullOrEmpty(Parameter.DefaultValue)) {
        //        //    Parameter.ValueCsv = Parameter.DefaultValue;
        //        //}
        //        //if(string.IsNullOrEmpty(Parameter.ValueCsv)) {
        //        //    Parameter.ValueCsv = "0";
        //        //}
        //        //return Parameter.ValueCsv == "1";
        //        return CurrentValueViewModel.BoolValue;
        //    }
        //    set {
        //        if (IsChecked != value) {
        //            CurrentValueViewModel.Value = value ? "1":"0";
        //            OnPropertyChanged(nameof(IsChecked));
        //        }
        //    }
        //}

        public override bool IsValid => true;
        #endregion

        #region Constructors

        public MpCheckBoxParameterViewModel() : base() { }

        public MpCheckBoxParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }


        #endregion
    }
}
