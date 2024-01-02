using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Plugin {
    public static class MpPluginExtensions {

        private static MpParameterRequestItemFormat ValidateGet(MpPluginParameterRequestFormat req, object paramId) {
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
        public static bool GetRequestParamBoolValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ParseOrConvertToBool();
        }

        public static int GetRequestParamIntValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ParseOrConvertToInt();
        }

        public static double GetRequestParamDoubleValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ParseOrConvertToDouble();
        }

        public static string GetRequestParamStringValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value;
        }

        public static List<string> GetRequestParamStringListValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value);
        }

        public static bool HasParam(this MpPluginParameterRequestFormat req, object paramId) {
            if (req == null || req.items == null || req.items.All(x => !x.paramId.Equals(paramId))) {
                return false;
            }
            return true;
        }
    }
}
