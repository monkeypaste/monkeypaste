using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpDynamicTextTemplateViewModel : MpAvTextTemplateViewModelBase {
        #region Constructors
        public MpDynamicTextTemplateViewModel() : base(null) { }

        public MpDynamicTextTemplateViewModel(MpAvTemplateCollectionViewModel parent) : base(parent) { }

        #endregion
    }
}
