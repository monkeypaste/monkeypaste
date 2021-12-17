using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpImageHighlightBehavior : MpHighlightBehaviorBase<MpImageItemView> {
        protected override TextRange ContentRange => null;

        public override MpHighlightType HighlightType => MpHighlightType.Content;

        protected override void ScrollToSelectedItem() {
        }
    }
}
