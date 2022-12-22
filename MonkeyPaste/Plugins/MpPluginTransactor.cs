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
        #region Constants
        // TODO In Release Mode change these to smaller values
        private const int _ANALYZE_TIMEOUT_MS = 1000000;
        private const int _PROCESS_TIMEOUT_MS = 500000;
        #endregion

        #region Public Methods

        public static async Task<MpPluginTransactionBase> PerformTransaction(
            MpPluginFormat pluginFormat, 
            MpIPluginComponentBase pluginComponent,
            Dictionary<object,string> paramValues,
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
                    .FirstOrDefault(x => x.paramId.Equals(contentParam.paramId)).value;

                // TODO (for http) phone home w/ request half of transaction and await return

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

                // LOG TRANSACTION (create record of params, ref to plugin source(local/remote))

                int ci_trans_id = await MpPluginLogger.LogTransactionAsync(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);

                // TODO (for http) phone home w/ response to finish transaction record and await return

                // PROCESS RESPONSE
                try {
                    at.ResponseContent = await MpPluginResponseConverter.ConvertAsync(at, sourceCopyItem, ci_trans_id, suppressWrite);
                    return at;
                }
                catch (Exception ex) {
                    return await HandleErrorAsync(ex, pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);
                }
            }

            return null;
        }

        public static async Task<T> ValidatePluginResponseAsync<T>(
            MpPluginRequestFormatBase request,
            MpPluginResponseFormatBase response,
            Func<Task<T>> retryFunc) where T : MpPluginResponseFormatBase {
            if (response == null) {
                //MpConsole.WriteTraceLine($"Clipboard Reader Plugin error, no response from {handler.ToString()}, (ignoring its assigned formats) ");
                MpConsole.WriteTraceLine($"Clipboard Reader Plugin error, empty response ");
                return null;
            }
            if (!string.IsNullOrWhiteSpace(response.errorMessage)) {
                MpConsole.WriteTraceLine($"Plugin error (ignoring new clipboard data): " + response.errorMessage);
                return null;
            }
            if (response.otherMessage != null) {
                MpConsole.WriteLine(response.otherMessage);
            }
            response = await HandlePluginNotifcationsAsync<T>(request, response, retryFunc);

            return response as T;
        }

        #endregion

        #region Private Methods

        private static async Task<MpPluginResponseFormatBase> HandlePluginNotifcationsAsync<T>(
            MpPluginRequestFormatBase request, 
            MpPluginResponseFormatBase response,
            Func<Task<T>> retryFunc,
            int cur_idx = 0) where T:MpPluginResponseFormatBase {
            if (response.userNotifications == null || response.userNotifications.Count == 0) {
                return response;
            }

            for (int i = cur_idx; i < response.userNotifications.Count; i++) {
                var nf = response.userNotifications[i];
                nf.FixCommand = new MpCommand(() => {
                    MpPlatformWrapper.Services.NativeMessageBox.ShowOkCancelMessageBoxAsync("test", "Fix me");
                });

                var result = await MpNotificationBuilder.ShowNotificationAsync(nf);
                if (result == MpNotificationDialogResultType.Ignore) {
                    continue;
                }
                if(result == MpNotificationDialogResultType.Retry) {
                    // should be after fix cycle
                    // user changed settings or whatever and request is called again
                    T retry_response = await retryFunc.Invoke();
                    
                    response = await HandlePluginNotifcationsAsync(request, retry_response, retryFunc, i);
                }
            }
            return response as T;
        }

        private static async Task<MpPluginTransactionBase> HandleErrorAsync(
            Exception ex, 
            MpPluginFormat pluginFormat, 
            MpPluginTransactionBase at, MpCopyItem sourceCopyItem, object sourceHandler, bool suppressWrite = false) {
            MpConsole.WriteTraceLine(ex);
            at.TransactionErrorMessage = ex.ToString();
            await MpPluginLogger.LogTransactionAsync(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite)
                                    .TimeoutAfter(TimeSpan.FromMilliseconds(_PROCESS_TIMEOUT_MS));

            var userAction = await MpNotificationBuilder.ShowNotificationAsync(
                notificationType: MpNotificationType.InvalidRequest,
                body: ex.Message,
                maxShowTimeMs: 5000);
            
            if (at.Response == null) {
                at.Response = new MpPluginResponseFormatBase();
            }
            if (userAction == MpNotificationDialogResultType.Retry) {
                
                at.Response.retryMessage = "Retry";
            } else {
                at.Response.errorMessage = "Transaction Error";
            }

            return at;
        }

        #endregion
    }
}
