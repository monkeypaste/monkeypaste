using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public static class MpAttachedPropertyHelpers {
        private static Dictionary<object, Dictionary<string, object>> _instancePropertyLookup =
                                              new Dictionary<object, Dictionary<string, object>>();

        public static T GetInstanceProperty<T>(object obj, string propName) {
            var propVal = GetInstanceProperty_Internal(obj, propName);
            if (propVal == null) {
                return default(T);
            }
            return (T)propVal;
        }

        private static object GetInstanceProperty_Internal(object obj, string propName) {
            if (_instancePropertyLookup.TryGetValue(obj, out var objPropLookup)) {
                if (objPropLookup.TryGetValue(propName, out var propVal)) {
                    return propVal;
                }
            }
            return null;
        }

        public static void AddOrReplaceInstanceProperty<T>(object obj, string propName, T newValue) {
            AddOrReplaceInstanceProperty_Internal(obj, propName, newValue);
        }
        private static void AddOrReplaceInstanceProperty_Internal(object obj, string propName, object newValue) {
            if (_instancePropertyLookup.TryGetValue(obj, out Dictionary<string, object> objPropLookup)) {
                //instance has properties
                if (objPropLookup.ContainsKey(propName)) {
                    //instance property already has value
                    objPropLookup[propName] = newValue;
                    _instancePropertyLookup[obj] = objPropLookup;
                } else {
                    //this is a new property for isntance
                    objPropLookup.Add(propName, newValue);
                    _instancePropertyLookup[obj] = objPropLookup;
                }
            } else {
                //instance has not defined any properties
                _instancePropertyLookup.Add(
                    obj,
                    new Dictionary<string, object>() { { propName, newValue } });
            }
        }
    }

}
