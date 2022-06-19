using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContactTemplateViewModel : MpViewModelBase<MpTemplateCollectionViewModel> {
        #region Properties

        #region Model
        public MpContact Contact { get; set; }

        public MpTextTemplate TextToken { get; set; }
        #endregion

        #endregion

        #region Constructor

        public MpContactTemplateViewModel(MpTemplateCollectionViewModel parent) : base(parent) {
        }

        #endregion
    }
}
