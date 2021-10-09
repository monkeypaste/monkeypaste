using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;
using Xamarin.Forms;
using SQLiteNetExtensions.Extensions.TextBlob;
using System.Text;

namespace MonkeyPaste {
    [Table("MpCopyItemTemplate")]
    public class MpCopyItemTemplate : MpDbModelBase, MpIQuilEmbedable, ICloneable {
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

        public string HexColor { get; set; }
        
        public string TemplateName { get; set; }

        [Ignore]
        public string TemplateText { get; set; }

        #endregion

        #region Fk Models

        [ManyToOne]
        public MpCopyItem CopyItem { get; set; }
        #endregion

        public static MpCopyItemTemplate Create(int copyItemId,string templateName, string templateColor = "") {
            var dupCheck = MpDb.Instance.GetItems<MpCopyItemTemplate>().Where(x =>x.CopyItemId == copyItemId && x.TemplateName.ToLower() == templateName.ToLower()).FirstOrDefault();
            if (dupCheck != null) {
                return dupCheck;
            }
            var newCopyItemTemplate = new MpCopyItemTemplate() {
                CopyItemTemplateGuid = System.Guid.NewGuid(),
                CopyItemId = copyItemId,
                TemplateName = templateName,
                HexColor = string.IsNullOrEmpty(templateColor) ? MpHelpers.Instance.GetRandomColor().ToHex() : templateColor
            };

            MpDb.Instance.AddOrUpdate<MpCopyItemTemplate>(newCopyItemTemplate);

            return newCopyItemTemplate;
        }

        public MpCopyItemTemplate() : base() { }

        public string ToHtml() {
            var c = string.IsNullOrEmpty(HexColor) ? Color.Red : Color.FromHex(HexColor);
            return string.Format(
                @"<span class='template_btn' contenteditable='false' templatename='{0}' templatecolor='{1}' templateid='{2}' style='background-color: {3}; color: {4};'>{0}</span>",
                TemplateName,
                HexColor,
                Id,
                string.Format(@"rgb({0},{1},{2})", c.R, c.G, c.B),
                MpHelpers.Instance.IsBright(c) ? "black" : "white");
        }

        public string ToDocToken() {
            return string.Format(
                @"{{{0},{1},{2}}}",
                Id,
                TemplateName,
                HexColor);
        }

        public string GetTokenName() {
            return "MpCopyItemTemplate";
        }

        public int GetTokenId() {
            return Id;
        }

        public string Serialize(object element) {
            var sb = new StringBuilder();
            foreach(var idx in element as List<int>) {
                sb.Append(idx);
            }
            return sb.ToString();
        }

        public object Deserialize(string text, Type type) {            
            var idxList = new List<int>();
            if(string.IsNullOrEmpty(text)) {
                return idxList;
            }
            foreach(var idx in text.Split(new string[] {","},StringSplitOptions.RemoveEmptyEntries)) {
                idxList.Add(Convert.ToInt32(idx));
            }
            return idxList;
        }

        public object Clone() {
            var ccit = new MpCopyItemTemplate() {
                Id = this.Id,
                CopyItemTemplateGuid = this.CopyItemTemplateGuid,
                TemplateName = this.TemplateName,
                HexColor = this.HexColor,
                TemplateText = this.TemplateText
            };
            return ccit;
        }
    }
}
