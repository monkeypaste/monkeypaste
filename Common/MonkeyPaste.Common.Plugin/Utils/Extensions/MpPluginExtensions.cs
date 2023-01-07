using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    public static class MpPluginExtensions {

        private static MpIParameterKeyValuePair ValidateGet(MpPluginRequestFormatBase req, object paramId) {
            if (paramId == null) {
                throw new NullReferenceException("paramId is null, must have value");
            }
            if (req == null ||
                req.items == null) {
                throw new NullReferenceException($"Request error, no data");
            }

            var kvp = req.items.FirstOrDefault(x => x.paramId.ToString().Equals(paramId.ToString()));
            if (kvp == null) {
                throw new NullReferenceException($"param '{paramId}' not found");
            }
            return kvp;
        }
        public static bool GetRequestParamBoolValue(this MpPluginRequestFormatBase req, object paramId) {
            var kvp = ValidateGet(req,paramId);
            if (bool.TryParse(kvp.value, out bool boolVal)) {
                return boolVal;
            }
            throw new FormatException($"Error parsing bool for param '{paramId}'. Value was: '{kvp.value}'");
        }

        public static int GetRequestParamIntValue(this MpPluginRequestFormatBase req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            if (int.TryParse(kvp.value, out int intVal)) {
                return intVal;
            }
            throw new FormatException($"Error parsing int for param '{paramId}'. Value was: '{kvp.value}'");
        }

        public static double GetRequestParamDoubleValue(this MpPluginRequestFormatBase req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            if (double.TryParse(kvp.value, out double val)) {
                return val;
            }
            throw new FormatException($"Error parsing double for param '{paramId}'. Value was: '{kvp.value}'");
        }

        public static string GetRequestParamStringValue(this MpPluginRequestFormatBase req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value;
        }
        
        public static List<string> GetRequestParamStringListValue(this MpPluginRequestFormatBase req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value);
        }

        public static bool HasParam(this MpPluginRequestFormatBase req, object paramId) {
            if(req == null || req.items == null || req.items.All(x=>!x.paramId.Equals(paramId))) {
                return false;
            }
            return true;
        }
    }
}
