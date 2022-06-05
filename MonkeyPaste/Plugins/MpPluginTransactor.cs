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
                    var errorActionResult = await HandleError(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                    return errorActionResult;
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
                    var errorActionResult = await HandleError(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                    return errorActionResult;
                }

                int sourceId = await MpPluginLogger.LogTransaction(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);

                try {
                    at.ResponseContent = await MpPluginResponseConverter.Convert(at, sourceCopyItem, sourceId, suppressWrite);
                }
                catch (Exception ex) {
                    var errorActionResult = await HandleError(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                    return errorActionResult;
                }

                return at;
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

            var userAction = await MpNotificationCollectionViewModel.Instance.ShowUserAction(
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
