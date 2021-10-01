using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpContextMenuViewModel : MpViewModelBase<object>  {

        #region Properties
        public object TargetViewModel { get; set; }
        #endregion

        #region Public Methods
        public MpContextMenuViewModel() : base(null) { }

        public MpContextMenuViewModel(object target) : base(null) {
            TargetViewModel = target;
        }

        #endregion

        #region Private Methods

       

        #endregion
    }
}
