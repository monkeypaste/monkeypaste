using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using SkiaSharp;
using SQLite;
using SQLiteNetExtensions.Attributes;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpColor : MpDbModelBase {                       
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        [PrimaryKey,AutoIncrement]
        [Column("pk_MpColorId")]
        public override int Id { get; set; }

        [Ignore]
        public Color Color {
            get {
                return Color.FromRgba(R, G, B, A);
            }
            set {   
                R = (byte)(value.R * 255);
                G = (byte)(value.G * 255);
                B = (byte)(value.B * 255);
                A = (byte)(value.A * 255);
            }
        }

        [Ignore]
        public Brush ColorBrush {
            get {
                if(Color == null) {
                    return Brush.Red;
                }
                return new SolidColorBrush(Color);
            }
        }
        public MpColor() : base(typeof(MpColor)) { }

        public MpColor(double r, double g, double b, double a) : this() {
            R = (byte)(r * 255);
            G = (byte)(g * 255);
            B = (byte)(b * 255);
            A = (byte)(a * 255);
        }

        public MpColor(Color c) : this(c.R, c.G, c.B, c.A) { }

        public static async Task<MpColor> GetColorById(int colorId) {
            var allColors = await MpDb.Instance.GetItems<MpColor>();
            var udbpl = allColors.Where(x => x.Id == colorId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static async Task<List<MpColor>> CreatePrimaryColorList(SKBitmap skbmp, int listCount = 5) {
            //var sw = new Stopwatch();
            //sw.Start();
            
            var primaryIconColorList = new List<MpColor>();
            var hist = await MpImageHistogram.Instance.GetStatistics(skbmp);
            foreach (var kvp in hist) {
                var c = new MpColor(kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, 255);

                //Console.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if (primaryIconColorList.Count == listCount) {
                    break;
                }
                //between 0-255 where 0 is black 255 is white
                var rgDiff = Math.Abs((int)c.Color.R - (int)c.Color.G);
                var rbDiff = Math.Abs((int)c.Color.R - (int)c.Color.B);
                var gbDiff = Math.Abs((int)c.Color.G - (int)c.Color.B);
                var totalDiff = rgDiff + rbDiff + gbDiff;

                //0-255 0 is black
                var grayScaleValue = 0.2126 * (int)c.Color.R + 0.7152 * (int)c.Color.G + 0.0722 * (int)c.Color.B;
                var relativeDist = primaryIconColorList.Count == 0 ? 1 : MpHelpers.Instance.ColorDistance(primaryIconColorList[primaryIconColorList.Count - 1].Color, c.Color);
                if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
                    primaryIconColorList.Add(c);
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < listCount; i++) {
                primaryIconColorList.Add(new MpColor(MpHelpers.Instance.GetRandomColor()));
            }
            //sw.Stop();
            //Console.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }
    }
}
