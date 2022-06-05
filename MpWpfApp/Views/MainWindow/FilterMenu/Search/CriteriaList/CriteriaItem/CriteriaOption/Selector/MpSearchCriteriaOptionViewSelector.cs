using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpSearchCriteriaOptionViewSelector : DataTemplateSelector {
        public DataTemplate EnumerableTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate DateTemplate { get; set; }
        public DataTemplate RGBATemplate { get; set; }
        public DataTemplate EmptyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            var scovm = item as MpSearchCriteriaOptionViewModel;
            if(scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.EnumerableValue)) {
                return EmptyTemplate;
            }
            if(scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.Enumerable)) {
                return EnumerableTemplate;
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.Hex)) {
                return TextTemplate;
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.ByteX4) || 
                scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.UnitDecimalX4)) {
                return RGBATemplate;
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.Text)) {
                return TextTemplate;
            }
            if (scovm.UnitType.HasFlag(MpSearchCriteriaUnitType.DateTime)) {
                return DateTemplate;
            }
            throw new Exception("Uknown Item Type" + scovm.UnitType.EnumToLabel());
        }
    }
}
