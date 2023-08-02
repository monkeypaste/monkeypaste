using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]

    [TemplatePart("PART_Popup", typeof(Popup))]
    [PseudoClasses(":separator", ":icon", ":open", ":pressed", ":selected")]
    public class MpAvMenuItem : MenuItem {

    }
}
