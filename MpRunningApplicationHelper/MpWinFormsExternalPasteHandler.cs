using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;

namespace MpProcessHelper {
    public class MpWinFormsExternalPasteHandler : MpIExternalPasteHandler {

        #region MpIExternalPasteHandler Implementation

        public async Task PasteDataObject(MpDataObject mpdo, IntPtr handle, bool finishWithEnterKey = false) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
