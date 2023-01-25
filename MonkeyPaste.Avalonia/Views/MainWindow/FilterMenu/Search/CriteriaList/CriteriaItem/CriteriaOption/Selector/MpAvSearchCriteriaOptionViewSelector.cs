
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using Pango;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaOptionViewSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public IControl Build(object param) {
            string key = null;
            var scovm = param as MpAvSearchCriteriaOptionViewModel;
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.EnumerableValue)) {
                key = "EmptyOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.Enumerable)) {
                key = "EnumerableOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.Hex)) {
                key = "TextOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.ByteX4) ||
                scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.UnitDecimalX4)) {
                key = "RGBAOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.Text)) {
                key = "TextOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.DateTime)) {
                key = "DateOptionTemplate";
            }
            if(string.IsNullOrEmpty(key)) {
                return null;
            }
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvSearchCriteriaOptionViewModel;
        }
    }
}
