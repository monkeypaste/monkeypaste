using Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MonkeyPaste {
    //public class MpBootstrappedItem {

    //    public Type ItemType { get; set; }
    //    public Dictionary<string,object> Parameters { get; set; }

    //    public MpBootstrappedItem() { }

    //    public MpBootstrappedItem(Type itemType) {
    //        ItemType = itemType;
    //    }

    //    public MpBootstrappedItem(Type itemType, string paramName, object paramVal) : this(itemType) {
    //        Parameters = new Dictionary<string, object>();
    //        Parameters.Add(paramName, paramVal);
    //    }
    //}
    public abstract class MpBootstrapperBase {

        protected List<Type> ignoreTypes = new List<Type>();
        protected int _curIdx = 0;
        protected Stopwatch sw = new Stopwatch();

        private MpINativeInterfaceWrapper _niw;
        protected ContainerBuilder ContainerBuilder { get; private set; }

        public MpBootstrapperBase(MpINativeInterfaceWrapper niw) {
            _niw = niw;
            MpViewModelBase.OnLoaded += MpViewModelBase_OnLoaded;
            MpSingleton2.OnLoaded += MpViewModelBase_OnLoaded;

            Initialize();
            FinishInitialization();
        }

        protected virtual void Initialize() {
            if(_niw == null) {
                throw new System.Exception("Must have native interface wrapper to initialize");
            }
            ContainerBuilder = new ContainerBuilder();
            //Register(typeof(MpMessenger));
            //Register(typeof(MpNativeWrapper),"niw",_niw);
            //Register(typeof(MpPreferences),"prefIo",_niw.GetPreferenceIO());
            //Register(typeof(MpDb),"dbInfo",_niw.GetDbInfo());
            //Register(typeof(MpDataModelProvider),"queryInfo",_niw.GetQueryInfo());
            //Register(typeof(MpPluginManager));
            //Register(typeof(MpRegEx));

            ContainerBuilder.RegisterType<MpMessenger>().AsSelf()
 .AsImplementedInterfaces()
 .SingleInstance().AutoActivate();
            ContainerBuilder.RegisterType<MpNativeWrapper>().SingleInstance().WithParameter("niw", _niw);
            ContainerBuilder.RegisterType<MpPreferences>().SingleInstance().WithParameter("prefIo", _niw.GetPreferenceIO());
            ContainerBuilder.RegisterType<MpDb>().SingleInstance().WithParameter("dbInfo", _niw.GetDbInfo());
            ContainerBuilder.RegisterType<MpDataModelProvider>().SingleInstance().WithParameter("queryInfo", _niw.GetQueryInfo());
            ContainerBuilder.RegisterType<MpPluginManager>().SingleInstance();
            ContainerBuilder.RegisterType<MpRegEx>().SingleInstance();

            while (_curIdx < 7) {
                Thread.Sleep(100);
            }
            return;
            

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
        private void MpViewModelBase_OnLoaded(object sender, object e) {
            //if (!ignoreTypes.Contains(sender.GetType())) {
            //    return;
            //}
            sw.Stop();

            MpConsole.WriteLine($"{sender.GetType()} loaded in {sw.ElapsedMilliseconds} ms");
            _curIdx++;
            sw.Start();
        }
        protected void Register(object arg, string paramName = null, object paramValue = null) {

            sw.Start();

            object obj = null;
            Type objType = null;
            if (arg is Type) {
                objType = arg as Type;
                if (string.IsNullOrEmpty(paramName)) {
                    ContainerBuilder.RegisterType(objType).SingleInstance().AutoActivate();
                } else {
                    ContainerBuilder.RegisterType(objType).SingleInstance().WithParameter(paramName, paramValue);
                }
                //obj = MpResolver.Resolve(objType);                
            } else {
                obj = arg;
                objType = arg.GetType();
                if (string.IsNullOrEmpty(paramName)) {
                    ContainerBuilder.RegisterInstance(arg).SingleInstance();
                } else {
                    throw new Exception("Not sure how to deal w/ this case yet");
                    //ContainerBuilder.RegisterInstance(obj).SingleInstance().WithParameter(paramName, paramValue);
                }
            }
            ignoreTypes.Add(objType);
            return;

            if (obj == null) {
                return;
            }
            var isBusyPropInfo = obj.GetType().GetProperty("IsBusy");
            if (isBusyPropInfo == null) {
                return;
            }
            bool isBusy = (bool)isBusyPropInfo.GetValue(obj);
            while (isBusy) {
                Thread.Sleep(100);
                isBusy = (bool)isBusyPropInfo.GetValue(obj);
            }
            sw.Stop();

            MpConsole.WriteLine($"{obj.GetType()} loaded in {sw.ElapsedMilliseconds} ms");
        }
    }
}



            