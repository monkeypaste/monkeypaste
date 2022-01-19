using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpImageHistogram {
        private static readonly Lazy<MpImageHistogram> _Lazy = new Lazy<MpImageHistogram>(() => new MpImageHistogram());
        public static MpImageHistogram Instance { get { return _Lazy.Value; } }

        //public int[] R { get; private set; }
        //public int[] G { get; private set; }
        //public int[] B { get; private set; }

        public List<KeyValuePair<PixelColor, int>> GetStatistics(BitmapSource bmpSource) {
            var countDictionary = new Dictionary<PixelColor, int>();
            var pixels = MpHelpers.Instance.GetPixels(bmpSource);

            for (int x = 0; x < bmpSource.PixelWidth; x++) {
                for (int y = 0; y < bmpSource.PixelHeight; y++) {
                    PixelColor currentColor = pixels[x, y];
                    if(currentColor.Alpha == 0) {
                        continue;
                    }
                    //If a record already exists for this color, set the count, otherwise just set it as 0
                    int currentCount = countDictionary.ContainsKey(currentColor) ? countDictionary[currentColor] : 0;

                    if (currentCount == 0) {
                        //If this color doesnt already exists in the dictionary, add it
                        countDictionary.Add(currentColor, 1);
                    } else {
                        //If it exists, increment the value and update it
                        countDictionary[currentColor] = currentCount + 1;
                    }
                }
            }
            
            //order the list from most used to least used before returning
            return countDictionary.OrderByDescending(o => o.Value).ToList();
        }
    }
}
