using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpIcon : MpDbObject {
        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; }

        public byte[] IconImage { get; set; }
        public byte[] IconBorderImage { get; set; }
        public byte[] IconHighlightBorderImage { get; set; }
        public byte[] IconHighlightSelectedBorderImage { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int Color1Id { get; set; }
        [ManyToOne]
        public MpColor Color1 { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int Color2Id { get; set; }
        [ManyToOne]
        public MpColor Color2 { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int Color3Id { get; set; }
        [ManyToOne]
        public MpColor Color3 { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int Color4Id { get; set; }
        [ManyToOne]
        public MpColor Color4 { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int Color5Id { get; set; }
        [ManyToOne]
        public MpColor Color5 { get; set; }
 

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
