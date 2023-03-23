using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDataObjectExtensions {
        public static async Task<MpCopyItem> ToCopyItemAsync(this IDataObject avdo) {
            bool from_ext = !avdo.ContainsFullContentItem();
            MpPortableDataObject mpdo = await Mp.Services.DataObjectHelperAsync.ReadDragDropDataObjectAsync(avdo) as MpPortableDataObject;

            string drag_ctvm_pub_handle = mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            if (!string.IsNullOrEmpty(drag_ctvm_pub_handle)) {
                var drag_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drag_ctvm_pub_handle);
                if (drag_ctvm != null) {
                    // tile sub-selection drop

                    mpdo.SetData(MpPortableDataFormats.LinuxUriList, new string[] { Mp.Services.SourceRefBuilder.ConvertToRefUrl(drag_ctvm.CopyItem) });
                }
            }

            MpCopyItem result_ci = await Mp.Services.CopyItemBuilder.BuildAsync(
                mpdo,
                transType: MpTransactionType.Created,
                force_ext_sources: from_ext);
            return result_ci;
        }
    }
}
