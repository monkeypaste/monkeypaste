using MonkeyPaste;
using System;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAnalyticItemResultViewModel : MpAnalyticItemComponentViewModel {
        #region Properties

        #region State

        public bool HasResult => !string.IsNullOrEmpty(Result);

        #endregion

        #region Model
        public string Result { get; set; }
        #endregion

        #endregion

        #region Constructors
        public MpAnalyticItemResultViewModel() : base(null) { }

        public MpAnalyticItemResultViewModel(MpAnalyticItemViewModel parent) : base(parent) { }


        #endregion
    }
}
