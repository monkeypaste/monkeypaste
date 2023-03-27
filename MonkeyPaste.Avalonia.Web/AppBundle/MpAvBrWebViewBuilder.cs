using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Web {
    public class MpAvBrWebViewBuilder :
        MpAvINativeControlBuilder,
        MpIJsImporter,
        MpAvIWebViewInterop {
        //private object embed;
        private Dictionary<string, JSObject> _hostIframeLookup = new Dictionary<string, JSObject>();
        #region Interfaces

        async Task MpIJsImporter.ImportAllAsync() {
            await JSHost.ImportAsync("embed.js", "./embed.js");
            //string editor_path = @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia.Web\AppBundle\Editor\";

            //var efl = Directory.EnumerateFiles(editor_path);
            //foreach (var fp in efl) {
            //    string mod_url = "./" + fp
            //        .Replace(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia.Web\AppBundle\", string.Empty);
            //    await JSHost.ImportAsync(Path.GetFileName(fp), mod_url);
            //    MpConsole.WriteLine($"imported mod name: '{Path.GetFileName(fp)}' mod url: '{mod_url}'");
            //}

        }
        #region MpAvINativeControlBuilder Implementation
        public IPlatformHandle Build(IPlatformHandle parent, Func<IPlatformHandle> createDefault, MpIWebViewHost host) {
            string url = @"file:///C:/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste.Avalonia.Web/AppBundle/Editor/index.html";
            if (host is MpAvPlainHtmlConverterWebView) {
                url += "?converter=true";
            }
            var iframe = EmbedInterop.CreateIframe(host.HostGuid, url);

            _hostIframeLookup.AddOrReplace(host.HostGuid, iframe);
            return new JSObjectControlHandle(iframe);
        }
        #endregion

        #region MpAvIWebViewInterop Implementation

        public void SendMessage(MpAvIPlatformHandleHost nwvh, string msg) {
            if (nwvh is MpIWebViewHost wvh &&
                _hostIframeLookup.TryGetValue(wvh.HostGuid, out JSObject iframe)) {
                EmbedInterop.SendMessageToIframe(iframe, msg);
                return;
            } else {
                MpConsole.WriteLine($"Cannot send msg, iframe not found for host '{nwvh}'");
            }
        }

        public void ReceiveMessage(string bindingName, string msg) {

            MpConsole.WriteLine($"Received '{bindingName}' w/ data: '{msg}'");
        }

        public void Bind(MpIWebViewBindable handler) {
            if (handler is MpAvNativeWebViewHost wvn) {

                EventHandler<string> navReg = (s, e) => {
                    if (_hostIframeLookup.TryGetValue(wvn.HostGuid, out JSObject iframe)) {
                        EmbedInterop.NavigateIframe(iframe, e);
                    }
                };

                handler.OnNavigateRequest += navReg;


                // TODO add detach when unload here?
            }
        }

        #endregion

        #endregion
    }

    public static partial class EmbedInterop {
        public const string EMBED_PATH = "embed.js";

        [JSImport("globalThis.document.createElement")]
        public static partial JSObject CreateElement(string tagName);

        [JSImport("createIframe", EMBED_PATH)]
        public static partial JSObject CreateIframe(string hostGuid, string srcUrl);


        [JSImport("sendMessageToIframe", EMBED_PATH)]
        public static partial JSObject SendMessageToIframe(JSObject iframe, string msg);

        [JSExport]
        public static void receiveMessageFromIframe(string hostGuid, string fn, string msg) {
            if (App.MainView is Control c &&
                c.GetVisualDescendants<MpAvNativeWebViewHost>() is IEnumerable<MpAvNativeWebViewHost> nwvhl &&
                nwvhl.Cast<MpIWebViewHost>()
                .FirstOrDefault(x => x.HostGuid.ToLower() == hostGuid) is MpIWebViewHost wvh) {
                Dispatcher.UIThread.Post(() => {
                    wvh.BindingHandler.HandleBindingNotification(fn.ToEnum<MpAvEditorBindingFunctionType>(), msg);
                });
            } else {
                MpConsole.WriteLine($"[HOST] Cannot receive iframe msg type '{fn}' msg '{msg}'. Cannot find hostGuid '{hostGuid}' ");
            }
        }

        [JSImport("navigateIframe", EMBED_PATH)]
        public static partial JSObject NavigateIframe(JSObject iframe, string url);

        [JSExport]
        public static void iframeNavigated(string hostGuid, string url) {
            if (App.MainView is Control c &&
                c.GetVisualDescendants<MpAvNativeWebViewHost>() is IEnumerable<MpAvNativeWebViewHost> nwvhl &&
                nwvhl.Cast<MpIWebViewHost>()
                .FirstOrDefault(x => x.HostGuid.ToLower() == hostGuid) is MpIWebViewHost wvh) {
                Dispatcher.UIThread.Post(() => {
                    if (wvh is MpIWebViewBindable wvb) {
                        wvb.OnNavigated(url);
                    }
                });
            } else {
                MpConsole.WriteLine($"[HOST] Cannot notify iframe navigated to url '{url}'. Cannot find hostGuid '{hostGuid}' ");
            }
        }

        [JSImport("getWindow", EMBED_PATH)]
        public static partial JSObject GetWindow();
    }
}
