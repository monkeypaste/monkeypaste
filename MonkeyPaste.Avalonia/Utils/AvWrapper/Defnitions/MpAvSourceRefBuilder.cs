using Avalonia.Controls;
using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvSourceRefBuilder : MpISourceRefBuilder {
        public const string INTERNAL_SOURCE_DOMAIN = "https://localhost";

        public async Task<MpISourceRef> FetchOrCreateSourceAsync(string sourceUrl) {
            if(!Uri.IsWellFormedUriString(sourceUrl, UriKind.Absolute)) {
                // whats the url?
                Debugger.Break();
                return null;
            }
            if(Uri.TryCreate(sourceUrl,UriKind.Absolute, out Uri uri) && uri.IsFile) {
                // this SHOULD be an executable file path but not checking 
                // to stay portable
                var app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(
                    new MpPortableProcessInfo() { 
                        ProcessPath = uri.LocalPath });
                return app;
            }
            if(!sourceUrl.StartsWith(INTERNAL_SOURCE_DOMAIN.ToLower())) {
                // should appId be a default arg to this method? 
                // should this check last process for appId instead?
                // does THIS app need to be ref'd instead of last?
                Debugger.Break();
                // add or fetch external url
                var url = await MpPlatformWrapper.Services.UrlBuilder.CreateAsync(sourceUrl);
                return url;
            }
            if(!sourceUrl.Contains("?")) {
                // internal url should have params, is it malformed?
                Debugger.Break();
                return null;
            }
            // convert internal source ref to ci,app or url
            string ref_param_str = sourceUrl.SplitNoEmpty("?")[1];
            if(string.IsNullOrWhiteSpace(ref_param_str)) {
                // internal url should have params, is it malformed?
                Debugger.Break();
                return null;
            }

            var param_lookup =
                ref_param_str
                .SplitNoEmpty("&")
                .ToDictionary(
                    x => x.SplitNoEmpty("=")[0].ToLower(),
                    x => x.SplitNoEmpty("=")[1].ToLower());

            if(!param_lookup.ContainsKey("type")) {
                // whats the parameters?
                Debugger.Break();
                return null;
            }
            MpTransactionSourceType source_type = param_lookup["type"].ToEnum<MpTransactionSourceType>();
            int source_id = param_lookup.ContainsKey("id") ? int.Parse(param_lookup["id"]) : 0;
            string ci_public_handle = param_lookup.ContainsKey("handle") ? param_lookup["handle"] : null;

            if(source_id < 1 && string.IsNullOrWhiteSpace(ci_public_handle)) {
                if(source_type == MpTransactionSourceType.UserDevice) {
                    // editor->read-only, use current userDeviceId
                    source_id = MpDefaultDataModelTools.ThisUserDeviceId;
                } else {
                    // missing locator
                    Debugger.Break();
                    return null;
                }                
            }
            if(source_type == MpTransactionSourceType.CopyItem &&
                !string.IsNullOrWhiteSpace(ci_public_handle)) {
                // resolve publicHandle to ciid

                var ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle.ToLower() == ci_public_handle.ToLower());
                if (ctvm == null) {
                    // how did it get the ref?
                    Debugger.Break();
                    return null;
                }
                source_id = ctvm.CopyItemId;
            }

            var result = await MpDataModelProvider.GetSourceRefBySourceypeAndSourceIdAsync(source_type, source_id);
            return result;
        }

        public async Task<List<MpTransactionSource>> AddTransactionSourcesAsync(
            int copyItemTransactionId, 
            IEnumerable<Tuple<MpISourceRef,string>> transactionSources) {
            if(transactionSources == null) {
                return null;
            }
            var sources = new List<MpTransactionSource>();
            foreach(var source_ref in transactionSources) {

                var cis = await MpTransactionSource.CreateAsync(
                    transactionId: copyItemTransactionId,
                    sourceObjId: source_ref.Item1.SourceObjId,
                    sourceType: source_ref.Item1.SourceType,
                    sourceArgs: source_ref.Item2);
                sources.Add(cis);
            }
            return sources;
        }

        public async Task<IEnumerable<MpISourceRef>> GatherSourceRefsAsync(MpPortableDataObject mpdo, bool forceExtSources = false) {
            
            List<MpISourceRef> refs = new List<MpISourceRef>();
            if (mpdo.TryGetData(MpPortableDataFormats.CefAsciiUrl, out byte[] urlBytes) &&
                urlBytes.ToDecodedString() is string urlRef) {
                MpISourceRef sr = await MpPlatformWrapper.Services.SourceRefBuilder.FetchOrCreateSourceAsync(urlRef);
                if (sr != null) {
                    // occurs on sub-selection drop onto pintray or tag
                    refs.Add(sr);
                }
            }
            IEnumerable<string> uri_strings = null;
            if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out IEnumerable<string> uril)) {
                uri_strings = uril;
            } else if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out string uril_str)) {
                if(uril_str.StartsWith("[") && MpJsonObject.DeserializeObject<List<string>>(uril_str) is List<string> urilist) {
                    uril_str = string.Join("\r\n", urilist);
                }
                uri_strings = uril_str.SplitNoEmpty("\r\n");
            }
            if(uri_strings != null) {
                var list_refs = await Task.WhenAll(
                    uri_strings.Select(x => MpPlatformWrapper.Services.SourceRefBuilder.FetchOrCreateSourceAsync(x)));
                refs.AddRange(list_refs);
            }

            if (refs.Count == 0 || forceExtSources) {
                // external ole create
                var ext_refs = await GatherExternalSourceRefsAsync(mpdo);
                if (ext_refs != null && ext_refs.Count() > 0) {
                    refs.AddRange(ext_refs);
                }
            }
            if (refs.Count == 0) {
                // fallback
                var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
                refs.Add(this_app);
            }
            return refs;
        }

        private async Task<IEnumerable<MpISourceRef>> GatherExternalSourceRefsAsync(MpPortableDataObject mpdo) {
            var last_pinfo = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;

            //if(OperatingSystem.IsLinux()) {
            //    // this maybe temporary but linux not following process watching convention because its SLOW
            //    string exe_path = MpX11ShellHelpers.GetExeWithArgsToExePath(MpPlatformWrapper.Services.ProcessWatcher.LastProcessPath);
            //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(exe_path);
            //} else {
            //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(MpPlatformWrapper.Services.ProcessWatcher.LastHandle);
            //}
            if (last_pinfo == null) {
                Debugger.Break();
                return null;
            }
            var app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(last_pinfo);

            MpUrl url = null;
            string source_url = MpAvHtmlClipboardData.FindSourceUrl(mpdo);
            if (!string.IsNullOrWhiteSpace(source_url)) {
                url = await MpPlatformWrapper.Services.UrlBuilder.CreateAsync(
                    url: source_url,
                    appId: app == null ? 0 : app.Id);
            }

            List<MpISourceRef> ext_refs = new List<MpISourceRef>();
            if (url != null) {
                // NOTE url added first
                ext_refs.Add(url);
            }
            if (app != null) {
                ext_refs.Add(app);
            }
            return ext_refs;
        }

        public bool IsAnySourceRejected(IEnumerable<MpISourceRef> refs) {
            foreach (var source_ref in refs) {
                if (source_ref is MpUrl url &&
                    (url.IsDomainRejected || url.IsDomainRejected)) {
                    MpConsole.WriteLine($"Rejected url detected. Url: '{url}'");
                    return true;
                } else if (source_ref is MpApp app &&
                    app.IsAppRejected) {
                    MpConsole.WriteLine($"Rejected app detected. App: '{app}'");
                    return true;
                }
            }
            return false;
        }

        public string ConvertToRefUrl(MpISourceRef sr, string base64Args = null) {
            string queryParam = base64Args == null ? string.Empty : $"&args={base64Args}";
            return $"{INTERNAL_SOURCE_DOMAIN}?type={sr.SourceType.ToString()}&id={sr.SourceObjId}{queryParam}";
        }

        public string ParseRefArgs(string ref_url) {
            if(string.IsNullOrEmpty(ref_url)) {
                return null;
            }
            if(ref_url.SplitNoEmpty("&").FirstOrDefault(x=>x.StartsWith("args")) is string queryArg
                && !string.IsNullOrEmpty(queryArg) &&
                string.Join("=",queryArg.Split("=").Skip(1)) is string base64ArgStr &&
                !string.IsNullOrEmpty(base64ArgStr)) {
                return base64ArgStr.ToStringFromBase64();
            }
            return null;
        }
        public byte[] ToUrlAsciiBytes(MpISourceRef sr) {
            // for clipboard storage as CefAsciiBytes format
            return ConvertToRefUrl(sr).ToBytesFromString(Encoding.ASCII);
        }


    }
}
