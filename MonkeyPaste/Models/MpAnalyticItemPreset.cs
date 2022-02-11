using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAnalyticItemPreset : MpDbModelBase, ICloneable {
        #region Columns
        [Column("pk_MpAnalyticItemPresetId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemPresetGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        //[ForeignKey(typeof(MpAnalyticItem))]
        //[Column("fk_MpAnalyticItemId")]
        public int AnalyzerPluginSudoId { get; set; }

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


        public int Pinned { get; set; } = 0;
        #endregion

        #region Fk Models

        //[ManyToOne]
        //public MpAnalyticItem AnalyticItem { get; set; }

        [OneToOne]
        public MpIcon Icon { get; set; }

        [OneToOne]
        public MpShortcut Shortcut { get; set; }

        [OneToMany]
        public List<MpAnalyticItemPresetParameterValue> PresetParameterValues { get; set; } = new List<MpAnalyticItemPresetParameterValue>();
        #endregion

        #region Properties

        [Ignore]
        public bool IsPinned {
            get => Pinned == 1;
            set => Pinned = value == true ? 1 : 0;
        }

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
            int analyzerSudoId = 0, 
            string label = "",
            MpIcon icon = null, 
            bool isDefault = false, 
            bool isQuickAction = false, 
            int sortOrderIdx = -1, 
            string description = "",
            List<MpAnalyticItemParameterFormat> parameters = null) {
            
            if(icon == null) {
                throw new Exception("needs icon");
            }
            if(analyzerSudoId <= 0) {
                throw new Exception("needs analyzer id");
            }
            if(parameters == null || parameters.Count == 0) {
                throw new Exception("needs parameters");
            }

            var newAnalyticItemPreset = new MpAnalyticItemPreset() {
                AnalyticItemPresetGuid = System.Guid.NewGuid(),
                AnalyzerPluginSudoId = analyzerSudoId,
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

            foreach(var param in parameters.OrderBy(x=>x.sortOrderIdx)) {
                string defValue = string.Empty;
                if(param.values != null && param.values.Count > 0) {
                    if(param.values.Any(x=>x.isDefault)) {
                        defValue = string.Join(",",param.values.Where(x => x.isDefault).Select(x=>x.value));
                    } else {
                        defValue = param.values[0].value;
                    }
                }

                var paramPreset = await MpAnalyticItemPresetParameterValue.Create(
                    newAnalyticItemPreset,
                    param.enumId,
                    defValue);

                newAnalyticItemPreset.PresetParameterValues.Add(paramPreset);
            }
            return newAnalyticItemPreset;
        }

        public MpAnalyticItemPreset() : base() { }

        public object Clone() {
            var caip = new MpAnalyticItemPreset() {
                AnalyticItemPresetGuid = this.AnalyticItemPresetGuid,
                Label = this.Label + " Clone",
                Description = this.Description,
                PresetParameterValues = this.PresetParameterValues.Select(x=>x.Clone() as MpAnalyticItemPresetParameterValue).ToList()
            };
            return caip;
        }
    }
}
