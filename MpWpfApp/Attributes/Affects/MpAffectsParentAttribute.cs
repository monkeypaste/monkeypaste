using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpAffectsParentAttribute : MpAffectsBaseAttribute {
        public MpAffectsParentAttribute() { }

        public override int FindAndNotifyProperties(object vmObj, string propertyName, int affectedCount = 0) {
            if(vmObj.GetType() == typeof(object)) {
                return affectedCount;
            }
            dynamic vm = vmObj;
            var pvmObj = vm.Parent as object;

            if(pvmObj == null) {
                return affectedCount;
            }
            //scan view model for all properties with is child vm attribute
            IEnumerable<PropertyInfo> parentVmPropInfos = pvmObj.GetType()
                                                            .GetProperties()
                                                            .Where(x => x.GetCustomAttribute<MpDependsOnChildAttribute>() != null &&
                                                                        x.GetCustomAttribute<MpDependsOnChildAttribute>().PropertyNames.Contains(propertyName));

            foreach (var ppi in parentVmPropInfos) {
                MethodInfo onPropChangeMethod = pvmObj.GetType().GetMethod("OnPropertyChanged");
                onPropChangeMethod.Invoke(pvmObj, new object[] { ppi.Name });
                affectedCount++;
            }
            //recurse into each child 
            return affectedCount + FindAndNotifyProperties(pvmObj, propertyName, affectedCount);
        }
    }
}
