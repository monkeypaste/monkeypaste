using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;
using Xamarin.Forms;
using FFImageLoading.Forms;
using SkiaSharp;
using System.Linq;

namespace MonkeyPaste {
    public class MpIcon : MpDbModelBase {
        #region Columns
        [PrimaryKey,AutoIncrement]
        [Column("pk_MpIconId")]
        public override int Id { get; set; }

        [Column("MpIconGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid IconGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconDbImageId")]
        public int IconImageId { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconBorderDbImageId")]
        public int IconBorderImageId { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconHighlightBorderDbImageId")]
        public int IconBorderHighlightImageId { get; set; }
        
        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_IconSelectedHighlightBorderDbImageId")]
        public int IconBorderHighlightSelectedImageId { get; set; }
        #endregion

        #region Fk Objects
        [OneToOne(foreignKey:nameof(IconImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconImage { get; set; }

        [OneToOne(foreignKey: nameof(IconBorderImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconBorderImage { get; set; }

        [OneToOne(foreignKey: nameof(IconBorderHighlightImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconBorderHighlightImage { get; set; }

        [OneToOne(foreignKey: nameof(IconBorderHighlightSelectedImageId), CascadeOperations = CascadeOperation.All)]
        public MpDbImage IconBorderHighlightSelectedImage { get; set; }

        public string HexColor1 { get; set; }
        public string HexColor2 { get; set; }
        public string HexColor3 { get; set; }
        public string HexColor4 { get; set; }
        public string HexColor5 { get; set; }
        #endregion

        public static async Task<List<string>> CreatePrimaryColorList(SKBitmap skbmp, int listCount = 5) {
            //var sw = new Stopwatch();
            //sw.Start();

            var primaryIconColorList = new List<string>();
            var hist = await MpImageHistogram.Instance.GetStatistics(skbmp);
            foreach (var kvp in hist) {
                var c = Color.FromRgba(kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, 255);

                //Console.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if (primaryIconColorList.Count == listCount) {
                    break;
                }
                //between 0-255 where 0 is black 255 is white
                var rgDiff = Math.Abs((int)c.R - (int)c.G);
                var rbDiff = Math.Abs((int)c.R - (int)c.B);
                var gbDiff = Math.Abs((int)c.G - (int)c.B);
                var totalDiff = rgDiff + rbDiff + gbDiff;

                //0-255 0 is black
                var grayScaleValue = 0.2126 * (int)c.R + 0.7152 * (int)c.G + 0.0722 * (int)c.B;
                var relativeDist = primaryIconColorList.Count == 0 ? 1 : MpHelpers.Instance.ColorDistance(Color.FromHex(primaryIconColorList[primaryIconColorList.Count - 1]), c);
                if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
                    primaryIconColorList.Add(c.ToHex());
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < listCount; i++) {
                primaryIconColorList.Add(MpHelpers.Instance.GetRandomColor().ToHex());
            }

            //sw.Stop();
            //Console.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }

        public static async Task<MpIcon> GetIconById(int id) {
            var allicons = await MpDb.Instance.GetItemsAsync<MpIcon>();
            return allicons.Where(x => x.Id == id).FirstOrDefault();
        }

        public static async Task<MpIcon> Create(string iconImgBase64) {            
            var newImage = new MpDbImage() {
                //ImageBytes = iconImg
                ImageBase64 = iconImgBase64
            };
            await MpDb.Instance.AddItemAsync<MpDbImage>(newImage);
            
            var iconSkBmp = new MpImageConverter().Convert(iconImgBase64, typeof(SKBitmap)) as SKBitmap;
            var colorList = await CreatePrimaryColorList(iconSkBmp);
            // TODO create border images here
            var newIcon = new MpIcon() {
                //IconImageId = newImage.Id,
                IconImage = newImage,
                HexColor1 = colorList[0],
                HexColor2 = colorList[1],
                HexColor3 = colorList[2],
                HexColor4 = colorList[3],
                HexColor5 = colorList[4],
            };
            await MpDb.Instance.AddItemAsync<MpIcon>(newIcon);

            return newIcon;
        }
        public MpIcon() {
        }

        


        //public override void DeleteFromDatabase() {
        //    throw new NotImplementedException();
        //}

        //public override string ToString() {
        //    throw new NotImplementedException();
        //}

        //public override void WriteToDatabase() {
        //    throw new NotImplementedException();
        //}

        //private BitmapSource CreateBorder(BitmapSource img, double scale) {
        //    var border = MpHelpers.Instance.TintBitmapSource(img, Colors.White);
        //    var borderSize = new Size(img.Width * scale, img.Height * scale);
        //    return MpHelpers.Instance.ResizeBitmapSource(border, borderSize);
        //}
    }
}
