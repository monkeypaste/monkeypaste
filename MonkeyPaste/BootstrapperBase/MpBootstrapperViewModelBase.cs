using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public abstract class MpBootstrapperViewModelBase : MpViewModelBase, MpIProgressLoader {
        #region Statics

        public static bool IsLoaded { get; protected set; } = false;

        protected static List<MpBootstrappedItem> _items = new List<MpBootstrappedItem>();

        #endregion

        #region Properties

        #region MpIProgressLoader Implementation

        public string IconResourceKey => MpBase64Images.AppIcon;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Detail {
            get => $"{(int)(PercentLoaded * 100.0)}%";
            set => throw new NotImplementedException();
        }

        public double PercentLoaded { get; set; } = 0.0;

        public MpNotificationDialogType DialogType => MpNotificationDialogType.StartupLoader;

        #endregion


        #endregion

        public MpBootstrapperViewModelBase(MpINativeInterfaceWrapper niw) {
            _items.AddRange(
                new List<MpBootstrappedItem>() {
                    new MpBootstrappedItem(typeof(MpConsole)),
                    new MpBootstrappedItem(typeof(MpNativeWrapper),niw),
                    new MpBootstrappedItem(typeof(MpCursor),niw.Cursor),
                    new MpBootstrappedItem(typeof(MpPreferences),niw.PreferenceIO),       
                    new MpBootstrappedItem(typeof(MpTempFileManager)),
                    new MpBootstrappedItem(typeof(MpRegEx)),
                    new MpBootstrappedItem(typeof(MpDb),niw.DbInfo),
                    new MpBootstrappedItem(typeof(MpDataModelProvider),niw.QueryInfo),
                    new MpBootstrappedItem(typeof(MpPluginManager))
                }
                );
        }

        public abstract Task Init();

        protected void ReportItemLoading(MpBootstrappedItem item, int index) {
            MpConsole.WriteLine("Loading " + item.Label + " at idx: " + index);
            if(!MpNotificationCollectionViewModel.Instance.IsVisible) {
                return;
            }

            var lnvm = MpNotificationCollectionViewModel.Instance.CurrentNotificationViewModel as MpLoaderNotificationViewModel;
            if(lnvm == null) {
                // NOTE this occurs when warnings exist and loader is finished
                return;
            }
            PercentLoaded = (double)(index + 1) / (double)_items.Count;

            OnPropertyChanged(nameof(Detail));

            Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

            int dotCount = index % 4;
            Title = "LOADING";
            for (int i = 0; i < dotCount; i++) {
                Title += ".";
            }
        }

    }
}



            