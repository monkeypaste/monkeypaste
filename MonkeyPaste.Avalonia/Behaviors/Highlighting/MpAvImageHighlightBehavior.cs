using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvImageHighlightBehavior : MpAvHighlightBehaviorBase<MpAvClipTileContentView> {
        protected override MpTextRange ContentRange => null;

        public override MpHighlightType HighlightType => MpHighlightType.Content;

        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
        }

        public override void ClearHighlighting() {
            base.ClearHighlighting();
        }

        public override async Task ApplyHighlightingAsync() {
            await Task.Delay(1);
        }
    }
}
