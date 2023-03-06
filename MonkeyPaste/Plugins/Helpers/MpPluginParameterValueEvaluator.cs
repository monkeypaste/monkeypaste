using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpPluginParameterValueEvaluator {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public static string[] PhysicalComparePropertyPaths {
            get {
                var paths = new List<string>();
                for (int i = 0; i < Enum.GetNames(typeof(MpContentQueryPropertyPathType)).Length; i++) {
                    string path = string.Empty;
                    MpContentQueryPropertyPathType cppt = (MpContentQueryPropertyPathType)i;
                    switch (cppt) {
                        case MpContentQueryPropertyPathType.ItemData:
                        case MpContentQueryPropertyPathType.ItemType:
                        case MpContentQueryPropertyPathType.Title:
                        case MpContentQueryPropertyPathType.CopyDateTime:
                        case MpContentQueryPropertyPathType.CopyCount:
                        case MpContentQueryPropertyPathType.PasteCount:
                            path = cppt.ToString();
                            break;
                        case MpContentQueryPropertyPathType.AppName:
                        case MpContentQueryPropertyPathType.AppPath:
                            path = string.Format(@"Source.App.{0}", cppt.ToString());
                            break;
                        case MpContentQueryPropertyPathType.UrlPath:
                        case MpContentQueryPropertyPathType.UrlTitle:
                        case MpContentQueryPropertyPathType.UrlDomainPath:
                            path = string.Format(@"Source.App.{0}", cppt.ToString());
                            break;
                        default:
                            break;
                    }
                    paths.Add(path);
                }
                return paths.ToArray();
            }
        }
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public static async Task<string> GetParameterRequestValueAsync(
            MpParameterControlType controlType,
            MpParameterValueUnitType valueType,
            string curVal,
            MpCopyItem ci) {
            switch (valueType) {
                case MpParameterValueUnitType.PlainTextContentQuery:
                    curVal = await GetParameterQueryResultAsync(controlType, curVal, ci, false);
                    break;
                case MpParameterValueUnitType.RawDataContentQuery:
                    curVal = await GetParameterQueryResultAsync(controlType, curVal, ci, true);
                    break;
                case MpParameterValueUnitType.Base64Text:
                    curVal = curVal.ToBytesFromBase64String().ToBase64String();
                    break;
                case MpParameterValueUnitType.FileSystemPath:
                    if (string.IsNullOrWhiteSpace(curVal)) {
                        break;
                    }
                    curVal = await curVal.ToFileAsync();
                    break;
                default:
                    curVal = Mp.Services.StringTools.ToPlainText(curVal);
                    break;
            }
            return curVal;
        }
        public static async Task<object> QueryPropertyAsync(MpCopyItem ci, MpContentQueryPropertyPathType queryPathType) {
            if (ci == null) {
                return null;
            }
            switch (queryPathType) {
                case MpContentQueryPropertyPathType.None:
                case MpContentQueryPropertyPathType.LastOutput:
                    return null;
                case MpContentQueryPropertyPathType.ItemRefUrl:
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
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private static async Task<string> GetParameterQueryResultAsync(MpParameterControlType controlType, string curVal, MpCopyItem ci, bool asRawData) {
            if (MpParameterFormat.IsControlCsvValue(controlType)) {
                // for csv values, split decode actual text to get query result then return re-encoded csv
                var csvProps = MpParameterFormat.GetControlCsvProps(controlType);
                var decoded_vals = curVal.ToListFromCsv(csvProps);
                var decoded_val_results = new List<string>();
                foreach (var decoded_val in decoded_vals) {
                    string decoded_val_result = await GetParameterQueryResultAsync(MpParameterControlType.TextBox, decoded_val, ci, asRawData);
                    decoded_val_results.Add(decoded_val_result);
                }
                return decoded_val_results.ToCsv(csvProps);
            }

            for (int i = 1; i < Enum.GetNames(typeof(MpContentQueryPropertyPathType)).Length; i++) {
                // example content query: '{Title} is a story about {ItemData}'
                MpContentQueryPropertyPathType ppt = (MpContentQueryPropertyPathType)i;
                if (ppt == MpContentQueryPropertyPathType.LastOutput) {
                    continue;
                }

                string pptPathEnumName = ppt.ToString();
                string pptToken = "{" + pptPathEnumName + "}";

                if (curVal.Contains(pptToken)) {
                    string contentValue = await QueryPropertyAsync(ci, ppt) as string;

                    if (!asRawData) {
                        contentValue = await GetParameterRequestValueAsync(controlType, MpParameterValueUnitType.PlainText, contentValue, ci);
                    }
                    string pptTokenBackup = "{@" + ppt.ToString() + "@}";
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
