using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginParameterBuilder {
        public static async Task<MpAvPluginParameterViewModelBase> CreateParameterViewModelAsync(
            MpPluginPresetParameterValue aipv, 
            MpIPluginHost pluginHost,
            MpIPluginComponentViewModel componentViewModel) {
            MpPluginParameterControlType controlType = pluginHost.ComponentFormat.parameters.FirstOrDefault(x => x.paramId == aipv.ParamId).controlType;

            MpAvPluginParameterViewModelBase naipvm = null;

            switch (controlType) {
                case MpPluginParameterControlType.List:
                case MpPluginParameterControlType.MultiSelectList:
                case MpPluginParameterControlType.EditableList:
                case MpPluginParameterControlType.ComboBox:
                    naipvm = new MpAvEnumerableParameterViewModel(componentViewModel);
                    break;
                case MpPluginParameterControlType.PasswordBox:
                case MpPluginParameterControlType.TextBox:
                    naipvm = new MpAvTextBoxParameterViewModel(componentViewModel);
                    break;
                case MpPluginParameterControlType.CheckBox:
                    naipvm = new MpAvCheckBoxParameterViewModel(componentViewModel);
                    break;
                case MpPluginParameterControlType.Slider:
                    naipvm = new MpAvSliderParameterViewModel(componentViewModel);
                    break;
                case MpPluginParameterControlType.DirectoryChooser:
                case MpPluginParameterControlType.FileChooser:
                    naipvm = new MpAvFileChooserParameterViewModel(componentViewModel);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpPluginParameterControlType), controlType));
            }

            await naipvm.InitializeAsync(aipv);

            return naipvm;
        }
    }
}
