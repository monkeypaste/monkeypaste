
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpBootstrappedItem {
        public object ItemArg { get; set; }
        public Type ItemType { get; set; }

        public string Label => ItemType.ToString().Replace("Mp", string.Empty).ToLabel();

        public MpBootstrappedItem() { }

        public MpBootstrappedItem(Type itemType) {
            ItemType = itemType;
        }

        public MpBootstrappedItem(Type itemType, object arg) : this(itemType) {
            ItemArg = arg;
        }

        public async Task Register() {
            var sw = Stopwatch.StartNew();
            object itemObj = null;
            object[] args = ItemArg == null ? null : new[] { ItemArg };
            MethodInfo initMethodInfo;
            PropertyInfo propertyInfo = ItemType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

            if(propertyInfo == null) {
                initMethodInfo = ItemType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public);
                if (initMethodInfo == null) {
                    return;
                }
            } else {
                itemObj = propertyInfo.GetValue(null, null);

                initMethodInfo = ItemType.GetMethod("Init");
                if(itemObj == null || initMethodInfo == null) {
                    return;
                }
            }

            if(initMethodInfo.ReturnType == typeof(Task)) {
                var initTask = (Task)initMethodInfo.Invoke(itemObj, args);
                await initTask;
            } else {
                initMethodInfo.Invoke(itemObj, args);
            }

            sw.Stop();
            MpConsole.WriteLine($"{ItemType} loaded in {sw.ElapsedMilliseconds} ms");

            await Task.Delay(300);
        }
    }
}



            