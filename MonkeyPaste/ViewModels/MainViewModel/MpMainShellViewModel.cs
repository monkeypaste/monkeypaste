using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpMainShellViewModel : MpViewModelBase {
        public MpTagCollectionViewModel TagCollectionViewModel { get; set; }

        public MpMainShellViewModel() {
            TagCollectionViewModel = new MpTagCollectionViewModel();
        }
    }
}
