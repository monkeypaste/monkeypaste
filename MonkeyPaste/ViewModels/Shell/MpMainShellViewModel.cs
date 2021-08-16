using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpMainShellViewModel : MpViewModelBase {
        #region Properties
        public MpTagTileCollectionViewModel TagCollectionViewModel { get; set; }
        #endregion

        #region Public Methods
        public MpMainShellViewModel() {
            Task.Run(async () => {
                await MpDb.Instance.Init();

                MpTempFileManager.Instance.Init();
                //MpSocketClient.StartClient("192.168.43.209");

               // MpSyncManager.Instance.Init(MpDb.Instance);

                TagCollectionViewModel = new MpTagTileCollectionViewModel();
            });
        }

        public void OnShellDisapperaing(object sender, EventArgs e) {

        }
        #endregion

        #region Commands
        public ICommand SyncCommand => new Command<object>(async (args) => {


            MpDbLogTracker.PrintDbLog();
            //var ms = Application.Current.MainPage as MpMainShell;
            //var curDbBytes = MpDb.Instance.GetDbFileBytes();
            //ms.StorageService.CreateFile(@"mp_clone", curDbBytes, @".db");
        });

        public static bool SendFilesByFTP(
            string password, 
            string userName, 
            string originFile, 
            string destinationFile) {
            try {

                //
                Uri severUri = new Uri(destinationFile);

                //
                if (severUri.Scheme != Uri.UriSchemeFtp)
                    return false;

                //
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(severUri);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.UsePassive = false;//true;
                request.KeepAlive = false;
                request.Timeout = System.Threading.Timeout.Infinite;
                request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
                request.Credentials = new NetworkCredential(userName, password);

                StreamReader sourceStream = new StreamReader(originFile);
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();
                request.ContentLength = fileContents.Length;

                //
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();
                requestStream.Dispose();

                //
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                //
                response.Close();
                response.Dispose();

                //
                return true;
            }
            catch (Exception e) {
                MpConsole.WriteTraceLine("SendFilesByFTP", e);
                return (false);
            }
        }
        #endregion
    }
}
