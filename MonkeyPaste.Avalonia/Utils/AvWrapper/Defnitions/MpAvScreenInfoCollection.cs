using Avalonia.Controls;
using Avalonia.Rendering;
using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfoCollection : MpIPlatformScreenInfoCollection {
        private Window _w;

        public IEnumerable<MpIPlatformScreenInfo> Screens {
            get => _w.Screens.All
                        .Select((x, i) =>
                            new MpAvScreenInfo() {
                                Bounds = new MpRect(
                                    new MpPoint(
                                        x.Bounds.X / ((IRenderRoot)_w).RenderScaling, 
                                        x.Bounds.Y) / ((IRenderRoot)_w).RenderScaling,
                                    new MpSize(
                                        x.Bounds.Width / ((IRenderRoot)_w).RenderScaling, 
                                        x.Bounds.Height / ((IRenderRoot)_w).RenderScaling)),
                                IsPrimary = x.Primary,
                                Name = $"Monitor {i}",
                                WorkArea = new MpRect(
                                    new MpPoint(
                                        x.WorkingArea.X / ((IRenderRoot)_w).RenderScaling, 
                                        x.WorkingArea.Y / ((IRenderRoot)_w).RenderScaling),
                                    new MpSize(
                                        x.WorkingArea.Width / ((IRenderRoot)_w).RenderScaling, 
                                        x.WorkingArea.Height / ((IRenderRoot)_w).RenderScaling)),
                                PixelDensity = ((IRenderRoot)_w).RenderScaling
                            });


            //get => new List<MpAvScreenInfo>() {
            //                    new MpAvScreenInfo() {
            //                        Bounds = new MpRect(
            //                            new MpPoint(_w.Bounds.X,_w.Bounds.Y),
            //                            new MpSize(_w.Bounds.Width, _w.Bounds.Height)),
            //                        IsPrimary = true,
            //                        Name = $"Monitor 0",
            //                        WorkArea = new MpRect(
            //                            new MpPoint(_w.Bounds.X,_w.Bounds.Y),
            //                            new MpSize(_w.Bounds.Width, _w.Bounds.Height)),
            //                        PixelDensity = 1.75
            //                    } 
            //};
            set => throw new System.Exception("Not supported");
        }
            

        public MpAvScreenInfoCollection(Window w) => _w = w;
    }
}
