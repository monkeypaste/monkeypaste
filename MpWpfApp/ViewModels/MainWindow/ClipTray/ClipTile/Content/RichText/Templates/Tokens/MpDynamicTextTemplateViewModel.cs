using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpDynamicTextTemplateViewModel : MpTextTemplateViewModelBase {
        #region Constructors
        public MpDynamicTextTemplateViewModel() : base(null) { }

        public MpDynamicTextTemplateViewModel(MpTemplateCollectionViewModel parent) : base(parent) { }

        #endregion
    }
}
