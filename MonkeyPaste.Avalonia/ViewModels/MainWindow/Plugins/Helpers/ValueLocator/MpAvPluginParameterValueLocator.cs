using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginParameterValueLocator {
        public static async Task<IEnumerable<MpPluginPresetParameterValue>> LocateValuesAsync(
            MpParameterHostType hostType,
            int paramHostId,
            MpIPluginHost pluginHost) {
            // get all preset values from db
            var presetValues = await MpDataModelProvider.GetPluginPresetValuesByPresetIdAsync(hostType,paramHostId);

            // loop through plugin formats parameters and add or replace (if found in db) to the preset values
            foreach (var paramFormat in pluginHost.ComponentFormat.parameters) {
                if (!presetValues.Any(x => paramFormat.paramId.Equals(x.ParamId))) {
                    // if no value is found in db for a parameter defined in manifest...

                    string paramVal = string.Empty;
                    if (paramFormat.values != null && paramFormat.values.Count > 0) {
                        // ensure value encoding is correct for control type
                        MpCsvFormatProperties csvProps =
                            paramFormat.controlType == MpPluginParameterControlType.MultiSelectList ||
                            paramFormat.controlType == MpPluginParameterControlType.EditableList ?
                                MpCsvFormatProperties.DefaultBase64Value : MpCsvFormatProperties.Default;

                        // if parameter has a predefined value (a case when not would be a text box that needs input so its value is empty)
                        var def_param_vals = paramFormat.values.Where(x => x.isDefault).ToList();
                        if (def_param_vals.Count == 0) {
                            // if no default is defined use first available value
                            def_param_vals.Add(paramFormat.values[0]);
                        }
                        paramVal = def_param_vals.Select(x => x.value).ToList().ToCsv(csvProps);
                    }
                    var newPresetVal = await MpPluginPresetParameterValue.CreateAsync(
                        hostType: hostType,
                        presetId: paramHostId,
                        paramId: paramFormat.paramId,
                        value: paramVal
                        //format: paramFormat
                        );

                    presetValues.Add(newPresetVal);
                }
            }
            //presetValues.ForEach(x => x.ParameterFormat = AnalyzerFormat.parameters.FirstOrDefault(y => y.paramName == x.ParamName));
            return presetValues;
        }


    }
}
