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

            if(pluginComponent is MpIAnalyzeAsyncComponent || pluginComponent is MpIAnalyzeComponent) {
                MpAnalyzerTransaction trans = new MpAnalyzerTransaction() {
                    RequestTime = DateTime.Now,
                };

                // CREATE REQUEST
                try {
                    trans.Request = await MpPluginRequestBuilder.BuildRequestAsync(
                                        pluginFormat.analyzer.parameters,
                                        paramValues,
                                        sourceCopyItem);
                } catch(Exception ex) {
                    return await HandleErrorAsync(ex, pluginFormat, trans, sourceCopyItem, sourceHandler, suppressWrite);
                }

                // FIND CONTENT
                MpParameterFormat contentParam = pluginFormat.analyzer.parameters
                    .FirstOrDefault(x => x.unitType == MpParameterValueUnitType.PlainTextContentQuery);
                                
                trans.RequestContent = contentParam == null ? 
                    null :
                    (trans.Request as MpAnalyzerPluginRequestFormat).items
                    .FirstOrDefault(x => x.paramId.Equals(contentParam.paramId)).value;

                // TODO (for http) phone home w/ request half of transaction and await return

                // GET RESPONSE
                try {
                    if(pluginComponent is MpIAnalyzeAsyncComponent analyzeAsyncComponent) {
                        trans.Response = await analyzeAsyncComponent.AnalyzeAsync(trans.Request as MpAnalyzerPluginRequestFormat);
                    } else if(pluginComponent is MpIAnalyzeComponent analyzeComponent) {
                        trans.Response = analyzeComponent.Analyze(trans.Request as MpAnalyzerPluginRequestFormat);
                    }
                    trans.ResponseTime = DateTime.Now;
                }
                catch (Exception ex) {
                    return await HandleErrorAsync(ex, pluginFormat, trans, sourceCopyItem, sourceHandler, suppressWrite);
                }

                // LOG TRANSACTION (create record of params, ref to plugin source(local/remote))

                //int ci_trans_id = await MpPluginLogger.LogTransactionAsync(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);

                // TODO (for http) phone home w/ response to finish transaction record and await return

                // PROCESS RESPONSE
                try {
                    trans.ResponseContent = await MpPluginResponseConverter.ConvertAsync(pluginFormat, trans, sourceCopyItem, sourceHandler, suppressWrite);
                    return trans;
                }
                catch (Exception ex) {
                    var error_response = await HandleErrorAsync(ex, pluginFormat, trans, sourceCopyItem, sourceHandler, suppressWrite);
                    return error_response;
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
                    MpPlatform.Services.NativeMessageBox.ShowOkCancelMessageBoxAsync("test", "Fix me");
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
            MpPluginTransactionBase trans, 
            MpCopyItem sourceCopyItem, object sourceHandler, bool suppressWrite = false) {
            MpConsole.WriteTraceLine(ex);

            var pp = sourceHandler as MpPluginPreset;

            MpPlatform.Services.TransactionBuilder.PerformTransactionAsync(
                        copyItemId: sourceCopyItem.Id,
                        reqType: MpJsonMessageFormatType.ParameterRequest,
                        req: trans.Request.SerializeJsonObject(),
                        respType: MpJsonMessageFormatType.Error,
                        resp: ex.Message,
                        ref_urls: new[] { 
                            MpPlatform.Services.SourceRefBuilder.ConvertToRefUrl(pp, trans.Request.SerializeJsonObjectToBase64()) 
                        },
                        label: "Error").FireAndForgetSafeAsync();

            var userAction = await MpNotificationBuilder.ShowNotificationAsync(
                notificationType: MpNotificationType.InvalidRequest,
                body: ex.Message,
                maxShowTimeMs: 5000);
            
            if (trans.Response == null) {
                trans.Response = new MpPluginResponseFormatBase();
            }
            if (userAction == MpNotificationDialogResultType.Retry) {
                
                trans.Response.retryMessage = "Retry";
            } else {
                trans.Response.errorMessage = "Error";
            }

            return trans;
        }

        #endregion
    }
}
