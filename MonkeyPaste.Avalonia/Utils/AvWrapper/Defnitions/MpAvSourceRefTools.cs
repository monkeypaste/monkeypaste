﻿using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvSourceRefTools : MpISourceRefTools {
        public const string INTERNAL_SOURCE_DOMAIN = "https://localhost";

        public async Task<MpISourceRef> FetchOrCreateSourceAsync(string sourceUrl) {
            if (Uri.IsWellFormedUriString(sourceUrl, UriKind.Absolute)) {
                if (Uri.TryCreate(sourceUrl, UriKind.Absolute, out Uri uri) && uri.IsFile) {
                    // this SHOULD be an executable file path but not checking 
                    // to stay portable
                    var app = await Mp.Services.AppBuilder.CreateAsync(
                        new MpPortableProcessInfo() {
                            ProcessPath = uri.LocalPath
                        });
                    return app;
                }
            } else if (sourceUrl.Length > MpUrlHelpers.MAX_DOT_NET_URL_LENGTH) {
                // allow since these are internal url's (will occur for large data esp. images)
            } else {

                // whats the url?
                Debugger.Break();
                return null;
            }

            var ref_tuple = ParseUriForSourceRef(sourceUrl);

            if (ref_tuple == null ||
                ref_tuple.Item1 == MpTransactionSourceType.None ||
                ref_tuple.Item2 < 1) {
                // add or fetch external url
                var url = await Mp.Services.UrlBuilder.CreateAsync(sourceUrl);
                return url;
            }

            var result = await MpDataModelProvider.GetSourceRefBySourceypeAndSourceIdAsync(ref_tuple.Item1, ref_tuple.Item2);
            return result;
        }


        public Tuple<MpTransactionSourceType, int> ParseUriForSourceRef(string uri) {
            Tuple<MpTransactionSourceType, int> no_match_result = new Tuple<MpTransactionSourceType, int>(MpTransactionSourceType.None, 0);
            if (!uri.StartsWith(INTERNAL_SOURCE_DOMAIN.ToLower())) {
                return no_match_result;
            }
            if (!uri.Contains("?")) {
                // internal url should have params, is it malformed?
                Debugger.Break();
                return no_match_result;
            }
            // convert internal source ref to ci,app or url
            string ref_param_str = uri.SplitNoEmpty("?")[1];
            if (string.IsNullOrWhiteSpace(ref_param_str)) {
                // internal url should have params, is it malformed?
                Debugger.Break();
                return no_match_result;
            }

            var param_lookup =
                ref_param_str
                .SplitNoEmpty("&")
                .ToDictionary(
                    x => x.SplitNoEmpty("=")[0].ToLower(),
                    x => x.SplitNoEmpty("=")[1].ToLower());

            if (!param_lookup.ContainsKey("type")) {
                // whats the parameters?
                Debugger.Break();
                return no_match_result;
            }
            MpTransactionSourceType source_type = param_lookup["type"].ToEnum<MpTransactionSourceType>();
            int source_id = param_lookup.ContainsKey("id") ? int.Parse(param_lookup["id"]) : 0;

            if (source_id == 0) {
                if (source_type == MpTransactionSourceType.CopyItem) {
                    string ci_public_handle = param_lookup.ContainsKey("handle") ? param_lookup["handle"] : null;

                    if (MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == ci_public_handle) is MpAvClipTileViewModel ctvm) {
                        // resolve public handle
                        source_id = ctvm.CopyItemId;
                    }
                } else if (source_type == MpTransactionSourceType.UserDevice) {
                    // editor->read-only, use current userDeviceId
                    source_id = MpDefaultDataModelTools.ThisUserDeviceId;
                } else {
                    return no_match_result;
                }
            }
            return new Tuple<MpTransactionSourceType, int>(source_type, source_id);
        }

        public async Task<List<MpTransactionSource>> AddTransactionSourcesAsync(
            int copyItemTransactionId,
            IEnumerable<Tuple<MpISourceRef, string>> transactionSources) {
            if (transactionSources == null) {
                return null;
            }
            var sources = new List<MpTransactionSource>();
            foreach (var source_ref in transactionSources) {

                var cis = await MpTransactionSource.CreateAsync(
                    transactionId: copyItemTransactionId,
                    sourceObjId: source_ref.Item1.SourceObjId,
                    sourceType: source_ref.Item1.SourceType,
                    sourceArgs: source_ref.Item2);
                sources.Add(cis);
            }
            return sources;
        }

        public async Task<IEnumerable<MpISourceRef>> GatherSourceRefsAsync(object mpOrAvDataObj, bool forceExtSources = false) {
            MpAvDataObject avdo = null;
            if (mpOrAvDataObj is IDataObject ido) {
                avdo = ido.ToPlatformDataObject();
            }
            if (avdo == null) {
                avdo = new MpAvDataObject();
            }

            List<MpISourceRef> refs = new List<MpISourceRef>();
            if (avdo.TryGetData(MpPortableDataFormats.CefAsciiUrl, out byte[] urlBytes) &&
                urlBytes.ToDecodedString(Encoding.ASCII, true) is string urlRef &&
                Uri.IsWellFormedUriString(urlRef, UriKind.Absolute)) {

                MpISourceRef sr = await Mp.Services.SourceRefTools.FetchOrCreateSourceAsync(urlRef);
                if (sr != null) {
                    // occurs on sub-selection drop onto pintray or tag
                    refs.Add(sr);
                }
            }
            if (avdo.TryGetUriList(out List<string> uri_strings)) {
                var list_refs = await Task.WhenAll(
                    uri_strings.Select(x => Mp.Services.SourceRefTools.FetchOrCreateSourceAsync(x)));
                refs.AddRange(list_refs);
            }

            if (refs.Count == 0 || forceExtSources) {
                // external ole create
                var ext_refs = await GatherExternalSourceRefsAsync(avdo);
                if (ext_refs != null && ext_refs.Count() > 0) {
                    // NOTE ensure ext is in front so if url is present it is written first
                    refs.InsertRange(0, ext_refs);
                }
            }
            if (refs.Count == 0) {
                // fallback
                var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
                refs.Add(this_app);
            }
            return refs;
        }

        private async Task<IEnumerable<MpISourceRef>> GatherExternalSourceRefsAsync(MpAvDataObject avdo) {
            var last_pinfo = Mp.Services.ProcessWatcher.LastProcessInfo;

            //if(OperatingSystem.IsLinux()) {
            //    // this maybe temporary but linux not following process watching convention because its SLOW
            //    string exe_path = MpX11ShellHelpers.GetExeWithArgsToExePath(MpPlatformWrapper.Services.ProcessWatcher.LastProcessPath);
            //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(exe_path);
            //} else {
            //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(MpPlatformWrapper.Services.ProcessWatcher.LastHandle);
            //}
            if (last_pinfo == null) {
                if (Mp.Services.PlatformInfo.IsDesktop) {

                    Debugger.Break();
                }
                return null;
            }
            var app = await Mp.Services.AppBuilder.CreateAsync(last_pinfo);

            MpUrl url = null;
            string source_url = MpAvHtmlClipboardData.FindSourceUrl(avdo);
            if (!string.IsNullOrWhiteSpace(source_url)) {
                url = await Mp.Services.UrlBuilder.CreateAsync(
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
                    (url.IsDomainRejected || url.IsUrlRejected)) {
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
            if (string.IsNullOrEmpty(ref_url)) {
                return null;
            }
            if (ref_url.SplitNoEmpty("&").FirstOrDefault(x => x.StartsWith("args")) is string queryArg
                && !string.IsNullOrEmpty(queryArg) &&
                string.Join("=", queryArg.Split("=").Skip(1)) is string base64ArgStr &&
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