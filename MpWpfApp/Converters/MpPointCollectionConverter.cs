using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpPointCollectionConverter : IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value.GetType() == typeof(MpObservableCollection<Point>) && targetType == typeof(PointCollection)) {
                var pointCollection = new PointCollection();
                foreach (var point in value as MpObservableCollection<Point>)
                    pointCollection.Add(point);
                return pointCollection;
            }
            return null;

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return null; //not needed
        }

        #endregion
    }
}
