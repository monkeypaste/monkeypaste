using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public class MpAnalyzerTransaction : MpPluginTransaction {
        public DateTime RequestTime { get; set; }
        public DateTime? ResponseTime { get; set; }

        public object Request { get; set; }
        public object Response { get; set; }

        public object RequestContent { get; set; }
        public object ResponseContent { get; set; }

    }
}
