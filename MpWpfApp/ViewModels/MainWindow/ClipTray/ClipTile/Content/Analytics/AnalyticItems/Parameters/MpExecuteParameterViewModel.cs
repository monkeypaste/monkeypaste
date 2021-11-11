using System;

namespace MpWpfApp {
    public class MpExecuteParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties
        public override bool IsValid => true;
        #endregion

        #region Constructors

        public MpExecuteParameterViewModel() : base() { }

        public MpExecuteParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion
    }
}
