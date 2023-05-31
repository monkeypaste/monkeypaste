using MonkeyPaste.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
//using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpLoaderItemViewModel : MpViewModelBase<MpLoaderViewModelBase> {
        private string[] _removedSubStrings = new string[] {
            ".",
            "Wpf",
            "View",
            "Model",
            "Av",
            "Mp",
            "Item",
            "Collection",
            " "
        };

        public object ItemArg { get; set; }
        public Type ItemType { get; set; }

        //private string _label;
        //public string Label {
        //    get {
        //        if (_label.IsNullOrEmpty()) {
        //            //_label = ItemType.ToString();
        //            //foreach (var rss in _removedSubStrings) {
        //            //    _label = _label.Replace(rss, string.Empty);
        //            //}
        //            //_label = _label.ToLabel();
        //            _label =
        //                ItemType
        //                .ToString()
        //                .SplitNoEmpty(".")
        //                .Last();
        //        }
        //        return _label;
        //    }
        //    set {
        //        if (_label != value) {
        //            _label = value;
        //            OnPropertyChanged(nameof(Label));
        //        }
        //    }
        //}
        public string Label { get; set; }

        public MpLoaderItemViewModel() : base(null) { }

        public MpLoaderItemViewModel(MpLoaderViewModelBase parent, Type itemType, string label = "") : base(parent) {
            Label = label;
            ItemType = itemType;
        }

        public MpLoaderItemViewModel(MpLoaderViewModelBase parent, Type itemType, string label = "", object arg = null) : this(parent, itemType, label) {
            ItemArg = arg;
        }

        public async Task LoadItemAsync(bool static_fallback = false) {
            object itemObj = null;
            object[] args = ItemArg == null ? null : new[] { ItemArg };
            MethodInfo initMethodInfo;
            PropertyInfo instancePropertyInfo = ItemType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

            if (instancePropertyInfo == null || static_fallback) {
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
                            //MpConsole.WriteTraceLine("Error,couldn't load " + ItemType);
                            //return;
                            await LoadItemAsync(true);
                            return;
                        }
                    }
                }
            }

            if (initMethodInfo.ReturnType == typeof(Task)) {
                if (itemObj is MpIBootstrappedItem bsi) {
                    Label = bsi.Label;
                }

                var initTask = (Task)initMethodInfo.Invoke(itemObj, args);
                await initTask;

                if (itemObj is MpIAsyncObject ao) {
                    while (ao.IsBusy) {
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



