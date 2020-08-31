using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpRichTextTokenViewModel : MpViewModelBase {
        #region Private Variables

        private MpSubTextToken _token = null;

        #endregion

        #region Properties


        #endregion

        #region Publc Methods

        public MpRichTextTokenViewModel(MpSubTextToken token) {
            _token = token;
        }

        #endregion
    }
}
