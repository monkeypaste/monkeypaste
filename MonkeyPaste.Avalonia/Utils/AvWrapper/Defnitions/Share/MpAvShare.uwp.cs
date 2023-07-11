using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Notifications;
using WinRT;

namespace MonkeyPaste.Avalonia {
    [System.Runtime.InteropServices.ComImport, System.Runtime.InteropServices.Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDataTransferManagerInterop {
        //unsafe void GetForWindow([System.Runtime.InteropServices.In] IntPtr appWindow, [System.Runtime.InteropServices.In] ref Guid riid, [Optional] void** dataTransferManager);
        IntPtr GetForWindow([System.Runtime.InteropServices.In] IntPtr appWindow, [System.Runtime.InteropServices.In] ref Guid riid);
        void ShowShareUIForWindow(IntPtr appWindow);
    }
    public partial class MpAvShare {

        Task PlatformRequestAsync(MpAvShareTextRequest request) {
            // from https://github.com/microsoft/microsoft-ui-xaml/issues/4886
            try {
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;

                IntPtr windowHandle = MpAvWindowManager.ActiveWindow.TryGetPlatformHandle().Handle;
                IDataTransferManagerInterop interop = Windows.ApplicationModel.DataTransfer.DataTransferManager.As<IDataTransferManagerInterop>();

                var guid = Guid.Parse("a5caee9b-8708-49d1-8d36-67d25a8da00c");
                var iop = DataTransferManager.As<IDataTransferManagerInterop>();
                var dataTransferManager = DataTransferManager.FromAbi(iop.GetForWindow(windowHandle, guid));

                dataTransferManager.DataRequested += ShareTextHandler;

                interop.ShowShareUIForWindow(windowHandle);

                void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e) {
                    var newRequest = e.Request;

                    newRequest.Data.Properties.Title = request.Title ?? Mp.Services.ThisAppInfo.ThisAppProductName;

                    if (!string.IsNullOrWhiteSpace(request.Text)) {
                        newRequest.Data.SetText(request.Text);
                    }

                    if (!string.IsNullOrWhiteSpace(request.Uri)) {
                        newRequest.Data.SetWebLink(new Uri(request.Uri));
                    }

                    dataTransferManager.DataRequested -= ShareTextHandler;
                    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
                }

                return Task.CompletedTask;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(string.Empty, ex);
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
                return Task.CompletedTask;
            }
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
            var dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += ShareTextHandler;

            DataTransferManager.ShowShareUI();

            void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e) {
                var newRequest = e.Request;

                newRequest.Data.SetStorageItems(storageFiles.ToArray());
                newRequest.Data.Properties.Title = request.Title ?? Mp.Services.ThisAppInfo.ThisAppProductName;

                dataTransferManager.DataRequested -= ShareTextHandler;
            }
        }
    }
}
