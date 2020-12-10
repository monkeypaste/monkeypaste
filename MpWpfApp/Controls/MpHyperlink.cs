using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpHyperlink : Hyperlink {
        public MpHyperlink() : this(new MpTemplateHyperlinkViewModel()) {
        }

        public MpHyperlink(MpTemplateHyperlinkViewModel thlvm) : base() {
            DataContext = thlvm;
        }
    }
}
