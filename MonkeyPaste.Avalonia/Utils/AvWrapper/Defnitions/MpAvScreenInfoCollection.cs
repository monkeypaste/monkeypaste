
using Avalonia;
using Avalonia.Controls;
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
                } else if (App.MainWindow is Control mainView) {
                    // NOTE Pretty sure client size is equiv to workarea but
                    // either way not sure how to get bounds (or workarea if vice versa) here 
                    screens = new[] {
                        new Screen(
                            PixelScaling,
                            new PixelRect(mainView.GetVisualRoot().ClientSize.ToAvPixelSize()),
                            new PixelRect(mainView.GetVisualRoot().ClientSize.ToAvPixelSize()),
                            true)
                    };
                }
                if (screens == null) {
                    return new List<MpIPlatformScreenInfo>();
                }

                return
                   screens
                    .Select((x, i) =>
                        new MpAvScreenInfo() {
                            Bounds = new MpRect(
                                new MpPoint(
                                    x.Bounds.X / PixelScaling,
                                    x.Bounds.Y) / PixelScaling,
                                new MpSize(
                                    x.Bounds.Width / PixelScaling,
                                    x.Bounds.Height / PixelScaling)),
                            IsPrimary = x.IsPrimary,
                            Name = $"Monitor {i}",
                            WorkArea = new MpRect(
                                new MpPoint(
                                    x.WorkingArea.X / PixelScaling,
                                    x.WorkingArea.Y / PixelScaling),
                                new MpSize(
                                    x.WorkingArea.Width / PixelScaling,
                                    x.WorkingArea.Height / PixelScaling)),
                            PixelDensity = PixelScaling
                        });
            }
        }

        public MpAvScreenInfoCollection() {

        }
    }
}
