
using Microsoft.CSharp.RuntimeBinder;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
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

        private string _label;
        public string Label {
            get {
                if(_label.IsNullOrEmpty()) {
                    _label = ItemType.ToString();
                    foreach (var rss in _removedSubStrings) {
                        _label = _label.Replace(rss, string.Empty);
                    }
                    _label = _label.ToLabel();
                }
                return _label;
            }
            set {
                if(_label != value) {
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public MpBootstrappedItemViewModel() : base(null) { }

        public MpBootstrappedItemViewModel(MpBootstrapperViewModelBase parent, Type itemType) : base(parent) {
            ItemType = itemType;
        }

        public MpBootstrappedItemViewModel(MpBootstrapperViewModelBase parent, Type itemType, object arg) : this(parent,itemType) {
            ItemArg = arg;
        }

        public async Task LoadItemAsync() {
           // await Device.InvokeOnMainThreadAsync(async () => {
                
            var sw = Stopwatch.StartNew();
            object itemObj = null;
            object[] args = ItemArg == null ? null : new[] { ItemArg };
            MethodInfo initMethodInfo;
            PropertyInfo instancePropertyInfo = ItemType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

            if (instancePropertyInfo == null) {
                // singleton
                initMethodInfo = ItemType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public);
                if (initMethodInfo == null) {
                    //asyn singleton
                    initMethodInfo = ItemType.GetMethod("InitAsync", BindingFlags.Static | BindingFlags.Public);
                    if(initMethodInfo == null) {
                        initMethodInfo = ItemType.GetMethod("InitializeAsync", BindingFlags.Static | BindingFlags.Public);
                        if (initMethodInfo == null) {
                            MpConsole.WriteTraceLine("Error,couldn't load " + ItemType);
                            return;
                        }
                    }
                }
            } else {
                //static class
                itemObj = instancePropertyInfo.GetValue(null, null);

                initMethodInfo = ItemType.GetMethod("Init");
                if (itemObj == null || initMethodInfo == null) {
                    initMethodInfo = ItemType.GetMethod("InitAsync");
                    if(initMethodInfo == null) {
                        initMethodInfo = ItemType.GetMethod("InitializeAsync");
                        if (initMethodInfo == null) {
                            MpConsole.WriteTraceLine("Error,couldn't load " + ItemType);
                            return;
                        }
                    }                    
                }
            }

            if (initMethodInfo.ReturnType == typeof(Task)) {
                MpViewModelBase vm = itemObj as MpViewModelBase;
                if(vm is MpIBootstrappedItem bsi) {
                    Label = bsi.Label;
                }

                var initTask = (Task)initMethodInfo.Invoke(itemObj, args);
                await initTask;

                if(vm != null) {
                    while (vm.IsBusy) {
                        await Task.Delay(100);
                    }
                }
                    
            } else {
                if (itemObj is MpIBootstrappedItem bsi) {
                    Label = bsi.Label;
                }
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



            