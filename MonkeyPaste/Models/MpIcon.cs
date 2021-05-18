using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpIcon : MpDbObject {
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        public byte[] IconImage { get; set; }
        public byte[] IconBorderImage { get; set; }
        public byte[] IconHighlightBorderImage { get; set; }
        public byte[] IconHighlightSelectedBorderImage { get; set; }

        public string Color1 { get; set; }
        public string Color2 { get; set; }
        public string Color3 { get; set; }
        public string Color4 { get; set; }
        public string Color5 { get; set; }

        //private BitmapSource CreateBorder(BitmapSource img, double scale) {
        //    var border = MpHelpers.Instance.TintBitmapSource(img, Colors.White);
        //    var borderSize = new Size(img.Width * scale, img.Height * scale);
        //    return MpHelpers.Instance.ResizeBitmapSource(border, borderSize);
        //}
    }
}
