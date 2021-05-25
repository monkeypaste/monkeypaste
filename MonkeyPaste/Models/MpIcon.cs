using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpIcon : MpDbObject {
        #region Columns
        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        public int IconImageId { get; set; }
        [ForeignKey(typeof(MpDbImage))]
        public int IconBorderImageId { get; set; }
        [ForeignKey(typeof(MpDbImage))]
        public int IconBorderHighlightImageId { get; set; }
        [ForeignKey(typeof(MpDbImage))]
        public int IconBorderHighlightSelectedImageId { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int Color1Id { get; set; }
        [ForeignKey(typeof(MpColor))]
        public int Color2Id { get; set; }
        [ForeignKey(typeof(MpColor))]
        public int Color3Id { get; set; }
        [ForeignKey(typeof(MpColor))]
        public int Color4Id { get; set; }
        [ForeignKey(typeof(MpColor))]
        public int Color5Id { get; set; }
        #endregion

        #region Fk Objects
        [OneToOne]
        public MpDbImage IconImage { get; set; }
        [OneToOne]
        public MpDbImage IconBorderImage { get; set; }
        [OneToOne]
        public MpDbImage IconBorderHighlightImage { get; set; }
        [OneToOne]
        public MpDbImage IconBorderHighlightSelectedImage { get; set; }

        [ManyToOne]
        public MpColor Color1 { get; set; }
        [ManyToOne]
        public MpColor Color2 { get; set; }
        [ManyToOne]
        public MpColor Color3 { get; set; }
        [ManyToOne]
        public MpColor Color4 { get; set; }
        [ManyToOne]
        public MpColor Color5 { get; set; }
        #endregion

        public MpIcon() : base(typeof(MpIcon)) { }
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
