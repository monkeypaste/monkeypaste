using System;
using System.Linq;
using System.Reactive.Linq;

namespace MonkeyPaste {
    public class MpShortcutRef {
        public MpShortcutType ShortcutType { get; set; }
        public int CommandId { get; set; }

        public string CommandParameter {
            get {
                if(CommandId != default) {
                    return CommandId.ToString();
                }
                return null;
            }
        }

        private MpShortcutRef(MpShortcutType stype) {
            ShortcutType = stype;
        }

        public static MpShortcutRef Create(object args) {
            if(args is object[] argParts && 
                argParts.Any(x=>x is MpShortcutType)) {
                var stype = argParts.FirstOrDefault(x => x is MpShortcutType);
                if(stype is MpShortcutType st) {
                    MpShortcutRef sref = new MpShortcutRef(st);
                    if(argParts.Length == 1) {
                        if(MpShortcut.IsUserDefinedShortcut(st)) {
                            throw new Exception("Shortcut Ref error, user-defined shortcuts need CommandId(modelId)");
                        }
                        return sref;
                    }
                    if(MpShortcut.IsUserDefinedShortcut(st) &&
                        argParts.First(x=>x is int) is int commandId) {
                        sref.CommandId = commandId;
                    } 
                    return sref;
                }
            } else if(args is MpShortcutType st) {
                return new MpShortcutRef(st);
            }
            return null;
        }
    }
}
