using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpRichTextBox : RichTextBox {
        public new MpEventEnabledFlowDocument Document { get; set; }
    }
}
