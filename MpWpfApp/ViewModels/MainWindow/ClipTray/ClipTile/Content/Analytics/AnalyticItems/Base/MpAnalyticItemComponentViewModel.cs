namespace MpWpfApp {
    public abstract class MpAnalyticItemComponentViewModel : MpViewModelBase<MpAnalyticItemViewModel> {
        #region Properties

        #region State

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemComponentViewModel() : base(null) { }

        public MpAnalyticItemComponentViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion
    }
}
