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

            var pvmb = param as MpAvParameterViewModelBase;
            if (pvmb == null) {
                return null;
            }
            string keyStr = pvmb.ControlType.ToString() + "ParameterTemplate";

            if (pvmb.ControlType == MpParameterControlType.FileChooser ||
               pvmb.ControlType == MpParameterControlType.DirectoryChooser) {
                keyStr = "FileChooserParameterTemplate";
            }
            return AvailableTemplates[keyStr].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvParameterViewModelBase;
        }
    }
}
