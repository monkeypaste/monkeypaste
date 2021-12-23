using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Column("InputFormatTypeId")]
        public int InputFormatTypeId { get; set; } = 0;

        [Column("Title")]
        public string Title { get; set; } = string.Empty;

        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        [Column("SortOrderIdx")]
        public int SortOrderIdx { get; set; } = -1;

        public string EndPoint { get; set; }

        public string ApiKey { get; set; }
        #endregion

        #region Fk Models

        [OneToOne]
        public MpIcon Icon { get; set; }

        //[OneToMany]
        //public List<MpAnalyticItemParameter> Parameters { get; set; } = new List<MpAnalyticItemParameter>();

        [OneToMany]
        public List<MpAnalyticItemPreset> Presets { get; set; } = new List<MpAnalyticItemPreset>();
        
        [OneToOne]
        public MpBillableItem BillableItem { get; set; }

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

        public static async Task<MpAnalyticItem> Create(
            string endPoint,
            string apiKey,
            MpInputFormatType format,
            string title,
            string description,
            int sortOrderIdx = -1) {
            var dupItem = await MpDataModelProvider.Instance.GetAnalyticItemByEndpoint(endPoint);
            if (dupItem != null) {
                dupItem = await MpDb.Instance.GetItemAsync<MpAnalyticItem>(dupItem.Id);
                return dupItem;
            }

            if (sortOrderIdx < 0) {
                sortOrderIdx = await MpDataModelProvider.Instance.GetAnalyticItemCount();
            }

            var domainStr = MpHelpers.Instance.GetUrlDomain(endPoint);
            var favIconImg64 = await MpHelpers.Instance.GetUrlFaviconAsync(domainStr);

            var icon = await MpIcon.Create(favIconImg64);

            var newAnalyticItem = new MpAnalyticItem() {
                AnalyticItemGuid = System.Guid.NewGuid(),
                EndPoint = endPoint,
                ApiKey = apiKey,
                IconId = icon.Id,
                Icon = icon,
                InputFormatType = format,
                Title = title,
                Description = description,
                SortOrderIdx = sortOrderIdx
            };

            await newAnalyticItem.WriteToDatabaseAsync();

            return newAnalyticItem;
        }

        public MpAnalyticItem() { }
    }
}
