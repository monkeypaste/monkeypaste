using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginPresetLocator {
        public static async Task<IEnumerable<MpPluginPreset>> LocatePresetsAsync(
            MpIPluginHost host, 
            IEnumerable<MpPluginPreset> manifest_presets,
            bool enableOnReset = false) {
            var db_presets = await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(host.PluginGuid);

            bool isNew = db_presets.Count == 0;
            bool isManifestModified = db_presets.Any(x => x.ManifestLastModifiedDateTime < host.PluginFormat.manifestLastModifiedDateTime);
            bool needsReset = isNew || isManifestModified;
            if (needsReset) {

                while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                    // if this is first load of the plugin the icon may not be added to icon collection yet so wait for it
                    await Task.Delay(100);
                }
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == host.IconId);

                MpNotificationBuilder.ShowMessageAsync(
                    msgType: MpNotificationType.PluginUpdated,
                    title: $"Analyzer '{host.PluginFormat.title}' Updated",
                    iconSourceObj: ivm.IconBase64,
                    body: "Reseting presets to default...")
                    .FireAndForgetSafeAsync();

                db_presets = await ResetPresetsAsync(host, db_presets,manifest_presets);
                isNew = true;

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

        private static async Task<List<MpPluginPreset>> ResetPresetsAsync(
            MpIPluginHost pluginHost, List<MpPluginPreset> db_presets, IEnumerable<MpPluginPreset> manifest_presets) {
            //if manifest has been modified
            //(for now clear all presets and either load predefined presets or create from parameter default values)

            // TODO maybe less forceably handle add/remove/update of presets when manifest changes
            db_presets = db_presets == null ? await MpDataModelProvider.GetPluginPresetsByPluginGuidAsync(pluginHost.PluginGuid) : db_presets;
            foreach (var preset in db_presets) {
                var vals = await MpDataModelProvider.GetPluginPresetValuesByPresetIdAsync(MpParameterHostType.Preset, preset.Id);
                await Task.WhenAll(vals.Select(x => x.DeleteFromDatabaseAsync()));
            }
            await Task.WhenAll(db_presets.Select(x => x.DeleteFromDatabaseAsync()));

            db_presets.Clear();
            if (manifest_presets.IsNullOrEmpty()) {
                //only generate default preset if no presets defined in manifest
                var defualtPreset = await CreateDefaultPresetModelAsync(pluginHost);
                db_presets.Add(defualtPreset);
            } else {
                //when presets are defined in manifest create the preset and its values in the db
                foreach (var presetFormat in pluginHost.ComponentFormat.presets) {
                    var presetModel = await MpPluginPreset.CreateAsync(
                        pluginGuid: pluginHost.PluginGuid,
                        isDefault: presetFormat.isDefault,
                        label: presetFormat.label,
                        iconId: pluginHost.IconId,
                        sortOrderIdx: pluginHost.ComponentFormat.presets.IndexOf(presetFormat),
                        description: presetFormat.description,
                        //format: AnalyzerPluginFormat,
                        manifestLastModifiedDateTime: pluginHost.PluginFormat.manifestLastModifiedDateTime);

                    foreach (var presetValueModel in presetFormat.values) {
                        // only creat preset values in db, they will then be picked up when the preset vm is initialized
                        var aipv = await MpPluginPresetParameterValue.CreateAsync(
                            hostType: MpParameterHostType.Preset,
                            presetId: presetModel.Id,
                            paramId: presetValueModel.paramId,
                            value: presetValueModel.value
                            //format: AnalyzerPluginFormat.parameters.FirstOrDefault(x => x.paramName == presetValueModel.paramName)
                            );
                    }

                    db_presets.Add(presetModel);
                }
                if (db_presets.All(x => x.IsDefault == false) && db_presets.Count > 0) {
                    db_presets[0].IsDefault = true;
                }
            }
            return db_presets;
        }


        public static async Task<MpPluginPreset> CreateDefaultPresetModelAsync(MpIPluginHost pluginHost, int existingDefaultPresetId = 0, int sortOrderIdx = 0) {
            var aip = await MpPluginPreset.CreateAsync(
                                pluginGuid: pluginHost.PluginGuid,
                                isDefault: true,
                                label: $"{pluginHost.PluginFormat.title} - Default",
                                iconId: pluginHost.IconId,
                                sortOrderIdx: sortOrderIdx,
                                description: $"Auto-generated default preset for '{pluginHost.PluginFormat.title}'",
                                //format: AnalyzerPluginFormat,
                                manifestLastModifiedDateTime: pluginHost.PluginFormat.manifestLastModifiedDateTime,
                                existingDefaultPresetId: existingDefaultPresetId);

            return aip;
        }
    }
}
