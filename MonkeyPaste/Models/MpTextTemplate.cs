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
        //Content,
        //Analyzer,
        //Action,
        Contact,
        DateTime
    }

    public class MpTextTemplate : MpDbModelBase, MpIClonableDbModel<MpTextTemplate> {
        #region Constants
        public const string TextTemplateOpenToken = @"{t{";
        public const string TextTemplateCloseToken = @"}t}";

        public const string TextTemplateOpenTokenRtf = @"\{t\{";
        public const string TextTemplateCloseTokenRtf = @"\}t\}";
        #endregion

        #region Statics

        public static MpRichTextFormatInfoFormat DefaultRichTextFormat {
            get {
                string randColor = MpColorHelpers.GetRandomHexColor();
                return new MpRichTextFormatInfoFormat() {
                    inlineFormat = new MpInlineTextFormatInfoFormat() {
                        background = randColor,
                        color = MpColorHelpers.IsBright(randColor) ? MpSystemColors.black : MpSystemColors.white,
                        font = "Consolas",
                        size = 12
                    }
                };
            }
        }
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
        
        //[JsonProperty("templateColor")]
        public string HexColor { get; set; }


        [JsonProperty("templateDeltaFormat")]
        public string TemplateDeltaFormat { get; set; } = string.Empty;

        [JsonProperty("templateHtmlFormat")]
        public string TemplateHtmlFormat { get; set; } = string.Empty;

        public string RichTextFormatJson { get; set; } = string.Empty;
  
        #endregion

        #region Fk Models
        #endregion

        #region Properties

        [Ignore]
        public MpRichTextFormatInfoFormat RichTextFormat {
            get => JsonConvert.DeserializeObject<MpRichTextFormatInfoFormat>(RichTextFormatJson);
            set => RichTextFormatJson = JsonConvert.SerializeObject(value);
        }

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
                return TextTemplateOpenToken+Guid+TextTemplateCloseToken;
            }
        }

        [Ignore]
        public string EncodedTemplateRtf {
            get {
                return TextTemplateOpenTokenRtf + Guid + TextTemplateCloseTokenRtf;
            }
        }

        #endregion

        public static async Task<MpTextTemplate> Create(
            string guid = "",
            MpTextTemplateType templateType = MpTextTemplateType.Dynamic,
            string templateName = "", 
            string templateColor = "",
            string templateTypeData = "",
            string rtfFormatJson = "",
            string deltaFormatJson = "") {
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
                RichTextFormatJson = string.IsNullOrWhiteSpace(rtfFormatJson) ? 
                                        DefaultRichTextFormat.Serialize() : rtfFormatJson,
                TemplateDeltaFormat = deltaFormatJson
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
                RichTextFormatJson = this.RichTextFormatJson,
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

        public override string ToString() {
            return EncodedTemplate;
        }
    }
}
