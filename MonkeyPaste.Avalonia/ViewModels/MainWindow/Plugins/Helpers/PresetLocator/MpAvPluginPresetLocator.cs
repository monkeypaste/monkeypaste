using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginPresetLocator {
        public static async Task<IEnumerable<MpPluginPreset>> LocatePresetsAsync(
            MpIParameterHostViewModel presetHost,
            bool enableOnReset = false,
            bool showMessages = false) {

            var db_presets = await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(presetHost.PluginGuid);

            bool isNew = db_presets.Count == 0;
            bool has_manifest_changed = presetHost.PluginFormat.IsManifestChangedFromBackup;
            bool is_any_preset_out_of_date = db_presets.Any(x => x.ManifestLastModifiedDateTime < presetHost.PluginFormat.manifestLastModifiedDateTime);
            bool needsReset = isNew || is_any_preset_out_of_date || has_manifest_changed;
            if (needsReset) {

                while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                    // if this is first load of the plugin the icon may not be added to icon collection yet so wait for it
                    await Task.Delay(100);
                }
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == presetHost.IconId);

                if (showMessages) {
                    // hide clipboard since its like 30 msgs
                    Mp.Services.NotificationBuilder.ShowMessageAsync(
                    msgType: MpNotificationType.PluginUpdated,
                    title: $"Plugin Updated",
                    iconSourceObj: ivm.IconBase64,
                    body: presetHost.PluginFormat.title)
                    .FireAndForgetSafeAsync();
                }



                db_presets = await CreateOrUpdatePresetsAsync(presetHost, db_presets);

                // now that db params are sync'd w/ new manifest update manifest backup for subsequent loads
                presetHost.PluginFormat.backupCheckPluginFormat = MpPluginLoader.CreateLastLoadedBackupPluginFormat(presetHost.PluginFormat);

                if (enableOnReset) {
                    // this is supposed to handle initial startup for CoreClipboard handler when no formats are enabled 
                    // but there's many cases that this may not be initial startup so:
                    // TODO instead of doing should notify clipboard collection that default was reset and only enable formats
                    // that don't have another handler with that format enabled
                    db_presets.ForEach(x => x.IsEnabled = true);

                    await Task.WhenAll(db_presets.Select(x => x.WriteToDatabaseAsync()));
                }
            }
            return db_presets.OrderBy(x => x.SortOrderIdx);
        }

        private static async Task<List<MpPluginPreset>> CreateOrUpdatePresetsAsync(
            MpIParameterHostViewModel pluginHost,
            List<MpPluginPreset> db_presets) {
            // when manifest changes this will:
            // 1. Create default presets if none exists
            // 2. Add/Remove preset parameter values based on parameter format differences since last successful load

            if (db_presets.Count == 0) {
                if (pluginHost.ComponentFormat.presets == null ||
                    pluginHost.ComponentFormat.presets.Count == 0) {
                    // when no presets exist in db or manifest, derive default from values
                    var presetModel = await CreateOrResetManifestPresetModelAsync(pluginHost, string.Empty);
                    db_presets.Add(presetModel);
                } else {
                    // load predefined presets into db w/ there static preset guids parsed from manifest 
                    foreach (var preset_guid in pluginHost.ComponentFormat.presets.Select(x => x.guid)) {
                        var presetModel = await CreateOrResetManifestPresetModelAsync(pluginHost, preset_guid);
                        db_presets.Add(presetModel);
                    }
                }
            }

            var last_successful_loaded_parameters =
                pluginHost.BackupComponentFormat == null || pluginHost.BackupComponentFormat.parameters == null ?
                    null : pluginHost.BackupComponentFormat.parameters;

            var modified_params = GetNewOrChangedParameters(
                pluginHost.ComponentFormat.parameters,
                last_successful_loaded_parameters);

            var params_to_remove = modified_params.Where(x => x.Item1 == null).Select(x => x.Item2);
            var params_to_add = modified_params.Where(x => x.Item2 == null).Select(x => x.Item1);

            foreach (var db_preset in db_presets) {
                // get each presets currently stored values
                var db_vals = await MpDataModelProvider.GetAllParameterHostValuesAsync(MpParameterHostType.Preset, db_preset.Id);

                // remove any no longer existing stored parameter values
                var db_vals_to_remove = db_vals.Where(x => params_to_remove.Any(y => y.ParamId.Equals(x.ParamId)));
                await Task.WhenAll(db_vals_to_remove.Select(x => x.DeleteFromDatabaseAsync()));

                // add new parameter value to preset
                foreach (MpParameterFormat param_to_add in params_to_add) {
                    foreach (var param_value_to_add in param_to_add.values) {
                        // add new param values to each preset

                        _ = await MpParameterValue.CreateAsync(
                            hostType: MpParameterHostType.Preset,
                            hostId: db_preset.Id,
                            paramId: param_to_add.paramId,
                            value: param_value_to_add.value);
                    }
                }

                // update preset manifest timestamp to sync with current document
                db_preset.ManifestLastModifiedDateTime = pluginHost.PluginFormat.manifestLastModifiedDateTime;
                await db_preset.WriteToDatabaseAsync();
            }
            return db_presets;
        }

        private static IEnumerable<Tuple<MpIParamterValueProvider, MpIParamterValueProvider>> GetNewOrChangedParameters(
            IEnumerable<MpIParamterValueProvider> manifest_parameters,
            IEnumerable<MpIParamterValueProvider> backup_parameters) {

            // this somewhat crudely decides if parameters were change by:
            // 1. if current parameter enumId did not previously exist
            // 2. if backup parameter enumId does not now exist


            manifest_parameters = manifest_parameters == null ? new List<MpIParamterValueProvider>() : manifest_parameters;
            backup_parameters = backup_parameters == null ? new List<MpIParamterValueProvider>() : backup_parameters;
            var result = new List<Tuple<MpIParamterValueProvider, MpIParamterValueProvider>>();

            // NOTE current db values are not checked and by design will fallback on parameter viewmodels validation for alterations
            // case 1
            var added_params = manifest_parameters.Where(x => backup_parameters.All(y => y.ParamId != x.ParamId));
            result.AddRange(added_params.Select(x => new Tuple<MpIParamterValueProvider, MpIParamterValueProvider>(x, null)));

            // case 2
            var removed_params = backup_parameters.Where(x => manifest_parameters.All(y => y.ParamId != x.ParamId));
            result.AddRange(removed_params.Select(x => new Tuple<MpIParamterValueProvider, MpIParamterValueProvider>(null, x)));

            return result;
        }


        public static async Task<MpPluginPreset> CreateOrResetManifestPresetModelAsync(
            MpIParameterHostViewModel pluginHost, string presetGuid, int sortOrderIdx = 0) {

            MpPluginPresetFormat preset_format = null;
            if (pluginHost.ComponentFormat.presets != null) {
                preset_format = pluginHost.ComponentFormat.presets.FirstOrDefault(x => x.guid == presetGuid);
            }

            if (preset_format == null) {
                // create empty preset..all properties fallback onto host
                preset_format = new MpPluginPresetFormat() {
                    guid = presetGuid
                };
                if (pluginHost.ComponentFormat.presets != null && pluginHost.ComponentFormat.presets.Count > 0) {
                    pluginHost.ComponentFormat.presets.Add(preset_format);
                } else {
                    pluginHost.ComponentFormat.presets = new List<MpPluginPresetFormat>() { preset_format };
                }

            }

            int preset_icon_id = pluginHost.IconId;
            if (!string.IsNullOrEmpty(preset_format.iconUri)) {
                preset_icon_id = await MpAvPluginIconLocator.LocatePluginIconIdAsync(pluginHost.PluginFormat, preset_format.iconUri);
            }
            string preset_label = preset_format.label;
            if (string.IsNullOrEmpty(preset_label)) {
                int manifest_preset_idx = pluginHost.ComponentFormat.presets.IndexOf(preset_format);
                string host_label = pluginHost.PluginFormat.title;
                if (pluginHost.ComponentFormat is MpILabelText lt &&
                    !string.IsNullOrEmpty(lt.LabelText)) {
                    // for clipboard handlers use display name instead of plugin name, 
                    // plugin name doesn't describe item
                    host_label += $" {lt.LabelText}";
                }
                preset_label = $"{host_label} - Default{manifest_preset_idx + 1}";
            }

            string preset_description = preset_format.description;
            if (string.IsNullOrEmpty(preset_description)) {
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
