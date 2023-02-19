
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfo : MpIPlatformScreenInfo {
        public MpRect Bounds { get; set; } = new MpRect();
        public MpRect WorkArea { get; set; } = new MpRect();
        public bool IsPrimary { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Scaling { get; set; }
        public MpPoint PixelsPerInch => new MpPoint(Scaling * 96, Scaling * 96);

        public MpAvScreenInfo() : base() { }

        public MpAvScreenInfo(Screen s) : this(s, 0) { }
        public MpAvScreenInfo(Screen s, int idx) {
            Name = $"Monitor {idx}";
            Scaling = s.Scaling;
            Bounds = s.Bounds.ToPortableRect(Scaling);
            WorkArea = s.WorkingArea.ToPortableRect(Scaling);
            IsPrimary = s.IsPrimary;


            //Bounds = new MpRect(
            //                    new MpPoint(
            //                        s.Bounds.X / Scaling,
            //                        s.Bounds.Y) / Scaling,
            //                    new MpSize(
            //                        s.Bounds.Width / Scaling,
            //                        s.Bounds.Height / Scaling));
            //WorkArea = new MpRect(
            //    new MpPoint(
            //        s.WorkingArea.X / Scaling,
            //        s.WorkingArea.Y / Scaling),
            //    new MpSize(
            //        s.WorkingArea.Width / Scaling,
            //        s.WorkingArea.Height / Scaling));
        }
    }
}
