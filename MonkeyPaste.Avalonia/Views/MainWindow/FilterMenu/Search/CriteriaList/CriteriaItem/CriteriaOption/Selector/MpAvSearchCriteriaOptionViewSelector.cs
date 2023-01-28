
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
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitFlags.EnumerableValue)) {
                //key = "EmptyOptionTemplate";
                key = null;
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitFlags.Enumerable)) {
                key = "EnumerableOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitFlags.Hex)) {
                key = "TextOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitFlags.ByteX4) ||
                scovm.UnitType.HasFlag(MpSearchCriteriaUnitFlags.UnitDecimalX4)) {
                key = "RGBAOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitFlags.Text)) {
                key = "TextOptionTemplate";
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitFlags.DateTime)) {
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
