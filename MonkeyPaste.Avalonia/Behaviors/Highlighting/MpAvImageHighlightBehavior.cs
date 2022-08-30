using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvImageHighlightBehavior : MpAvHighlightBehaviorBase<MpAvClipTileContentView> {
        protected override MpAvITextRange ContentRange => null;

        public override MpHighlightType HighlightType => MpHighlightType.Content;

        public override async Task ScrollToSelectedItemAsync() {
            await Task.Delay(1);
        }
    }
}
