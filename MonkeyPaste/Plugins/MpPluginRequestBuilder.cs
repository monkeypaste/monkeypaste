using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;

namespace MonkeyPaste {
    public static class MpPluginRequestBuilder {
        public static async Task<MpAnalyzerPluginRequestFormat> BuildRequest(
            List<MpAnalyticItemParameterFormat> paramFormats,
            Dictionary<int,string> paramValues,
            MpCopyItem sourceContent) {
            
            List<MpAnalyzerPluginRequestItemFormat> requestItems = new List<MpAnalyzerPluginRequestItemFormat>();

            foreach(var paramFormat in paramFormats) {
                MpAnalyzerPluginRequestItemFormat requestItem = await BuildRequestItem(
                    paramFormat,
                    paramValues[paramFormat.paramId],
                    sourceContent);
                requestItems.Add(requestItem);
            }

            return new MpAnalyzerPluginRequestFormat() {
                items = requestItems
            };
        }

        private static async Task<MpAnalyzerPluginRequestItemFormat> BuildRequestItem(
            MpAnalyticItemParameterFormat paramFormat, 
            string paramValue,
            MpCopyItem sourceContent) {
            return new MpAnalyzerPluginRequestItemFormat() { 
                paramId = paramFormat.paramId, 
                value = await GetParameterRequestValue(paramFormat.unitType,paramValue, sourceContent)
            };
        }

        private static async Task<string> GetParameterRequestValue(MpAnalyticItemParameterValueUnitType valueType, string curVal, MpCopyItem ci) {
            switch (valueType) {
                case MpAnalyticItemParameterValueUnitType.ContentQuery:
                    curVal = await GetParameterQueryResult(curVal, ci);
                    break;
                case MpAnalyticItemParameterValueUnitType.Base64Text:
                    curVal = curVal.ToByteArray().ToBase64String();
                    break;
                case MpAnalyticItemParameterValueUnitType.FileSystemPath:
                    if (string.IsNullOrWhiteSpace(curVal)) {
                        break;
                    }
                    curVal = curVal.ToFile();
                    break;
                default:
                    curVal = MpNativeWrapper.Services.StringTools.ToPlainText(curVal);
                    break;
            }
            return curVal;
        }

        private static async Task<string> GetParameterQueryResult(string curVal, MpCopyItem ci) {
            for (int i = 1; i < Enum.GetNames(typeof(MpCopyItemPropertyPathType)).Length; i++) {
                // example content query: '{Title} is a story about {ItemData}'
                var ppt = (MpCopyItemPropertyPathType)i;
                string pptPathEnumName = ppt.ToString();
                string pptToken = "{" + pptPathEnumName + "}";

                if (curVal.Contains(pptToken)) {
                    string contentValue = await MpCopyItem.QueryProperty(ci, ppt) as string;
                    contentValue = await GetParameterRequestValue(MpAnalyticItemParameterValueUnitType.PlainText, contentValue, ci);
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
    }
}
