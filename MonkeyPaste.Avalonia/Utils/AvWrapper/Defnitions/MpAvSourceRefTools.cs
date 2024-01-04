using Avalonia.Input;
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
        public bool IsInternalUrl(string url) {
            if (string.IsNullOrEmpty(url)) {
                return false;
            }
            return url.ToLower().StartsWith(INTERNAL_SOURCE_DOMAIN);
        }
        public bool IsExternalSource(MpISourceRef sr) {
            if (sr is MpApp app) {
                return app.Id != MpDefaultDataModelTools.ThisAppId;
            }
            if (sr is MpUrl url) {
                return !url.UrlPath.StartsWith(MpAvSourceRefTools.INTERNAL_SOURCE_DOMAIN);
            }
            return false;
        }
        public bool IsSourceRejected(MpISourceRef sr) {
            if (sr is MpApp app) {
                return app.IsAppRejected;
            }
            if (sr is MpUrl url) {
                return url.IsDomainRejected || url.IsUrlRejected;
            }
            return false;
        }
        public async Task<MpISourceRef> FetchOrCreateSourceAsync(string sourceUrl, object arg = null) {
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
                MpDebug.Break();
                return null;
            }

            var ref_tuple = ParseUriForSourceRef(sourceUrl);

            if (ref_tuple == null ||
                ref_tuple.Item1 == MpTransactionSourceType.None ||
                ref_tuple.Item2 < 1) {
                // add or fetch external url
                MpDebug.Assert(arg is int && (int)arg > 0, $"New Url '{sourceUrl}' needs app id", true);
                if (arg is not int appId) {
                    return null;
                }
                var url = await Mp.Services.UrlBuilder.CreateAsync(sourceUrl, appId);
                return url;
            }

            var result = await MpDataModelProvider.GetSourceRefBySourceTypeAndSourceIdAsync(ref_tuple.Item1, ref_tuple.Item2);
            return result;
        }

        public async Task<MpApp> FetchOrCreateAppRefAsync(MpPortableProcessInfo ppi) {
            MpApp app_ref = null;
            if (MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(ppi) is MpAvAppViewModel avm) {
                app_ref = avm.App;
            } else {
                app_ref = await Mp.Services.AppBuilder.CreateAsync(ppi);
                if (app_ref == null) {
                    MpDebug.Break($"Error creating app from proess info '{ppi}'");
                    return null;
                }
            }
            return app_ref;
        }
        public async Task<string> FetchOrCreateAppRefUrlAsync(MpPortableProcessInfo ppi) {
            var result = await FetchOrCreateAppRefAsync(ppi);
            if (result == null) {
                return null;
            }
            return ConvertToInternalUrl(result);
        }

        public Tuple<MpTransactionSourceType, int> ParseUriForSourceRef(string uri) {
            Tuple<MpTransactionSourceType, int> no_match_result = new Tuple<MpTransactionSourceType, int>(MpTransactionSourceType.None, 0);
            if (!uri.StartsWith(INTERNAL_SOURCE_DOMAIN.ToLower())) {
                return no_match_result;
            }
            if (!uri.Contains("?")) {
                // internal url should have params, is it malformed?
                MpDebug.Break();
                return no_match_result;
            }
            // convert internal source ref to ci,app or url
            string ref_param_str = uri.SplitNoEmpty("?")[1];
            if (string.IsNullOrWhiteSpace(ref_param_str)) {
                // internal url should have params, is it malformed?
                MpDebug.Break();
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
                MpDebug.Break();
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
            IEnumerable<MpISourceRef> transactionSources) {
            if (transactionSources == null) {
                return null;
            }
            var sources = new List<MpTransactionSource>();
            foreach (var source_ref in transactionSources) {
                if (source_ref == null) {
                    continue;
                }
                var cis = await MpTransactionSource.CreateAsync(
                    transactionId: copyItemTransactionId,
                    sourceObjId: source_ref.SourceObjId,
                    sourceType: source_ref.SourceType
                    //sourceArgs: source_ref.Item2
                    );
                sources.Add(cis);
            }
            return sources;
        }

        public async Task<IEnumerable<MpISourceRef>> GatherSourceRefsAsync(object mpOrAvDataObj) {
            MpAvDataObject avdo = null;
            if (mpOrAvDataObj is IDataObject ido) {
                avdo = ido.ToPlatformDataObject();
            }
            if (avdo == null) {
                avdo = new MpAvDataObject();
            }

            var ext_refs = await GatherExternalSourceRefsAsync(avdo);
            List<MpISourceRef> refs =
                ext_refs == null ?
                    new List<MpISourceRef>() :
                    ext_refs.ToList();

            if (avdo.TryGetUriList(out List<string> uri_strings)) {
                var list_refs = await Task.WhenAll(uri_strings.Select(x => FetchOrCreateSourceAsync(x)));
                refs.AddRange(list_refs.Where(x => x != null));
            }

            if (refs.Count == 0) {
                // fallback
                var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
                refs.Add(this_app);
            }
            // only return non-null, unique refs
            return
                refs
                .Where(x => x != null)
                .DistinctBy(x => new { x.SourceType, x.SourceObjId })
                .OrderByDescending(x => x.Priority);
        }

        private async Task<IEnumerable<MpISourceRef>> GatherExternalSourceRefsAsync(MpAvDataObject avdo) {
            if (avdo.TryGetData(MpPortableDataFormats.INTERNAL_CONTENT_ID_FORMAT, out object ciidObj) &&
                int.TryParse(ciidObj.ToString(), out int ciid)) {
                // when full/partial content ref present use refs sources INSTEAD of external
                var srl = await MpDataModelProvider.GetCopyItemSourceRefsByCopyItemIdAsync(ciid);
                return srl;
            }
            MpPortableProcessInfo last_pinfo = null;

            if (avdo.TryGetData<MpPortableProcessInfo>(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, out var pi)) {
                last_pinfo = pi;
            }
            if (last_pinfo == null &&
                avdo.TryGetData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, out string pi_str) &&
                !string.IsNullOrWhiteSpace(pi_str)) {
                last_pinfo = MpJsonExtensions.DeserializeObject<MpPortableProcessInfo>(pi_str);
            }


            if (last_pinfo == null) {
                // no direct external source available
                return null;
            }
            MpApp app = await FetchOrCreateAppRefAsync(last_pinfo);
            //if(OperatingSystem.IsLinux()) {
            //    // this maybe temporary but linux not following process watching convention because its SLOW
            //    string exe_path = MpX11ShellHelpers.GetExeWithArgsToExePath(MpPlatformWrapper.Services.ProcessWatcher.LastProcessPath);
            //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(exe_path);
            //} else {
            //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(MpPlatformWrapper.Services.ProcessWatcher.LastHandle);
            //}

            MpUrl url = null;

            string source_url = FindSourceUrl(avdo);
            if (!string.IsNullOrWhiteSpace(source_url) &&
                Uri.IsWellFormedUriString(source_url, UriKind.Absolute) &&
                app != null) {
                url = await FetchOrCreateSourceAsync(source_url, app.Id) as MpUrl;
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
        private string FindSourceUrl(MpAvDataObject avdo) {
            string cb_html_or_fragment = null;
            if (avdo.ContainsData(MpPortableDataFormats.LinuxSourceUrl) &&
                       avdo.GetData(MpPortableDataFormats.LinuxSourceUrl) is byte[] url_bytes &&
                       url_bytes.ToDecodedString(Encoding.ASCII, true) is string source_url_str) {
                // on linux html is not in fragment format like windows and firefox supports this format
                // but chrome doesn't
                return source_url_str;
            }
            if (avdo.TryGetData(MpPortableDataFormats.CefAsciiUrl, out byte[] urlBytes) &&
                urlBytes.ToDecodedString(Encoding.ASCII, true) is string urlRef) {
                return urlRef;
            }
            if (avdo.ContainsData(MpPortableDataFormats.Xhtml) &&
                        avdo.GetData(MpPortableDataFormats.Xhtml) is byte[] htmlBytes &&
                        htmlBytes.ToDecodedString() is string avhtmlStr) {

                // HTML
                cb_html_or_fragment = avhtmlStr;
            }
            if (string.IsNullOrEmpty(cb_html_or_fragment) &&
                avdo.ContainsData(MpPortableDataFormats.Html) &&
                avdo.GetData(MpPortableDataFormats.Html) is string cefhtmlStr) {
                cb_html_or_fragment = cefhtmlStr;
            }
            if (string.IsNullOrWhiteSpace(cb_html_or_fragment)) {
                return null;
            }
            return MpRichHtmlContentConverterResult.ParseHtmlFragmentForSourceUrl(cb_html_or_fragment);
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

        public string ConvertToInternalUrl(MpISourceRef sr) {
            if (sr == null) {
                return string.Empty;
            }
            return ConvertToInternalUrl(sr.SourceType, sr.SourceObjId);
        }
        public string ConvertToInternalUrl(MpTransactionSourceType sourceType, int sourceId) {

            return $"{INTERNAL_SOURCE_DOMAIN}?type={sourceType.ToString()}&id={sourceId}";
        }
        public string ConvertToAbsolutePath(MpISourceRef sr) {
            if (sr is MpApp app) {
                return app.AppPath;
            }
            if (sr is MpUrl url) {
                return url.UrlPath;
            }
            return ConvertToInternalUrl(sr);
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
            return ConvertToInternalUrl(sr).ToBytesFromString(Encoding.ASCII);
        }


    }
}
