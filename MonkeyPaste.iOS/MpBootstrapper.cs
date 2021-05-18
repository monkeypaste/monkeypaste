using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Autofac;

namespace MonkeyPaste.iOS {
    public class MpBootstrapper : MonkeyPaste.MpBootstrapper {
        protected override void Initialize()
        {
            base.Initialize();
            ContainerBuilder.RegisterType<MpPhotoImporter>().As<MpIPhotoImporter>().SingleInstance();
        }
    }    
}