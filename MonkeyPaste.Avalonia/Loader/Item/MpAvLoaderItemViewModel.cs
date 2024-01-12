using MonkeyPaste.Common.Plugin;
using System;
using System.Reflection;
using System.Threading.Tasks;
//using Xamarin.Forms;

namespace MonkeyPaste.Avalonia {
    public class MpAvLoaderItemViewModel : MpAvViewModelBase {
        public object[] ItemArgs { get; set; }
        public Type ItemType { get; set; }
        public string Label { get; set; }

        public MpAvLoaderItemViewModel() : base(null) { }

        public MpAvLoaderItemViewModel(Type itemType, string label = "") : this() {
            Label = label;
            ItemType = itemType;
        }

        public MpAvLoaderItemViewModel(Type itemType, string label = "", params object[] args) : this(itemType, label) {
            ItemArgs = args;
        }

        public async Task LoadItemAsync(bool static_fallback = false) {
            object itemObj = null;
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

                var initTask = (Task)initMethodInfo.Invoke(itemObj, ItemArgs);
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
                initMethodInfo.Invoke(itemObj, ItemArgs);
            }
        }
    }
}



