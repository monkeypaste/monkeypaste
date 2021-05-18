using Autofac;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
    
namespace MonkeyPaste {
    public abstract class MpBootstrapper {
        protected ContainerBuilder ContainerBuilder { get; private set; }

        public MpBootstrapper() {
            Initialize();
            FinishInitialization();
        }

        protected virtual void Initialize() {
            ContainerBuilder = new ContainerBuilder();

            ContainerBuilder.RegisterType<MpMainShell>();

            ContainerBuilder.RegisterType<MpFormsLocalStorage>().As<MpILocalStorage>();

            ContainerBuilder.RegisterType<MpDb>().As<MpICopyItemImporter>();

            var currentAssembly = Assembly.GetExecutingAssembly();

            foreach (var type in currentAssembly.DefinedTypes.Where(e => e.IsSubclassOf(typeof(ContentPage))))
            {
                ContainerBuilder.RegisterType(type.AsType());
            }
            foreach (var type in currentAssembly.DefinedTypes.Where(e => e.IsSubclassOf(typeof(MpViewModelBase))))
            {
                ContainerBuilder.RegisterType(type.AsType());
            }

            ContainerBuilder.RegisterType<MpDb>().SingleInstance();
        }

        private void FinishInitialization() {
            var container = ContainerBuilder.Build();
            MpResolver.Initialize(container);
        }
    }
}



            