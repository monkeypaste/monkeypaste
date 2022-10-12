using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpPluginLogger {
        public static async Task<int> LogTransactionAsync(
            MpPluginFormat pluginFormat,
            MpPluginTransactionBase trans, 
            MpCopyItem sourceContent, 
            object sourceHandler,
            bool suppressWrite = false) {
            if(trans is MpAnalyzerTransaction at) {
                int sourceId = await LogAnalyzerTransactionAsync(
                    pluginFormat, at, 
                    sourceContent, 
                    sourceHandler as MpPluginPreset, 
                    suppressWrite);
                return sourceId;
            }
            return 0;
        }

        private static async Task<int> LogAnalyzerTransactionAsync(
            MpPluginFormat pluginFormat,
            MpAnalyzerTransaction trans,
            MpCopyItem sourceContent,
            MpPluginPreset preset,
            bool suppressWrite = false) {
            if (trans.Response == null) {
                trans.Response = new MpPluginResponseFormatBase();
            }
            int transAppId = MpPrefViewModel.Instance.ThisAppSource.AppId;
            int transUrlId = MpPrefViewModel.Instance.ThisAppSource.UrlId;

            if (trans.Response is MpPluginResponseFormatBase prf) {                
                MpCopyItemTransactionType transType = MpCopyItemTransactionType.None;
                //MpISourceTransaction transModel = null;
                int transId = 0;

                if (pluginFormat.analyzer.http != null) {
                    string urlPath;
                    if (string.IsNullOrEmpty(trans.Request.ToString())) {
                        urlPath = pluginFormat.analyzer.http.request.url.raw;
                    } else if (pluginFormat.Component is MpHttpPlugin httpPlugin) {
                        urlPath = httpPlugin.GetRequestUri(trans.Request.items);
                    } else {
                        throw new MpUserNotifiedException("Http Plugin Component does not exist");
                    }

                    var url = MpPlatformWrapper.Services.UrlBuilder.CreateAsync(urlPath, preset.Label);                    

                    var httpTrans = await MpHttpTransaction.Create(
                        presetId: preset.Id,
                        //url: urlPath,
                        //urlName: SelectedItem.FullName,
                        urlId: url.Id,
                        ip: MpNetworkHelpers.GetExternalIp4Address(),
                        timeSent: trans.RequestTime,
                        timeReceived: trans.ResponseTime,
                        bytesSent: trans.Request.ByteCount(),
                        bytesReceived: trans.Response == null ? 0 : trans.Response.ByteCount(),
                        errorMsg: trans.TransactionErrorMessage,
                        suppressWrite: suppressWrite);

                    transId = httpTrans.Id;
                    transType = MpCopyItemTransactionType.Http;
                    transUrlId = url.Id;
                } else {
                    var pf = MpPluginLoader.Plugins.FirstOrDefault(x => x.Value.guid == pluginFormat.guid);
                    if (!string.IsNullOrWhiteSpace(pf.Key)) {
                        string manifestPath = pf.Key;
                        string pluginDir = Path.GetDirectoryName(manifestPath);
                        string pluginName = Path.GetFileName(pluginDir);
                        string processPath = Path.Combine(pluginDir, pluginName + ".exe");
                        
                        if (pluginFormat.ioType.isCli) {
                            var pluginProcessInfo = new MpPortableProcessInfo() {
                                ProcessPath = processPath,
                                MainWindowTitle = preset.Label
                            };
                            var app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(pluginProcessInfo);

                            var cliTrans = await MpCliTransaction.Create(
                                presetId: preset.Id,
                                //cliPath: pf.Value.ComponentPath,
                                appId: app.Id,
                                //cliName: SelectedItem.FullName,
                                workingDirectory: pluginDir,
                                args: trans.Request.ToString(),
                                transDateTime: trans.RequestTime,
                                errorMsg: trans.TransactionErrorMessage,
                                suppressWrite: suppressWrite);

                            transType = MpCopyItemTransactionType.Cli;
                            transId = cliTrans.Id;
                            transAppId = app.Id;
                        } else if (pluginFormat.ioType.isDll) {                            
                            processPath = processPath.Replace(".exe", ".dll");

                            var dllTrans = await MpDllTransaction.Create(
                                presetId: preset.Id,
                                dllPath: processPath,
                                //dllName: SelectedItem.FullName,
                                args: trans.Request.ToString(),
                                transDateTime: trans.RequestTime,
                                errorMsg: trans.TransactionErrorMessage,
                                suppressWrite: suppressWrite);

                            transType = MpCopyItemTransactionType.Dll;
                            transId = dllTrans.Id;
                        } else {
                            throw new MpUserNotifiedException($"Uknown ioType for plugin defined in '{manifestPath}'");
                        }
                    }
                }

                if (transId <= 0) {
                    throw new Exception("Unknown error processing analyzer transaction");
                }

                if (string.IsNullOrEmpty(trans.TransactionErrorMessage)) {
                    var cit = await MpCopyItemTransaction.Create(
                    transType: transType,
                    transObjId: transId,
                    copyItemId: sourceContent.Id,
                    responseJson: JsonConvert.SerializeObject(trans.Response),
                    suppressWrite: suppressWrite);

                    var source = await MpSource.Create(
                        copyItemTransactionId: cit.Id,
                        appId: transAppId,
                        urlId: transUrlId,
                        suppressWrite: suppressWrite);
                    return source.Id;
                }
            }
            return 0;
        }
    }
}
