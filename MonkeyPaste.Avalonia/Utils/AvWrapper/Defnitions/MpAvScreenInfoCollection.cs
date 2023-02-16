using Avalonia.Rendering;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfoCollection : MpIPlatformScreenInfoCollection {

        public double PixelScaling {
            get {
                //return 1.0d;

                if (App.Desktop == null || App.Desktop.MainWindow == null ||
                    OperatingSystem.IsMacOS()) {
                    // NOTE windows seems to now account for scaling on bounds/workarea but mac does
                    // maybe this is a project setting thing?
                    return 1.0d;
                }

                return ((IRenderRoot)App.Desktop.MainWindow).RenderScaling;
            }
        }

        public IEnumerable<MpIPlatformScreenInfo> Screens {
            get {
                if (App.Desktop == null || App.Desktop.MainWindow == null) {
                    return new List<MpIPlatformScreenInfo>();
                }

                return
                    App.Desktop.MainWindow.Screens.All
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
