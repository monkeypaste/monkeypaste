using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpContentDescriptionViewModel : MpViewModelBase<MpContentItemViewModel> {
        #region Private Variables

        #endregion

        #region Public Methods
        public MpContentDescriptionViewModel() : base(null) { }

        public MpContentDescriptionViewModel(MpContentItemViewModel parent) : base(parent) {

        }
        #endregion
    }
}
