using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using SkiaSharp;
using Xamarin.Forms;

namespace MonkeyPaste {
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }

    public class MpImageHistogram {
        private static readonly Lazy<MpImageHistogram> _Lazy = new Lazy<MpImageHistogram>(() => new MpImageHistogram());
        public static MpImageHistogram Instance { get { return _Lazy.Value; } }

        public List<KeyValuePair<SKColor, int>> GetStatistics(SKBitmap bitmap) {
            var countDictionary = new Dictionary<SKColor, int>();
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    SKColor currentColor = bitmap.GetPixel(x, y);
                    //if (currentColor.Alpha == 0) {
                    //    continue;
                    //}
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

        public async Task<List<KeyValuePair<SKColor, int>>> GetStatisticsAsync(SKBitmap bitmap) {
            var countDictionary = new Dictionary<SKColor, int>();
            await Task.Run(() => {
                for (int x = 0; x < bitmap.Width; x++) {
                    for (int y = 0; y < bitmap.Height; y++) {
                        SKColor currentColor = bitmap.GetPixel(x, y);
                        if (currentColor.Alpha == 0) {
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
            });
            //order the list from most used to least used before returning
            return countDictionary.OrderByDescending(o => o.Value).ToList();
        }
        public async Task<List<KeyValuePair<SKColor, int>>> GetStatistics2(CachedImage cachedImage) {
            var countDictionary = new Dictionary<SKColor, int>();

            byte[] imageArray = await cachedImage.GetImageAsPngAsync((int)cachedImage.Width, (int)cachedImage.Height);

            using (MemoryStream mStream = new MemoryStream()) {
                mStream.Write(imageArray, 0, imageArray.Length);

                var bitmap = new SKBitmap();

                // pin the managed array so that the GC doesn't move it
                var gcHandle = GCHandle.Alloc(imageArray, GCHandleType.Pinned);

                // install the pixels with the color type of the pixel data
                var info = new SKImageInfo((int)cachedImage.Width, (int)cachedImage.Height, SKImageInfo.PlatformColorType);
                var result = bitmap.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes);

                for (int x = 0; x < bitmap.Width; x++) {
                    for (int y = 0; y < bitmap.Height; y++) {
                        SKColor currentColor = bitmap.GetPixel(x, y);
                        if (currentColor.Alpha == 0) {
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
}
