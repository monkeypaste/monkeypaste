using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    [Flags]
    public enum MpAnalyzerInputFormatFlags {
        None = 0,
        Text = 1,
        Image = 2,
        File = 4
    }

    [Flags]
    public enum MpAnalyzerOutputFormatFlags {
        None = 0,
        Text = 1,
        Image = 2,
        BoundingBox = 4,
        File = 8
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
        public int InputFormatFlagsVal { get; set; } = 0;

        [Column("e_MpOutputFormatType")]
        public int OutputFormatFlagsVal { get; set; } = 0;

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
        public MpAnalyzerInputFormatFlags InputFormatFlags {
            get {
                return (MpAnalyzerInputFormatFlags)InputFormatFlagsVal;
            }
            set {
                InputFormatFlagsVal = (int)value;
            }
        }

        [Ignore]
        public MpAnalyzerOutputFormatFlags OutputFormatFlags {
            get {
                return (MpAnalyzerOutputFormatFlags)OutputFormatFlagsVal;
            }
            set {
                OutputFormatFlagsVal = (int)value;
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
            string endPoint = "",
            string apiKey = "",
            MpAnalyzerInputFormatFlags inputFormat = MpAnalyzerInputFormatFlags.None,
            MpAnalyzerOutputFormatFlags outputFormat = MpAnalyzerOutputFormatFlags.None,
            string title = "",
            string description = "",
            string parameterFormatResourcePath = "",
            int sortOrderIdx = -1,
            int iconId = 0,
            string guid = "") {
            MpAnalyticItem dupItem = null;

            if (!string.IsNullOrEmpty(guid)) {
                dupItem = await MpDataModelProvider.GetAnalyticItemByGuid(guid);
                if (dupItem != null) {
                    dupItem = await MpDb.GetItemAsync<MpAnalyticItem>(dupItem.Id);
                    return dupItem;
                }
            }

            dupItem = await MpDataModelProvider.GetAnalyticItemByEndpoint(endPoint);
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
                InputFormatFlags = inputFormat,
                OutputFormatFlags = outputFormat,
                Title = title,
                Description = description,
                ParameterFormatResourcePath = parameterFormatResourcePath,
                SortOrderIdx = sortOrderIdx
            };

            await newAnalyticItem.WriteToDatabaseAsync();


            //if(File.Exists(parameterFormatResourcePath)) {
            //    string formatJson = MpFileIo.ReadTextFromFileOrResource(parameterFormatResourcePath);
            //    var analyzerFormat = JsonConvert.DeserializeObject<MpAnalyticItemFormat>(formatJson);
            //    var presets = new List<MpAnalyticItemPreset>();
            //    foreach (var presetFormat in analyzerFormat.ParameterFormats) {
            //        int idx = analyzerFormat.ParameterFormats.IndexOf(presetFormat);

            //        var aip = await MpAnalyticItemPreset.Create(
            //            analyticItem: newAnalyticItem,
            //            isDefault: idx == 0,
            //            label: presetFormat.Label,
            //            icon: newAnalyticItem.Icon,
            //            sortOrderIdx: idx,
            //            description: presetFormat.Description);

            //        var aipvl = new List<MpAnalyticItemPresetParameterValue>();
            //        foreach (var paramVal in presetFormat.Values) {
            //            var aippv = await MpAnalyticItemPresetParameterValue.Create(
            //                parentItem: aip,
            //                paramEnumId: presetFormat.EnumId,
            //                value: paramVal.Value);
                               
            //            aipvl.Add(aippv);
            //        }

            //        presets.Add(aip);
            //    }

            //    newAnalyticItem.Presets = presets;
            //}

            return newAnalyticItem;
        }

        public MpAnalyticItem() { }
    }
}
