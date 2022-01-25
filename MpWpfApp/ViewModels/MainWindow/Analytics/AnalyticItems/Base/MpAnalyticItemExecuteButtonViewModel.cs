using MonkeyPaste;
using System;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAnalyticItemExecuteButtonViewModel<T> : MpAnalyticItemComponentViewModel<T> where T:Enum{
        #region Properties
        #endregion

        #region Constructors

        public MpAnalyticItemExecuteButtonViewModel() : base(null) { }

        public MpAnalyticItemExecuteButtonViewModel(MpAnalyticItemViewModel<T> parent) : base(parent) { }

        #endregion
    }
}
