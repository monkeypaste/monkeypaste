using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvApplication = Avalonia.Application;

namespace MonkeyPaste.Avalonia.Web {
    public class MpAvBrScreenInfo : MpAvScreenInfoBase {
        public override bool IsPrimary =>
            true;

        public MpAvBrScreenInfo() {
            if (AvApplication.Current is ISingleViewApplicationLifetime mobile &&
                mobile.MainView.GetVisualRoot() is IRenderRoot rr) {
                Scaling = 1;
                Bounds = new MpRect(MpPoint.Zero, rr.ClientSize.ToPortableSize());
                WorkArea = Bounds;
                IsPrimary = true;
            } else {
                Scaling = 2.25;
                Bounds = new MpRect(MpPoint.Zero, new MpSize(3840, 2160));
                WorkArea = Bounds;
                IsPrimary = true;
            }
        }
    }
}
