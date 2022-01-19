using Autofac;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using MonkeyPaste;
using System;
using Autofac.Builder;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace MpWpfApp {
    public class MpBootstrapper : MpBootstrapperBase {
        new List<Type> ignoreTypes = new List<Type> {
                typeof(MpIconCollectionViewModel),
                typeof(MpAppCollectionViewModel),
                typeof(MpUrlCollectionViewModel),
                typeof(MpSourceCollectionViewModel)
            };

        public MpBootstrapper(MpINativeInterfaceWrapper niw) : base(niw) { }

        public static void Init() {
            var instance = new MpBootstrapper(new MpWpfWrapper());
        }

        protected override void Initialize() {
            base.Initialize();
            MpViewModelBase.OnLoaded += MpViewModelBase_OnLoaded;
            MpSingleton2.OnLoaded += MpViewModelBase_OnLoaded;

            MpIIconBuilder iconBuilder = new MpImageHelper.MpImageHelper();
            ContainerBuilder.RegisterInstance<MpIIconBuilder>(iconBuilder).SingleInstance();
            ContainerBuilder.RegisterType<MpImageHelper.MpImageHelper>().SingleInstance();
            ContainerBuilder.RegisterType<MpProcessHelper.MpProcessManager>().SingleInstance().WithParameter("ib", iconBuilder);
            
            Register(ignoreTypes[0]);

            //Register(typeof(MpIconCollectionViewModel), null, null);
            //Register(typeof(MpAppCollectionViewModel), null, null);
            //Register(typeof(MpUrlCollectionViewModel), null, null);
            //Register(typeof(MpSourceCollectionViewModel), null, null);

            //ContainerBuilder.RegisterType<MpIconCollectionViewModel>().SingleInstance();
            //ContainerBuilder.RegisterType<MpAppCollectionViewModel>().SingleInstance();
            //ContainerBuilder.RegisterType<MpUrlCollectionViewModel>().SingleInstance();
            //ContainerBuilder.RegisterType<MpSourceCollectionViewModel>().SingleInstance();

            var currentAssembly = Assembly.GetExecutingAssembly();
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

            while (_curIdx < ignoreTypes.Count) {
                Thread.Sleep(100);
            }
            return;
            //ContainerBuilder.RegisterAssemblyTypes(currentAssembly)
            //                    .Where(x => x.IsSubclassOf(typeof(MpSingleton2))).SingleInstance();

            //ContainerBuilder.RegisterAssemblyTypes(currentAssembly)
            //                    .Where(x => x.IsSubclassOf(typeof(MpSingletonViewModel2))).SingleInstance();

        }

        private void MpViewModelBase_OnLoaded(object sender, object e) {
            if(sender.GetType() != ignoreTypes[_curIdx]) {
                return;
            }
            MpConsole.WriteLine($"{sender.GetType()} loaded in {sw.ElapsedMilliseconds} ms");
            _curIdx++;
            if (_curIdx < 4) {
                Register(ignoreTypes[_curIdx]);
                sw.Start();
            }
        }

        
    }
}



            