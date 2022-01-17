using Autofac;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using MonkeyPaste;
using System;

namespace MpWpfApp {
    public class MpBootstrapper : MpBootstrapperBase {
        public static void Init() {
            var instance = new MpBootstrapper();
        }

        protected override void Initialize() {
            base.Initialize();

            //ContainerBuilder.RegisterType<MpMainWindowViewModel>().SingleInstance();

            var currentAssembly = Assembly.GetExecutingAssembly();
            ContainerBuilder.RegisterAssemblyTypes(currentAssembly)
                                .Where(x => x.IsSubclassOf(typeof(MpSingleton2))).SingleInstance();

            //ContainerBuilder.RegisterAssemblyTypes(currentAssembly)
            //                    .Where(x => x.IsSubclassOf(typeof(MpSingletonViewModel)));
        }


    }
}



            