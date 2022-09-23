
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

        public bool IsViewDependant { get; set; } = false;

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
            object itemObj = null;
            object[] args = ItemArg == null ? null : new[] { ItemArg };
            MethodInfo initMethodInfo;
            PropertyInfo instancePropertyInfo = ItemType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

            if (instancePropertyInfo == null) {
                // class
                initMethodInfo = ItemType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public);
                if (initMethodInfo == null) {
                    initMethodInfo = ItemType.GetMethod("InitAsync", BindingFlags.Static | BindingFlags.Public);
                    if (initMethodInfo == null) {
                        initMethodInfo = ItemType.GetMethod("InitializeAsync", BindingFlags.Static | BindingFlags.Public);
                        if (initMethodInfo == null) {
                            MpConsole.WriteTraceLine("Error,couldn't load " + ItemType);
                            return;
                        }
                    }
                }
            } else {
                // singleton
                itemObj = instancePropertyInfo.GetValue(null, null);

                initMethodInfo = ItemType.GetMethod("Init");
                if (itemObj == null || initMethodInfo == null) {
                    //asyn singleton
                    initMethodInfo = ItemType.GetMethod("InitAsync");
                    if (initMethodInfo == null) {
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
                if (vm is MpIBootstrappedItem bsi) {
                    Label = bsi.Label;
                }

                var initTask = (Task)initMethodInfo.Invoke(itemObj, args);
                await initTask;

                if (vm != null) {
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
        }
    }
}



            