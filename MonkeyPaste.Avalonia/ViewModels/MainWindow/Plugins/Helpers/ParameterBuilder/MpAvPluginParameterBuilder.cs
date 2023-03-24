using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginParameterBuilder {
        public static async Task<MpAvParameterViewModelBase> CreateParameterViewModelAsync(
            MpViewModelBase parent,
            MpParameterFormat pf) {
            var result = await CreateParameterViewModelAsync_internal(pf.controlType, null, parent);
            return result;
        }
        public static async Task<MpAvParameterViewModelBase> CreateParameterViewModelAsync(
            MpParameterValue aipv,
            MpIParameterHostViewModel host) {
            MpParameterControlType controlType = MpParameterControlType.None;
            var param = host.ComponentFormat.parameters.FirstOrDefault(x => x.paramId == aipv.ParamId);
            if (param == null) {
                // must be a bug still with reset preset, check 
                Debugger.Break();
                return null;
            }
            controlType = param.controlType;
            var result = await CreateParameterViewModelAsync_internal(controlType, aipv, host as MpViewModelBase);
            return result;
        }

        private static async Task<MpAvParameterViewModelBase> CreateParameterViewModelAsync_internal(
            MpParameterControlType controlType,
            MpParameterValue aipv,
            MpViewModelBase parent) {
            MpAvParameterViewModelBase naipvm = null;


            switch (controlType) {
                case MpParameterControlType.List:
                case MpParameterControlType.MultiSelectList:
                case MpParameterControlType.EditableList:
                case MpParameterControlType.ComboBox:
                    naipvm = new MpAvEnumerableParameterViewModel(parent);
                    break;
                case MpParameterControlType.PasswordBox:
                case MpParameterControlType.TextBox:
                    naipvm = new MpAvTextBoxParameterViewModel(parent);
                    break;
                case MpParameterControlType.CheckBox:
                    naipvm = new MpAvCheckBoxParameterViewModel(parent);
                    break;
                case MpParameterControlType.Button:
                    naipvm = new MpAvButtonParameterViewModel(parent);
                    break;
                case MpParameterControlType.Slider:
                    naipvm = new MpAvSliderParameterViewModel(parent);
                    break;
                case MpParameterControlType.DirectoryChooser:
                case MpParameterControlType.FileChooser:
                    naipvm = new MpAvFileChooserParameterViewModel(parent);
                    break;
                case MpParameterControlType.ComponentPicker:
                    naipvm = new MpAvComponentPickerParameterViewModel(parent);
                    break;
                case MpParameterControlType.ShortcutRecorder:
                    naipvm = new MpAvShortcutRecorderParameterViewModel(parent);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpParameterControlType), controlType));
            }

            await naipvm.InitializeAsync(aipv);

            return naipvm;
        }
    }
}
