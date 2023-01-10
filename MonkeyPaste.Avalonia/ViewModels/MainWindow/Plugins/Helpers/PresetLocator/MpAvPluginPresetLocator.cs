using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using MonkeyPaste;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginPresetLocator {
        public static async Task<IEnumerable<MpPluginPreset>> LocatePresetsAsync(
            MpIParameterHostViewModel presetHost, 
            bool enableOnReset = false) {
            var db_presets = await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(presetHost.PluginGuid);

            bool isNew = db_presets.Count == 0;
            bool isManifestModified = db_presets.Any(x => x.ManifestLastModifiedDateTime < presetHost.PluginFormat.manifestLastModifiedDateTime);
            bool needsReset = isNew || isManifestModified;
            if (needsReset) {

                while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                    // if this is first load of the plugin the icon may not be added to icon collection yet so wait for it
                    await Task.Delay(100);
                }
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == presetHost.IconId);

                MpNotificationBuilder.ShowMessageAsync(
                    msgType: MpNotificationType.PluginUpdated,
                    title: $"Analyzer '{presetHost.PluginFormat.title}' {(isNew ? "Added":"Updated")}",
                    iconSourceObj: ivm.IconBase64,
                    body: $"{(isNew ? "Creating Default Presets" : "Resetting presets to default")}")
                    .FireAndForgetSafeAsync();


                db_presets = await CreateOrUpdatePresetsAsync(presetHost, db_presets);

                // now that db params are sync'd w/ new manifest update manifest backup for subsequent loads
                presetHost.PluginFormat.backupCheckPluginFormat = MpPluginLoader.CreateLastLoadedBackupPluginFormat(presetHost.PluginFormat);

                if(enableOnReset) {
                    // this is supposed to handle initial startup for CoreClipboard handler when no formats are enabled 
                    // but there's many cases that this may not be initial startup so:
                    // TODO instead of doing should notify clipboard collection that default was reset and only enable formats
                    // that don't have another handler with that format enabled
                    db_presets.ForEach(x => x.IsEnabled = true);

                    await Task.WhenAll(db_presets.Select(x => x.WriteToDatabaseAsync()));

                    //MessageBox.Show("All CoreClipboard formats have been set to enabled");
                }
            }



            //presets.ForEach(x => x.ComponentFormat = AnalyzerPluginFormat);
            return db_presets.OrderBy(x => x.SortOrderIdx);
        }

        private static async Task<List<MpPluginPreset>> CreateOrUpdatePresetsAsync(
            MpIParameterHostViewModel pluginHost, 
            List<MpPluginPreset> db_presets) {
            // when manifest changes this will:
            // 1. Create default presets if none exists
            // 2. Add/Remove preset parameter values based on parameter format differences since last successful load

            if (db_presets.Count == 0) {
                // when no presets exist create empty default preset
                foreach (var preset_guid in pluginHost.ComponentFormat.presets.Select(x => x.guid)) {
                    var presetModel = await CreateOrResetManifestPresetModelAsync(pluginHost, preset_guid);
                    db_presets.Add(presetModel);
                }
            }

            var last_successful_loaded_parameters =
                pluginHost.BackupComponentFormat == null || pluginHost.BackupComponentFormat.parameters == null ?
                    null : pluginHost.BackupComponentFormat.parameters;

            var modified_params = GetNewOrChangedParameters(
                pluginHost.ComponentFormat.parameters,
                last_successful_loaded_parameters);

            var params_to_remove = modified_params.Where(x => x.Item1 == null).Select(x=>x.Item2);
            var params_to_add = modified_params.Where(x => x.Item2 == null).Select(x=>x.Item1);

            foreach(var db_preset in db_presets) {
                // get each presets currently stored values
                var db_vals = await MpDataModelProvider.GetAllParameterHostValuesAsync(MpParameterHostType.Preset, db_preset.Id);

                // remove any no longer existing stored parameter values
                var db_vals_to_remove = db_vals.Where(x => params_to_remove.Any(y => y.paramId.Equals(x.ParamId)));
                await Task.WhenAll(db_vals_to_remove.Select(x => x.DeleteFromDatabaseAsync()));

                // add new parameter value to preset
                foreach (var param_to_add in params_to_add) {
                    foreach(var param_value_to_add in param_to_add.values) {
                        // add new param values to each preset

                        _ = await MpPluginPresetParameterValue.CreateAsync(
                            hostType: MpParameterHostType.Preset,
                            presetId: db_preset.Id,
                            paramId: param_to_add.paramId,
                            value: param_value_to_add.value);
                    }                    
                }
            }
            return db_presets;
        }

        private static IEnumerable<Tuple<MpParameterFormat, MpParameterFormat>> GetNewOrChangedParameters(
            IEnumerable<MpParameterFormat> manifest_parameters,
            IEnumerable<MpParameterFormat> backup_parameters) {
            // this somewhat crudely decides if parameters were change by:
            // 1. if current parameter enumId did not previously exist
            // 2. if backup parameter enumId does not now exist

            // NOTE current db values are not checked and by design will fallback on parameter viewmodels validation for alterations
            foreach(var manifest_param in manifest_parameters) {
                MpParameterFormat matched_backup_param = null;
                if(backup_parameters != null) {
                    matched_backup_param = backup_parameters.FirstOrDefault(x => !x.paramId.Equals(manifest_param.paramId));
                }                
                if (matched_backup_param == null) {
                    // case 1
                    yield return new Tuple<MpParameterFormat, MpParameterFormat>(manifest_param, null);
                }
            }
            
            if(backup_parameters == null) {
                yield break;
            }

            foreach(var backup_param in backup_parameters) {
                MpParameterFormat matched_manifest_param = null;
                if (manifest_parameters != null) {
                    matched_manifest_param = manifest_parameters.FirstOrDefault(x => !x.paramId.Equals(backup_param.paramId));
                }
                if (matched_manifest_param == null) {
                    // case 2
                    yield return new Tuple<MpParameterFormat, MpParameterFormat>(null,backup_param);
                }
            }
        }


        public static async Task<MpPluginPreset> CreateOrResetManifestPresetModelAsync(
            MpIParameterHostViewModel pluginHost, string presetGuid, int sortOrderIdx = 0) {

            var preset_format = pluginHost.ComponentFormat.presets.FirstOrDefault(x => x.guid == presetGuid);
            if(preset_format == null) {
                // why isn't the preset in the format? it should be deleted if not
                Debugger.Break();
                return null;
            }
            int preset_icon_id = pluginHost.IconId;
            if(!string.IsNullOrEmpty(preset_format.iconUri)) {
                preset_icon_id = await MpAvPluginIconLocator.LocatePluginIconIdAsync(pluginHost, preset_format.iconUri);
            }
            string preset_label = preset_format.label;
            if(string.IsNullOrEmpty(preset_label)) {
                int manifest_preset_idx = pluginHost.ComponentFormat.presets.IndexOf(preset_format);
                preset_label = $"{pluginHost.PluginFormat.title} - Default{manifest_preset_idx + 1}";
            }

            string preset_description = preset_format.description;
            if(string.IsNullOrEmpty(preset_description)) {
                preset_description = $"Auto-generated default preset for '{preset_label}'";
            }
            var preset_model = await MpPluginPreset.CreateOrUpdateAsync(
                                pluginGuid: pluginHost.PluginGuid,
                                guid: presetGuid,
                                isDefault: preset_format.isDefault,
                                label: preset_label,
                                iconId: preset_icon_id,
                                sortOrderIdx: sortOrderIdx,
                                description: preset_description,
                                manifestLastModifiedDateTime: pluginHost.PluginFormat.manifestLastModifiedDateTime);

            
            return preset_model;
        }
    }
}
