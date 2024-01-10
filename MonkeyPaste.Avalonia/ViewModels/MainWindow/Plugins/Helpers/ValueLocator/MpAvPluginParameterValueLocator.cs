using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginParameterValueLocator {
        public static async Task<IEnumerable<MpParameterValue>> LocateValuesAsync(
            MpParameterHostType hostType,
            int paramHostId,
            MpIParameterHostViewModel pluginHost) {
            MpPreset host_db_preset = null;
            if (hostType == MpParameterHostType.Preset) {
                host_db_preset = await MpDataModelProvider.GetItemAsync<MpPreset>(paramHostId);
            }
            // get all preset values from db
            var param_db_values = await MpDataModelProvider.GetAllParameterHostValuesAsync(hostType, paramHostId);

            // loop through plugin formats parameters and add or replace (if found in db) the preset values
            foreach (MpParameterFormat paramFormat in pluginHost.ComponentFormat.parameters) {
                if (paramFormat.isValueDeferred) {
                    // make deferred paramValue request
                    var req = new MpPluginDeferredParameterValueRequestFormat() { paramId = paramFormat.paramId };
                    MpPluginDeferredParameterValueResponseFormat resp = await RequestDefferredValuesAsync(pluginHost, req);

                    if (resp == null) {
                        // values will just be empty, up to plugin configuration to handle that
                    } else {
                        paramFormat.values = resp.Values;
                    }
                }
                if (!param_db_values.Any(x => paramFormat.paramId.Equals(x.ParamId))) {
                    // if no paramValue is found in db for a parameter defined in manifest...

                    string paramVal = string.Empty;
                    if (host_db_preset != null && pluginHost.ComponentFormat.presets != null) {
                        // when param is part of a preset prefer manifest preset paramValue
                        var host_format_preset = pluginHost.ComponentFormat.presets.FirstOrDefault(x => x.guid == host_db_preset.Guid);
                        if (host_format_preset == null) {
                            // manifest presets should hardset preset guid from manifest file, is this an old analyzer?
                            //MpDebug.Break();
                        } else if (host_format_preset.values != null) {
                            var host_format_preset_val = host_format_preset.values.FirstOrDefault(x => x.paramId.Equals(paramFormat.paramId));
                            if (host_format_preset_val != null) {
                                // this parameter has a preset paramValue in manifest
                                paramVal = host_format_preset_val.value.ToListFromCsv(paramFormat.CsvProps).ToCsv(paramFormat.CsvProps);
                            }
                        }
                    }
                    if (paramFormat.isSharedValue) {
                        // for persistent param's set paramValue using any other preset if found
                        MpDebug.Assert(string.IsNullOrEmpty(paramVal), "Preset w/ persistent param validation failed (should be caught in plugin loader)");

                        var existing_persist_pvl = await MpDataModelProvider.GetAllParameterValueInstancesForPluginAsync(pluginHost.PluginGuid, paramFormat.paramId);
                        if (existing_persist_pvl.Any()) {
                            paramVal = existing_persist_pvl.FirstOrDefault().Value;
                        }
                    }

                    if (string.IsNullOrEmpty(paramVal) &&
                        paramFormat.values != null && paramFormat.values.Count > 0) {
                        // ensure paramValue encoding is correct for control type

                        // if parameter has a predefined paramValue (a case when not would be a text box that needs input so its paramValue is empty)
                        var def_param_vals = paramFormat.values.Where(x => x.isDefault).ToList();
                        if (def_param_vals.Count == 0) {
                            // if no default is defined use first available paramValue
                            def_param_vals.Add(paramFormat.values[0]);
                        }

                        paramVal = def_param_vals.Select(x => x.value).ToList().ToCsv(paramFormat.CsvProps);
                    }

                    var newPresetVal = await MpParameterValue.CreateAsync(
                        hostType: hostType,
                        hostId: paramHostId,
                        paramId: paramFormat.paramId,
                        value: paramVal);

                    param_db_values.Add(newPresetVal);
                }
            }
            //presetValues.ForEach(x => x.ParameterFormat = AnalyzerFormat.parameters.FirstOrDefault(y => y.paramName == x.ParamName));
            return param_db_values;
        }


        private static async Task<MpPluginDeferredParameterValueResponseFormat> RequestDefferredValuesAsync(MpIParameterHostViewModel pluginHost, MpPluginDeferredParameterValueRequestFormat req) {
            string method_name = nameof(MpISupportDeferredValue.RequestParameterValue);
            string on_type = typeof(MpISupportDeferredValue).FullName;
            var resp = await pluginHost.PluginFormat.IssueRequestAsync<MpPluginDeferredParameterValueResponseFormat>(method_name, on_type, req);
            return resp;
        }
    }
}
