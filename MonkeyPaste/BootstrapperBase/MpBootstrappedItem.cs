
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpBootstrappedItem {
        public Type ItemType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public MpBootstrappedItem() { }

        public MpBootstrappedItem(Type itemType) {
            ItemType = itemType;
        }

        public MpBootstrappedItem(Type itemType, string paramName, object paramVal) : this(itemType) {
            Parameters = new Dictionary<string, object>();
            Parameters.Add(paramName, paramVal);
        }

        public async Task Register() {
            var sw = Stopwatch.StartNew();

            object itemObj = null;
            if (Parameters == null) {
                Activator.CreateInstance(ItemType);
            } else {
                List<object> args = new List<object>();
                foreach(var kvp in Parameters) {
                    args.Add(kvp.Key);
                    args.Add(kvp.Value);
                }
                Activator.CreateInstance(ItemType, args.ToArray());
            }
                
                
            
            var initMethod = ItemType.GetMethod("Init");
            var initTask = (Task)initMethod.Invoke(itemObj, null);
            await initTask;

            sw.Stop();
            MpConsole.WriteLine($"{itemObj.GetType()} loaded in {sw.ElapsedMilliseconds} ms");
        }
    }
}



            