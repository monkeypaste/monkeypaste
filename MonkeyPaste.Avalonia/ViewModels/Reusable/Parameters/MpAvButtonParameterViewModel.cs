using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvButtonParameterViewModel : MpAvParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvButtonParameterViewModel() : base(null) { }

        public MpAvButtonParameterViewModel(MpViewModelBase parent) : base(parent) { }


        #endregion

        #region Commands

        public ICommand ClickCommand { get; set; } // must be set at runtime
        #endregion
    }
}
