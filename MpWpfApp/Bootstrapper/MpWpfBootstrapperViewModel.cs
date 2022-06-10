using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using MpProcessHelper;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.IO;
using System.Windows.Media;
using MpClipboardHelper;
using System.Collections;

namespace MpWpfApp {
    public class MpWpfBootstrapperViewModel : MpBootstrapperViewModelBase {

        public MpWpfBootstrapperViewModel(MpIPlatformWrapper niw) : base(niw) {
            if(_items == null) {
                _items = new List<MpBootstrappedItemViewModel>();
            }

            _items.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpDocumentHtmlExtension)),
                    new MpBootstrappedItemViewModel(this,typeof(MpProcessManager), Properties.Settings.Default.IgnoredProcessNames),
                    //new MpBootstrappedItemViewModel(this,typeof(MpProcessAutomation)),
                    new MpBootstrappedItemViewModel(this,typeof(MpScreenInformation)),
                    new MpBootstrappedItemViewModel(this,typeof(MpThemeColors)),

                    new MpBootstrappedItemViewModel(this,typeof(MpMeasurements)),
                    new MpBootstrappedItemViewModel(this,typeof(MpFileSystemWatcher)),

                    new MpBootstrappedItemViewModel(this,typeof(MpIconCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAppCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpUrlCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpSourceCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpSystemTrayViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpSoundPlayerGroupCollectionViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpClipTileSortViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpSearchBoxViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAnalyticItemCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpClipboardHandlerCollectionViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpClipTrayViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpShortcutCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpTagTrayViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpMainWindowViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpActionCollectionViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpContextMenuView)),

                    new MpBootstrappedItemViewModel(this,typeof(MpDragDropManager)),

                    new MpBootstrappedItemViewModel(this,typeof(MpClipboardManager)),

                    new MpBootstrappedItemViewModel(this,typeof(MpWpfDataObjectHelper))
                    //new MpBootstrappedItem(typeof(MpMouseHook))
                });
        }

        public override async Task Init() {
            var sw = Stopwatch.StartNew();

            // NOTE Move this later (to first load init native data in app.cs) start
            Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr = string.Empty;

            Properties.Settings.Default.IgnoredProcessNames =
                "csrss" + Environment.NewLine + //Client Server Runtime Subsystem
                "dwm" + Environment.NewLine + //desktop window manager
                "mmc"; // Microsoft Management Console (like event viewer)


            Properties.Settings.Default.Save();


            List<int> doNotShowNotifications = null;
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr)) {
                doNotShowNotifications = Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).ToList();
            }

            await MpNotificationCollectionViewModel.Instance.Init(doNotShowNotifications);

            var nw = new MpNotificationWindow();
            await MpNotificationCollectionViewModel.Instance.RegisterWithWindow(nw);
            //nw.DataContext = MpNotificationCollectionViewModel.Instance;

            await MpNotificationCollectionViewModel.Instance.BeginLoader(this);

            await Task.Delay(300);
            // Parallel

            //await Task.Run(
            //    () => _items
            //            .AsParallel()
            //            .WithDegreeOfParallelism(_items.Count)
            //            .Select(x => x.LoadItem()).ToList());

            // Async
            //await Task.WhenAll(
            //    _items.Select(
            //        x => LoadItem(_items[_items.IndexOf(x)],_items.IndexOf(x))));

            // Sequential (58831 ms 05/30/2022)
            for (int i = 0; i < _items.Count; i++) {
                await LoadItem(_items[i], i);
            }

            MpPlatformWrapper.Services.ClipboardMonitor = MpClipboardManager.MonitorService;
            MpPlatformWrapper.Services.DataObjectRegistrar = MpClipboardManager.RegistrarService;
            MpPortableDataFormats.Init(MpPlatformWrapper.Services.DataObjectRegistrar);

            await Task.Delay(500);
            MpNotificationCollectionViewModel.Instance.FinishLoading();

            //MpRtfToHtmlConverter.Test();

            //var cil = await MpDb.GetItemsAsync<MpCopyItem>();
            //foreach (var ci in cil) {
            //    ci.PreferredFormat = MpCopyItem.GetDefaultFormatForItemType(ci.ItemType);
            //    await ci.WriteToDatabaseAsync();
            //}
            //Debugger.Break();

            //string testHtml = "<p><br></p><p style='font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17); font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>This article can be considered as the fourth instalment in the following sequence of articles:</p><ol style='margin: 10px 0px; padding: 0px 0px 0px 40px; border: 0px; color: rgb(17, 17, 17); font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><span class='ql-ui' contenteditable='false'></span><a href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa' style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Multiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks</a></li><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><span class='ql-ui' contenteditable='false'></span><a href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A' style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Basics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework</a></li><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><span class='ql-ui' contenteditable='false'></span><a href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B' style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Multiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples</a></li></ol><p style='font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17); font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>If you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.</p><p><br></p>";

            //string rtf = MpQuillHtmlToRtfConverter.ConvertQuillHtmlToRtf(testHtml);

            //MpFileIo.WriteTextToFile(@"C:\Users\tkefauver\Desktop\quillHtmlToRtfTest.rtf", rtf);
            //Debugger.Break();
            //MpQuillHtmlToRtfConverter.Test();
            sw.Stop();
            MpConsole.WriteLine($"Bootstrapper loaded in {sw.ElapsedMilliseconds} ms");

            IsLoaded = true;
        }
    }
}