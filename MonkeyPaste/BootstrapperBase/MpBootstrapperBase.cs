using Autofac;
using System.Linq;
using System.Reflection;
    
namespace MonkeyPaste {
    public abstract class MpBootstrapperBase {
        protected ContainerBuilder ContainerBuilder { get; private set; }

        public MpBootstrapperBase() {
            Initialize();
            FinishInitialization();
        }

        protected virtual void Initialize() {
            ContainerBuilder = new ContainerBuilder();

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



            