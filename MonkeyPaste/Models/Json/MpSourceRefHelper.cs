using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common;
using System.Linq;
using System.Text;

namespace MonkeyPaste {

    public interface MpISourceRef {
        int SourceObjId { get; }
        MpCopyItemSourceType SourceType { get; }
    }

    
    public class MpSourceRefHelper : MpISourceRef {
        public const string INTERNAL_SOURCE_DOMAIN = "https://localhost";
        public static MpSourceRefHelper ParseFromInternalUrl(string url) {
            if(!IsInternalSourceRef(url)) {
                return null;
            }
            var sr = new MpSourceRefHelper();
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

        public static string ToUrl(MpISourceRef sr) {
            return sr.ToString();
        }

        public static byte[] ToUrlAsciiBytes(MpISourceRef sr) {
            // for clipboard storage as CefAsciiBytes format
            return ToUrl(sr).ToBytesFromString(Encoding.ASCII);
        }

        public string SourcePublicHandle { get; set; }
        public int SourceObjId { get; set; } = 0;
        public MpCopyItemSourceType SourceType { get; set; } = MpCopyItemSourceType.None;

        public override string ToString() {
            return $"{INTERNAL_SOURCE_DOMAIN}?type={SourceType.ToString()}&id={SourceObjId}";
        }
    }
}
