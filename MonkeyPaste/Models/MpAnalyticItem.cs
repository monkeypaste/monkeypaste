using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public enum MpOutputFormatType {
        None = 0,
        Text = 1,
        Image = 2,
        BoundingBox = 4,
        CustomFile = 8
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

        [Column("e_MpCopyItemType")]
        public int InputFormatTypeId { get; set; } = 0;

        [Column("e_MpOutputFormatType")]
        public int OutputFormatTypeId { get; set; } = 0;

        [Column("Title")]
        public string Title { get; set; } = string.Empty;

        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        [Column("SortOrderIdx")]
        public int SortOrderIdx { get; set; } = -1;

        public string EndPoint { get; set; }

        public string ApiKey { get; set; }

        public string ParameterFormatResourcePath { get; set; }
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
        public MpCopyItemType InputFormatType {
            get {
                return (MpCopyItemType)InputFormatTypeId;
            }
            set {
                InputFormatTypeId = (int)value;
            }
        }

        [Ignore]
        public MpOutputFormatType OutputFormatType {
            get {
                return (MpOutputFormatType)OutputFormatTypeId;
            }
            set {
                OutputFormatTypeId = (int)value;
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
            MpCopyItemType inputFormat,
            MpOutputFormatType outputFormat,
            string title,
            string description,
            string parameterFormatResourcePath,
            int sortOrderIdx = -1,
            int iconId = 0,
            string guid = "") {
            var dupItem = await MpDataModelProvider.GetAnalyticItemByEndpoint(endPoint);
            if (dupItem != null) {
                dupItem = await MpDb.GetItemAsync<MpAnalyticItem>(dupItem.Id);
                return dupItem;
            }

            if (sortOrderIdx < 0) {
                sortOrderIdx = await MpDataModelProvider.GetAnalyticItemCount();
            }

            

            MpIcon icon = null;
            if (iconId > 0) {
                icon = await MpDb.GetItemAsync<MpIcon>(iconId);
            } else {
                var domainStr = MpHelpers.GetUrlDomain(endPoint);
                var favIconImg64 = await MpHelpers.GetUrlFaviconAsync(domainStr);
                await MpIcon.Create(favIconImg64);
            }
            

            var newAnalyticItem = new MpAnalyticItem() {
                AnalyticItemGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                EndPoint = endPoint,
                ApiKey = apiKey,
                IconId = icon.Id,
                Icon = icon,
                InputFormatType = inputFormat,
                OutputFormatType = outputFormat,
                Title = title,
                Description = description,
                ParameterFormatResourcePath = parameterFormatResourcePath,
                SortOrderIdx = sortOrderIdx
            };

            await newAnalyticItem.WriteToDatabaseAsync();

            //create default preset
            var defPreset = await MpAnalyticItemPreset.Create(
                analyticItem: newAnalyticItem,
                label: "Default",
                icon: newAnalyticItem.Icon,
                isDefault: true,
                isQuickAction: false,
                sortOrderIdx: 0,
                description: $"This is the default preset for '{newAnalyticItem.Title}' and cannot be removed");

            await defPreset.WriteToDatabaseAsync();

            return newAnalyticItem;
        }

        public MpAnalyticItem() { }
    }
}
