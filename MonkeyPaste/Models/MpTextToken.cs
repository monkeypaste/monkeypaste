﻿using System;
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
    [Table("MpTextToken")]
    public class MpTextToken : MpDbModelBase, MpIQuilEmbedable, ICloneable {
        #region Constants
        public const string TEMPLATE_PREFIX = "<";
        public const string TEMPLATE_SUFFIX = ">";
        #endregion

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTextTokenId")]
        public override int Id { get; set; }

        [Column("MpTextTokenGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid TextTokenGuid {
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

        [Ignore]
        public string TemplateToken {
            get {
                return string.Format(
                                        @"{0}{1}{2}",
                                        TEMPLATE_PREFIX,
                                        TemplateName,
                                        TEMPLATE_SUFFIX);
            }
        }

        #endregion

        #region Fk Models

        //[ManyToOne]
        //public MpCopyItem CopyItem { get; set; }
        #endregion

        public static async Task<MpTextToken> Create(int copyItemId,string templateName, string templateColor = "") {
            var dupCheck = await MpDataModelProvider.GetTemplateByNameAsync(copyItemId, templateName); //MpDb.GetItems<MpTextToken>().Where(x =>x.CopyItemId == copyItemId && x.TemplateName.ToLower() == templateName.ToLower()).FirstOrDefault();
            if (dupCheck != null) {
                return dupCheck;
            }
            var newTextToken = new MpTextToken() {
                TextTokenGuid = System.Guid.NewGuid(),
                CopyItemId = copyItemId,
                TemplateName = templateName,
                HexColor = string.IsNullOrEmpty(templateColor) ? MpHelpers.GetRandomColor().ToHex() : templateColor
            };

            await newTextToken.WriteToDatabaseAsync();

            return newTextToken;
        }

        public MpTextToken() : base() { }

        public string ToHtml() {
            var c = string.IsNullOrEmpty(HexColor) ? Color.Red : Color.FromHex(HexColor);
            return string.Format(
                @"<span class='template_btn' contenteditable='false' templatename='{0}' templatecolor='{1}' templateid='{2}' style='background-color: {3}; color: {4};'>{0}</span>",
                TemplateName,
                HexColor,
                Id,
                string.Format(@"rgb({0},{1},{2})", c.R, c.G, c.B),
                MpHelpers.IsBright(c) ? "black" : "white");
        }

        public string ToDocToken() {
            return string.Format(
                @"{{{0},{1},{2}}}",
                Id,
                TemplateName,
                HexColor);
        }

        public string GetTokenName() {
            return "MpTextToken";
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
            return Clone(false);
        }

        public object Clone(bool isReplica) {
            var ccit = new MpTextToken() {                
                CopyItemId = this.CopyItemId,
                TemplateName = this.TemplateName,
                HexColor = this.HexColor,
                TemplateText = this.TemplateText,
                Id = isReplica ? 0 : this.Id,
                TextTokenGuid = isReplica ? System.Guid.NewGuid() : this.TextTokenGuid,
            };
            return ccit;
        }
    }
}