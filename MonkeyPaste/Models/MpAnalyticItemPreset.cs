using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAnalyticItemPreset : MpDbModelBase, ICloneable {
        #region Columns
        [Column("pk_MpAnalyticItemPresetId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemPresetGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpAnalyticItem))]
        [Column("fk_MpAnalyticItemId")]
        public int AnalyticItemId { get; set; }

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        [ForeignKey(typeof(MpShortcut))]
        [Column("fk_MpShortcutId")]
        public int ShortcutId { get; set; } = 0;

        [Column("b_IsDefault")]
        public int Default { get; set; } = 0;

        [Column("Label")]
        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column("SortOrderIdx")]
        public int SortOrderIdx { get; set; } = -1;

        [Column("IsQuickAction")]
        public int QuickAction { get; set; } = 0;

        #endregion

        #region Fk Models

        [ManyToOne]
        public MpAnalyticItem AnalyticItem { get; set; }

        [OneToOne]
        public MpIcon Icon { get; set; }

        [OneToOne]
        public MpShortcut Shortcut { get; set; }

        [OneToMany]
        public List<MpAnalyticItemPresetParameterValue> PresetParameterValues { get; set; } = new List<MpAnalyticItemPresetParameterValue>();
        #endregion

        #region Properties

        [Ignore]
        public bool IsDefault {
            get {
                return Default == 1;
            }
            set {
                if (IsDefault != value) {
                    Default = value ? 1 : 0;
                }
            }
        }

        [Ignore]
        public bool IsQuickAction {
            get {
                return QuickAction == 1;
            }
            set {
                if (IsQuickAction != value) {
                    QuickAction = value ? 1 : 0;
                }
            }
        }

        [Ignore]
        public Guid AnalyticItemPresetGuid {
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

        public static async Task<MpAnalyticItemPreset> Create(
            MpAnalyticItem analyticItem, 
            string label,
            MpIcon icon = null, bool isDefault = false, bool isQuickAction = false, int sortOrderIdx = -1, string description = "") {
            if (analyticItem == null || analyticItem.Icon == null) {
                throw new Exception("Preset must be associated with an item");
            }
            icon = icon == null ? analyticItem.Icon : icon;
            var newAnalyticItemPreset = new MpAnalyticItemPreset() {
                Id = 0,
                AnalyticItemPresetGuid = System.Guid.NewGuid(),
                AnalyticItem = analyticItem,
                AnalyticItemId = analyticItem.Id,
                Icon = icon,
                IconId = icon.Id,
                Label = label,
                Description = description,
                SortOrderIdx = sortOrderIdx,
                IsQuickAction = isQuickAction,
                Shortcut = null,
                ShortcutId = 0,
                IsDefault = isDefault
            };

            await newAnalyticItemPreset.WriteToDatabaseAsync();


            var paramlist = JsonConvert.DeserializeObject<MpAnalyticItemFormat>(
                analyticItem.ParameterFormatJson, new MpJsonEnumConverter()).ParameterFormats;

            foreach(var param in paramlist.OrderBy(x=>x.SortOrderIdx)) {
                string defValue = string.Empty;
                if(param.Values != null && param.Values.Count > 0) {
                    if(param.Values.Any(x=>x.IsDefault)) {
                        defValue = param.Values.FirstOrDefault(x => x.IsDefault).Value;
                    } else {
                        defValue = param.Values[0].Value;
                    }
                }

                var paramPreset = await MpAnalyticItemPresetParameterValue.Create(
                    newAnalyticItemPreset,
                    param.EnumId,
                    defValue);
            }
            return newAnalyticItemPreset;
        }

        public MpAnalyticItemPreset() : base() { }

        public object Clone() {
            var caip = new MpAnalyticItemPreset() {
                AnalyticItemId = this.AnalyticItemId,
                Label = this.Label + " Clone",
                Description = this.Description,
                PresetParameterValues = this.PresetParameterValues.Select(x=>x.Clone() as MpAnalyticItemPresetParameterValue).ToList()
            };
            return caip;
        }
    }
}
