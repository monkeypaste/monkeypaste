namespace MpWpfApp {
    public class MpResultParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        public bool HasResult => !string.IsNullOrEmpty(SelectedValue?.Value);

        public override bool IsValid => true;

        #endregion

        #region Constructors

        public MpResultParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion
    }
}
