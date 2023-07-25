

using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileTitleHighlightBehavior : MpAvTextBoxBaseHighlightBehavior {
        protected override bool CanMatch() {
            return Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasTitleMatchFilterFlag());
        }

        public override MpHighlightType HighlightType =>
            MpHighlightType.Title;
        public override MpContentQueryBitFlags AcceptanceFlags =>
            MpContentQueryBitFlags.Title;
    }
}
