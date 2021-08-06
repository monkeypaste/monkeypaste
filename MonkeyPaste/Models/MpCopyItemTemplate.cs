using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;
using Xamarin.Forms;

namespace MonkeyPaste {
    [Table("MpCopyItemTemplate")]
    public class MpCopyItemTemplate : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemTemplateId")]
        public override int Id { get; set; }

        [Column("MpCopyItemTemplateGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid CopyItemTemplateGuid {
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

        
        [Column("fk_MpCopyItemId")]
        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId")]
        public int ColorId { get; set; }

        public string TemplateName { get; set; }

        [Ignore]
        public string TemplateText { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpCopyItem CopyItem { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpColor Color { get; set; }
        #endregion

        public MpCopyItemTemplate() {
        }

        public string ToHtml() {
            // 
            return string.Format(
                @"<span class='template_btn' contenteditable='false' templatename='{0}' templatecolor='{1}' templateid='{2}' style='background-color: {3}; color: {4};'>{0}</span>",
                TemplateName,
                Color.Color.ToHex(),
                Id,
                string.Format(@"rgb({0},{1},{2})", Color.R, Color.G, Color.B),
                MpHelpers.Instance.IsBright(Color.Color) ? "black" : "white");
        }

        public string ToQuillEncoded() {
            return string.Format(
                @"{{{0}}}}",
                Id);
        }
    }
}
