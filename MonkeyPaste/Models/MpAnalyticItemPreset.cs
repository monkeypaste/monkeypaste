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
    public class MpAnalyticItemPreset : MpDbModelBase, MpIClonableDbModel<MpAnalyticItemPreset> {
        #region Columns
        [Column("pk_MpAnalyticItemPresetId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemPresetGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string AnalyzerPluginGuid { get; set; }

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

        public DateTime ManifestLastModifiedDateTime { get; set; } = DateTime.MinValue;

        public int Pinned { get; set; } = 0;

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Fk Models

        [OneToMany(CascadeOperations = CascadeOperation.All)]
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
            string analyzerPluginGuid = "", 
            string label = "",
            string description = "",
            int iconId = 0, 
            bool isDefault = false, 
            bool isQuickAction = false, 
            int sortOrderIdx = -1, 
            List<MpAnalyticItemParameterFormat> parameters = null,
            List<MpAnalyzerPresetValueFormat> values = null,
            DateTime? manifestLastModifiedDateTime = null,
            int existingDefaultPresetId = 0) {
            
            if(iconId == 0) {
                throw new Exception("needs icon");
            }
            if(string.IsNullOrEmpty(analyzerPluginGuid)) {
                throw new Exception("needs analyzer id");
            }
            if(parameters == null || parameters.Count == 0) {
                throw new Exception("needs parameters");
            }

            var newAnalyticItemPreset = new MpAnalyticItemPreset() {
                Id = existingDefaultPresetId,   // only not 0 when reseting default preset
                AnalyticItemPresetGuid = System.Guid.NewGuid(),
                AnalyzerPluginGuid = analyzerPluginGuid,
                Label = label,
                Description = description,
                IconId = iconId,
                IsDefault = isDefault,
                IsQuickAction = isQuickAction,
                SortOrderIdx = sortOrderIdx,
                ShortcutId = 0,
                ManifestLastModifiedDateTime = manifestLastModifiedDateTime.HasValue ? manifestLastModifiedDateTime.Value : DateTime.Now};

            await newAnalyticItemPreset.WriteToDatabaseAsync();

            foreach(var param in parameters) {                
                string defValue = string.Empty;
                if(values != null && values.Count > 0) {
                    var paramVal = values.FirstOrDefault(x => x.enumId == param.enumId);
                    if(paramVal != null) {
                        defValue = paramVal.value;
                    }
                } 
                if (string.IsNullOrEmpty(defValue) && param.values != null && param.values.Count > 0) {
                    if (param.values.Any(x => x.isDefault)) {
                        defValue = string.Join(",", param.values.Where(x => x.isDefault).Select(x => x.value));
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

        #region MpIClonableDbModel Implementation

        public async Task<MpAnalyticItemPreset> CloneDbModel() {
            // NOTE does not clone ShortcutId,IsDefault or IsQuickAction

            var caip = new MpAnalyticItemPreset() {
                AnalyticItemPresetGuid = System.Guid.NewGuid(),
                AnalyzerPluginGuid = this.AnalyzerPluginGuid,
                Label = this.Label + " - Copy",
                Description = this.Description,
                ManifestLastModifiedDateTime = this.ManifestLastModifiedDateTime,
                PresetParameterValues = new List<MpAnalyticItemPresetParameterValue>()
            };

            if(IconId > 0) {
                var icon = await MpDb.GetItemAsync<MpIcon>(IconId);
                var ci = await icon.CloneDbModel();
                caip.IconId = ci.Id;
            }

            // NOTE writing to db before creating preset values because they rely on cloned preset pk
            await caip.WriteToDatabaseAsync();

            if(PresetParameterValues == null || PresetParameterValues.Count == 0) {
                PresetParameterValues = await MpDataModelProvider.GetAnalyticItemPresetValuesByPresetId(Id);
            }
            foreach(var ppv in PresetParameterValues) {
                var cppv = await ppv.CloneDbModel();
                cppv.AnalyticItemPresetId = caip.Id;
                await cppv.WriteToDatabaseAsync();

                caip.PresetParameterValues.Add(cppv);
            }

            return caip;
        }

        #endregion

        public MpAnalyticItemPreset() : base() { }



        
    }
}
