using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public class MpAnalyzerTransaction : MpPluginTransactionBase {
        public object RequestContent { get; set; }
        public MpCopyItem ResponseContent { get; set; }
    }
}
