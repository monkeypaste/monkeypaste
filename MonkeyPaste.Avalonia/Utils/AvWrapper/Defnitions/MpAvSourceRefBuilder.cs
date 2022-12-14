using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common;
using System;
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
            MpCopyItemSourceType source_type = param_lookup["type"].ToEnum<MpCopyItemSourceType>();
            int source_id = param_lookup.ContainsKey("id") ? int.Parse(param_lookup["id"]) : 0;
            string ci_public_handle = param_lookup.ContainsKey("handle") ? param_lookup["handle"] : null;

            if(source_id < 1 && string.IsNullOrWhiteSpace(ci_public_handle)) {
                // missing locator
                Debugger.Break();
                return null;
            }
            switch(source_type) {
                case MpCopyItemSourceType.App:
                    var app = await MpDataModelProvider.GetItemAsync<MpApp>(source_id);
                    return app;
                case MpCopyItemSourceType.Url:
                    var url = await MpDataModelProvider.GetItemAsync<MpUrl>(source_id);
                    return url;
                case MpCopyItemSourceType.CopyItem:
                    if(string.IsNullOrEmpty(ci_public_handle)) {
                        var ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(source_id);
                        return ci;
                    }
                    var ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle.ToLower() == ci_public_handle.ToLower());
                    if(ctvm == null) {
                        // how did it get the ref?
                        Debugger.Break();
                        return null;
                    }
                    return ctvm.CopyItem;
                default:
                    throw new UnauthorizedAccessException($"Unknown source type: '{source_type}'");
            }
        }

        public string ConvertToRefUrl(MpISourceRef sr) {
            return $"{INTERNAL_SOURCE_DOMAIN}?type={sr.SourceType.ToString()}&id={sr.SourceObjId}";
        }

        public byte[] ToUrlAsciiBytes(MpISourceRef sr) {
            // for clipboard storage as CefAsciiBytes format
            return ConvertToRefUrl(sr).ToBytesFromString(Encoding.ASCII);
        }


    }
}
