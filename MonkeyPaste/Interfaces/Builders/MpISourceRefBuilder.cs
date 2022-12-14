using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISourceRefBuilder {
        Task<MpISourceRef> FetchOrCreateSourceAsync(string uri);
        string ConvertToRefUrl(MpISourceRef sr);
        byte[] ToUrlAsciiBytes(MpISourceRef sr);
    }
}
