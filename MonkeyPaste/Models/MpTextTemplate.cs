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
using Newtonsoft.Json;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public enum MpTextTemplateType {
        None = 0,
        Dynamic,
        Static,
        Content,
        Analyzer,
        Action,
        Contact,
        DateTime
    }

    public class MpTextTemplate : MpDbModelBase, MpIClonableDbModel<MpTextTemplate> {
        #region Constants
        public const string TextTemplateOpenToken = @"\{t\{";
        public const string TextTemplateCloseToken = @"\}t\}";
        #endregion

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTextTokenId")]
        public override int Id { get; set; }

        [Column("MpTextTemplateGuid")]
        [JsonProperty("templateGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        //[Column("fk_MpCopyItemId")]
        //[ForeignKey(typeof(MpCopyItem))]
        //public int CopyItemId { get; set; }

        [JsonProperty("templateType")]
        public string TemplateTypeStr { get; set; }
        [JsonProperty("templateData")]
        public string TemplateData { get; set; }

        [JsonProperty("templateName")]
        public string TemplateName { get; set; }
        [JsonProperty("templateColor")]
        public string HexColor { get; set; }


        [JsonProperty("templateDeltaFormat")]
        public string TemplateDeltaFormat { get; set; } = string.Empty;
        [JsonProperty("templateHtmlFormat")]
        public string TemplateHtmlFormat { get; set; } = string.Empty;
  
        #endregion

        #region Fk Models

        //[ManyToOne]
        //public MpCopyItem CopyItem { get; set; }
        #endregion

        #region Properties


        //[Ignore]
        //public MpQuillEditorFormats TemplateFormatInfo {
        //    get => JsonConvert.DeserializeObject<MpQuillEditorFormats>(TemplateDeltaFormat);
        //    set => TemplateDeltaFormat = JsonConvert.SerializeObject(value);
        //}

        [Ignore]
        public MpTextTemplateType TemplateType {
            get => TemplateTypeStr.ToEnum<MpTextTemplateType>();
            set => TemplateTypeStr = value.ToString();
        }

        [Ignore]
        public Guid TextTemplateGuid {
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

        [Ignore]
        public string TemplateText { get; set; }

        [Ignore]
        public string EncodedTemplate {
            get {
                return "{t{"+Guid+"}t}";
            }
        }

        [Ignore]
        public string EncodedTemplateRtf {
            get {
                return TextTemplateOpenToken + Guid + TextTemplateCloseToken;
            }
        }

        #endregion

        public static async Task<MpTextTemplate> Create(
            int copyItemId = 0,
            string guid = "",
            MpTextTemplateType templateType = MpTextTemplateType.Dynamic,
            string templateName = "", 
            string templateColor = "",
            string templateTypeData = "",
            string formatInfo = "") {
            //if(!string.IsNullOrEmpty(guid)) {
            //    var dupCheck = await MpDataModelProvider.GetTextTemplateByGuid(guid);
            //    if (dupCheck != null) {
            //        //if item exists then write will update it
            //        templateId = dupCheck.Id;
            //    }
            //}

            var newTextTemplate = new MpTextTemplate() {
                TextTemplateGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                //CopyItemId = copyItemId,
                TemplateName = templateName,
                HexColor = string.IsNullOrEmpty(templateColor) ? MpColorHelpers.GetRandomHexColor() : templateColor,
                TemplateType = templateType,
                TemplateData = templateTypeData,
                TemplateDeltaFormat = formatInfo
            };

            await newTextTemplate.WriteToDatabaseAsync();

            return newTextTemplate;
        }

        public MpTextTemplate() : base() { }


        public async Task<MpTextTemplate> CloneDbModel(bool suppressWrite = false) {
            var ccit = new MpTextTemplate() {
                Id = suppressWrite ? this.Id : 0,
                //CopyItemId = this.CopyItemId,
                TemplateName = this.TemplateName,
                HexColor = this.HexColor,
                TemplateText = this.TemplateText,
                TemplateData = this.TemplateData,
                TemplateType = this.TemplateType,                
                Guid = suppressWrite ? this.Guid : System.Guid.NewGuid().ToString(),
            };
            if(!suppressWrite) {
                await ccit.WriteToDatabaseAsync();
            }
            return ccit;
        }
    }
}
