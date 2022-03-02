using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIModelBuilder {
        Task<object> Build(object[] args);
    }
    public interface MpIModelBuilder<T> : MpIModelBuilder where T:MpDbModelBase {
        new Task<T> Build(object args);
    }

    public interface MpICopyItemBuilder {
        Task<MpCopyItem> Create();
    }

    public interface MpIUrlBuilder {
        Task<MpUrl> Create(string url);
    }


    public interface MpIProcessIconBuilder {
        MpIIconBuilder IconBuilder { get; set; }
        string GetBase64BitmapFromFolderPath(string filepath);
        string GetBase64BitmapFromFilePath(string filepath);
    }

    public interface MpIAppBuilder : MpIModelBuilder<MpApp> {
        string GetProcessApplicationName(object handleInfo);
        Task<MpApp> Build(object handleInfo, MpIProcessIconBuilder pib);
        
    }
}
