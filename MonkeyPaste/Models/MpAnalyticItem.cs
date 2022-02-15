using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    [Flags]
    public enum MpAnalyzerInputFormatFlags {
        None = 0,
        Text = 1,
        Image = 2,
        File = 4
    }

    [Flags]
    public enum MpAnalyzerOutputFormatFlags {
        None = 0,
        Text = 1,
        Image = 2,
        BoundingBox = 4,
        File = 8
    }

    public class MpAnalyticItem : MpJsonModelBase {
        public int IconId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int SortOrderIdx { get; set; } = -1;

        public List<MpAnalyticItemPreset> Presets { get; set; } = new List<MpAnalyticItemPreset>();
        
        public MpBillableItem BillableItem { get; set; }

        public MpAnalyzerInputFormatFlags InputFormatFlags { get; set; }

        public MpAnalyzerOutputFormatFlags OutputFormatFlags { get; set; }


        public static async Task<MpAnalyticItem> Create(
            MpAnalyzerInputFormatFlags inputFormat = MpAnalyzerInputFormatFlags.None,
            MpAnalyzerOutputFormatFlags outputFormat = MpAnalyzerOutputFormatFlags.None,
            string title = "",
            string description = "",
            int sortOrderIdx = -1,
            string iconUrl = "",
            string guid = "") {

            if (sortOrderIdx < 0) {
                sortOrderIdx = await MpDataModelProvider.GetAnalyticItemCount();
            }

            MpIcon icon = null;

            if (!string.IsNullOrEmpty(iconUrl)) {
                var bytes = await MpFileIo.ReadBytesFromUriAsync(iconUrl);
                icon = await MpIcon.Create(bytes.ToBase64String(), false);
            } 
            icon = icon == null ? MpPreferences.ThisAppIcon : icon;

            var newAnalyticItem = new MpAnalyticItem() {
                Guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid,
                IconId = icon.Id,
                InputFormatFlags = inputFormat,
                OutputFormatFlags = outputFormat,
                Title = title,
                Description = description,
                SortOrderIdx = sortOrderIdx
            };

            return newAnalyticItem;
        }

        public MpAnalyticItem() { }
    }
}
