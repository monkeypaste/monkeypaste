using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Provider;

namespace MonkeyPaste.Droid
{
    public class MpPhotoImporter : MpIPhotoImporter
    {
        private bool hasCheckedPermission;
        private string[] result;
        public bool ContinueWithPermission(bool granted)
        {
            if (!granted)
            {
                return false;
            }
            Android.Net.Uri imageUri =
                    MediaStore.Images.Media.ExternalContentUri;
            //var cursor =
            //             MainActivity.Current.ContentResolver.Query(imageUri, null,
            //             MediaStore.IMediaColumns.MimeType + "=? or " +
            //             MediaStore.IMediaColumns.MimeType + "=?",
            //             new string[] { "image/jpeg", "image/png" },
            //             MediaStore.IMediaColumns.DateModified);
            //var paths = new List<string>();
            //while (cursor.MoveToNext())
            //{
            //    var imageFormatDescriptor = MainActivity.Current.ContentResolver.OpenFileDescriptor(imageUri, "r");
            //    string path = cursor.GetString(imageFormatDescriptor.Fd);
            //    paths.Add(path);
            //}
            var cursor =
             MainActivity.Current.ContentResolver.Query(imageUri, null,
             MediaStore.Images.ImageColumns.MimeType + "=? or " +
             MediaStore.Images.ImageColumns.MimeType + "=?",
             new string[] { "image/jpeg", "image/png" },
                        MediaStore.Images.ImageColumns.DateModified);
            var paths = new List<string>();
            while (cursor.MoveToNext())
            {
                //string imageFormatDescriptor = MainActivity.Current.Get
                string path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data));
                paths.Add(path);
            }
            result = paths.ToArray();
            hasCheckedPermission = true;
            return true;
        }

        public async Task<ObservableCollection<Photo>> Get(int start, int count, Quality quality = Quality.Low)
        {
            if (result == null)
            {
                var succeded = await Import();
                if (!succeded)
                {
                    return new ObservableCollection<Photo>();
                }
            }
            if (result.Length == 0)
            {
                return new ObservableCollection<Photo>();
            }
            Index startIndex = start;
            Index endIndex = start + count;
            if (endIndex.Value >= result.Length)
            {
                endIndex = result.Length - 1;
            }
            if (startIndex.Value > endIndex.Value)
            {
                return new ObservableCollection<Photo>();
            }
            var photos = new ObservableCollection<Photo>();
            foreach (var path in result[startIndex..endIndex])
            {
                var filename = Path.GetFileName(path);
                if(!File.Exists(path))
                {
                    continue;
                }
                var fileBytes = File.ReadAllBytes(path);
                var photo = new Photo()
                {
                    Bytes = fileBytes,
                    Filename = filename
                };
                photos.Add(photo);
                //File.WriteAllBytes(destinationPath, fileBytes);
                //File.Delete(sourcePath);
                //using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                //{
                //    using (var memoryStream = new MemoryStream())
                //    {
                //        stream.CopyTo(memoryStream);
                //        var photo = new Photo()
                //        {
                //            Bytes = memoryStream.ToArray(),
                //            Filename = filename
                //        };
                //        photos.Add(photo);
                //    }                        
                //}

            }
            return photos;
        }

        public async Task<ObservableCollection<Photo>> Get(List<string> filenames, Quality quality = Quality.Low)
        {
            if (result == null)
            {
                var succeded = await Import();
                if (!succeded)
                {
                    return new ObservableCollection<Photo>();
                }
            }
            if (result.Length == 0)
            {
                return new ObservableCollection<Photo>();
            }
            var photos = new ObservableCollection<Photo>();
            foreach (var path in result)
            {
                var filename = Path.GetFileName(path);
                if (!filenames.Contains(filename))
                {
                    continue;
                }
                //var filename = Path.GetFileName(path);
                if (!File.Exists(path))
                {
                    continue;
                }
                var fileBytes = File.ReadAllBytes(path);
                var photo = new Photo()
                {
                    Bytes = fileBytes,
                    Filename = filename
                };
                photos.Add(photo);
                //using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                //{
                //    using (var memoryStream = new MemoryStream())
                //    {
                //        stream.CopyTo(memoryStream);
                //        var photo = new Photo()
                //        {
                //            Bytes = memoryStream.ToArray(),
                //            Filename = filename
                //        };
                //        photos.Add(photo);
                //    }                        
                //}

            }
            return photos;
        }

        private async Task<bool> Import()
        {
            string[] permissions = { Manifest.Permission.ReadExternalStorage };
            if (MainActivity.Current.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted)
            {
                ContinueWithPermission(true);
                return true;
            }
            MainActivity.Current.RequestPermissions(permissions, 33);
            while (hasCheckedPermission)
            {
                await Task.Delay(100);
            }
            return MainActivity.Current.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted;
        }

    }
}
