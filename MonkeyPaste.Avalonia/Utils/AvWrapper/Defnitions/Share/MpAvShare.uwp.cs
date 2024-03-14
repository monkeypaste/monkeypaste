using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
#if WAP
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage; 
#endif

namespace MonkeyPaste.Avalonia {
    [ComImport, Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDataTransferManagerInterop {
        IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
        void ShowShareUIForWindow(IntPtr appWindow);
    }
    public partial class MpAvShare {
#if WAP

        private DataTransferManager ShowShareUi(TypedEventHandler<DataTransferManager, DataRequestedEventArgs> dataRequestedHandler) {
            IntPtr windowHandle = MpAvWindowManager.ActiveWindow.TryGetPlatformHandle().Handle;
            IDataTransferManagerInterop interop = DataTransferManager.As<IDataTransferManagerInterop>();

            var guid = Guid.Parse("a5caee9b-8708-49d1-8d36-67d25a8da00c");
            var iop = DataTransferManager.As<IDataTransferManagerInterop>();
            var dataTransferManager = DataTransferManager.FromAbi(iop.GetForWindow(windowHandle, guid));

            dataTransferManager.DataRequested += dataRequestedHandler;
            interop.ShowShareUIForWindow(windowHandle);
            return dataTransferManager;
        }

        Task PlatformRequestAsync(MpAvShareTextRequest request) {
            // from https://github.com/microsoft/microsoft-ui-xaml/issues/4886
            DataTransferManager dtm = null;
            TypedEventHandler<DataTransferManager, DataRequestedEventArgs> ShareTextHandler = null;
            ShareTextHandler = (sender, e) => {
                var newRequest = e.Request;

                newRequest.Data.Properties.Title = request.Title ?? Mp.Services.ThisAppInfo.ThisAppProductName;

                if (!string.IsNullOrWhiteSpace(request.Text)) {
                    newRequest.Data.SetText(request.Text);
                }

                if (!string.IsNullOrWhiteSpace(request.Uri)) {
                    newRequest.Data.SetWebLink(new Uri(request.Uri));
                }

                dtm.DataRequested -= ShareTextHandler;
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            };

            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            try {
                dtm = ShowShareUi(ShareTextHandler);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error showing shareUi:", ex);
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            }

            return Task.CompletedTask;
        }

        async Task PlatformRequestAsync(MpAvShareMultipleFilesRequest request) {
            var storageFiles = new List<IStorageFile>();
            foreach (var file in request.Files) {
                if (!file.FullPath.IsFile()) {
                    continue;
                }
                IStorageFile storage_file = await StorageFile.GetFileFromPathAsync(file.FullPath);
                storageFiles.Add(storage_file);
            }

            DataTransferManager dtm = null;
            TypedEventHandler<DataTransferManager, DataRequestedEventArgs> shareFileHandler = null;
            shareFileHandler = (sender, e) => {
                var newRequest = e.Request;

                newRequest.Data.SetStorageItems(storageFiles.ToArray());
                newRequest.Data.Properties.Title = request.Title ?? Mp.Services.ThisAppInfo.ThisAppProductName;

                dtm.DataRequested -= shareFileHandler;
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            };
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            try {
                dtm = ShowShareUi(shareFileHandler);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error showing shareUi:", ex);
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            }
        } 
#else
        async Task PlatformRequestAsync(MpAvShareRequestBase request) {
            await Task.Delay(1);
        }
#endif
    }
}
