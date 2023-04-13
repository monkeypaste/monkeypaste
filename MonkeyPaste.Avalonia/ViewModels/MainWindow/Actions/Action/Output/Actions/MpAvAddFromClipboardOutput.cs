using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvAddFromClipboardOutput : MpAvActionOutput {
        public MpPortableDataObject ClipboardDataObject { get; set; }
        public override object OutputData => ClipboardDataObject;

        public override string ActionDescription {
            get {
                return $"DataObject created w/ formats: '{(ClipboardDataObject == null ? string.Empty : string.Join(",", ClipboardDataObject.DataFormatLookup.Select(x => x.Key.Name)))}'";
            }
        }
    }

}
