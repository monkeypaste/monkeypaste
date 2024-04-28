using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpFtpTools {
        public static async Task<FtpStatusCode> FtpFileUploadAsync(string ftpUrl, string userName, string password, string filePath, bool overwrite = false) {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(userName, password);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (Stream requestStream = request.GetRequestStream()) {
                await fileStream.CopyToAsync(requestStream);
            }

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync()) {
                return response.StatusCode;
            }
        }
        public static FtpStatusCode FtpFileUpload(string ftpUrl, string userName, string password, string filePath, bool overwrite = false) {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(userName, password);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                using (Stream requestStream = request.GetRequestStream()) {
                    fileStream.CopyTo(requestStream);
                }
            }
            

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) {
                return response.StatusCode;
            }
        }
    }
}
