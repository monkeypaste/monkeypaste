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

        public static FlowDocument operator +(MpEventEnabledFlowDocument a, MpEventEnabledFlowDocument b) {
            if (a == null || b == null) {
                if (a == null && b == null) {
                    return string.Empty.ToFlowDocument();
                }
                if (a == null) {
                    return b;
                }
                return a;
            }
            return MpWpfRichDocumentExtensions.CombineFlowDocuments(a, b, true);
        }
    }
}
