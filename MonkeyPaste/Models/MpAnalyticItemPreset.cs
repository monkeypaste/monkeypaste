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

        [Column("Label")]
        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column("SortOrderIdx")]
        public int SortOrderIdx { get; set; } = -1;

        [Column("IsQuickAction")]
        public int QuickAction { get; set; } = 0;

        [Column("IsReadOnly")]
        public int ReadOnly { get; set; } = 0;
        #endregion

        #region Fk Models

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpAnalyticItem AnalyticItem { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpIcon Icon { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpShortcut Shortcut { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeDelete)]
        public List<MpAnalyticItemPresetParameterValue> PresetParameterValues { get; set; } = new List<MpAnalyticItemPresetParameterValue>();
        #endregion

        #region Properties

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
        public bool IsReadOnly {
            get {
                return ReadOnly == 1;
            }
            set {
                if (IsReadOnly != value) {
                    ReadOnly = value ? 1 : 0;
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
            MpIcon icon = null, bool isDefault = false, bool isReadOnly = false, bool isQuickAction = false, int sortOrderIdx = -1, string description = "") {
            if (analyticItem == null) {
                throw new Exception("Preset must be associated with an item");
            }
            var dupItem = await MpDataModelProvider.Instance.GetAnalyticItemPresetByLabel(analyticItem.Id, label);
            if (dupItem != null) {
                MpConsole.WriteLine($"Updating preset {label} for {analyticItem.Title}");

                dupItem = await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(dupItem.Id);
                dupItem.AnalyticItemId = analyticItem.Id;
                dupItem.Label = label;
                dupItem.Description = description;
                dupItem.SortOrderIdx = sortOrderIdx;
                dupItem.IsReadOnly = isReadOnly;
                dupItem.IsQuickAction = isQuickAction;
                dupItem.IconId = icon == null ? 0 : icon.Id;
                await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemPreset>(dupItem);
                return dupItem;
            }

            var newAnalyticItemPreset = new MpAnalyticItemPreset() {
                AnalyticItemPresetGuid = System.Guid.NewGuid(),
                AnalyticItem = analyticItem,
                AnalyticItemId = analyticItem.Id,
                Label = label,
                Description = description,
                SortOrderIdx = sortOrderIdx,
                IsReadOnly = isReadOnly,
                IsQuickAction = isQuickAction,
                IconId = icon == null ? 0 : icon.Id,
                Icon = icon
            };

            await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemPreset>(newAnalyticItemPreset);

            return newAnalyticItemPreset;
        }

        public MpAnalyticItemPreset() : base() { }

        public object Clone() {
            var caip = new MpAnalyticItemPreset() {
                AnalyticItemId = this.AnalyticItemId,
                IconId = this.IconId,
                Label = this.Label + " Clone",
                Description = this.Description,
                PresetParameterValues = this.PresetParameterValues.Select(x=>x.Clone() as MpAnalyticItemPresetParameterValue).ToList()
            };
            return caip;
        }
    }
}
