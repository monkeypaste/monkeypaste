using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace MonkeyPaste
{
    public class MpFormsLocalStorage : MpIPhotoGalleryManager {
        public const string FavoritePhotosKey = "FavoritePhotos";
        public async Task<List<string>> GetAsync()
        {
            await Task.Delay(1);
            if (Application.Current.Properties.ContainsKey(FavoritePhotosKey))
            {
                var filenames = (string)Application.Current.Properties[FavoritePhotosKey];
                return JsonConvert.DeserializeObject<List<string>>(filenames);
            }
            return new List<string>();
        }

        public async Task StoreAsync(string filename)
        {
            var filenames = await GetAsync();
            filenames.Add(filename);
            var json = JsonConvert.SerializeObject(filenames);
            Application.Current.Properties[FavoritePhotosKey] = json;
            await Application.Current.SavePropertiesAsync();
        }

    }
}
