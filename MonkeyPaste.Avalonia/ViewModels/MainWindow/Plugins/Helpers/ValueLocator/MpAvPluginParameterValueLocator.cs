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
            MpPluginPreset host_db_preset = null;
            if (hostType == MpParameterHostType.Preset) {
                host_db_preset = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(paramHostId);
            }
            // get all preset values from db
            var param_db_values = await MpDataModelProvider.GetAllParameterHostValuesAsync(hostType, paramHostId);

            // loop through plugin formats parameters and add or replace (if found in db) to the preset values
            foreach (MpParameterFormat paramFormat in pluginHost.ComponentFormat.parameters) {
                if (!param_db_values.Any(x => paramFormat.paramId.Equals(x.ParamId))) {
                    // if no value is found in db for a parameter defined in manifest...

                    string paramVal = string.Empty;
                    if (host_db_preset != null && pluginHost.ComponentFormat.presets != null) {
                        // when param is part of a preset prefer manifest preset value
                        var host_format_preset = pluginHost.ComponentFormat.presets.FirstOrDefault(x => x.guid == host_db_preset.Guid);
                        if (host_format_preset == null) {
                            // manifest presets should hardset preset guid from manifest file, is this an old analyzer?
                            //Debugger.Break();
                        } else if (host_format_preset.values != null) {
                            var host_format_preset_val = host_format_preset.values.FirstOrDefault(x => x.paramId.Equals(paramFormat.paramId));
                            if (host_format_preset_val != null) {
                                // this parameter has a preset value in manifest

                                paramVal = host_format_preset_val.value.ToListFromCsv(paramFormat.CsvProps).ToCsv(paramFormat.CsvProps);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(paramVal) &&
                        paramFormat.values != null && paramFormat.values.Count > 0) {
                        // ensure value encoding is correct for control type

                        // if parameter has a predefined value (a case when not would be a text box that needs input so its value is empty)
                        var def_param_vals = paramFormat.values.Where(x => x.isDefault).ToList();
                        if (def_param_vals.Count == 0) {
                            // if no default is defined use first available value
                            def_param_vals.Add(paramFormat.values[0]);
                        }

                        paramVal = def_param_vals.Select(x => x.value).ToList().ToCsv(paramFormat.CsvProps);
                    }

                    var newPresetVal = await MpParameterValue.CreateAsync(
                        hostType: hostType,
                        hostId: paramHostId,
                        paramId: paramFormat.paramId,
                        value: paramVal
                        //format: paramFormat
                        );

                    param_db_values.Add(newPresetVal);
                }
            }
            //presetValues.ForEach(x => x.ParameterFormat = AnalyzerFormat.parameters.FirstOrDefault(y => y.paramName == x.ParamName));
            return param_db_values;
        }


    }
}
