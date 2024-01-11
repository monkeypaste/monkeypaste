using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpPluginParameterValueEvaluator {
        #region Private Variable
        #endregion

        #region Constants
        public const string TOKEN_OPEN = "{";
        public const string TOKEN_CLOSE = "}";
        public const string TOKEN_INNER_BACKUP = "@";
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public static async Task<string> GetParameterRequestValueAsync(
            MpParameterControlType controlType,
            MpParameterValueUnitType valueType,
            string curVal,
            MpCopyItem ci,
            object last_output_args = null) {
            switch (valueType) {
                case MpParameterValueUnitType.PlainTextContentQuery:
                    curVal = await GetParameterQueryResultAsync(controlType, curVal, ci, false, false, last_output_args);
                    break;
                case MpParameterValueUnitType.UriEscapedPlainTextContentQuery:
                    curVal = await GetParameterQueryResultAsync(controlType, curVal, ci, false, true, last_output_args);
                    break;
                case MpParameterValueUnitType.RawDataContentQuery:
                    curVal = await GetParameterQueryResultAsync(controlType, curVal, ci, true, false, last_output_args);
                    break;
                default:
                    curVal = Mp.Services.StringTools.ToPlainText(curVal);
                    break;
            }
            return curVal;
        }
        public static async Task<object> QueryPropertyAsync(
            MpCopyItem ci,
            MpContentQueryPropertyPathType queryPathType,
            object last_output_args) {
            if (ci == null) {
                return null;
            }
            switch (queryPathType) {
                case MpContentQueryPropertyPathType.None:
                    return null;
                case MpContentQueryPropertyPathType.LastOutput:
                    return QueryLastOutput(last_output_args);
                case MpContentQueryPropertyPathType.ItemData:
                case MpContentQueryPropertyPathType.ItemType:
                case MpContentQueryPropertyPathType.Title:
                case MpContentQueryPropertyPathType.CopyDateTime:
                case MpContentQueryPropertyPathType.CopyCount:
                case MpContentQueryPropertyPathType.PasteCount:
                    return ci.GetPropertyValue(queryPathType.ToString());
                case MpContentQueryPropertyPathType.SourceDeviceType:
                    var deviceTypeInt = await MpDataModelProvider.GetSortableCopyItemViewPropertyAsync<int>(ci.Id, queryPathType.ToString());
                    return (MpUserDeviceType)deviceTypeInt;
                default:
                    //UrlPath,UrlTitle,UrlDomainPath,AppPath,AppName,SourceDeviceName,SourceDeviceType
                    var resultStr = await MpDataModelProvider.GetSortableCopyItemViewPropertyAsync<string>(ci.Id, queryPathType.ToString());
                    return resultStr;

            }
        }

        public static string GetQueryToken(MpContentQueryPropertyPathType cqppt) {
            return $"{TOKEN_OPEN}{cqppt}{TOKEN_CLOSE}";
        }
        public static string GetQueryBackupToken(MpContentQueryPropertyPathType cqppt) {
            return $"{TOKEN_OPEN}{TOKEN_INNER_BACKUP}{cqppt}{TOKEN_INNER_BACKUP}{TOKEN_CLOSE}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private static string QueryLastOutput(object last_output_args) {
            if (last_output_args is not object[] argParts ||
                argParts[0] is not MpAvActionOutput ao ||
                argParts[1].ToStringOrEmpty() is not string json_path_query) {
                return null;
            }
            MpPluginResponseFormatBase prf = ao.OutputData as MpPluginResponseFormatBase;
            if (prf == null && ao is MpAvAnalyzeOutput ano) {
                prf = ano.PluginResponse;
            }

            if (prf != null && !string.IsNullOrWhiteSpace(json_path_query)) {
                return MpJsonPathProperty.Query(prf, json_path_query);
            } else if (ao.OutputData != null) {
                return ao.OutputData.ToString();
            }
            return null;
        }

        private static async Task<string> GetParameterQueryResultAsync(
            MpParameterControlType controlType,
            string curVal,
            MpCopyItem ci,
            bool asRawData,
            bool uriEscaped,
            object last_output_args) {
            if (controlType.IsControlCsvValue()) {
                // for csv values, split decode actual text to get query result then return re-encoded csv
                var csvProps = controlType.GetControlCsvProps();
                var decoded_vals = curVal.ToListFromCsv(csvProps);
                var decoded_val_results = new List<string>();
                foreach (var decoded_val in decoded_vals) {
                    string decoded_val_result = await GetParameterQueryResultAsync(MpParameterControlType.TextBox, decoded_val, ci, asRawData, uriEscaped, last_output_args);
                    decoded_val_results.Add(decoded_val_result);
                }
                return decoded_val_results.ToCsv(csvProps);
            }

            for (int i = 1; i < Enum.GetNames(typeof(MpContentQueryPropertyPathType)).Length; i++) {
                // example content query: '{Title} is a story about {ItemData}'
                MpContentQueryPropertyPathType ppt = (MpContentQueryPropertyPathType)i;
                string pptToken = GetQueryToken(ppt);

                if (curVal.Contains(pptToken)) {
                    string contentValue = await QueryPropertyAsync(ci, ppt, last_output_args) as string;

                    if (!asRawData) {
                        contentValue = await GetParameterRequestValueAsync(controlType, MpParameterValueUnitType.PlainText, contentValue, ci, last_output_args);
                        if (uriEscaped) {
                            try {

                                contentValue = Uri.EscapeDataString(contentValue);
                            }
                            catch (Exception ex) {
                                MpConsole.WriteTraceLine($"Error escaping data string: {contentValue}", ex);
                                contentValue = string.Empty;
                            }
                        }
                    }
                    string pptTokenBackup = GetQueryBackupToken(ppt);
                    if (curVal.Contains(pptTokenBackup)) {
                        //this content query token has conflicts so use the backup
                        // example content query needing backup: '{Title} is {Title} but {@Title@} is content'
                        pptToken = pptTokenBackup;
                    }
                    curVal = curVal.Replace(pptToken, contentValue);
                }
            }
            return curVal;
        }
        #endregion

        #region Commands
        #endregion





    }
}
