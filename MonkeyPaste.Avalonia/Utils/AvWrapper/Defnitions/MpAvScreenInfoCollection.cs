
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfoCollection : MpIPlatformScreenInfoCollection {

        public double PixelScaling {
            get {
                //return 1.0d;
                if (OperatingSystem.IsMacOS()) {
                    // NOTE windows seems to now account for scaling on bounds/workarea but mac does
                    // maybe this is a project setting thing?
                    return 1.0d;
                }
                if (App.MainWindow is Control c) {
                    return c.GetVisualRoot().RenderScaling;
                }

                return 1.0d;
            }
        }

        public IEnumerable<MpIPlatformScreenInfo> Screens {
            get {
                IEnumerable<Screen> screens = null;
                if (App.MainWindow is Window w) {
                    screens = w.Screens.All;
                } else if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime mobile &&
                            mobile.MainView is Control mainView &&
                            mainView.GetVisualRoot() is IRenderRoot rr) {
                    // NOTE Pretty sure client size is equiv to workarea but
                    // either way not sure how to get bounds (or workarea if vice versa) here 
                    screens = new[] {
                        rr.AsScreen(PixelScaling)
                    };
                }
                if (screens == null) {
                    return new List<MpIPlatformScreenInfo>();
                }

                return screens.Select((x, idx) => new MpAvScreenInfo(x, idx));
            }
        }

        public MpAvScreenInfoCollection() {

        }
    }
}
