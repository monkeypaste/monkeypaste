using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpKeyGestureViewModel :MpViewModelBase<object> {
        #region Properties

        #region View Models

        public ObservableCollection<MpKeyViewModel> Keys { get; set; } = new ObservableCollection<MpKeyViewModel>();

        #endregion

        #region Models

        

        #endregion

        #endregion

        public MpKeyGestureViewModel() : base(null) {
            Keys = new ObservableCollection<MpKeyViewModel>();
        }

        public override string ToString() {
            List<KeyValuePair<MpKeyViewModel, DateTime>> downs = new List<KeyValuePair<MpKeyViewModel, DateTime>>();
            List<KeyValuePair<MpKeyViewModel, DateTime>> ups = new List<KeyValuePair<MpKeyViewModel, DateTime>>();

            foreach (var k in Keys) {
                for (int downIdx = 0; downIdx < k.UpDownTimes.Count; downIdx+=2) {
                    downs.Add( new KeyValuePair<MpKeyViewModel, DateTime>( k, k.UpDownTimes[downIdx]));
                }
                for (int upIdx = 1; upIdx < k.UpDownTimes.Count; upIdx += 2) {
                    ups.Add(new KeyValuePair<MpKeyViewModel, DateTime>(k, k.UpDownTimes[upIdx]));
                }
            }
            downs.OrderBy(x => x.Value);
            ups.OrderBy(x => x.Value);

            TimeSpan maxComboDiff = TimeSpan.FromMilliseconds(100);
            DateTime lastUp = DateTime.MinValue;
            List<string> combos = new List<string>();

            for(int i = 0;i < ups.Count;i++) {
                if(i > 0) {
                    var downsToRemove = downs.Where(x => x.Value < ups[i - 1].Value).ToList();
                    for(int j = 0;j < downsToRemove.Count;j++) {
                        downs.Remove(downsToRemove[j]);
                    }
                }
                DateTime upTime = ups[i].Value;
                var comboVml = downs.Where(x => x.Value < upTime);
                var combo = comboVml.Select(x => x.Key).ToList();
                combo.OrderBy(x => x.Key.Priority);
                combos.Add(string.Join(" + ", combo.Select(x => x.Key)));
            }

            //while(true) {
            //    var minUp = ups.Min(x => x.Value);

            //    bool ignoreAdd = false;
            //    if(lastUp > DateTime.MinValue) {
            //        TimeSpan diff = minUp - lastUp;
            //        if(diff <= maxComboDiff) {
            //            ignoreAdd = true;
            //        }
            //    }
            //    if(!ignoreAdd) {
            //        var combo = downs.Where(x => x.Value < minUp && x.Value > lastUp).Select(x => x.Key).ToList();

            //        combo.OrderBy(x => x.Key.Priority);
            //        combos.Add(string.Join("+", combo.Select(x => x.Key)));
            //        if (downs.All(x => x.Value < minUp)) {
            //            break;
            //        }
            //        lastUp = minUp;                    
            //    }

            //    var upToRemove = ups.FirstOrDefault(x => x.Value == minUp);
            //    ups.Remove(upToRemove);
            //}

            return string.Join(",", combos);
        }
    } 
}
