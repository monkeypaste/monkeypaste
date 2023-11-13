using MonkeyPaste.Common;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using MonoMac.ScriptingBridge;
using System;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoginLoadTools {
        private bool _isLoginLoadEnabled;
        public bool IsLoadOnLoginEnabled => _isLoginLoadEnabled;

        public void SetLoadOnLogin(bool isLoadOnLogin, bool silent = false) {
            if (isLoadOnLogin) {
                _isLoginLoadEnabled = EnableLoginLoad_deprecated();
                return;
            }
            // TODO figure out how to disable
            // TODO figure out how to check if enabled

        }

        #region Helpers

        private bool EnableLoginLoad_deprecated() {
            // TODO need to do something like this https://github.com/alexzielenski/StartAtLoginController\
            // more info https://stackoverflow.com/questions/35339277/make-swift-cocoa-app-launch-on-startup-on-os-x-10-11/35356972#35356972

            // deprecated from https://stackoverflow.com/questions/17220540/how-to-make-xamarin-mac-app-open-at-login

            bool success = false;
            nint dllHandle = Dlfcn.dlopen(ApplicationServices, 0);
            nint shared_items_handle = nint.Zero;
            nint insert_app_to_shared_items_result_handle = nint.Zero;
            try {
                NSString kLSSharedFileListSessionLoginItems = Dlfcn.GetStringConstant(dllHandle, "kLSSharedFileListSessionLoginItems");
                shared_items_handle = LSSharedFileListCreate(
                     nint.Zero,
                     kLSSharedFileListSessionLoginItems.Handle,
                     nint.Zero);

                NSString kLSSharedFileListItemLast = Dlfcn.GetStringConstant(dllHandle, "kLSSharedFileListItemLast");
                insert_app_to_shared_items_result_handle = LSSharedFileListInsertItemURL(
                    shared_items_handle,
                    kLSSharedFileListItemLast.Handle,
                    nint.Zero,
                    nint.Zero,
                    NSBundle.MainBundle.BundleUrl.Handle,
                    nint.Zero,
                    nint.Zero);

                success = true;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error enabling login load. ", ex);
            }
            finally {
                CFRelease(shared_items_handle);
                CFRelease(insert_app_to_shared_items_result_handle);
            }
            return success;
        }

        #region Imports
        //needed library
        const string ApplicationServices = "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";

        [DllImport(ApplicationServices)]
        static extern nint LSSharedFileListCreate(
                nint inAllocator,
                nint inListType,
                nint listOptions);

        [DllImport(ApplicationServices, CharSet = CharSet.Unicode)]
        static extern void CFRelease(
            nint cf
        );


        [DllImport(ApplicationServices)]
        extern static nint LSSharedFileListInsertItemURL(
            nint inList,
            nint insertAfterThisItem,
            nint inDisplayName,
            nint inIconRef,
            nint inURL,
            nint inPropertiesToSet,
            nint inPropertiesToClear);
        #endregion

        #endregion
    }
}
