using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpPluginLogger {
        public static async Task<int> LogTransaction(
            MpPluginFormat pluginFormat,
            MpPluginTransaction trans, 
            MpCopyItem sourceContent, 
            object sourceHandler,
            bool suppressWrite = false) {
            if(trans is MpAnalyzerTransaction at) {
                int sourceId = await LogAnalyzerTransaction(
                    pluginFormat, at, 
                    sourceContent, 
                    sourceHandler as MpAnalyticItemPreset, 
                    suppressWrite);
                return sourceId;
            }
            return 0;
        }

        private static async Task<int> LogAnalyzerTransaction(
            MpPluginFormat pluginFormat,
            MpAnalyzerTransaction trans,
            MpCopyItem sourceContent,
            MpAnalyticItemPreset preset,
            bool suppressWrite = false) {
            if (trans.Response == null) {
                trans.Response = new MpPluginResponseFormat();
            }

            if (trans.Response is MpPluginResponseFormat prf) {                
                MpCopyItemTransactionType transType = MpCopyItemTransactionType.None;
                MpISourceTransaction transModel = null;
                if (pluginFormat.analyzer.http != null) {
                    string urlPath;
                    if (string.IsNullOrEmpty(trans.Request.ToString())) {
                        urlPath = pluginFormat.analyzer.http.request.url.raw;
                    } else if (pluginFormat.Component is MpHttpPlugin httpPlugin) {
                        urlPath = httpPlugin.GetRequestUri(trans.Request.ToString());
                    } else {
                        throw new MpUserNotifiedException("Http Plugin Component does not exist");
                    }
                    transType = MpCopyItemTransactionType.Http;

                    transModel = await MpHttpTransaction.Create(
                        presetId: preset.Id,
                        url: urlPath,
                        //urlName: SelectedItem.FullName,
                        ip: MpNetworkHelpers.GetExternalIp4Address(),
                        timeSent: trans.RequestTime,
                        timeReceived: trans.ResponseTime,
                        bytesSent: trans.Request.ByteCount(),
                        bytesReceived: trans.Response == null ? 0 : trans.Response.ByteCount(),
                        errorMsg: trans.TransactionErrorMessage,
                        suppressWrite: suppressWrite);

                } else {
                    var pf = MpPluginManager.Plugins.FirstOrDefault(x => x.Value.guid == pluginFormat.guid);
                    if (!string.IsNullOrWhiteSpace(pf.Key)) {
                        string manifestPath = pf.Key;
                        string pluginDir = Path.GetDirectoryName(manifestPath);
                        string pluginName = Path.GetFileName(pluginDir);
                        string processPath = Path.Combine(pluginDir, pluginName + ".exe");

                        if (pluginFormat.ioType.isCli) {
                            transType = MpCopyItemTransactionType.Cli;

                            transModel = await MpCliTransaction.Create(
                                presetId: preset.Id,
                                cliPath: pf.Value.ComponentPath,
                                //cliName: SelectedItem.FullName,
                                workingDirectory: pluginDir,
                                args: trans.Request.ToString(),
                                transDateTime: trans.RequestTime,
                                errorMsg: trans.TransactionErrorMessage,
                                suppressWrite: suppressWrite);
                        } else if (pluginFormat.ioType.isDll) {
                            transType = MpCopyItemTransactionType.Dll;
                            processPath = processPath.Replace(".exe", ".dll");

                            transModel = await MpDllTransaction.Create(
                                presetId: preset.Id,
                                dllPath: processPath,
                                //dllName: SelectedItem.FullName,
                                args: trans.Request.ToString(),
                                transDateTime: trans.RequestTime,
                                errorMsg: trans.TransactionErrorMessage,
                                suppressWrite: suppressWrite);
                        } else {
                            throw new MpUserNotifiedException($"Uknown ioType for plugin defined in '{manifestPath}'");
                        }
                    }
                }

                if (transModel == null) {
                    throw new Exception("Unknown error processing analyzer transaction");
                }

                if (string.IsNullOrEmpty(trans.TransactionErrorMessage)) {
                    var cit = await MpCopyItemTransaction.Create(
                    transType: transType,
                    transObjId: transModel.RootId,
                    copyItemId: sourceContent.Id,
                    responseJson: JsonConvert.SerializeObject(trans.Response),
                    suppressWrite: suppressWrite);

                    var source = await MpSource.Create(
                        copyItemTransactionId: cit.Id,
                        suppressWrite: suppressWrite);
                    return source.Id;
                }
            }
            return 0;
        }
    }
}
