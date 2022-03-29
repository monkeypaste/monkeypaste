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
        public const string TEMPLATE_PREFIX = "<";
        public const string TEMPLATE_SUFFIX = ">";
        #endregion

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTextTokenId")]
        public override int Id { get; set; }

        [Column("MpTextTemplateGuid")]
        [JsonProperty("templateGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }
                
        [Column("fk_MpCopyItemId")]
        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        [Column("e_TemplateTypeId")]
        public int TemplateTypeId { get; set; }

        public string TemplateName { get; set; }

        public string TemplateTypeData { get; set; }

        // TODO this should prolly be a serialized JSON obj of text format...
        public string TemplateFormatInfoStr { get; set; } = string.Empty;

        public string HexColor { get; set; }        

        #endregion

        #region Fk Models

        //[ManyToOne]
        //public MpCopyItem CopyItem { get; set; }
        #endregion

        #region Properties

        [Ignore]
        [JsonProperty("templateType")]
        public string TemplateTypeStr { get; set; }

        [Ignore]
        public MpTextFormatInfoFormat TemplateFormatInfo {
            get => JsonConvert.DeserializeObject<MpTextFormatInfoFormat>(TemplateFormatInfoStr);
            set => TemplateFormatInfoStr = JsonConvert.SerializeObject(value);
        }

        [Ignore]
        public MpTextTemplateType TemplateType {
            get => (MpTextTemplateType)TemplateTypeId;
            set => TemplateTypeId = (int)value;
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
                return string.Format(@"{0}{1}{2}",
                        TEMPLATE_PREFIX,
                        Guid,
                        TEMPLATE_SUFFIX);
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
            int templateId = 0;
            if(!string.IsNullOrEmpty(guid)) {
                var dupCheck = await MpDataModelProvider.GetTextTemplateByGuid(guid);
                if (dupCheck != null) {
                    //if item exists then write will update it
                    templateId = dupCheck.Id;
                }
            }

            var newTextTemplate = new MpTextTemplate() {
                Id = templateId,
                TextTemplateGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                CopyItemId = copyItemId,
                TemplateName = templateName,
                HexColor = string.IsNullOrEmpty(templateColor) ? MpHelpers.GetRandomColor().ToHex() : templateColor,
                TemplateType = templateType,
                TemplateTypeData = templateTypeData,
                TemplateFormatInfoStr = formatInfo
            };

            await newTextTemplate.WriteToDatabaseAsync();

            return newTextTemplate;
        }

        public MpTextTemplate() : base() { }


        public async Task<MpTextTemplate> CloneDbModel() {
            var ccit = new MpTextTemplate() {
                CopyItemId = this.CopyItemId,
                TemplateName = this.TemplateName,
                HexColor = this.HexColor,
                TemplateText = this.TemplateText,
                TextTemplateGuid = System.Guid.NewGuid(),
            };
            await ccit.WriteToDatabaseAsync();
            return ccit;
        }
    }
}
