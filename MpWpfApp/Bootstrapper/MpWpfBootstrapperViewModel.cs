using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using MpProcessHelper;
using MonkeyPaste.Plugin;
using CefSharp.Wpf;
using CefSharp;
using System.IO;
using CefSharp.SchemeHandler;

namespace MpWpfApp {
    public class MpWpfBootstrapperViewModel : MpBootstrapperViewModelBase {

        public MpWpfBootstrapperViewModel(MpINativeInterfaceWrapper niw) : base(niw) {
            if(_items == null) {
                _items = new List<MpBootstrappedItem>();
            }

            _items.AddRange(
                new List<MpBootstrappedItem>() {
                    new MpBootstrappedItem(typeof(MpProcessManager)),
                    new MpBootstrappedItem(typeof(MpProcessAutomation)),

                    new MpBootstrappedItem(typeof(MpThemeColors)),

                    new MpBootstrappedItem(typeof(MpMeasurements)),
                    new MpBootstrappedItem(typeof(MpFileSystemWatcher)),

                    new MpBootstrappedItem(typeof(MpIconCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpAppCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpUrlCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpSourceCollectionViewModel)),


                    new MpBootstrappedItem(typeof(MpSystemTrayViewModel)),

                    new MpBootstrappedItem(typeof(MpSoundPlayerGroupCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpClipTileSortViewModel)),
                    new MpBootstrappedItem(typeof(MpSearchBoxViewModel)),

                    new MpBootstrappedItem(typeof(MpAnalyticItemCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpClipTrayViewModel)),

                    new MpBootstrappedItem(typeof(MpShortcutCollectionViewModel)),


                    new MpBootstrappedItem(typeof(MpTagTrayViewModel)),
                    new MpBootstrappedItem(typeof(MpMainWindowViewModel)),

                    new MpBootstrappedItem(typeof(MpActionCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpContextMenu)),

                    new MpBootstrappedItem(typeof(MpDragDropManager)),

                    new MpBootstrappedItem(typeof(MpClipboardHelper.MpClipboardManager),MpWpfPasteHelper.Instance),

                    new MpBootstrappedItem(typeof(MpWpfPasteHelper)),
                    new MpBootstrappedItem(typeof(MpDataObject), new MpWpfPasteObjectBuilder())
                    //new MpBootstrappedItem(typeof(MpMouseHook))
                });
        }

        public override async Task Init() {
            // NOTE Remove this later start
            Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr = string.Empty;
            Properties.Settings.Default.Save();
            // NOTE Remove this later finish

            List<int> doNotShowNotifications = null;
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr)) {
                doNotShowNotifications = Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).ToList();
            }

            await MpNotificationCollectionViewModel.Instance.Init(doNotShowNotifications);

            var nbv = new MpNotificationWindow();
            await MpNotificationCollectionViewModel.Instance.RegisterWithWindow(nbv.NotificationBalloon);
            nbv.DataContext = MpNotificationCollectionViewModel.Instance;

            await MpNotificationCollectionViewModel.Instance.BeginLoader(this);

            for (int i = 0; i < _items.Count; i++) {
                ReportItemLoading(_items[i], i);
                await _items[i].Register();
            }

            MpNotificationCollectionViewModel.Instance.FinishLoading();

            InitCef();
            IsLoaded = true;
                       

            //MpProcessHelper.MpProcessAutomation.StartProcess(
            //    processPath: @"C:\Users\tkefauver\Desktop\test.bat",
            //    args: "",
            //    asAdministrator: true,
            //    isSilent: false,
            //    useShellExecute: false,
            //    workingDirectory: @"C:\Users\tkefauver\Desktop\DS4Windows\",
            //    showError: true,
            //    windowState: MpProcessHelper.WinApi.ShowWindowCommands.Normal,
            //    out string stdOut,
            //    out string stdErr);

            //MpConsole.WriteLine("Output: " + stdOut);
            //MpConsole.WriteLine("Errors: " + stdErr);
        }

        private static void InitCef() {
            //var settings = new CefSettings();

            //// Increase the log severity so CEF outputs detailed information, useful for debugging
            //settings.LogSeverity = LogSeverity.Verbose;
            //// By default CEF uses an in memory cache, to save cached data e.g. to persist cookies you need to specify a cache path
            //// NOTE: The executing user must have sufficient privileges to write to this folder.
            //settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");

            //Cef.Initialize(settings);

            //To support High DPI this must be before CefSharp.BrowserSubprocess.SelfHost.Main so the BrowserSubprocess is DPI Aware
            Cef.EnableHighDPISupport();

            var exitCode = CefSharp.BrowserSubprocess.SelfHost.Main(new string[] { });

            if (exitCode >= 0) {
                return;
            }

            var settings = new CefSettings() {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                //CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                //BrowserSubprocessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName
            };
            settings.RegisterScheme(new CefCustomScheme {
                SchemeName = "localfolder",
                DomainName = "cefsharp",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(
            rootFolder: Path.Combine(Environment.CurrentDirectory, "Resources/Html/Editor"),
            hostName: "cefsharp",
            defaultPage: "Editor2.html" // will default to index.html
        )
            });
            //Example of setting a command line argument
            //Enables WebRTC
            // - CEF Doesn't currently support permissions on a per browser basis see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access
            // - CEF Doesn't currently support displaying a UI for media access permissions
            //
            //NOTE: WebRTC Device Id's aren't persisted as they are in Chrome see https://bitbucket.org/chromiumembedded/cef/issues/2064/persist-webrtc-deviceids-across-restart
            //settings.CefCommandLineArgs.Add("enable-media-stream");
            ////https://peter.sh/experiments/chromium-command-line-switches/#use-fake-ui-for-media-stream
            //settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            ////For screen sharing add (see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access#comment-58677180)
            //settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");

            //Don't perform a dependency check
            //By default this example calls Cef.Initialzie in the CefSharp.MinimalExample.Wpf.App
            //constructor for purposes of providing a self contained single file example we call it here.
            //You could remove this code and use the CefSharp.MinimalExample.Wpf.App example if you 
            //set BrowserSubprocessPath to an absolute path to your main application exe.
            Cef.Initialize(settings, performDependencyCheck: false);
        }
    }
}