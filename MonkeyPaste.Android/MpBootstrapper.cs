using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace MonkeyPaste.Droid {
    public class MpBootstrapper : MonkeyPaste.MpBootstrapper
    {
        protected override void Initialize()
        {
            base.Initialize();
            ContainerBuilder.RegisterType<MpPhotoImporter>().As<MpIPhotoImporter>().SingleInstance();
        }
    }
}