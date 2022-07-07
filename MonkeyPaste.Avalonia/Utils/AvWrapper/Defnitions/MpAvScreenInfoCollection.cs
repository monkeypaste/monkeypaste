using Avalonia.Controls;
using Avalonia.Rendering;
using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfoCollection : MpIPlatformScreenInfoCollection {
        private Window _w;

        public double PixelScaling {
            get {
                if (_w == null ||
                    OperatingSystem.IsMacOS()) {
                    // NOTE windows seems to now account for scaling on bounds/workarea but mac does
                    // maybe this is a project setting thing?
                    return 1.0d;
                }

                return ((IRenderRoot)_w).RenderScaling;
            }
        }

        public IEnumerable<MpIPlatformScreenInfo> Screens => _w.Screens.All
            .Select((x, i) =>
                new MpAvScreenInfo() {
                    Bounds = new MpRect(
                        new MpPoint(
                            x.Bounds.X / PixelScaling, 
                            x.Bounds.Y) / PixelScaling,
                        new MpSize(
                            x.Bounds.Width / PixelScaling, 
                            x.Bounds.Height / PixelScaling)),
                    IsPrimary = x.Primary,
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
            

        public MpAvScreenInfoCollection(Window w) => _w = w;
    }
}
