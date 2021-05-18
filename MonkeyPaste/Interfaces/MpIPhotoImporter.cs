using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste
{
    public interface MpIPhotoImporter
    {
        Task<ObservableCollection<Photo>> Get(int start, int count, Quality quality = Quality.Low);
        Task<ObservableCollection<Photo>> Get(List<string> filenames, Quality quality = Quality.Low);
    }
    public enum Quality
    {
        Low,
        High
    }
}
