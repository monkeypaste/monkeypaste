using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpInputFormatType {
        None = 0,
        Text,
        Image,
        CustomFile
    }

    public class MpAnalyticItem : MpDbModelBase {
        #region Columns
        [Column("pk_MpAnalyticItemId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        [Column("fk_MpInputFormatTypeId")]
        public int InputFormatTypeId { get; set; } = 0;

        [Column("Title")]
        public string Title { get; set; } = string.Empty;

        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        [Column("ApiKey")]
        public string ApiKey { get; set; } = string.Empty;

        [Column("EndPoint")]
        public string EndPoint { get; set; } = string.Empty;

        #endregion

        #region Fk Models

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpIcon Icon { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<MpAnalyticItemParameter> Parameters { get; set; } = new List<MpAnalyticItemParameter>();

        #endregion

        #region Properties

        [Ignore]
        public MpInputFormatType InputFormatType {
            get {
                return (MpInputFormatType)InputFormatTypeId;
            }
            set {
                if (InputFormatTypeId != (int)value) {
                    InputFormatTypeId = (int)value;
                }
            }
        }

        [Ignore]
        public Guid AnalyticItemGuid {
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

        #endregion

        public static async Task<MpAnalyticItem> Create(string endPoint, string apiKey, MpInputFormatType format, string title, string description, MpIcon icon) {
            var dupItem = await MpDataModelProvider.Instance.GetAnalyticItemByEndpoint(endPoint);
            if (dupItem != null) {
                dupItem = await MpDb.Instance.GetItemAsync<MpAnalyticItem>(dupItem.Id);
                return dupItem;
            }

            var newAnalyticItem = new MpAnalyticItem() {
                AnalyticItemGuid = System.Guid.NewGuid(),
                EndPoint = endPoint,
                ApiKey = apiKey,
                IconId = icon.Id,
                Icon = icon,
                InputFormatType = format,
                Title = title,
                Description = description
            };

            await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItem>(newAnalyticItem);

            return newAnalyticItem;
        }

        public MpAnalyticItem() { }
    }
}
