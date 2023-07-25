using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvContentTextBoxHighlightBehavior : MpAvTextBoxBaseHighlightBehavior {
        protected override bool CanMatch() {
            return Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasContentMatchFilterFlag());
        }

        public override MpHighlightType HighlightType =>
            MpHighlightType.Content;
        public override MpContentQueryBitFlags AcceptanceFlags =>
            MpContentQueryBitFlags.Annotations |
            MpContentQueryBitFlags.Content;
    }
}
