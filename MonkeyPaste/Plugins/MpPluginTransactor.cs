using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpPluginTransactor {
        // TODO In Release Mode change these to smaller values
        private const int _ANALYZE_TIMEOUT_MS = 1000000;
        private const int _PROCESS_TIMEOUT_MS = 500000;

        public static async Task<object> PerformTransaction(
            MpPluginFormat pluginFormat, 
            MpIPluginComponentBase pluginComponent,
            Dictionary<int,string> paramValues,
            MpCopyItem sourceCopyItem,
            object sourceHandler,
            bool suppressWrite = false) { 

            // This method:
            // 0. checks data between each step and returns error info or retry message to calling analyzer if there's a problem
            // 1. prepares plugin parameters using sourceCopyItem and given param values
            // 2. sends request to the plugin
            // 3. checks response from the plugin to ensure it was successful
            // 4. logs the transaction (needed for restful api call auditing or  yet to be implemented more detailed source information) 
            // 5. Converts the response to new content and/or updates source content (since from another, potentially 3rd party module)
            // 6. Returns new or updated content

            if(pluginComponent is MpIAnalyzeAsyncComponent || pluginComponent is MpIAnalyzeComponent) {
                MpAnalyzerTransaction at = new MpAnalyzerTransaction() {
                    RequestTime = DateTime.Now,
                };

                // CREATE REQUEST
                try {
                    at.Request = await MpPluginRequestBuilder.BuildRequest(
                                        pluginFormat.analyzer.parameters,
                                        paramValues,
                                        sourceCopyItem);
                } catch(Exception ex) {
                    var errorOrRetryActionResult = await HandleError(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                    return errorOrRetryActionResult;
                }

                // FIND CONTENT
                MpPluginParameterFormat contentParam = pluginFormat.analyzer.parameters
                    .FirstOrDefault(x => x.unitType == MpPluginParameterValueUnitType.PlainTextContentQuery);
                                
                at.RequestContent = contentParam == null ? 
                    null :
                    (at.Request as MpAnalyzerPluginRequestFormat).items
                    .FirstOrDefault(x => x.paramId == contentParam.paramId).value;

                // GET RESPONSE
                try {
                    if(pluginComponent is MpIAnalyzeAsyncComponent analyzeAsyncComponent) {
                        at.Response = await analyzeAsyncComponent.AnalyzeAsync(JsonConvert.SerializeObject(at.Request));
                    } else if(pluginComponent is MpIAnalyzeComponent analyzeComponent) {
                        at.Response = analyzeComponent.Analyze(JsonConvert.SerializeObject(at.Request));
                    }
                    at.ResponseTime = DateTime.Now;
                }
                catch (Exception ex) {
                    var errorOrRetryActionResult = await HandleError(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                    return errorOrRetryActionResult;
                }

                // LOG TRANSACTION

                int sourceId = await MpPluginLogger.LogTransaction(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);

                // PROCESS RESPONSE
                try {
                    at.ResponseContent = await MpPluginResponseConverter.Convert(at, sourceCopyItem, sourceId, suppressWrite);
                    return at;
                }
                catch (Exception ex) {
                    var errorOrRetryActionResult = await HandleError(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                    return errorOrRetryActionResult;
                }
            }

            return null;
        }

        private static async Task<string> HandleError(
            Exception ex, 
            MpPluginFormat pluginFormat, 
            MpPluginTransaction at, MpCopyItem sourceCopyItem, object sourceHandler, bool suppressWrite = false) {
            MpConsole.WriteTraceLine(ex);
            at.TransactionErrorMessage = ex.ToString();
            await MpPluginLogger.LogTransaction(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite)
                                    .TimeoutAfter(TimeSpan.FromMilliseconds(_PROCESS_TIMEOUT_MS));

            var userAction = await MpNotificationCollectionViewModel.Instance.ShowNotification(
                dialogType: MpNotificationDialogType.InvalidRequest,
                exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                msg: ex.Message,
                maxShowTimeMs: 5000);
            if (userAction == MpDialogResultType.Retry) {
                return MpPluginResponseFormat.RETRY_MESSAGE;
            }

            return MpPluginResponseFormat.ERROR_MESSAGE;
        }
    }
}
