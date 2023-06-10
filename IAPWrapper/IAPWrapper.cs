using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Services.Store;

namespace IAPWrapper {

    public class IAPWrapper {
        //Declare the IInitializeWithWindow interface in your app's code with the ComImport attribute
        [ComImport]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IInitializeWithWindow {
            void Initialize(IntPtr hwnd);
        }

        //Get a StoreContext object by using the GetDefault method
        private static StoreContext storeContext = StoreContext.GetDefault();

        [DllExport(CallingConvention.StdCall)]
        public static string Purchase(string storeID) {
            try {

                _ = PurchaseAsync(storeID);
                return "Done";
            }
            catch (Exception ex) {
                return "Exception:" + ex.ToString() + "exMessage:" + ex.Message;
            }
        }

        static async Task PurchaseAsync(string storeID) {
            IInitializeWithWindow initWindow = (IInitializeWithWindow)(object)storeContext;
            var ptr = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            //Call the IInitializeWithWindow.Initialize method, and pass the handle of the window
            //to be the owner for any modal dialogs that are shown by StoreContext methods.
            initWindow.Initialize(ptr);

            var result = await storeContext.RequestPurchaseAsync(storeID);

            if (result.ExtendedError != null) {
                MessageBox.Show(result.ExtendedError.Message, result.ExtendedError.HResult.ToString());
            }

            MessageBox.Show("Purchase Finished with status " + result.Status);
        }
    }
}
