
using Microsoft.CSharp.RuntimeBinder;
using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpBootstrappedItemViewModel :MpViewModelBase<MpBootstrapperViewModelBase> {
        private string[] _removedSubStrings = new string[] {
            ".",
            "Wpf",
            "View Model",
            "Mp",
            "Collection",
            " "
        };
        public object ItemArg { get; set; }
        public Type ItemType { get; set; }

        public string Label {
            get {
                string label = ItemType.ToString();
                foreach(var rss in _removedSubStrings) {
                    label = label.Replace(rss, string.Empty);
                }
                return label.ToLabel();
            }
        }

        public MpBootstrappedItemViewModel() : base(null) { }

        public MpBootstrappedItemViewModel(MpBootstrapperViewModelBase parent, Type itemType) : base(parent) {
            ItemType = itemType;
        }

        public MpBootstrappedItemViewModel(MpBootstrapperViewModelBase parent, Type itemType, object arg) : this(parent,itemType) {
            ItemArg = arg;
        }

        public async Task LoadItem() {
           // await Device.InvokeOnMainThreadAsync(async () => {
                
            var sw = Stopwatch.StartNew();
            object itemObj = null;
            object[] args = ItemArg == null ? null : new[] { ItemArg };
            MethodInfo initMethodInfo;
            PropertyInfo instancePropertyInfo = ItemType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

            if (instancePropertyInfo == null) {
                initMethodInfo = ItemType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public);
                if (initMethodInfo == null) {
                    return;
                }
            } else {
                itemObj = instancePropertyInfo.GetValue(null, null);

                initMethodInfo = ItemType.GetMethod("Init");
                if (itemObj == null || initMethodInfo == null) {
                    return;
                }
            }

            if (initMethodInfo.ReturnType == typeof(Task)) {
                MpViewModelBase vm = itemObj as MpViewModelBase;
                var initTask = (Task)initMethodInfo.Invoke(itemObj, args);
                await initTask;

                if(vm != null) {
                    while (vm.IsBusy) {
                        await Task.Delay(100);
                    }
                }
                    
            } else {
                initMethodInfo.Invoke(itemObj, args);
            }

            sw.Stop();
            MpConsole.WriteLine($"{ItemType} loaded in {sw.ElapsedMilliseconds} ms");
           // ReportLoaded();
            //await Task.Delay(300);
            //});            
        }
        private void ReportLoaded() {
            var lnvm = MpNotificationCollectionViewModel.Instance.Notifications.FirstOrDefault(x => x is MpLoaderNotificationViewModel);
            if (lnvm == null) {
                // NOTE this occurs when warnings exist and loader is finished
                return;
            }
            Parent.LoadedCount++;
            Parent.OnPropertyChanged(nameof(Parent.PercentLoaded));
            Parent.OnPropertyChanged(nameof(Parent.Detail));

            Parent.Body = string.IsNullOrWhiteSpace(Label) ? Parent.Body : Label;
            
            int dotCount = Parent.LoadedCount % 4;
            Parent.Title = "LOADING";
            for (int i = 0; i < dotCount; i++) {
                Parent.Title += ".";
            }
        }
    }
}



            