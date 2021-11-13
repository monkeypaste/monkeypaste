using System;

namespace MpWpfApp {
    public class MpCheckBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        public override bool IsValid => true;
        #endregion

        #region Constructors

        public MpCheckBoxParameterViewModel() : base() { }

        public MpCheckBoxParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion
    }
}
