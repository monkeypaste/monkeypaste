using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MonkeyPaste.Plugin.Wpf_Template {
    public interface MpIPlugin {
        string GetName();

        MpIPluginComponent[] GetComponents();
    }

    public interface MpIPluginComponent {
        void Create(object obj);
    }

    public interface MpIContentPluginComponent : MpIPluginComponent {
        object GetDataObject();

        string[] GetHandledDataFormats();
    }

    public interface MpIAnalyticPluginComponent : MpIPluginComponent {
        string AnalyzeText(string text);
        string AnalyzeImage(ImageSource img);
        string AnalyzeFile(string path);
    }
}
