using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using SQLite;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpPluginPreset : 
        MpDbModelBase, 
        MpIClonableDbModel<MpPluginPreset> {

        #region Columns
        [Column("pk_MpPluginPresetId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpPluginPresetGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string PluginGuid { get; set; }

        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        [Column("fk_MpShortcutId")]
        public int ShortcutId { get; set; } = 0;

        [Column("b_IsDefault")]
        public int Default { get; set; } = 0;

        [Column("b_IsEnabled")]
        public int Enabled { get; set; } = 0;

        [Column("Label")]
        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column("SortOrderIdx")]
        public int SortOrderIdx { get; set; } = -1;

        [Column("IsQuickAction")]
        public int QuickAction { get; set; } = 0;

        [Column("b_IsActionPreset")]
        public int ActionPreset { get; set; } = 0;

        public DateTime ManifestLastModifiedDateTime { get; set; } = DateTime.MinValue;

        public int Pinned { get; set; } = 0;

        public DateTime LastSelectedDateTime { get; set; }

        #endregion


        #region Properties
        [Ignore]
        public bool IsActionPreset {
            get => ActionPreset == 1;
            set => ActionPreset = value == true ? 1 : 0;
        }

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
        public bool IsEnabled {
            get => Enabled == 1;
            set => Enabled = value ? 1 : 0;
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

        //[Ignore]
        //public MpAnalyzerPluginFormat AnalyzerFormat { get; set; }

        //[Ignore]
        //public MpClipboardHandlerFormat ClipboardFormat { get; set; }

        //[Ignore]
        //public MpPluginComponentBaseFormat ComponentFormat { get; set; }

        #endregion

        public static async Task<MpPluginPreset> CreateAsync(
            string pluginGuid = "", 
            string label = "",
            string description = "",
            int iconId = 0, 
            bool isDefault = false, 
            bool isQuickAction = false, 
            bool isActionPreset = false,
            int sortOrderIdx = -1, 
            DateTime? manifestLastModifiedDateTime = null,
            //MpPluginComponentBaseFormat format = null,
            int existingDefaultPresetId = 0) {
            //if(format == null) {
            //    throw new Exception("must have format");
            //}
            if(iconId == 0) {
                throw new Exception("needs icon");
            }
            if(string.IsNullOrEmpty(pluginGuid)) {
                throw new Exception("needs analyzer id");
            }
            if(sortOrderIdx < 0) {
                sortOrderIdx = await MpDataModelProvider.GetPluginPresetCountByPluginGuidAsync(pluginGuid);
            }

            var newPluginPreset = new MpPluginPreset() {
                Id = existingDefaultPresetId,   // only not 0 when reseting default preset
                AnalyticItemPresetGuid = System.Guid.NewGuid(),
                PluginGuid = pluginGuid,
                Label = label,
                Description = description,
                IconId = iconId,
                IsDefault = isDefault,
                IsQuickAction = isQuickAction,
                IsActionPreset = isActionPreset,
                SortOrderIdx = sortOrderIdx,
                ShortcutId = 0,
                ManifestLastModifiedDateTime = manifestLastModifiedDateTime.HasValue ? manifestLastModifiedDateTime.Value : DateTime.Now};

            //newPluginPreset.ComponentFormat = format;

            await newPluginPreset.WriteToDatabaseAsync();

            return newPluginPreset;
        }

        #region MpIClonableDbModel Implementation

        public async Task<MpPluginPreset> CloneDbModelAsync(bool deepClone = true, bool suppressWrite = false) {
            // NOTE does not clone ShortcutId,IsDefault or IsQuickAction

            var caip = new MpPluginPreset() {
                AnalyticItemPresetGuid = System.Guid.NewGuid(),
                PluginGuid = this.PluginGuid,
                Label = this.Label + " - Copy",
                Description = this.Description,
                ManifestLastModifiedDateTime = this.ManifestLastModifiedDateTime
            };

            if(deepClone) {
                if (IconId > 0) {
                    var icon = await MpDataModelProvider.GetItemAsync<MpIcon>(IconId);
                    var ci = await icon.CloneDbModelAsync(
                        deepClone: deepClone,
                        suppressWrite: suppressWrite);
                    caip.IconId = ci.Id;
                }
            }

            if(!suppressWrite) {
                // NOTE writing to db before creating preset values because they rely on cloned preset pk
                await caip.WriteToDatabaseAsync();
            }

            if(deepClone) {
                var presetValues = await MpDataModelProvider.GetPluginPresetValuesByPresetIdAsync(Id);
                foreach (var ppv in presetValues) {
                    var cppv = await ppv.CloneDbModelAsync(
                            deepClone: deepClone,
                            suppressWrite: suppressWrite);
                    cppv.PluginPresetId = caip.Id;
                    await cppv.WriteToDatabaseAsync();
                }
            }

            return caip;
        }

        #endregion

        public MpPluginPreset() : base() { }

        public override async Task DeleteFromDatabaseAsync() {
            if(Id < 1) {
                return;
            }
            List<Task> delete_tasks = new List<Task>();
            var pppvl = await MpDataModelProvider.GetPluginPresetValuesByPresetIdAsync(Id);
            if(pppvl != null && pppvl.Count > 0) {
                delete_tasks.AddRange(pppvl.Select(x => x.DeleteFromDatabaseAsync()));
            }

            
            var pptl = await MpDataModelProvider.GetPluginPresetTransactionsByPresetId(Id);
            if (pptl != null && pptl.Count > 0) {
                delete_tasks.AddRange(pptl.Cast<MpDbModelBase>().Select(x => x.DeleteFromDatabaseAsync()));

                var citl = await 
                    MpDataModelProvider.GetCopyItemTransactionsByTransactionTypeIdPairAsync(
                        pptl.Select(x=>new KeyValuePair<MpCopyItemTransactionType, int>(x.TransactionType,x.PresetId)));
                if (citl != null && citl.Count > 0) {
                    delete_tasks.AddRange(citl.Select(x => x.DeleteFromDatabaseAsync()));
                }
            }

            if (IconId > 0) {
                var icon = await MpDataModelProvider.GetItemAsync<MpIcon>(IconId);
                if(icon != null) {
                    delete_tasks.Add(icon.DeleteFromDatabaseAsync());
                }
            }

            delete_tasks.Add(base.DeleteFromDatabaseAsync());
            await Task.WhenAll(delete_tasks);
        }

    }
}
