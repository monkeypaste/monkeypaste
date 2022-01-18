using Autofac;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using MonkeyPaste;
using System;

namespace MpWpfApp {
    public class MpBootstrapper : MpBootstrapperBase {
        public MpBootstrapper(MpINativeInterfaceWrapper niw) : base(niw) { }

        public static void Init() {
            var instance = new MpBootstrapper(new MpWpfWrapper());
        }

        protected override void Initialize() {
            base.Initialize();

            var ignoreTypes = new Type[] {
                typeof(MpIconCollectionViewModel),
                typeof(MpAppCollectionViewModel),
                typeof(MpUrlCollectionViewModel),
                typeof(MpSourceCollectionViewModel)
            };

            ContainerBuilder.RegisterType<MpIconCollectionViewModel>().SingleInstance();
            ContainerBuilder.RegisterType<MpAppCollectionViewModel>().SingleInstance();
            ContainerBuilder.RegisterType<MpUrlCollectionViewModel>().SingleInstance();
            ContainerBuilder.RegisterType<MpSourceCollectionViewModel>().SingleInstance();

            var currentAssembly = Assembly.GetExecutingAssembly();
            //await MpIconCollectionViewModel.Instance.Init();
            //await MpAppCollectionViewModel.Instance.Init();
            //await MpUrlCollectionViewModel.Instance.Init();
            for (int i = 0; i < currentAssembly.GetTypes().Length; i++) {
                var curType = currentAssembly.GetTypes()[i];
                if(ignoreTypes.Contains(curType)) {
                    continue;
                }
                if (curType.IsSubclassOf(typeof(MpSingleton2)) ||
                    curType.IsSubclassOf(typeof(MpSingletonViewModel2))) {
                    ContainerBuilder.RegisterType(curType).SingleInstance();
                }
            }
            //ContainerBuilder.RegisterAssemblyTypes(currentAssembly)
            //                    .Where(x => x.IsSubclassOf(typeof(MpSingleton2))).SingleInstance();

            //ContainerBuilder.RegisterAssemblyTypes(currentAssembly)
            //                    .Where(x => x.IsSubclassOf(typeof(MpSingletonViewModel2))).SingleInstance();

        }
    }
}



            