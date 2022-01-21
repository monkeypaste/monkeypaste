
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpBootstrappedItem {
        public Type ItemType { get; set; }

        public MpBootstrappedItem() { }

        public MpBootstrappedItem(Type itemType) {
            ItemType = itemType;
        }


        public async Task Register() {
            var sw = Stopwatch.StartNew();

            // Get a PropertyInfo of specific property type(T).GetProperty(....)
            PropertyInfo propertyInfo;
            propertyInfo = ItemType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

            // Use the PropertyInfo to retrieve the value from the type by not passing in an instance
            object itemObj = propertyInfo.GetValue(null, null);

            var initMethod = ItemType.GetMethod("Init");
            var initTask = (Task)initMethod.Invoke(itemObj, null);
            await initTask;

            sw.Stop();
            MpConsole.WriteLine($"{itemObj.GetType()} loaded in {sw.ElapsedMilliseconds} ms");
        }
    }
}



            