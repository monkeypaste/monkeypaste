using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpAffectsChildAttribute : MpAffectsBaseAttribute {
        public MpAffectsChildAttribute() { }

        public override int FindAndNotifyProperties(object vm, string propertyName, int affectedCount = 0) {
            //scan view model for all properties with is child vm attribute
            IEnumerable<PropertyInfo> childVmPropInfos = vm.GetType()
                                                            .GetProperties()
                                                            .Where(x => x.GetCustomAttribute<MpChildViewModelAttribute>() != null);

            foreach (var vmpi in childVmPropInfos) {
                //get each child info attribute from child view model property
                MpChildViewModelAttribute cvma = vmpi.GetCustomAttribute<MpChildViewModelAttribute>();

                MethodInfo onPropChangeMethod = cvma.ChildType.GetMethod("OnPropertyChanged");

                IList childVms = null;
                if (cvma.IsCollection) {
                    //child view model is an observable collection
                    childVms = (IList)vmpi.GetValue(vm);
                    if (childVms.Count == 0) {
                        //empty list so continue to other children
                        continue;
                    }
                } else {
                    //child is just a view mdoel
                    var propValue = vmpi.GetValue(vm);
                    if (propValue == null) {
                        continue;
                    }
                    childVms = new List<object> { propValue };
                }

                if (childVms == null || childVms.Count == 0) {
                    continue;
                }
                //just scan one item for properties with depends on parent attribute
                IEnumerable<PropertyInfo> targetChildProps =
                    childVms[0].GetType()
                            .GetProperties()
                            .Where(x =>
                                x.GetCustomAttribute<MpDependsOnParentAttribute>() != null &&
                                    x.GetCustomAttribute<MpDependsOnParentAttribute>()
                                     .PropertyNames
                                     .Contains(propertyName));

                foreach (var childVm in childVms) {
                    //loop through each child vm and notify any dependant property
                    foreach (var depOnParentProp in targetChildProps) {
                        onPropChangeMethod.Invoke(childVm, new object[] { depOnParentProp.Name });
                        affectedCount++;
                    }
                    //recurse into each child 
                    affectedCount += FindAndNotifyProperties(childVm, propertyName, affectedCount);
                }
            }

            return affectedCount;
        }
    }
}
