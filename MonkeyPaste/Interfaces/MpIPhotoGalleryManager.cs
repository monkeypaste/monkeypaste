using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIPhotoGalleryManager {
        Task StoreAsync(string filename);
        Task<List<string>> GetAsync();
    }
}
