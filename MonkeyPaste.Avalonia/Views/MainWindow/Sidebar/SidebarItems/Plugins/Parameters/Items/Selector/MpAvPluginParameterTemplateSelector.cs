using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MonkeyPaste.Common.Plugin;
using Pango;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonkeyPaste.Avalonia {
    public class MpAvPluginParameterTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public IControl Build(object param) {
            if (param == null) {
                return null;
            }

            var pvmb = param as MpAvPluginParameterViewModelBase;
            if (pvmb == null) {
                return null;
            }
            string keyStr = pvmb.ControlType.ToString() + "ParameterTemplate";

            if (pvmb.ControlType == MpPluginParameterControlType.FileChooser ||
               pvmb.ControlType == MpPluginParameterControlType.DirectoryChooser) {
                keyStr = "FileChooserParameterTemplate";
            }

            //var g = (container as FrameworkElement).GetVisualAncestor<Grid>();
            //if (g == null || !g.Resources.Contains(keyStr)) {
            //    return null;
            //}
            //var result = g.Resources[keyStr] as DataTemplate;
            //return result;
            return AvailableTemplates[keyStr].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvPluginParameterViewModelBase;
        }
    }
}
