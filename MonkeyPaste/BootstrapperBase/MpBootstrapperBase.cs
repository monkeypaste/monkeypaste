using Autofac;
using System.Linq;
using System.Reflection;
    
namespace MonkeyPaste {
    public abstract class MpBootstrapperBase {
        private MpINativeInterfaceWrapper _niw;
        protected ContainerBuilder ContainerBuilder { get; private set; }

        public MpBootstrapperBase(MpINativeInterfaceWrapper niw) {
            _niw = niw;
            Initialize();
            FinishInitialization();
        }

        protected virtual void Initialize() {
            if(_niw == null) {
                throw new System.Exception("Must have native interface wrapper to initialize");
            }
            ContainerBuilder = new ContainerBuilder();

            ContainerBuilder.RegisterType<MpNativeWrapper>().SingleInstance().WithParameter("niw", _niw);
            ContainerBuilder.RegisterType<MpPreferences>().SingleInstance().WithParameter("prefIo", _niw.GetPreferenceIO());
            ContainerBuilder.RegisterType<MpDb>().SingleInstance().WithParameter("dbInfo", _niw.GetDbInfo());
            ContainerBuilder.RegisterType<MpDataModelProvider>().SingleInstance().WithParameter("queryInfo", _niw.GetQueryInfo());
            ContainerBuilder.RegisterType<MpPluginManager>().SingleInstance();
            //var currentAssembly = Assembly.GetExecutingAssembly();
            //for (int i = 0; i < currentAssembly.GetTypes().Length; i++) {
            //    var curType = currentAssembly.GetTypes()[i];
            //    if(curType.IsSubclassOf(typeof(MpSingleton2))) {
            //        if(curType == typeof(MpNativeWrapper)) {
            //            ContainerBuilder.RegisterType<MpNativeWrapper>().SingleInstance().WithParameter("niw", _niw);
            //        } else if(curType == typeof(MpPreferences)) {
            //            ContainerBuilder.RegisterType<MpPreferences>().SingleInstance().WithParameter("prefIo", _niw.GetPreferenceIO());
            //        } else {
            //            ContainerBuilder.RegisterType(curType).SingleInstance();
            //        }
            //    }
            //}

            //ContainerBuilder.RegisterAssemblyTypes(currentAssembly)
            //                    .Where(x => x.IsSubclassOf(typeof(MpSingleton2))).SingleInstance();

            //ContainerBuilder.RegisterType<MpPreferences>().SingleInstance();
            //ContainerBuilder.RegisterType<MpDb>().SingleInstance();
            //ContainerBuilder.RegisterType<MpDataModelProvider>().SingleInstance();
            //ContainerBuilder.RegisterType<MpMessenger>().SingleInstance();
        }

        private void FinishInitialization() {
            var container = ContainerBuilder.Build();
            MpResolver.Initialize(container);
        }
    }
}



            