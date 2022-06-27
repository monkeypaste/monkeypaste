using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MonkeyPaste {
    public static class MpPluginTransactor {
        // TODO In Release Mode change these to smaller values
        private const int _ANALYZE_TIMEOUT_MS = 1000000;
        private const int _PROCESS_TIMEOUT_MS = 500000;

        public static async Task<MpPluginTransactionBase> PerformTransaction(
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

            if(pluginComponent is MpIAnalyzeAsyncComponent || pluginComponent is MpIAnalyzerComponent) {
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
                    return await HandleErrorAsync(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
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
                        at.Response = await analyzeAsyncComponent.AnalyzeAsync(at.Request as MpAnalyzerPluginRequestFormat);
                    } else if(pluginComponent is MpIAnalyzerComponent analyzeComponent) {
                        at.Response = analyzeComponent.Analyze(at.Request as MpAnalyzerPluginRequestFormat);
                    }
                    at.ResponseTime = DateTime.Now;
                }
                catch (Exception ex) {
                    return await HandleErrorAsync(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                }

                // LOG TRANSACTION

                int sourceId = await MpPluginLogger.LogTransactionAsync(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);

                // PROCESS RESPONSE
                try {
                    at.ResponseContent = await MpPluginResponseConverter.Convert(at, sourceCopyItem, sourceId, suppressWrite);
                    return at;
                }
                catch (Exception ex) {
                    return await HandleErrorAsync(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                }
            }

            return null;
        }

        public static bool ValidatePluginResponse(MpPluginResponseFormatBase response) {
            if (response == null) {
                //MpConsole.WriteTraceLine($"Clipboard Reader Plugin error, no response from {handler.ToString()}, (ignoring its assigned formats) ");
                MpConsole.WriteTraceLine($"Clipboard Reader Plugin error, empty response ");
                return false;
            }
            if (response.errorMessage != null) {
                MpConsole.WriteTraceLine($"Plugin error (ignoring new clipboard data): " + response.errorMessage);
                return false;
            }
            if (response.otherMessage != null) {
                MpConsole.WriteLine(response.otherMessage);
            }

            return true;
        }

        private static async Task<MpPluginTransactionBase> HandleErrorAsync(
            Exception ex, 
            MpPluginFormat pluginFormat, 
            MpPluginTransactionBase at, MpCopyItem sourceCopyItem, object sourceHandler, bool suppressWrite = false) {
            MpConsole.WriteTraceLine(ex);
            at.TransactionErrorMessage = ex.ToString();
            await MpPluginLogger.LogTransactionAsync(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite)
                                    .TimeoutAfter(TimeSpan.FromMilliseconds(_PROCESS_TIMEOUT_MS));

            var userAction = await MpNotificationCollectionViewModel.Instance.ShowNotification(
                dialogType: MpNotificationDialogType.InvalidRequest,
                msg: ex.Message,
                maxShowTimeMs: 5000);
            
            if (at.Response == null) {
                at.Response = new MpPluginResponseFormatBase();
            }
            if (userAction == MpDialogResultType.Retry) {
                
                at.Response.retryMessage = "Retry";
            } else {
                at.Response.errorMessage = "Transaction Error";
            }

            return at;
        }
    }
}
