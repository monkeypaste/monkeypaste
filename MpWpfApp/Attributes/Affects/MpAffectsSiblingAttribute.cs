using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpAffectsSiblingAttribute : MpAffectsBaseAttribute {
        public MpAffectsSiblingAttribute() { }

        public override int FindAndNotifyProperties(object vmObj, string propertyName, int affectedCount = 0) {
            if (vmObj.GetType() == typeof(object)) {
                return affectedCount;
            }
            dynamic vm = vmObj;
            var pvmObj = vm.Parent as object;

            if (pvmObj == null) {
                return affectedCount;
            }
            //scan parent for child view models of same type
            IEnumerable<PropertyInfo> parentChildVmPropInfos =
                pvmObj.GetType().GetProperties()
                    .Where(x => x.GetCustomAttribute<MpChildViewModelAttribute>() != null &&
                                x.GetCustomAttribute<MpChildViewModelAttribute>().ChildType == vmObj.GetType());

            foreach (var pcvmpi in parentChildVmPropInfos) {
                //get each child info from parent view model (probably only one)

                MpChildViewModelAttribute cvma = pcvmpi.GetCustomAttribute<MpChildViewModelAttribute>();

                MethodInfo onPropChangeMethod = cvma.ChildType.GetMethod("OnPropertyChanged");

                IList childVms = null;
                if (cvma.IsCollection) {
                    //child view model is an observable collection
                    childVms = (IList)pcvmpi.GetValue(pvmObj);
                    if (childVms.Count == 0) {
                        //empty list so continue to other children
                        continue;
                    }
                } else {
                    //child is just a view mdoel
                    var propValue = pcvmpi.GetValue(pvmObj);
                    if (propValue == null) {
                        continue;
                    }
                    childVms = new List<object> { propValue };
                }

                if (childVms == null || childVms.Count == 0) {
                    continue;
                }
                //just scan first child for properties with depends on sibiling attribute w/ propertyName or is the same property
                IEnumerable<PropertyInfo> targetChildProps =
                    childVms[0].GetType().GetProperties()
                            .Where(x =>
                                x.GetCustomAttribute<MpDependsOnSiblingAttribute>() != null &&
                                (x.GetCustomAttribute<MpDependsOnSiblingAttribute>().PropertyNames.Contains(propertyName) ||
                                 x.Name == propertyName));

                foreach (var childVm in childVms) {
                    if(childVm == vmObj) {
                        continue;
                    }
                    //loop through each child vm and notify any dependant property
                    foreach (var depOnParentProp in targetChildProps) {
                        onPropChangeMethod.Invoke(childVm, new object[] { depOnParentProp.Name });
                        affectedCount++;
                    }
                }
            }
            //recurse into each child 
            return affectedCount;
        }
    }
}
