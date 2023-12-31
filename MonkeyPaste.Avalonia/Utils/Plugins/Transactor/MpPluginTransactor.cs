using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpPluginTransactor {
        #region Constants
        // TODO In Release Mode change these to smaller values
        private const int _ANALYZE_TIMEOUT_MS = 1000000;
        private const int _PROCESS_TIMEOUT_MS = 500000;
        #endregion

        #region Public Methods

        public static async Task<MpPluginTransactionBase> PerformTransactionAsync(
            MpPluginWrapper plugin,
            Dictionary<object, string> paramValues,
            MpCopyItem sourceCopyItem,
            object sourceHandler,
            bool is_retry,
            bool suppressWrite = false) {

            // This method:
            // 0. checks data between each step and returns error info or retry message to calling analyzer if there's a problem
            // 1. prepares plugin parameters using sourceCopyItem and given param values
            // 2. sends request to the plugin
            // 3. checks response from the plugin to ensure it was successful
            // 4. logs the transaction (needed for restful api call auditing or  et to be implemented more detailed source information) 
            // 5. Converts the response to new content and/or updates source content (since from another, potentially 3rd party module)
            // 6. Returns new or updated content

            MpAnalyzerTransaction trans = new MpAnalyzerTransaction() {
                RequestContent = sourceCopyItem,
                RequestTime = DateTime.Now,
            };

            // CREATE REQUEST
            try {
                trans.Request = await MpPluginRequestBuilder.BuildRequestAsync(
                                    plugin.analyzer.parameters,
                                    paramValues,
                                    sourceCopyItem,
                                    true);
            }
            catch (Exception ex) {
                return await HandleErrorAsync(ex, plugin, trans, sourceCopyItem, sourceHandler, suppressWrite);
            }

            // TODO (for http) phone home w/ request half of transaction and await return

            // GET RESPONSE
            try {
                trans.Response = await IssueRequestAsync(plugin, trans.Request as MpAnalyzerPluginRequestFormat);
                trans.ResponseTime = DateTime.Now;
            }
            catch (Exception ex) {
                return await HandleErrorAsync(ex, plugin, trans, sourceCopyItem, sourceHandler, suppressWrite);
            }

            // LOG TRANSACTION (create record of params, ref to plugin source(local/remote))

            //int ci_trans_id = await MpPluginLogger.LogTransactionAsync(pluginFormat, at, sourceCopyItem, sourceHandler, suppressWrite);

            // TODO (for http) phone home w/ response to finish transaction record and await return

            // PROCESS RESPONSE
            try {
                trans.ResponseContent =
                    await MpPluginResponseConverter.ConvertAsync(plugin, trans, paramValues, sourceCopyItem, sourceHandler, suppressWrite);
                return trans;
            }
            catch (Exception ex) {
                var error_response = await HandleErrorAsync(ex, plugin, trans, sourceCopyItem, sourceHandler, suppressWrite);
                return error_response;
            }
        }
        private static async Task<MpAnalyzerPluginResponseFormat> IssueRequestAsync(MpPluginWrapper plugin, MpAnalyzerPluginRequestFormat req) {
            string method_name = nameof(MpIAnalyzeComponent.Analyze);
            string on_type = typeof(MpIAnalyzeComponent).FullName;
            var resp = await plugin.IssueRequestAsync(method_name, on_type, req) as MpAnalyzerPluginResponseFormat;
            return resp;
        }

        public static async Task<T> ValidatePluginResponseAsync<T>(
            string pluginLabel,
            MpPluginParameterRequestFormat request,
            MpPluginResponseFormatBase response,
            Func<Task<T>> retryFunc) where T : MpPluginResponseFormatBase {
            if (response == null) {
                //MpConsole.WriteTraceLine($"Clipboard Reader Plugin error, no response from {handler.ToString()}, (ignoring its assigned formats) ");
                MpConsole.WriteLine($"Plugin response null");
                return null;
            }
            if (!string.IsNullOrWhiteSpace(response.errorMessage)) {
                MpConsole.WriteTraceLine($"Plugin error for '{pluginLabel}': {response.errorMessage}");

                Mp.Services.NotificationBuilder.ShowMessageAsync(
                    msgType: MpNotificationType.PluginResponseError,
                    title: string.Format(UiStrings.PluginErrNtfTitle, pluginLabel),
                    body: response.errorMessage).FireAndForgetSafeAsync();
                return null;
            }
            if (response.otherMessage != null) {
                MpConsole.WriteLine($"Plugin message for '{pluginLabel}': {response.otherMessage}");

                Mp.Services.NotificationBuilder.ShowMessageAsync(
                    msgType: MpNotificationType.PluginResponseOther,
                    title: pluginLabel,
                    body: response.otherMessage).FireAndForgetSafeAsync();
            }

            response = await HandlePluginNotifcationsAsync<T>(request, response, retryFunc);

            return response as T;
        }

        #endregion

        #region Private Methods
        private static async Task<MpPluginResponseFormatBase> HandlePluginNotifcationsAsync<T>(
            MpPluginParameterRequestFormat request,
            MpPluginResponseFormatBase response,
            Func<Task<T>> retryFunc,
            int cur_idx = 0) where T : MpPluginResponseFormatBase {
            if (response.userNotifications == null || response.userNotifications.Count == 0) {
                return response;
            }

            for (int i = cur_idx; i < response.userNotifications.Count; i++) {
                var nf = response.userNotifications[i];

                var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(nf);
                if (result == MpNotificationDialogResultType.Ignore) {
                    continue;
                }
                if (result == MpNotificationDialogResultType.Retry) {
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
            MpPluginWrapper pluginFormat,
            MpPluginTransactionBase trans,
            MpCopyItem sourceCopyItem, object sourceHandler, bool suppressWrite = false) {
            MpConsole.WriteTraceLine(ex);

            var pp = sourceHandler as MpPluginPreset;

            Mp.Services.TransactionBuilder.ReportTransactionAsync(
                        copyItemId: sourceCopyItem.Id,
                        reqType: MpJsonMessageFormatType.ParameterRequest,
                        req: trans.Request.SerializeJsonObject(),
                        respType: MpJsonMessageFormatType.Error,
                        resp: ex.Message,
                        ref_uris: new[] {
                            Mp.Services.SourceRefTools.ConvertToInternalUrl(pp)//, trans.Request.SerializeJsonObjectToBase64())
                        },
                        transType: MpTransactionType.Error).FireAndForgetSafeAsync();

            var userAction = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                notificationType: MpNotificationType.InvalidRequest,
                body: ex.Message,
                maxShowTimeMs: 5000);

            if (trans.Response == null) {
                trans.Response = new MpAnalyzerPluginResponseFormat();
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
