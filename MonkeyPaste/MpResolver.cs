using Autofac;
using System;

namespace MonkeyPaste {
    public static class MpResolver {
        //container holds config info on how to resolve types (from autofac)
        private static IContainer _container;

        public static void Initialize(IContainer container) {
            _container = container;
        }
        public static T Resolve<T>() {
            return _container.Resolve<T>();
        }

        public static object Resolve(Type t) {
            return _container.Resolve(t);
        }
    }
}

