using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common;
using System.Linq;

namespace MonkeyPaste {

    public interface MpISourceRef {
        int SourceObjId { get; }
        MpCopyItemSourceType SourceType { get; }
    }
    public class MpSourceRef : MpISourceRef {
        public const string INTERNAL_SOURCE_DOMAIN = "https://localhost";
        public static MpSourceRef ParseFromInternalUrl(string url) {
            if(!IsInternalSourceRef(url)) {
                return null;
            }
            var sr = new MpSourceRef();
            if(url.SplitNoEmpty("?") is string[] urlParts &&
                urlParts.Length > 1 &&
                urlParts[1].SplitNoEmpty("&") is string[] paramParts) {
                sr.SourceType = paramParts.FirstOrDefault(x => x.StartsWith("type=")).Replace("type=", string.Empty).ToEnum<MpCopyItemSourceType>();
                if(paramParts.Any(x=>x.StartsWith("id"))) {
                    sr.SourceObjId = int.Parse(paramParts.FirstOrDefault(x => x.StartsWith("id=")).Replace("id=", string.Empty));
                }
                if (paramParts.Any(x => x.StartsWith("handle"))) {
                    sr.SourcePublicHandle = paramParts.FirstOrDefault(x => x.StartsWith("handle=")).Replace("handle=", string.Empty);
                }
                return sr;
            }
            return null;
        }
        public static bool IsInternalSourceRef(string str) {
            return !string.IsNullOrEmpty(str) && str.StartsWith(INTERNAL_SOURCE_DOMAIN);
        }
        public static MpSourceRef ReplicateSource(MpISourceRef sr) {
            return new MpSourceRef() {
                SourceObjId = sr.SourceObjId,
                SourceType = sr.SourceType
            };
        }

        public string SourcePublicHandle { get; set; }
        public int SourceObjId { get; set; } = 0;
        public MpCopyItemSourceType SourceType { get; set; } = MpCopyItemSourceType.None;

        public string ToUrl() {
            return $"{INTERNAL_SOURCE_DOMAIN}?type={SourceType.ToString()}&id={SourceObjId}";
        }
    }

    public static class MpSourceRefExtensions {
        public static string ToSourceRefUrl(this MpISourceRef sr) {
            return MpSourceRef.ReplicateSource(sr).ToUrl();
        }
    }
}
