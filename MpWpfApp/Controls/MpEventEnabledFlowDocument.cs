using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpEventEnabledFlowDocument : FlowDocument {
        protected override bool IsEnabledCore {
            get {
                return true;
            }
        }
        public MpEventEnabledFlowDocument() : base() {
            //IsOptimalParagraphEnabled = true;
        }
    }
}
