using Autofac;
using MonkeyPaste.Repositories;
using MonkeyPaste.ViewModels.Base;
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
            var currentAssembly = Assembly.GetExecutingAssembly();
            ContainerBuilder = new ContainerBuilder();
            foreach (var type in currentAssembly.DefinedTypes
                                .Where(e =>
                                       e.IsSubclassOf(typeof(Page)) ||
                                       e.IsSubclassOf(typeof(MpViewModelBase)))) {
                ContainerBuilder.RegisterType(type.AsType());
            }
            ContainerBuilder.RegisterType<MpCopyItemRepository>().SingleInstance();
        }

        private void FinishInitialization() {
            var container = ContainerBuilder.Build();
            MpResolver.Initialize(container);
        }
    }
}



            